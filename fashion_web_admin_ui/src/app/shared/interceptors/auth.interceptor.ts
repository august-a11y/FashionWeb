import { HttpContextToken, HttpErrorResponse, HttpInterceptorFn, HttpRequest } from '@angular/common/http';
import { inject } from '@angular/core';
import { Router } from '@angular/router';
import { catchError, finalize, map, ReplaySubject, switchMap, take, throwError } from 'rxjs';
import { AdminApiAuthenticationApiClient, AuthenticatedResponse, RefreshTokenRequestDTO } from '../../api/admin-api.service.generated';
import { AlertService } from '../services/alert.service';
import { AuthSessionService } from '../services/auth-session.service';

const REFRESH_ATTEMPTED = new HttpContextToken<boolean>(() => false);
let isRefreshing = false;
let refreshTokenSubject = new ReplaySubject<AuthenticatedResponse>(1);
let lastWarningMessage: string | null = null;
let lastWarningAt = 0;

const ALERT_DEDUP_WINDOW_MS = 1200;

function isAuthEndpoint(url: string): boolean {
  return url.includes('/api/auth/login') || url.includes('/api/auth/register') || url.includes('/api/auth/refresh-token');
}

function withAccessToken(request: HttpRequest<unknown>, accessToken: string): HttpRequest<unknown> {
  return request.clone({
    setHeaders: {
      Authorization: `Bearer ${accessToken}`
    }
  });
}

function withRetryMark(request: HttpRequest<unknown>): HttpRequest<unknown> {
  return request.clone({
    context: request.context.set(REFRESH_ATTEMPTED, true)
  });
}

function isNetworkError(error: unknown): boolean {
  return error instanceof HttpErrorResponse && error.status === 0;
}

function shouldLogoutAfterRefreshFailure(error: unknown): boolean {
  if (!(error instanceof HttpErrorResponse)) {
    return true;
  }

  return error.status === 400 || error.status === 401 || error.status === 403;
}

function warnOnce(alertService: AlertService, message: string): void {
  const now = Date.now();
  if (lastWarningMessage === message && now - lastWarningAt < ALERT_DEDUP_WINDOW_MS) {
    return;
  }

  lastWarningMessage = message;
  lastWarningAt = now;
  alertService.warning(message);
}

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const authSessionService = inject(AuthSessionService);
  const authApiClient = inject(AdminApiAuthenticationApiClient);
  const alertService = inject(AlertService);
  const router = inject(Router);
  let request = req.clone({
    withCredentials: true
  });
  const accessToken = authSessionService.getAccessToken();
  request = accessToken && !isAuthEndpoint(request.url)
    ? withAccessToken(request, accessToken)
    : request;
  
  return next(request).pipe(
    catchError((error: unknown) => {
      if (!(error instanceof HttpErrorResponse) || error.status !== 401 || isAuthEndpoint(request.url)) {
        return throwError(() => error);
      }

      if (request.context.get(REFRESH_ATTEMPTED)) {
        return throwError(() => error);
      }

      const currentRefreshToken = authSessionService.getRefreshToken();
      if (!currentRefreshToken) {
        authSessionService.clearSession();
        warnOnce(alertService, 'Your session has expired. Please log in again.');
        void router.navigate(['/auth/login']);
        return throwError(() => error);
      }

      const currentAccessToken = authSessionService.getAccessToken();

      if (isRefreshing) {
        return refreshTokenSubject.pipe(
          take(1),
          switchMap((refreshedSession) => {
            const retriedRequest = withRetryMark(request);
            return next(withAccessToken(retriedRequest, refreshedSession.accessToken ?? ''));
          })
        );
      }

      isRefreshing = true;
      refreshTokenSubject = new ReplaySubject<AuthenticatedResponse>(1);

      const refreshRequest = new RefreshTokenRequestDTO({
        accessToken: currentAccessToken ?? '',
        refreshToken: currentRefreshToken
      });

      return authApiClient.refreshToken(refreshRequest).pipe(
        map((response) => response.data),
        switchMap((refreshedSession) => {
          if (!refreshedSession?.accessToken) {
            throw new Error('Invalid refresh token response.');
          }

          authSessionService.setSession(refreshedSession);
          refreshTokenSubject.next(refreshedSession);

          const retriedRequest = withRetryMark(request);
          return next(withAccessToken(retriedRequest, refreshedSession.accessToken));
        }),
        catchError((refreshError: unknown) => {
          refreshTokenSubject.error(refreshError);

          if (isNetworkError(refreshError)) {
            warnOnce(alertService, 'Network issue while refreshing session. Please try again.');
            return throwError(() => refreshError);
          }

          if (shouldLogoutAfterRefreshFailure(refreshError)) {
            authSessionService.clearSession();
            warnOnce(alertService, 'Your session has expired. Please log in again.');
            void router.navigate(['/auth/login']);
            return throwError(() => refreshError);
          }

          warnOnce(alertService, 'Could not refresh session. Please try again.');
          return throwError(() => refreshError);
        }),
        finalize(() => {
          isRefreshing = false;
        })
      );
    })
  );
};
