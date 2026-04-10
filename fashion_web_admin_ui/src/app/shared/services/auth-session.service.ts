import { Injectable } from '@angular/core';
import { IAuthenticatedResponse } from '../../api/admin-api.service.generated';

@Injectable({
  providedIn: 'root'
})
export class AuthSessionService {
  private readonly accessTokenStorageKey = 'auth.accessToken';
  private readonly refreshTokenStorageKey = 'auth.refreshToken';

  setSession(session: IAuthenticatedResponse): void {
    if (!session.accessToken || session.accessToken.trim().length === 0) {
      this.clearSession();
      return;
    }

    localStorage.setItem(this.accessTokenStorageKey, session.accessToken);

    if (session.refreshToken && session.refreshToken.trim().length > 0) {
      localStorage.setItem(this.refreshTokenStorageKey, session.refreshToken);
    }
  }

  getAccessToken(): string | null {
    return localStorage.getItem(this.accessTokenStorageKey);
  }

  getRefreshToken(): string | null {
    return localStorage.getItem(this.refreshTokenStorageKey);
  }

  isAuthenticated(): boolean {
    const token = this.getAccessToken();
    return typeof token === 'string' && token.trim().length > 0;
  }

  clearSession(): void {
    localStorage.removeItem(this.accessTokenStorageKey);
    localStorage.removeItem(this.refreshTokenStorageKey);
  }
}
