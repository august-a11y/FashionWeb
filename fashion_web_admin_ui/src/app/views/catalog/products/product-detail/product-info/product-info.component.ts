import { AfterViewInit, ChangeDetectorRef, Component, OnInit, OnDestroy } from '@angular/core';
import { CommonModule } from '@angular/common';
import { ActivatedRoute, Router } from '@angular/router';
import { AlertService } from '../../../../../shared/services/alert.service';
import { DataCacheService } from '../../../../../shared/services/data-cache.service';
import { Subject, finalize } from 'rxjs';
import { takeUntil } from 'rxjs/operators';
import { AdminApiProductsApiClient, ProductResponseDTO } from '../../../../../api/admin-api.service.generated';

@Component({
  selector: 'app-product-info',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './product-info.component.html',
  styleUrl: './product-info.component.scss',
})
export class ProductInfoComponent implements OnInit, AfterViewInit, OnDestroy {
  productId: string | null = null;
  product: ProductResponseDTO | null = null;
  isLoading = false;
  private readonly productDetailCacheTtlMs = 3 * 60 * 1000;
  private destroy$ = new Subject<void>();

  constructor(
    private route: ActivatedRoute,
    private router: Router,
    private alertService: AlertService,
    private dataCache: DataCacheService,
    private productsApi: AdminApiProductsApiClient,
    private cdr: ChangeDetectorRef
  ) {}

  ngOnInit(): void {
    // Keep ngOnInit lightweight to avoid expression-changed errors on first check.
  }

  ngAfterViewInit(): void {
    setTimeout(() => {
      this.route.parent?.paramMap.pipe(takeUntil(this.destroy$)).subscribe(params => {
        this.productId = params.get('id');
        if (this.productId) {
          this.loadProductDetails();
        }
        this.cdr.detectChanges();
      });

    }, 0);
    
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }

  loadProductDetails(): void {
    if (!this.productId) {
      this.product = null;
      this.cdr.detectChanges();
      return;
    }

    const cacheKey = `catalog:product-detail:${this.productId}`;
    this.isLoading = true;
    this.dataCache.getOrSet(
      cacheKey,
      () => this.productsApi.productsGET3(this.productId!),
      this.productDetailCacheTtlMs
    )
      .pipe(finalize(() => {
        this.isLoading = false;
        this.cdr.detectChanges();
      }))
      .subscribe({
        next: response => {
          this.product = response.data ?? null;
          this.cdr.detectChanges();
        },
        error: () => {
          this.product = null;
          this.alertService.error('Khong the tai thong tin san pham.');
          this.cdr.detectChanges();
        }
      });
  }

  goBack(): void {
    this.router.navigate(['/catalog/products']);
  }
}
