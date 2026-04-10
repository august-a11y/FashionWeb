import { AfterViewInit, ChangeDetectorRef, Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { HttpClient } from '@angular/common/http';
import { ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { ActivatedRoute, Router } from '@angular/router';
import {
  ADMIN_API_BASE_URL,
  AdminApiVariantsApiClient,
  AdminApiProductsApiClient,
  FileParameter,
  ProductResponseDTO,
  VariantDTOApiResponse,
  VariantDTO
} from '../../../../../api/admin-api.service.generated';
import { AlertService } from '../../../../../shared/services/alert.service';
import { DataCacheService } from '../../../../../shared/services/data-cache.service';
import { Subject, finalize } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { Inject, Optional } from '@angular/core';

@Component({
  selector: 'app-product-variants',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './product-variants.component.html',
  styleUrl: './product-variants.component.scss',
  standalone: true
})
export class ProductVariantsComponent implements OnInit, AfterViewInit, OnDestroy {
  productId: string | null = null;
  productName = '';
  variants: VariantDTO[] = [];
  isLoading = false;
  isDeleting = false;
  isSaving = false;
  showVariantForm = false;
  editingVariantId: string | null = null;
  thumbnailPreviewUrl = '';
  private variantsLoadRequestId = 0;

  variantForm: FormGroup;
  private destroy$ = new Subject<void>();
  private selectedThumbnailFile: FileParameter | null = null;

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private fb: FormBuilder,
    private variantsApi: AdminApiVariantsApiClient,
    private productsApi: AdminApiProductsApiClient,
    private alertService: AlertService,
    private dataCache: DataCacheService,
    private cdr: ChangeDetectorRef,
    private http: HttpClient,
    @Optional() @Inject(ADMIN_API_BASE_URL) private apiBaseUrl?: string
  ) {
    this.variantForm = this.fb.group({
      color: ['', []],
      size: ['', []],
      stockQuantity: [0, [Validators.required, Validators.min(0)]],
      price: [0, [Validators.required, Validators.min(0)]]
    });
  }

  ngOnInit(): void {
    // Keep ngOnInit lightweight to avoid expression-changed errors on first check.
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.route.parent?.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
        this.productId = params.get('id');
        if (this.productId) {
          this.loadProductDetails();
          this.loadVariants();
        }
        this.cdr.detectChanges();
      });
    }, 0);
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  get variantThumbnailPreview(): string {
    return this.thumbnailPreviewUrl;
  }

  loadProductDetails(): void {
    if (!this.productId) {
      this.productName = '';
      this.cdr.detectChanges();
      return;
    }

    const cacheKey = `catalog:product-detail:${this.productId}`;
    this.dataCache.getOrSet(
      cacheKey,
      () => this.productsApi.productsGET3(this.productId!),
      3 * 60 * 1000
    )
      .pipe(takeUntil(this.destroy$))
      .subscribe({
        next: (response) => {
          const product = response.data as ProductResponseDTO | undefined;
          this.productName = product?.name?.trim() || this.productId || 'Product';
          this.cdr.detectChanges();
        },
        error: () => {
          this.productName = this.productId || 'Product';
          this.cdr.detectChanges();
        }
      });
  }

  loadVariants(): void {
    if (!this.productId) return;

    const requestId = ++this.variantsLoadRequestId;
    this.isLoading = true;
    this.variantsApi.variantsGET(this.productId)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          if (requestId === this.variantsLoadRequestId) {
            this.isLoading = false;
            this.cdr.detectChanges();
          }
        })
      )
      .subscribe({
        next: (response: any) => {
          if (requestId !== this.variantsLoadRequestId) {
            return;
          }

          this.variants = response.data ?? [];
          this.cdr.detectChanges();
        },
        error: (error: any) => {
          if (requestId !== this.variantsLoadRequestId) {
            return;
          }

          this.alertService.error('Failed to load variants');
          console.error('Load variants error:', error);
          this.cdr.detectChanges();
        }
      });
  }

  openVariantForm(variant?: VariantDTO): void {
    if (variant) {
      this.editingVariantId = variant.id ?? null;
      this.selectedThumbnailFile = null;
      this.thumbnailPreviewUrl = variant.thumbnailUrl ?? '';
      this.variantForm.patchValue({
        color: variant.color,
        size: variant.size,
        stockQuantity: Number(variant.stockQuantity ?? 0),
        price: Number(variant.price ?? 0)
      });
    } else {
      this.editingVariantId = null;
      this.selectedThumbnailFile = null;
      this.thumbnailPreviewUrl = '';
      this.variantForm.reset({ stockQuantity: 0, price: 0 });
    }
    this.showVariantForm = true;
    this.cdr.detectChanges();
  }

  closeVariantForm(): void {
    this.showVariantForm = false;
    this.variantForm.reset({ stockQuantity: 0, price: 0 });
    this.editingVariantId = null;
    this.selectedThumbnailFile = null;
    this.thumbnailPreviewUrl = '';
    this.cdr.detectChanges();
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

  saveVariant(): void {
    if (!this.variantForm.valid || !this.productId || this.isSaving) {
      this.variantForm.markAllAsTouched();
      return;
    }

    const formValue = this.variantForm.getRawValue();
    const stockQuantity = Number(formValue.stockQuantity ?? 0);
    const price = Number(formValue.price ?? 0);
    const thumbnailFile = this.selectedThumbnailFile;

    this.isSaving = true;

    if (this.editingVariantId) {
      const request$ = thumbnailFile
        ? this.variantsApi.variantsPUT(
          this.editingVariantId,
          String(formValue.size ?? ''),
          String(formValue.color ?? ''),
          thumbnailFile,
          price,
          stockQuantity
        )
        : this.updateVariantWithoutThumbnail(
          this.editingVariantId,
          String(formValue.size ?? ''),
          String(formValue.color ?? ''),
          price,
          stockQuantity
        );

      request$
        .pipe(
          takeUntil(this.destroy$),
          finalize(() => {
            this.isSaving = false;
            this.cdr.detectChanges();
          })
        )
        .subscribe({
          next: response => {
            this.alertService.success(response.message ?? 'Cap nhat bien the thanh cong.');
            this.invalidateVariantRelatedCache();
            this.closeVariantForm();
            this.loadVariants();
          },
          error: () => {
            this.alertService.error('Cap nhat bien the that bai.');
          }
        });
      return;
    }

    if (!thumbnailFile) {
      this.isSaving = false;
      this.alertService.error('Vui long chon hinh anh de tai len cho bien the moi.');
      return;
    }

    this.variantsApi.variantsPOST(
      this.productId,
      String(formValue.size ?? ''),
      String(formValue.color ?? ''),
      thumbnailFile,
      price,
      stockQuantity
    )
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.isSaving = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: response => {
          this.alertService.success(response.message ?? 'Them bien the thanh cong.');
          this.invalidateVariantRelatedCache();
          this.closeVariantForm();
          this.loadVariants();
        },
        error: () => {
          this.alertService.error('Them bien the that bai.');
        }
      });
  }

  deleteVariant(variant: VariantDTO): void {
    if (!variant.id) {
      this.alertService.error('Khong tim thay id bien the de xoa.');
      return;
    }

    const label = [variant.color, variant.size].filter(Boolean).join(' - ') || variant.id;
    if (!confirm(`Xoa bien the: ${label}?`)) {
      return;
    }

    this.isDeleting = true;
    this.variantsApi.variantsDELETE(variant.id)
      .pipe(
        takeUntil(this.destroy$),
        finalize(() => {
          this.isDeleting = false;
          this.cdr.detectChanges();
        })
      )
      .subscribe({
        next: response => {
          this.alertService.success(response.message ?? 'Xoa bien the thanh cong.');
          this.invalidateVariantRelatedCache();
          this.loadVariants();
        },
        error: () => {
          this.alertService.error('Xoa bien the that bai.');
        }
      });
  }

  goBack(): void {
    this.router.navigate(['..', 'info'], { relativeTo: this.route.parent });
  }

  private invalidateVariantRelatedCache(): void {
    if (this.productId) {
      this.dataCache.invalidate(`catalog:product-detail:${this.productId}`);
    }
    this.dataCache.invalidateByPrefix('dashboard:variants:');
  }

  private updateVariantWithoutThumbnail(id: string, size: string, color: string, price: number, stockQuantity: number) {
    const url = `${this.apiBaseUrl ?? ''}/api/admin/variants/${encodeURIComponent(id)}`;
    const formData = new FormData();
    formData.append('Size', size);
    formData.append('Color', color);
    formData.append('Price', String(price));
    formData.append('StockQuantity', String(stockQuantity));
    return this.http.put<VariantDTOApiResponse>(url, formData);
  }
}
