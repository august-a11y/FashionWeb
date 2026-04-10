import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, convertToParamMap, Router } from '@angular/router';
import { of } from 'rxjs';

import { ProductInfoComponent } from './product-info.component';
import { AlertService } from '../../../../../shared/services/alert.service';
import { DataCacheService } from '../../../../../shared/services/data-cache.service';
import { AdminApiProductsApiClient } from '../../../../../api/admin-api.service.generated';

describe('ProductInfoComponent', () => {
  let component: ProductInfoComponent;
  let fixture: ComponentFixture<ProductInfoComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductInfoComponent],
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
          provide: AlertService,
          useValue: {
            success: jasmine.createSpy('success'),
            error: jasmine.createSpy('error'),
            warning: jasmine.createSpy('warning')
          }
        },
        {
          provide: DataCacheService,
          useValue: {
            getOrSet: (_key: string, fallbackFactory: () => any, _ttlMs: number) => fallbackFactory()
          }
        },
        {
          provide: AdminApiProductsApiClient,
          useValue: {
            productsGET3: () => of({ data: { id: 'product-1', name: 'Test product' } })
          }
        }
      ]
    })
    .compileComponents();

    fixture = TestBed.createComponent(ProductInfoComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
