import { Component, OnDestroy } from '@angular/core';
import { UserApiService } from '../../services/user.api.service';
import { CommonModule } from '@angular/common';
import { MatFormFieldModule } from '@angular/material/form-field';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CurrencyEnum } from '../../models/IncomeTransactionDtos/Money';
import { MatSelectModule } from '@angular/material/select';
import { Subscription, take } from 'rxjs';
import { Router } from '@angular/router';
import { MatInputModule } from '@angular/material/input';
import { MatButtonModule } from '@angular/material/button';
import { GetUserDto } from '../../models/RegisterDtos/GetUserDto';
import { AuthenticationService } from '../../services/authentication.service';

@Component({
  selector: 'app-profile',
  imports: [CommonModule, MatFormFieldModule, MatSelectModule, ReactiveFormsModule, MatFormFieldModule, MatInputModule, MatButtonModule],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.css'
})
export class ProfileComponent implements OnDestroy {
  updateUserForm : FormGroup =  this.fb.group({
    userName: ['', [Validators.required, Validators.minLength(2)]],
    password: ['', [Validators.required, Validators.minLength(8)]],
    currency: ['', [Validators.required]],
  });

  user!: GetUserDto;

  currencyOptions = Object.keys(CurrencyEnum).filter((key) =>
      isNaN(Number(key)));

  subscriptions: Subscription | undefined;

  constructor(private userApiService: UserApiService, private fb: FormBuilder, private router: Router, private authService: AuthenticationService) {
    const subscription = this.userApiService.getActiveUser().pipe(take(1)).subscribe((user) => {
      this.user = user;
      this.updateUserForm = this.fb.group({
        userName: [user.userName, [Validators.required, Validators.minLength(2)]],
        password: ['', [Validators.required, Validators.minLength(8)]],
        currency: [user.baseCurrency, [Validators.required]],
      });
      this.updateUserForm.get('userName')?.setValue(user.userName);
      this.updateUserForm.get('currency')?.setValue(user.baseCurrency);
    })
    this.subscriptions?.add(subscription);
  }
  ngOnDestroy(): void {
    this.subscriptions?.unsubscribe();
  }

  onSubmit() {
    if (this.updateUserForm.valid) {
      console.log('Form Data:', this.updateUserForm.value);
    } else {
      console.log('Form is invalid.');
    }
    const subscription = this.userApiService
    .updateUser({
      id: this.user?.id,
      userName: this.updateUserForm.get('userName')?.value,
      password: this.updateUserForm.get('password')?.value,
      baseCurrency: this.updateUserForm.get('currency')?.value,
    }).pipe(take(1))
    .subscribe(() => {
      this.authService.logout();
      this.router.navigate(['/login'])
    }
    );

    this.subscriptions?.add(subscription);
  }
}
