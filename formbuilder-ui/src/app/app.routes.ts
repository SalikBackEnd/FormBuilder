import { Routes } from '@angular/router';
import { authGuard } from './core/guards/auth.guard';

export const routes: Routes = [
  { path: '', redirectTo: '/login', pathMatch: 'full' },
  {
    path: 'login',
    loadComponent: () => import('./features/auth/login/login.component').then(c => c.LoginComponent)
  },
  {
    path: 'register',
    loadComponent: () => import('./features/auth/register/register.component').then(c => c.RegisterComponent)
  },
  {
    path: 'f/:slug',
    loadComponent: () => import('./features/public-form/public-form.component').then(c => c.PublicFormComponent)
  },
  {
    path: 'dashboard',
    canActivate: [authGuard],
    loadComponent: () => import('./features/dashboard/dashboard.component').then(c => c.DashboardComponent),
    children: [
      { path: '', loadComponent: () => import('./features/dashboard/form-list/form-list.component').then(c => c.FormListComponent) },
      { path: 'builder/:id', loadComponent: () => import('./features/form-builder/form-builder.component').then(c => c.FormBuilderComponent) },
      { path: 'submissions/:id', loadComponent: () => import('./features/submissions/submissions.component').then(c => c.SubmissionsComponent) }
    ]
  },
  { path: '**', redirectTo: '/login' }
];
