import { Component, OnDestroy } from '@angular/core';
import { Subscription, take } from 'rxjs';
import { AuthenticationService } from '../../services/authentication.service';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule, FormBuilder, FormGroup, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';

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

  constructor(
    private authService: AuthenticationService,
    private router: Router,
    private fb: FormBuilder
  ) {
    this.loginForm = this.fb.group({
      username: ['', [Validators.required]],
      password: ['', [Validators.required]]
    });
  }

  ngOnDestroy(): void {
    this.loginSubscription?.unsubscribe();
  }

  public onSubmit(): void {
    if (this.loginForm.valid) {
      this.loginValid = true;
      this.loginSubscription = this.authService
        .login({ userName: this.loginForm.value.username, password: this.loginForm.value.password })
        .pipe(take(1))
        .subscribe((data) => {
          if (data.token == '') {
            this.loginValid = false;
          } else {
            this.loginValid = true;
          }
          this.authService.saveToken(data.token);
          this.router.navigate(['/']);
        });
    } else {
      this.loginValid = false;
      this.loginForm.markAllAsTouched();
    }
  }
}
