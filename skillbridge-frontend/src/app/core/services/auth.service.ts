import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, tap } from 'rxjs';
import { environment } from '../../../environments/environment';
import type { AuthResponse, AuthUser } from '../models/auth.model';

const TOKEN_KEY = 'skillbridge_token';
const USER_KEY = 'skillbridge_user';

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = environment.apiBaseUrl;

  private readonly token = signal<string | null>(null);
  private readonly user = signal<AuthUser | null>(null);

  readonly isAuthenticated = computed(() => !!this.token());
  readonly currentUser = computed(() => this.user());

  constructor() {
    this.restoreSession();
  }

  getAccessToken(): string | null {
    return this.token();
  }

  registerEmployer(fullName: string, email: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/auth/register-employer`, {
        fullName,
        email,
        password
      })
      .pipe(tap((res) => this.persistSession(res)));
  }

  login(email: string, password: string): Observable<AuthResponse> {
    return this.http
      .post<AuthResponse>(`${this.baseUrl}/auth/login`, { email, password })
      .pipe(tap((res) => this.persistSession(res)));
  }

  logout(): void {
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(USER_KEY);
    this.token.set(null);
    this.user.set(null);
  }

  private persistSession(res: AuthResponse): void {
    localStorage.setItem(TOKEN_KEY, res.accessToken);
    const u: AuthUser = {
      userId: res.userId,
      email: res.email,
      fullName: res.fullName,
      role: res.role
    };
    localStorage.setItem(USER_KEY, JSON.stringify(u));
    this.token.set(res.accessToken);
    this.user.set(u);
  }

  private restoreSession(): void {
    const t = localStorage.getItem(TOKEN_KEY);
    const raw = localStorage.getItem(USER_KEY);
    if (!t || !raw) {
      return;
    }
    try {
      const u = JSON.parse(raw) as AuthUser;
      this.token.set(t);
      this.user.set(u);
    } catch {
      this.logout();
    }
  }
}
