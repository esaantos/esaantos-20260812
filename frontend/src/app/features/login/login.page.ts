import { Component, inject, signal } from '@angular/core';
import { FormBuilder, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule } from '@angular/material/input';
import { MatSnackBar } from '@angular/material/snack-bar';
import { Router } from '@angular/router';

import { AuthService } from '../../core/auth/auth.service';
import { extractApiErrorMessage } from '../../core/http/api-error';

@Component({
  selector: 'app-login-page',
  imports: [ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './login.page.html',
  styleUrl: './login.page.scss'
})
export class LoginPage {
  private readonly fb = inject(FormBuilder);
  private readonly auth = inject(AuthService);
  private readonly router = inject(Router);
  private readonly snackBar = inject(MatSnackBar);

  readonly form = this.fb.nonNullable.group({
    login: ['', Validators.required],
    senha: ['', Validators.required]
  });

  readonly submitting = signal(false);

  submit(): void {
    if (this.form.invalid) {
      this.form.markAllAsTouched();
      return;
    }

    this.submitting.set(true);

    this.auth.login(this.form.getRawValue()).subscribe({
      next: () => {
        this.submitting.set(false);
        this.router.navigate(['/usuarios']);
      },
      error: (err) => {
        this.submitting.set(false);
        this.snackBar.open(extractApiErrorMessage(err, 'Login ou senha inválidos.'), 'Fechar', { duration: 6000 });
      }
    });
  }
}
