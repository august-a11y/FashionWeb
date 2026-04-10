import { Injectable } from '@angular/core';
import { Observable, of } from 'rxjs';
import { finalize, shareReplay, tap } from 'rxjs/operators';

interface CacheEntry<T> {
  value: T;
  expiresAt: number;
}

@Injectable({
  providedIn: 'root'
})
export class DataCacheService {
  private readonly cache = new Map<string, CacheEntry<unknown>>();
  private readonly inFlight = new Map<string, Observable<unknown>>();

  getOrSet<T>(key: string, factory: () => Observable<T>, ttlMs: number): Observable<T> {
    const now = Date.now();
    const cached = this.cache.get(key) as CacheEntry<T> | undefined;
    if (cached && cached.expiresAt > now) {
      return of(cached.value);
    }

    const pending = this.inFlight.get(key) as Observable<T> | undefined;
    if (pending) {
      return pending;
    }

    const request$ = factory().pipe(
      tap((value) => {
        this.cache.set(key, {
          value,
          expiresAt: Date.now() + ttlMs
        });
      }),
      finalize(() => {
        this.inFlight.delete(key);
      }),
      shareReplay(1)
    );

    this.inFlight.set(key, request$ as Observable<unknown>);
    return request$;
  }

  invalidate(key: string): void {
    this.cache.delete(key);
    this.inFlight.delete(key);
  }

  invalidateByPrefix(prefix: string): void {
    for (const key of this.cache.keys()) {
      if (key.startsWith(prefix)) {
        this.cache.delete(key);
      }
    }

    for (const key of this.inFlight.keys()) {
      if (key.startsWith(prefix)) {
        this.inFlight.delete(key);
      }
    }
  }
}