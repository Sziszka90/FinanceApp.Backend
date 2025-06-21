import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { UserApiService } from '../../services/user.api.service';
import { CommonModule } from '@angular/common';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { CurrencyEnum } from '../../models/Money/Money';
import { Subscription, take } from 'rxjs';
import { Router } from '@angular/router';
import { GetUserDto } from '../../models/RegisterDtos/GetUserDto';
import { AuthenticationService } from '../../services/authentication.service';
import { UserFormModel } from 'src/models/Profile/UserFormModel';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-profile',
  imports: [
    CommonModule,
    ReactiveFormsModule,
    MatSelectModule
  ],
  templateUrl: './profile.component.html',
  styleUrl: './profile.component.scss'
})
export class ProfileComponent implements OnInit, OnDestroy {
  updateUserForm : FormGroup<UserFormModel>;
  user!: GetUserDto;
  currencyOptions = Object.keys(CurrencyEnum).filter((key) =>
      isNaN(Number(key)));
  subscriptions: Subscription | undefined;

  private userApiService = inject(UserApiService);
  private fb = inject(FormBuilder);
  private router = inject(Router);
  private authService = inject(AuthenticationService);


  constructor() {
    this.updateUserForm = this.fb.group<UserFormModel>({
        currency: new FormControl(CurrencyEnum.Unknown, [Validators.required]),
      });
  }

  ngOnInit(): void {
    this.userApiService.getActiveUser().pipe(take(1)).subscribe((user) => {
      this.user = user;
      this.updateUserForm.get('currency')?.setValue(user.baseCurrency);
    });
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
      baseCurrency: this.updateUserForm.get('currency')?.value ?? CurrencyEnum.Unknown,
    }).pipe(take(1))
    .subscribe(() => {
      this.router.navigate(['/'])
    }
    );

    this.subscriptions?.add(subscription);
  }
}
