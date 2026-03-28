import { Injectable, signal } from '@angular/core';
import { Router } from '@angular/router';
import { Client, LoginRequest, RegisterRequest, TokenResponse } from '../api/form-builder-api';
import { catchError, tap } from 'rxjs/operators';
import { Observable, throwError } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class AuthService {
  public isAuthenticatedSignal = signal<boolean>(this.hasToken());

  constructor(
    private authClient: Client,
    private router: Router
  ) {}

  private hasToken(): boolean {
    return !!localStorage.getItem('token');
  }

  public isAuthenticated(): boolean {
    return this.isAuthenticatedSignal();
  }

  public login(request: LoginRequest): Observable<TokenResponse> {
    return this.authClient.login(request).pipe(
      tap(response => {
        if (response.accessToken) {
          localStorage.setItem('token', response.accessToken);
          if (response.refreshToken) {
            localStorage.setItem('refreshToken', response.refreshToken);
          }
          this.isAuthenticatedSignal.set(true);
        }
      })
    );
  }

  public register(request: RegisterRequest): Observable<TokenResponse> {
    return this.authClient.register(request).pipe(
      tap(response => {
        if (response.accessToken) {
          localStorage.setItem('token', response.accessToken);
          if (response.refreshToken) {
            localStorage.setItem('refreshToken', response.refreshToken);
          }
          this.isAuthenticatedSignal.set(true);
        }
      })
    );
  }

  public logout(): void {
    const token = localStorage.getItem('token');
    if (token) {
      this.authClient.logout().pipe(catchError(() => throwError(() => new Error('Logout failed')))).subscribe();
    }
    this.clearAuth();
    this.router.navigate(['/login']);
  }

  public clearAuth(): void {
    localStorage.removeItem('token');
    localStorage.removeItem('refreshToken');
    this.isAuthenticatedSignal.set(false);
  }
}
