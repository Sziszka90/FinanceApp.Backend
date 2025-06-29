import { Component, inject, OnDestroy } from '@angular/core';
import { Subscription, take } from 'rxjs';
import { AuthenticationService } from '../../services/authentication.service';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { MatDialog } from '@angular/material/dialog';
import { ForgotPasswordRequestModalComponent } from '../forgot-password-modal/forgot-password-modal.component';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  standalone: true,
  imports: [
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    RouterLink,
  ],
})
export class LoginComponent implements OnDestroy {
  loginForm: FormGroup;
  loginValid = true;
  loginSubscription: Subscription | undefined;

  private authService = inject(AuthenticationService);
  private matDialog = inject(MatDialog);
  private router = inject(Router);
  private fb = inject(FormBuilder);

  constructor() {
    this.loginForm = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', [Validators.required]]
    });
  }

  forgotPassword(): void {
    const dialogRef = this.matDialog.open(ForgotPasswordRequestModalComponent, {
      width: '400px',
      height: 'auto',
    });
  }

  ngOnDestroy(): void {
    this.loginSubscription?.unsubscribe();
  }

  public onSubmit(): void {
    if (this.loginForm.valid) {
      this.loginValid = true;
      this.loginSubscription = this.authService
        .login({ email: this.loginForm.value.email, password: this.loginForm.value.password })
        .pipe(take(1))
        .subscribe((data) => {
          if (data.token == '') {
            this.loginValid = false;
          } else {
            this.loginValid = true;
            this.authService.saveToken(data.token);
            this.authService.userLoggedIn.next(true);
            this.router.navigate(['/']);
          }
        });
    } else {
      this.loginValid = false;
      this.loginForm.markAllAsTouched();
    }
  }
}
