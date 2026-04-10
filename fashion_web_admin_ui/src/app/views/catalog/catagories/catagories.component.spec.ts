import { ComponentFixture, TestBed } from '@angular/core/testing';
import { of } from 'rxjs';

import { CatagoriesComponent } from './catagories.component';
import { AdminApiCategoryApiClient } from '../../../api/admin-api.service.generated';
import { AlertService } from '../../../shared/services/alert.service';

describe('CatagoriesComponent', () => {
  let component: CatagoriesComponent;
  let fixture: ComponentFixture<CatagoriesComponent>;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
      imports: [CatagoriesComponent],
      providers: [
        {
          provide: AdminApiCategoryApiClient,
          useValue: {
            categoriesGET: () => of({ data: [] }),
            categoriesPOST: () => of({ message: 'created' }),
            categoriesPUT: () => of({ message: 'updated' }),
            categoriesDELETE: () => of({ message: 'deleted' })
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

    fixture = TestBed.createComponent(CatagoriesComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
