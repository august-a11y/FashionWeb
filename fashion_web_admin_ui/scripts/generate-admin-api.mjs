import fs from 'node:fs';
import path from 'node:path';
import { spawnSync } from 'node:child_process';
import { fileURLToPath } from 'node:url';

const __filename = fileURLToPath(import.meta.url);
const __dirname = path.dirname(__filename);
const rootDir = path.resolve(__dirname, '..');

const nswagConfigPath = path.join(rootDir, 'nswag-admin.json');
const tempDir = path.join(rootDir, '.nswag-temp');
const patchedSwaggerPath = path.join(tempDir, 'admin-swagger.patched.json');
const patchedNswagConfigPath = path.join(tempDir, 'nswag-admin.patched.json');

function readJson(filePath) {
  return JSON.parse(fs.readFileSync(filePath, 'utf8'));
}

function writeJson(filePath, value) {
  fs.writeFileSync(filePath, JSON.stringify(value, null, 2));
}

function ensure201ForAllPostOperations(openApiDoc) {
  const paths = openApiDoc?.paths;
  if (!paths || typeof paths !== 'object') {
    return 0;
  }

  let patchedCount = 0;
  for (const [route, pathItem] of Object.entries(paths)) {
    const postOperation = pathItem?.post;
    if (!postOperation) {
      continue;
    }

    postOperation.responses ??= {};
    if (postOperation.responses['201']) {
      continue;
    }

    if (postOperation.responses['200']) {
      postOperation.responses['201'] = structuredClone(postOperation.responses['200']);
      patchedCount += 1;
      continue;
    }

    // Fallback for uncommon specs where only default response exists.
    if (postOperation.responses.default) {
      postOperation.responses['201'] = structuredClone(postOperation.responses.default);
      patchedCount += 1;
      continue;
    }

    console.warn(`[nswag-admin] Skipped adding 201 for POST ${route} (no 200/default response found).`);
  }

  return patchedCount;
}

async function fetchSwagger(swaggerUrl) {
  // Local dev Swagger often uses self-signed certificates.
  process.env.NODE_TLS_REJECT_UNAUTHORIZED = '0';

  const response = await fetch(swaggerUrl);
  if (!response.ok) {
    throw new Error(`Failed to fetch swagger: ${response.status} ${response.statusText}`);
  }

  return response.json();
}

async function run() {
  const nswagConfig = readJson(nswagConfigPath);
  const swaggerUrl = nswagConfig?.documentGenerator?.fromDocument?.url;
  if (!swaggerUrl) {
    throw new Error('Missing documentGenerator.fromDocument.url in nswag-admin.json');
  }

  fs.mkdirSync(tempDir, { recursive: true });

  const openApiDoc = await fetchSwagger(swaggerUrl);
  const patchedCount = ensure201ForAllPostOperations(openApiDoc);
  writeJson(patchedSwaggerPath, openApiDoc);

  const patchedConfig = structuredClone(nswagConfig);
  const generatedOutput = patchedConfig?.codeGenerators?.openApiToTypeScriptClient?.output;
  if (typeof generatedOutput === 'string' && generatedOutput.length > 0) {
    patchedConfig.codeGenerators.openApiToTypeScriptClient.output = path.resolve(rootDir, generatedOutput);
  }
  patchedConfig.documentGenerator.fromDocument.url = patchedSwaggerPath;
  patchedConfig.documentGenerator.fromDocument.json = '';
  writeJson(patchedNswagConfigPath, patchedConfig);

  if (patchedCount > 0) {
    console.log(`[nswag-admin] Patched ${patchedCount} POST operation(s) to include 201 response.`);
  } else {
    console.log('[nswag-admin] No POST operation needed 201 patching.');
  }

  const nswagCliPath = path.join(rootDir, 'node_modules', 'nswag', 'bin', 'nswag.js');
  const result = spawnSync(process.execPath, [nswagCliPath, 'run', patchedNswagConfigPath], {
    stdio: 'inherit',
    cwd: rootDir,
    env: process.env
  });

  if (typeof result.status === 'number' && result.status !== 0) {
    process.exit(result.status);
  }

  if (result.error) {
    throw result.error;
  }
}

run().catch((error) => {
  console.error('[nswag-admin] Generation failed:', error instanceof Error ? error.message : error);
  process.exit(1);
});
