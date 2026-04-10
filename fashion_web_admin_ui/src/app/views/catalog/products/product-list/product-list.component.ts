import { CommonModule } from '@angular/common';
import { AfterViewInit, Component, OnInit, ChangeDetectorRef } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router } from '@angular/router';
import { finalize } from 'rxjs';
import {
  ADMIN_API_BASE_URL,
  AdminApiCategoryApiClient,
  AdminApiProductsApiClient,
  FileParameter,
  ProductResponseDTOApiResponse,
  ProductResponseDTO,
  ProductResponseDTOPageResponseApiResponse,
  CategoryDTO
} from '../../../../api/admin-api.service.generated';
import { AlertService } from '../../../../shared/services/alert.service';
import { DataCacheService } from '../../../../shared/services/data-cache.service';
import { Inject, Optional } from '@angular/core';

interface ProductViewModel {
  id: string;
  name: string;
  description: string;
  price: number;
  thumbnailUrl: string;
  categoryId?: string;
}

interface CategoryOption {
  id: string;
  name: string;
}

@Component({
  selector: 'app-product-list',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './product-list.component.html',
  styleUrl: './product-list.component.scss',
})
export class ProductListComponent implements OnInit, AfterViewInit {
  products: ProductViewModel[] = [];
  categories: CategoryOption[] = [];
  selectedCategoryId = '';
  currentPage = 1;
  totalPages = 1;
  totalItems = 0;
  readonly pageSize = 20;
  private readonly listCacheTtlMs = 3 * 60 * 1000;
  private productsLoadRequestId = 0;
  private categoriesLoadRequestId = 0;
  isLoading = false;
  isSaving = false;
  isDeleting = false;
  thumbnailPreviewUrl = '';

  editingProductId: string | null = null;
  private selectedThumbnailFile: FileParameter | null = null;

  readonly productForm: any;

  constructor(
    private fb: FormBuilder,
    private productApi: AdminApiProductsApiClient,
    private categoryApi: AdminApiCategoryApiClient,
    private alertService: AlertService,
    private dataCacheService: DataCacheService,
    private router: Router,
    private cdr: ChangeDetectorRef,
    private http: HttpClient,
    @Optional() @Inject(ADMIN_API_BASE_URL) private apiBaseUrl?: string
  ) {
    this.productForm = this.fb.group({
      name: ['', [Validators.required, Validators.maxLength(200)]],
      description: ['', [Validators.maxLength(1000)]],
      price: [0, [Validators.required, Validators.min(0)]],
      categoryId: ['']
    });
  }

