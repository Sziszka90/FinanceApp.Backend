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
import { CurrencyEnum } from '../../models/Money/Money';
import { GetTransactionGroupDto } from 'src/models/TransactionGroupDtos/GetTransactionGroupDto';
import { TransactionTypeEnum } from 'src/models/Enums/TransactionType.enum';

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
  templateUrl: './create-transaction-modal.component.html',
  styleUrl: './create-transaction-modal.component.scss',
  standalone: true,
})
export class CreateTransactionModalComponent implements OnInit {
  transactionForm: FormGroup;
  groupOptions: GetTransactionGroupDto[] = [];
  typeOptions: {name: string, value: TransactionTypeEnum}[] = [{name: "Expense", value: TransactionTypeEnum.Expense}, {name: "Income", value: TransactionTypeEnum.Income}];
  currencyOptions = Object.keys(CurrencyEnum).filter((key) =>
    isNaN(Number(key))
  );

  constructor(
    private dialogRef: MatDialogRef<CreateTransactionModalComponent>,
    private fb: FormBuilder,
    private transactionApiService: TransactionApiService
  ) {
    this.transactionForm = this.fb.group({
      name: new FormControl('', Validators.required),
      description: new FormControl(''),
      value: new FormControl(0, [Validators.required, Validators.min(0)]),
      currency: new FormControl('', Validators.required),
      transactionDate: new FormControl(new Date()),
      transactionType: new FormControl({name: "Expense", value: TransactionTypeEnum.Expense}),
      group: new FormControl(''),
    });

    this.transactionApiService
      .getAllTransactionGroups()
      .pipe(take(1))
      .subscribe((data) => {
        this.groupOptions = data;
        this.groupOptions.push({
          id: '',
          name: 'No group',
        }); // Add default empty option
      });
  }
  ngOnInit(): void {}

  closeDialog() {
    this.dialogRef.close();
  }

  onSubmit(): void {
    if (this.transactionForm.valid) {
      this.transactionApiService
        .createTransaction({
          name: this.transactionForm.get('name')?.value,
          description: this.transactionForm.get('description')?.value,
          value: {
            amount: this.transactionForm.get('value')?.value,
            currency: this.transactionForm.get('currency')!.value,
          },
          transactionDate: this.transactionForm.get('transactionDate')?.value,
          transactionType: this.transactionForm.get('transactionType')?.value,
          transactionGroupId: this.transactionForm.get('group')?.value.id,
        })
        .pipe(take(1))
        .subscribe(() => this.dialogRef.close(this.transactionForm.value));
    }
  }
}
