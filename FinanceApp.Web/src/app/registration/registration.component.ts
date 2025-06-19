import { CommonModule } from '@angular/common';
import { Component, inject, OnDestroy } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { Subscription, take } from 'rxjs';
import { Router } from '@angular/router';
import { CurrencyEnum } from '../../models/Money/Money';
import { UserApiService } from '../../services/user.api.service';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-registration',
  standalone: true,
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSelectModule
  ],
  templateUrl: './registration.component.html',
  styleUrls: ['./registration.component.scss'],
})
export class RegistrationComponent implements OnDestroy {
  private fb = inject(FormBuilder);
  private apiService = inject(UserApiService)
  private router = inject(Router);

  registrationForm: FormGroup;
  registrationSubscription: Subscription | undefined;
  currencyOptions = Object.keys(CurrencyEnum).filter((key) =>
    isNaN(Number(key))
  );

  constructor() {
    this.registrationForm = this.fb.group({
      userName: ['', [Validators.required, Validators.minLength(2)]],
      password: [
        '',
        [
          Validators.required,
          Validators.pattern('^(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{8,}$'),
          Validators.minLength(8),
        ],
      ],
      currency: ['', [Validators.required]],
    });
  }

  ngOnDestroy(): void {
    this.registrationSubscription?.unsubscribe();
  }

  onSubmit() {
    if (this.registrationForm.valid) {
      this.registrationSubscription = this.apiService
        .register({
          userName: this.registrationForm.get('userName')?.value,
          password: this.registrationForm.get('password')?.value,
          baseCurrency: this.registrationForm.get('currency')?.value,
        })
        .pipe(take(1))
        .subscribe(() => this.router.navigate(['/login']));
    } else {
      this.registrationForm.markAllAsTouched();
    }
  }
}
