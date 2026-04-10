import { ApplicationConfig } from '@angular/core';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { provideAnimationsAsync } from '@angular/platform-browser/animations/async';
import {
  ADMIN_API_BASE_URL,
  AdminApiCartApiClient,
  AdminApiAuthenticationApiClient,
  AdminApiCategoryApiClient,
  AdminApiDashboardApiClient,
  AdminApiOrderApiClient,
  AdminApiProductsApiClient,
  AdminApiVariantsApiClient
} from './api/admin-api.service.generated';
import { environment } from '../environments/environment';
import {
  provideRouter,
  withEnabledBlockingInitialNavigation,
  withHashLocation,
  withInMemoryScrolling,
  withRouterConfig,
  withViewTransitions
} from '@angular/router';
import { IconSetService } from '@coreui/icons-angular';
import { MessageService } from 'primeng/api';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeuix/themes/aura';
import { routes } from './app.routes';
import { authInterceptor } from './shared/interceptors/auth.interceptor';

export const appConfig: ApplicationConfig = {
  providers: [
    { provide: ADMIN_API_BASE_URL, useValue: environment.API_URL },
    provideHttpClient(withInterceptors([authInterceptor])),
    AdminApiCartApiClient,
    AdminApiAuthenticationApiClient,
    AdminApiCategoryApiClient,
    AdminApiDashboardApiClient,
    AdminApiOrderApiClient,
    AdminApiProductsApiClient,
    AdminApiVariantsApiClient,
    MessageService,
    providePrimeNG({
      theme: {
        preset: Aura
      }
    }),
    provideRouter(routes,
      withRouterConfig({
        onSameUrlNavigation: 'reload'
      }),
      withInMemoryScrolling({
        scrollPositionRestoration: 'top',
        anchorScrolling: 'enabled'
      }),
      withEnabledBlockingInitialNavigation(),
      withViewTransitions(),
      withHashLocation()
    ),
    IconSetService,
    provideAnimationsAsync()
  ]
};
