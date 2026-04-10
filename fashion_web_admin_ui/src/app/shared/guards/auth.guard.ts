import { inject } from '@angular/core';
import { CanActivateChildFn, CanActivateFn, CanMatchFn, Router, UrlSegment } from '@angular/router';
import { AuthSessionService } from '../services/auth-session.service';

export const authGuard: CanActivateFn = (_route, state) => {
  const authSessionService = inject(AuthSessionService);
  const router = inject(Router);

  if (authSessionService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/auth/login'], {
    queryParams: {
      returnUrl: state.url
    }
  });
};

export const authChildGuard: CanActivateChildFn = (_route, state) => {
  const authSessionService = inject(AuthSessionService);
  const router = inject(Router);

  if (authSessionService.isAuthenticated()) {
    return true;
  }

  return router.createUrlTree(['/auth/login'], {
    queryParams: {
      returnUrl: state.url
    }
  });
};

export const guestOnlyGuard: CanMatchFn = (_route, segments: UrlSegment[]) => {
  const authSessionService = inject(AuthSessionService);
  const router = inject(Router);

  if (!authSessionService.isAuthenticated()) {
    return true;
  }

  const fallbackUrl = '/' + segments.map((segment) => segment.path).join('/');
  return router.createUrlTree(['/dashboard'], {
    queryParams: fallbackUrl.length > 1 ? { from: fallbackUrl } : undefined
  });
};
