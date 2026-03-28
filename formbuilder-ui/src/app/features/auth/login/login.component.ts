import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { LoginRequest } from '../../../core/api/form-builder-api';

@Component({
  selector: 'app-login',
  standalone: true,
  imports: [CommonModule, FormsModule, RouterLink],
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.css']
})
export class LoginComponent {
  public email = '';
  public password = '';
  public isLoading = false;
  public errorMessage = '';

  constructor(private authService: AuthService, private router: Router) {}

  public onSubmit(): void {
    if (!this.email || !this.password) return;
    
    this.isLoading = true;
    this.errorMessage = '';

    this.authService.login(new LoginRequest({ email: this.email, password: this.password })).subscribe({
      next: () => {
        this.router.navigate(['/dashboard']);
      },
      error: (err) => {
        this.isLoading = false;
        if (err.status === 401 || err.status === 400 || err.status === 404) {
            this.errorMessage = 'Invalid login credentials.';
        } else {
            // NSwag might wrap the error
            try {
                const parsed = JSON.parse(err.response);
                this.errorMessage = parsed.message || 'An error occurred during login.';
            } catch {
                this.errorMessage = err?.message || 'Invalid login credentials.';
            }
        }
      }
    });
  }
}