  ngOnInit(): void {
    // Keep ngOnInit lightweight to avoid expression-changed errors on first check.
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.loadCategories();
      this.loadProducts();
      
    }, 0);
  }

  get canSubmit(): boolean {
    return this.productForm.valid && !this.isSaving;
  }

  get isEditMode(): boolean {
    return this.editingProductId !== null;
  }

  get hasPreviousPage(): boolean {
    return this.currentPage > 1;
  }

  get hasNextPage(): boolean {
    return this.currentPage < this.totalPages;
  }

  get productThumbnailPreview(): string {
    return this.thumbnailPreviewUrl;
  }

  loadProducts(forceRefresh = false, page: number | string = this.currentPage): void {
    const requestId = ++this.productsLoadRequestId;
    this.isLoading = true;
    const safePage = this.normalizePage(page, Number.MAX_SAFE_INTEGER);

    if (forceRefresh) {
      this.dataCacheService.invalidateByPrefix('catalog:products');
    }

    const cacheKey = `catalog:products:${this.selectedCategoryId || 'all'}:${safePage}:${this.pageSize}`;
    this.dataCacheService.getOrSet(
      cacheKey,
      // Both endpoints use (pageIndex, pageSize).
      () => this.selectedCategoryId
        ? this.categoryApi.productsGET(this.selectedCategoryId, safePage, this.pageSize)
        : this.productApi.productsGET2(safePage, this.pageSize),
      this.listCacheTtlMs
    )
      .pipe(finalize(() => {
        if (requestId === this.productsLoadRequestId) {
          this.isLoading = false;
          this.cdr.detectChanges();
        }
      }))
      .subscribe({
        next: (response: ProductResponseDTOPageResponseApiResponse) => {
          if (requestId !== this.productsLoadRequestId) {
            return;
          }

          const items = response.data?.items ?? [];
          const mappedProducts = items.map((item: ProductResponseDTO, index: number) => this.toProductViewModel(item, index));
          const apiTotalPages = Number(response.data?.totalPages ?? 1);

          this.totalItems = Number(response.data?.total ?? mappedProducts.length);
          this.totalPages = Math.max(1, apiTotalPages);
          this.currentPage = this.normalizePage(safePage, this.totalPages);
          this.products = mappedProducts;

          if (this.products.length === 0 && safePage > 1 && safePage > this.totalPages) {
            this.loadProducts(false, this.totalPages);
            return;
          }

          this.cdr.detectChanges();
        },
        error: () => {
          this.alertService.error('Khong the tai danh sach san pham.');
        }
      });
  }

  onCategoryChange(categoryId: string): void {
    this.selectedCategoryId = categoryId;
    this.currentPage = 1;
    this.loadProducts(false, 1);
  }

  goToPage(page: number | string): void {
    const targetPage = this.normalizePage(page, this.totalPages);
    if (targetPage === this.currentPage || this.isLoading) {
      return;
    }

    this.loadProducts(false, targetPage);
  }

  get visiblePageItems(): Array<number | '...'> {
    const total = this.totalPages;
    const current = this.currentPage;

    if (total <= 9) {
      return Array.from({ length: total }, (_, index) => index + 1);
    }

    const pages: Array<number | '...'> = [1];
    const start = Math.max(2, current - 1);
    const end = Math.min(total - 1, current + 1);

    if (start > 2) {
      pages.push('...');
    }

    for (let page = start; page <= end; page += 1) {
      pages.push(page);
    }

    if (end < total - 1) {
      pages.push('...');
    }

    pages.push(total);
    return pages;
  }

  loadCategories(forceRefresh = false): void {
    const requestId = ++this.categoriesLoadRequestId;
    if (forceRefresh) {
      this.dataCacheService.invalidateByPrefix('catalog:categories');
    }

    this.dataCacheService.getOrSet(
      'catalog:categories',
      () => this.categoryApi.categoriesGET(),
      this.listCacheTtlMs
    )
      .pipe(finalize(() => {
        if (requestId === this.categoriesLoadRequestId) {
          this.cdr.detectChanges();
        }
      }))
      .subscribe({
      next: response => {
        if (requestId !== this.categoriesLoadRequestId) {
          return;
        }

        const list = response.data ?? [];
        this.categories = list.map((item, index) => this.toCategoryOption(item, index));
        this.cdr.detectChanges();
      },
      error: () => {
        this.alertService.warning('Khong the tai danh muc. Ban van co the nhap categoryId thu cong neu can.');
      }
    });
  }

  startCreate(): void {
    this.editingProductId = null;
    this.productForm.reset({
      name: '',
      description: '',
      price: 0,
      categoryId: ''
    });
    this.selectedThumbnailFile = null;
    this.thumbnailPreviewUrl = '';
  }

  startEdit(product: ProductViewModel): void {
    this.editingProductId = product.id;
    this.selectedThumbnailFile = null;
    this.thumbnailPreviewUrl = product.thumbnailUrl ?? '';
    this.productForm.patchValue({
      name: product.name,
      description: product.description,
      price: product.price,
      categoryId: product.categoryId ?? ''
    });
  }

  onThumbnailFileChange(event: Event): void {
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0] ?? null;

    if (!file) {
      this.selectedThumbnailFile = null;
      this.cdr.detectChanges();
      return;
    }

    this.selectedThumbnailFile = { data: file, fileName: file.name };

    const reader = new FileReader();
    reader.onload = () => {
      this.thumbnailPreviewUrl = typeof reader.result === 'string' ? reader.result : '';
      this.cdr.detectChanges();
    };
    reader.readAsDataURL(file);
    input.value = '';
  }

  saveProduct(): void {
    if (!this.canSubmit || this.isSaving) {
      this.productForm.markAllAsTouched();
      return;
    }

    const formValue = this.productForm.getRawValue();
    const thumbnailFile = this.selectedThumbnailFile;

    this.isSaving = true;

    let request$;
    if (this.isEditMode && this.editingProductId) {
      request$ = thumbnailFile
        ? this.productApi.productsPUT(
          this.editingProductId,
          String(formValue.name ?? ''),
          String(formValue.description ?? ''),
          Number(formValue.price ?? 0),
          thumbnailFile
        )
        : this.updateProductWithoutThumbnail(
          this.editingProductId,
          String(formValue.name ?? ''),
          String(formValue.description ?? ''),
          Number(formValue.price ?? 0)
        );
    } else {
      if (!thumbnailFile) {
        this.isSaving = false;
        this.alertService.error('Vui long chon hinh anh de tai len cho san pham moi.');
        return;
      }

      request$ = this.productApi.productsPOST(
        String(formValue.name ?? ''),
        String(formValue.description ?? ''),
        Number(formValue.price ?? 0),
        0,
        thumbnailFile,
        String(formValue.categoryId ?? '')
      );
    }

    request$
      .pipe(finalize(() => {
        this.isSaving = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: response => {
          this.alertService.success(response?.message ?? 'Them san pham thanh cong.');
          this.invalidateCatalogCache();
          this.startCreate();
          this.loadProducts(true, this.currentPage);
        },
        error: () => {
          this.alertService.error('Them san pham that bai.');
        }
      });
  }

  deleteProduct(product: ProductViewModel): void {
    const confirmed = globalThis.confirm(`Xoa san pham "${product.name}"?`);
    if (!confirmed) {
      return;
    }

    this.isDeleting = true;
    this.productApi.productsDELETE(product.id)
      .pipe(finalize(() => {
        this.isDeleting = false;
      }))
      .subscribe({
        next: response => {
          this.alertService.success(response.message ?? 'Xoa san pham thanh cong.');
          this.invalidateCatalogCache();
          this.loadProducts(true, this.currentPage);
        },
        error: () => {
          this.alertService.error('Xoa san pham that bai.');
        }
      });
  }

  getCategoryName(categoryId?: string): string {
    if (!categoryId) {
      return '-';
    }

    const found = this.categories.find(category => category.id === categoryId);
    return found?.name ?? categoryId;
  }

  private toProductViewModel(item: ProductResponseDTO, index: number): ProductViewModel {
    return {
      id: item.id ?? `row-${index + 1}`,
      name: item.name ?? '',
      description: item.description ?? '',
      price: Number(item.price ?? 0),
      thumbnailUrl: item.thumbnailUrl ?? '',
      categoryId: item.categoryId
    };
  }

  private toCategoryOption(item: CategoryDTO, index: number): CategoryOption {
    return {
      id: item.id ?? `category-${index + 1}`,
      name: item.name ?? `Danh muc ${index + 1}`
    };
  }

  private invalidateCatalogCache(): void {
    this.dataCacheService.invalidateByPrefix('catalog:products');
    this.dataCacheService.invalidateByPrefix('dashboard:products');
    this.dataCacheService.invalidateByPrefix('dashboard:variants:');
  }

  private updateProductWithoutThumbnail(id: string, name: string, description: string, price: number) {
    const url = `${this.apiBaseUrl ?? ''}/api/admin/products/${encodeURIComponent(id)}`;
    const formData = new FormData();
    formData.append('Name', name);
    formData.append('Description', description);
    formData.append('Price', String(price));
    return this.http.put<ProductResponseDTOApiResponse>(url, formData);
  }

  private normalizePage(page: number | string, maxPage: number): number {
    const parsedPage = typeof page === 'number' ? page : Number(page);
    const normalizedPage = Number.isFinite(parsedPage) ? Math.trunc(parsedPage) : 1;
    if (normalizedPage < 1) {
      return 1;
    }

    return Math.min(normalizedPage, Math.max(1, maxPage));
  }

  navigateToVariants(productId: string): void {
    this.router.navigate(['/catalog/product', productId, 'variants']);
  }
}
