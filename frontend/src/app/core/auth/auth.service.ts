import { Injectable, computed, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';

import { environment } from '../../../environments/environment';
import { LoginRequest, LoginResponse } from './auth.models';

const STORAGE_KEY = 'gc.auth.session';

interface StoredSession {
  token: string;
  expiresAt: number;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly session = signal<StoredSession | null>(this.readStoredSession());

  readonly isAuthenticated = computed(() => {
    const current = this.session();
    return !!current && current.expiresAt > Date.now();
  });

  constructor(private readonly http: HttpClient) {}

  login(request: LoginRequest): Observable<LoginResponse> {
    return this.http.post<LoginResponse>(`${environment.apiUrl}/auth/login`, request).pipe(
      tap((response) => {
        const stored: StoredSession = {
          token: response.token,
          expiresAt: Date.now() + response.expiresIn * 1000,
        };
        localStorage.setItem(STORAGE_KEY, JSON.stringify(stored));
        this.session.set(stored);
      }),
    );
  }

  logout(): void {
    localStorage.removeItem(STORAGE_KEY);
    this.session.set(null);
  }

  getToken(): string | null {
    const current = this.session();
    return current && current.expiresAt > Date.now() ? current.token : null;
  }

  private readStoredSession(): StoredSession | null {
    const raw = localStorage.getItem(STORAGE_KEY);
    if (!raw) {
      return null;
    }

    try {
      const parsed = JSON.parse(raw) as StoredSession;
      return parsed.expiresAt > Date.now() ? parsed : null;
    } catch {
      return null;
    }
  }
}
