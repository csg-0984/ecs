import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { tap } from 'rxjs/operators';

interface LoginResponse { token: string; user: { id: string; email: string; name: string }; }

@Injectable({ providedIn: 'root' })
export class AuthService {
  private tokenKey = 'auth_token';
  constructor(private http: HttpClient) {}

  login(email: string, password: string) {
    return this.http.post<LoginResponse>('/api/auth/login', { email, password })
      .pipe(tap(res => localStorage.setItem(this.tokenKey, res.token)));
  }

  register(name: string, email: string, password: string) {
    return this.http.post('/api/auth/register', { name, email, password });
  }

  get token(): string | null { return localStorage.getItem(this.tokenKey); }
  logout() { localStorage.removeItem(this.tokenKey); }
  isAuthenticated(): boolean { return !!this.token; }
}