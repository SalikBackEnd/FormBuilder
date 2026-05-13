import { inject } from '@angular/core';
import { HttpInterceptorFn, HttpRequest, HttpHandlerFn, HttpErrorResponse } from '@angular/common/http';
import { throwError, BehaviorSubject, Observable } from 'rxjs';
import { catchError, filter, switchMap, take } from 'rxjs/operators';
import { API_BASE_URL, Client, RefreshRequest } from '../api/form-builder-api';
import { AuthService } from '../services/auth.service';

let isRefreshing = false;
const refreshDone$ = new BehaviorSubject<string | null>(null);

export const jwtInterceptor: HttpInterceptorFn = (req, next) => {
  const apiBaseUrl = inject(API_BASE_URL);
  const authService = inject(AuthService);
  const client = inject(Client);

  const isApiUrl = req.url.startsWith(apiBaseUrl);
  const isRefreshUrl = req.url.includes('/api/Auth/refresh');

  const token = localStorage.getItem('token');
  const authedReq = token && isApiUrl ? addBearer(req, token) : req;

  return next(authedReq).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status !== 401 || isRefreshUrl || !isApiUrl) {
        return throwError(() => err);
      }
      return handle401(req, next, apiBaseUrl, authService, client);
    })
  );
};

function addBearer(req: HttpRequest<unknown>, token: string): HttpRequest<unknown> {
  return req.clone({ setHeaders: { Authorization: `Bearer ${token}` } });
}

function handle401(
  req: HttpRequest<unknown>,
  next: HttpHandlerFn,
  apiBaseUrl: string,
  authService: AuthService,
  client: Client
): Observable<any> {
  if (isRefreshing) {
    // Queue this request until the refresh resolves
    return refreshDone$.pipe(
      filter((t): t is string => t !== null),
      take(1),
      switchMap(newToken => next(addBearer(req, newToken)))
    );
  }

  isRefreshing = true;
  refreshDone$.next(null);

  const accessToken = localStorage.getItem('token') ?? '';
  const refreshToken = localStorage.getItem('refreshToken') ?? '';

  return client.refresh(new RefreshRequest({ accessToken, refreshToken })).pipe(
    switchMap(response => {
      isRefreshing = false;
      const newToken = response.accessToken!;
      localStorage.setItem('token', newToken);
      if (response.refreshToken) {
        localStorage.setItem('refreshToken', response.refreshToken);
      }
      refreshDone$.next(newToken);
      return next(addBearer(req, newToken));
    }),
    catchError(refreshErr => {
      isRefreshing = false;
      refreshDone$.next(null);
      authService.clearAuth();
      return throwError(() => refreshErr);
    })
  );
}
