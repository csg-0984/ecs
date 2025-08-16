import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { BehaviorSubject, tap } from 'rxjs';

interface LoginResponse { token: string; user: { id: string; email: string; name: string; role: string }; }
export interface CurrentUser { id: string; email: string; name: string; role: string; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private tokenKey = 'auth_token';
  private user$ = new BehaviorSubject<CurrentUser | null>(null);
  constructor(private http: HttpClient) {
    const token = this.token;
    if (token) this.fetchMe().subscribe({ next: () => {}, error: () => this.logout() });
  }

  login(email: string, password: string) {
    return this.http.post<LoginResponse>('/api/auth/login', { email, password })
      .pipe(tap(res => {
        localStorage.setItem(this.tokenKey, res.token);
        this.user$.next(res.user);
      }));
  }

  register(name: string, email: string, password: string) {
    return this.http.post('/api/auth/register', { name, email, password });
  }

  fetchMe() {
    return this.http.get<CurrentUser>('/api/auth/me').pipe(tap(u => this.user$.next(u)));
  }

  get token(): string | null { return localStorage.getItem(this.tokenKey); }
  logout() { localStorage.removeItem(this.tokenKey); this.user$.next(null); }
  isAuthenticated(): boolean { return !!this.token; }

  currentUser$() { return this.user$.asObservable(); }
  hasRole(...roles: string[]) {
    const u = this.user$.value; if (!u) return false; return roles.includes(u.role);
  }
}