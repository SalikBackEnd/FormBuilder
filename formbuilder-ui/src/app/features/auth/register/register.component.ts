import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { RegisterRequest } from '../../../core/api/form-builder-api';
import { extractApiError } from '../../../core/utils/api-error';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-register',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.css']
})
export class RegisterComponent {
  public firstName = '';
  public lastName = '';
  public email = '';
  public password = '';
  public confirmPassword = '';

  public isLoading = signal(false);
  public errorMessage = signal('');

  constructor(
    private authService: AuthService,
    private router: Router,
    private toast: ToastService
  ) {}

  public onSubmit(): void {
    if (!this.email || !this.password || !this.firstName || !this.lastName) return;

    if (this.password !== this.confirmPassword) {
      this.errorMessage.set('Passwords do not match.');
      return;
    }

    this.isLoading.set(true);
    this.errorMessage.set('');

    this.authService.register(new RegisterRequest({
      firstName: this.firstName,
      lastName: this.lastName,
      email: this.email,
      password: this.password
    })).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.isLoading.set(false);
        const { message, errors } = extractApiError(err);
        this.errorMessage.set(errors?.length ? `${message} ${errors.join(' ')}` : message);
        this.toast.apiError(err);
      }
    });
  }
}
