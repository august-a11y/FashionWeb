# FashionWeb

## Running with Docker

The API image is published to Docker Hub as `vyle1008/fashionweb-api`. Use the provided `docker-compose.yml` to start the API together with SQL Server and Redis.

### Prerequisites
- [Docker](https://docs.docker.com/get-docker/) and Docker Compose installed

### Steps

```bash
# (Optional) Copy the example env file and customise the SA password
cp .env.example .env

# Pull the latest images (no local build required)
docker compose pull

# Start all services in the background
docker compose up -d

# Check that both services are running/healthy
docker compose ps

# Tail API logs
docker compose logs -f api
```

The API will be available at **http://localhost:8080** (Swagger UI at `/swagger` if enabled).

To stop and remove containers:

```bash
docker compose down
```

To also remove persistent volumes (database data):

```bash
docker compose down -v
```