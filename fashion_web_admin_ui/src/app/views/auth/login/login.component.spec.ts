import { ComponentFixture, TestBed } from '@angular/core/testing';
import { convertToParamMap, ActivatedRoute, Router } from '@angular/router';
import { of, throwError } from 'rxjs';

import { LoginComponent } from './login.component';
import { IconSetService } from '@coreui/icons-angular';
import { iconSubset } from '../../../icons/icon-subset';
import { AdminApiAuthenticationApiClient, SwaggerException } from '../../../api/admin-api.service.generated';
import { AlertService } from '../../../shared/services/alert.service';
import { AuthSessionService } from '../../../shared/services/auth-session.service';

describe('LoginComponent', () => {
  let component: LoginComponent;
  let fixture: ComponentFixture<LoginComponent>;
  let iconSetService: IconSetService;
  let authApiClientSpy: jasmine.SpyObj<AdminApiAuthenticationApiClient>;
  let authSessionServiceSpy: jasmine.SpyObj<AuthSessionService>;
  let alertServiceSpy: jasmine.SpyObj<AlertService>;
  let routerSpy: jasmine.SpyObj<Router>;
  let routeSnapshot: { queryParamMap: ReturnType<typeof convertToParamMap> };

  beforeEach(async () => {
    authApiClientSpy = jasmine.createSpyObj<AdminApiAuthenticationApiClient>('AdminApiAuthenticationApiClient', ['login']);
    authSessionServiceSpy = jasmine.createSpyObj<AuthSessionService>('AuthSessionService', ['setSession', 'clearSession', 'isAuthenticated', 'getAccessToken']);
    alertServiceSpy = jasmine.createSpyObj<AlertService>('AlertService', ['success', 'error', 'warning', 'info']);
    routerSpy = jasmine.createSpyObj<Router>('Router', ['navigateByUrl']);
    routeSnapshot = { queryParamMap: convertToParamMap({}) };

    authApiClientSpy.login.and.returnValue(of({ data: { accessToken: 'token', refreshToken: 'refresh' } } as any));

    await TestBed.configureTestingModule({
      imports: [LoginComponent],
      providers: [
        IconSetService,
        { provide: AdminApiAuthenticationApiClient, useValue: authApiClientSpy },
        { provide: AuthSessionService, useValue: authSessionServiceSpy },
        { provide: AlertService, useValue: alertServiceSpy },
        { provide: Router, useValue: routerSpy },
        {
          provide: ActivatedRoute,
          useValue: {
            snapshot: routeSnapshot
          }
        }
      ]
    }).compileComponents();
  });

  beforeEach(() => {
    iconSetService = TestBed.inject(IconSetService);
    iconSetService.icons = { ...iconSubset };

    fixture = TestBed.createComponent(LoginComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create', () => {
    expect(component).toBeTruthy();
  });

  it('should not submit when form is invalid', () => {
    component.loginForm.patchValue({
      username: '',
      password: ''
    });

    component.login();

    expect(authApiClientSpy.login).not.toHaveBeenCalled();
  });

  it('should save session and navigate to dashboard on successful login', () => {
    component.loginForm.patchValue({
      username: 'admin',
      password: 'password'
    });

    component.login();

    expect(authApiClientSpy.login).toHaveBeenCalled();
    expect(authSessionServiceSpy.setSession).toHaveBeenCalledWith(jasmine.objectContaining({ accessToken: 'token' }));
    expect(alertServiceSpy.success).toHaveBeenCalledWith('Login successful');
    expect(routerSpy.navigateByUrl).toHaveBeenCalledWith('/dashboard');
  });

  it('should use returnUrl when provided after successful login', () => {
    routeSnapshot.queryParamMap = convertToParamMap({ returnUrl: '/system/users' });
    component.loginForm.patchValue({
      username: 'admin',
      password: 'password'
    });

    component.login();

    expect(routerSpy.navigateByUrl).toHaveBeenCalledWith('/system/users');
  });

  it('should display backend message for SwaggerException response', () => {
    const swaggerException = new SwaggerException(
      'An unexpected server error occurred.',
      401,
      JSON.stringify({ message: 'Invalid username or password.' }),
      {},
      null
    );
    authApiClientSpy.login.and.returnValue(throwError(() => swaggerException));

    component.loginForm.patchValue({
      username: 'admin',
      password: 'wrong-password'
    });

    component.login();

    expect(alertServiceSpy.error).toHaveBeenCalledWith('Invalid username or password.');
  });
});
