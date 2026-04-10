import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { ProductListComponent } from './product-list.component';
import { AdminApiCategoryApiClient, AdminApiProductsApiClient } from '../../../../api/admin-api.service.generated';
import { AlertService } from '../../../../shared/services/alert.service';

describe('ProductListComponent', () => {
  let component: ProductListComponent;
  let fixture: ComponentFixture<ProductListComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [ProductListComponent],
      providers: [
        {
          provide: AdminApiProductsApiClient,
          useValue: {
            productsGET: () => of({ data: [] }),
            productsPOST: () => of({ message: 'created' }),
            productsPUT: () => of({ message: 'updated' }),
            productsDELETE: () => of({ message: 'deleted' })
          }
        },
        {
          provide: AdminApiCategoryApiClient,
          useValue: {
            categoriesGET: () => of({ data: [] })
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

    fixture = TestBed.createComponent(ProductListComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
