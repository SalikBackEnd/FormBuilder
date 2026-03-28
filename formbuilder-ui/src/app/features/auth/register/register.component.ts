import { Component } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { AuthService } from '../../../core/services/auth.service';
import { RegisterRequest } from '../../../core/api/form-builder-api';

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
  
  public isLoading = false;
  public errorMessage = '';

  constructor(private authService: AuthService, private router: Router) {}

  public onSubmit(): void {
    if (!this.email || !this.password || !this.firstName || !this.lastName) return;
    
    if (this.password !== this.confirmPassword) {
      this.errorMessage = 'Passwords do not match.';
      return;
    }

    this.isLoading = true;
    this.errorMessage = '';

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
        this.isLoading = false;
        try {
            const parsed = JSON.parse(err.response);
            this.errorMessage = parsed.message || 'An error occurred during registration.';
            if (parsed.errors) {
              this.errorMessage += ' ' + Object.values(parsed.errors).join(', ');
            }
        } catch {
            this.errorMessage = err?.message || 'Registration failed.';
        }
      }
    });
  }
}
