import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { of } from 'rxjs';

import { ProductVariantsComponent } from './product-variants.component';
import { AdminApiProductsApiClient, AdminApiVariantsApiClient } from '../../../../../api/admin-api.service.generated';
import { AlertService } from '../../../../../shared/services/alert.service';
import { DataCacheService } from '../../../../../shared/services/data-cache.service';

describe('ProductVariantsComponent', () => {
  let component: ProductVariantsComponent;
  let fixture: ComponentFixture<ProductVariantsComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductVariantsComponent],
      providers: [
        {
          provide: ActivatedRoute,
          useValue: {
            parent: {
              paramMap: of(convertToParamMap({ id: 'product-1' }))
            }
          }
        },
        {
          provide: Router,
          useValue: {
            navigate: jasmine.createSpy('navigate')
          }
        },
        {
          provide: AdminApiVariantsApiClient,
          useValue: {
            variantsGET: () => of({ data: [] })
          }
        },
        {
          provide: AdminApiProductsApiClient,
          useValue: {
            productsGET3: () => of({ data: { id: 'product-1', name: 'Test product' } })
          }
        },
        {
          provide: DataCacheService,
          useValue: {
            getOrSet: (_key: string, fallbackFactory: () => any, _ttlMs: number) => fallbackFactory(),
            invalidate: jasmine.createSpy('invalidate'),
            invalidateByPrefix: jasmine.createSpy('invalidateByPrefix')
          }
        },
        {
          provide: AlertService,
          useValue: {
            success: jasmine.createSpy('success'),
            error: jasmine.createSpy('error'),
            warning: jasmine.createSpy('warning')
          }
        }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductVariantsComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
