import { ComponentFixture, TestBed } from '@angular/core/testing';
import { ActivatedRoute, provideRouter } from '@angular/router';
import { of } from 'rxjs';

import { ButtonModule, CardModule, FormModule, GridModule } from '@coreui/angular';
import { IconModule } from '@coreui/icons-angular';
import { IconSetService } from '@coreui/icons-angular';
import { iconSubset } from '../../../icons/icon-subset';
import { AdminApiAuthenticationApiClient } from '../../../api/admin-api.service.generated';
import { AlertService } from '../../../shared/services/alert.service';
import { RegisterComponent } from './register.component';

describe('RegisterComponent', () => {
  let component: RegisterComponent;
  let fixture: ComponentFixture<RegisterComponent>;
  let iconSetService: IconSetService;

  beforeEach(async () => {
    await TestBed.configureTestingModule({
    imports: [CardModule, FormModule, GridModule, ButtonModule, IconModule, RegisterComponent],
    providers: [
      IconSetService,
      {
        provide: AdminApiAuthenticationApiClient,
        useValue: {
          register: () => of({ message: 'ok' })
        }
      },
      {
        provide: AlertService,
        useValue: {
          success: jasmine.createSpy('success'),
          error: jasmine.createSpy('error')
        }
      },
      provideRouter([]),
      {
        provide: ActivatedRoute,
        useValue: {
          snapshot: {
            params: {},
            queryParams: {}
          },
          parent: null
        }
      }
    ]
})
    .compileComponents();
  });

  beforeEach(() => {
    iconSetService = TestBed.inject(IconSetService);
    iconSetService.icons = { ...iconSubset };

    fixture = TestBed.createComponent(RegisterComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });
});
