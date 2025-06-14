import { Component, OnDestroy } from '@angular/core';
import { Subscription, take } from 'rxjs';
import { AuthenticationService } from '../../services/authentication.service';
import { MatCardModule } from '@angular/material/card'; // Import MatCardModule
import {
  MatError,
  MatFormField,
  MatFormFieldModule,
} from '@angular/material/form-field';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { Router, RouterLink } from '@angular/router';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  standalone: true,
  imports: [
    MatCardModule,
    MatButtonModule,
    MatFormField,
    MatError,
    CommonModule,
    FormsModule,
    ReactiveFormsModule,
    MatFormFieldModule,
    MatInputModule,
    RouterLink,
  ],
})
export class LoginComponent implements OnDestroy {
  public loginValid = true;
  public username = '';
  public password = '';
  loginSubscription: Subscription | undefined;

  constructor(
    private authService: AuthenticationService,
    private router: Router
  ) {}

  ngOnDestroy(): void {
    this.loginSubscription?.unsubscribe();
  }

  public onSubmit(): void {
    this.loginValid = true;
    this.loginSubscription = this.authService
      .login({ userName: this.username, password: this.password })
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
  }
}
