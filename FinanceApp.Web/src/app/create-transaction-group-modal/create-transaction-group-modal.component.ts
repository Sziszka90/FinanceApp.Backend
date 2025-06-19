import { Component, inject, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import {
  MatDialogRef,
} from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { TransactionApiService } from '../../services/transactions.api.service';
import { CurrencyEnum, Money } from 'src/models/Money/Money';
import { groupIconOptions } from 'src/models/Constants/group-icon-options.const';
import { MatSelectModule } from '@angular/material/select';
import { MatIconModule } from '@angular/material/icon';

@Component({
  selector: 'app-transaction-modal',
  imports: [
    ReactiveFormsModule,
    MatSelectModule,
    MatIconModule,
    CommonModule,
  ],
  templateUrl: './create-transaction-group-modal.component.html',
  styleUrl: './create-transaction-group-modal.component.scss',
  standalone: true,
})
export class CreateTransactionGroupModalComponent implements OnInit {
  private dialogRef = inject(MatDialogRef<CreateTransactionGroupModalComponent>);
  private fb = inject(FormBuilder);
  private transactionApiService = inject(TransactionApiService);

  public transactionForm: FormGroup;
  public groupIconOptions: string[] = groupIconOptions;
  public currencyOptions: string[] = Object.keys(CurrencyEnum)
  .filter(key => isNaN(Number(key))) as (keyof typeof CurrencyEnum)[];
  public selectedIcon: string = "";

  constructor()
  {
    this.transactionForm = this.fb.group({
      name: new FormControl('', Validators.required),
      description: new FormControl(''),
      value: new FormControl(),
      currency: new FormControl(),
      groupIcon: new FormControl('')
    });
  }

  ngOnInit(): void {}

  onClose(): void {
    this.dialogRef.close(false);
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

      var createdTransactionGroup = {
          name: this.transactionForm.get('name')?.value,
          description: this.transactionForm.get('description')?.value,
          limit: limit,
          groupIcon: this.transactionForm.get('groupIcon')?.value
        };

      this.transactionApiService
        .createTransactionGroup(createdTransactionGroup).subscribe((createdTransactionGroup) => {
          this.dialogRef.close(createdTransactionGroup);
      });
    }
  }
}
