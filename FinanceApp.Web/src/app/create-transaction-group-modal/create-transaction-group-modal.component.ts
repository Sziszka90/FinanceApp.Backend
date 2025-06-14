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
import { CurrencyEnum, Money } from 'src/models/Money/Money';
import { groupIconOptions } from 'src/models/Constants/group-icon-options.const';

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
  public groupIconOptions: string[] = groupIconOptions;
  public currencyOptions: string[] = Object.keys(CurrencyEnum)
  .filter(key => isNaN(Number(key))) as (keyof typeof CurrencyEnum)[];
  public selectedIcon: string = "";

  constructor(
    private dialogRef: MatDialogRef<CreateTransactionGroupModalComponent>,
    private fb: FormBuilder,
    private transactionApiService: TransactionApiService
  ) {
    this.transactionForm = this.fb.group({
      name: new FormControl('', Validators.required),
      description: new FormControl(''),
      value: new FormControl(),
      currency: new FormControl(),
      groupIcon: new FormControl('')
    });
  }

  ngOnInit(): void {}

  closeDialog() {
    this.dialogRef.close();
  }

  onSubmit(): void {
    if (this.transactionForm.valid) {

      var limit: Money | undefined = { amount: 0, currency: CurrencyEnum.EUR};

      var limitCurrency = this.transactionForm.get('currency')!.value;
      var limitValue = this.transactionForm.get('value')!.value;

      if(limitValue === null) {
        limit = undefined;
      } else {
        limit.amount = limitValue;
        limit.currency = limitCurrency;
      }

      this.transactionApiService
        .createTransactionGroup({
          name: this.transactionForm.get('name')?.value,
          description: this.transactionForm.get('description')?.value,
          limit: limit,
          groupIcon: this.transactionForm.get('groupIcon')?.value
        }).subscribe(() => {
          this.dialogRef.close(this.transactionForm.value);
      });
    }
  }
}
