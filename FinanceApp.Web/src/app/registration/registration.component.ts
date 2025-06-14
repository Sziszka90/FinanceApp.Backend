import { CommonModule } from '@angular/common';
import { Component, OnDestroy } from '@angular/core';
import {
  FormBuilder,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatFormFieldModule } from '@angular/material/form-field';
import { MatInputModule, MatLabel } from '@angular/material/input';
import { MatSelectModule } from '@angular/material/select';
import { Subscription, take } from 'rxjs';
import { MatDialog } from '@angular/material/dialog';
import { Router } from '@angular/router';
import { CurrencyEnum } from '../../models/Money/Money';
import { UserApiService } from '../../services/user.api.service';

@Component({
  selector: 'app-registration',
  imports: [
    MatInputModule,
    MatButtonModule,
    MatFormFieldModule,
    MatLabel,
    ReactiveFormsModule,
    MatSelectModule,
    CommonModule,
  ],
  templateUrl: './registration.component.html',
  styleUrls: ['./registration.component.scss'],
})
export class RegistrationComponent implements OnDestroy {
  registrationForm: FormGroup;
  registrationSubscription: Subscription | undefined;
  currencyOptions = Object.keys(CurrencyEnum).filter((key) =>
    isNaN(Number(key))
  );

  constructor(
    private fb: FormBuilder,
    private apiService: UserApiService,
    private router: Router
  ) {
    this.registrationForm = this.fb.group({
      userName: ['', [Validators.required, Validators.minLength(2)]],
      password: ['', [Validators.required, Validators.pattern("^(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{8,}$"), Validators.minLength(8)]],
      currency: ['', [Validators.required]],
    });
  }
  ngOnDestroy(): void {
    this.registrationSubscription?.unsubscribe();
  }

  onSubmit() {
    if (this.registrationForm.valid) {
      console.log('Form Data:', this.registrationForm.value);
    } else {
      console.log('Form is invalid.');
    }
    this.registrationSubscription = this.apiService
      .register({
        userName: this.registrationForm.get('userName')?.value,
        password: this.registrationForm.get('password')?.value,
        baseCurrency: this.registrationForm.get('currency')?.value,
      })
      .pipe(take(1))
      .subscribe(() => this.router.navigate(['/login']));
  }
}
