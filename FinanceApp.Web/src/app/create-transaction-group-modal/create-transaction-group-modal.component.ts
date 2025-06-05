import { Component, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatButtonModule } from '@angular/material/button';
import { MatSelectModule } from '@angular/material/select';
import {
  MatDialogActions,
  MatDialogClose,
  MatDialogContent,
  MatDialogRef,
  MatDialogTitle,
} from '@angular/material/dialog';
import { MatFormFieldModule, MatLabel } from '@angular/material/form-field';
import { MatDatepickerModule } from '@angular/material/datepicker';
import { MatInputModule } from '@angular/material/input';
import { CommonModule } from '@angular/common';
import { TransactionApiService } from '../../services/transactions.api.service';
import { take } from 'rxjs';
import { GetTransactionGroupDto } from 'src/models/TransactionGroupDtos/GetTransactionGroupDto';
import { CurrencyEnum } from 'src/models/Money/Money';

@Component({
  selector: 'app-transaction-modal',
  imports: [
    MatInputModule,
    MatDialogTitle,
    MatDialogContent,
    MatDialogActions,
    MatDialogClose,
    MatButtonModule,
    MatFormFieldModule,
    MatLabel,
    MatDatepickerModule,
    ReactiveFormsModule,
    MatSelectModule,
    CommonModule,
  ],
  templateUrl: './create-transaction-group-modal.component.html',
  styleUrl: './create-transaction-group-modal.component.scss',
  standalone: true,
})
export class CreateTransactionGroupModalComponent implements OnInit {
  public transactionForm: FormGroup;
  public groupOptions: GetTransactionGroupDto[] = [];
  public currencyOptions: string[] = Object.keys(CurrencyEnum)
  .filter(key => isNaN(Number(key))) as (keyof typeof CurrencyEnum)[];
  public selectedIcon: string = "";

  public groupIconOptions: string[] = [
    "fa-solid fa-utensils",
    "fa-solid fa-car",
    "fa-solid fa-house",
    "fa-solid fa-gamepad",
    "fa-solid fa-tv",
    "fa-solid fa-cart-shopping",
    "fa-solid fa-plane",
    "fa-solid fa-socks",
    "fa-solid fa-bath",
    "fa-solid fa-ellipsis",
  ];

  constructor(
    private dialogRef: MatDialogRef<CreateTransactionGroupModalComponent>,
    private fb: FormBuilder,
    private transactionApiService: TransactionApiService
  ) {
    this.transactionForm = this.fb.group({
      name: new FormControl('', Validators.required),
      description: new FormControl(''),
      value: new FormControl(0),
      currency: new FormControl(CurrencyEnum),
      groupIcon: new FormControl('')
    });

    this.transactionApiService
      .getAllTransactionGroups()
      .pipe(take(1))
      .subscribe((data) => {
        this.groupOptions = data;
        this.groupOptions.push({
          id: '',
          name: 'No group',
        });
      });
  }
  ngOnInit(): void {}

  closeDialog() {
    this.dialogRef.close();
  }

  onSubmit(): void {
    if (this.transactionForm.valid) {
      this.transactionApiService
        .createTransactionGroup({
          name: this.transactionForm.get('name')?.value,
          description: this.transactionForm.get('description')?.value,
          limit: {
            amount: this.transactionForm.get('value')?.value,
            currency: this.transactionForm.get('currency')!.value,
          },
          groupIcon: this.transactionForm.get('groupIcon')?.value
        })
        .pipe(take(1))
        .subscribe(() => this.dialogRef.close(this.transactionForm.value));
    }
  }
}
