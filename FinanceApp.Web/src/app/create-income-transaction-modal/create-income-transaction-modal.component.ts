import { Component, Inject, OnInit } from '@angular/core';
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
  MAT_DIALOG_DATA,
  MatDialog,
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
import { IncomeTransactionApiService } from '../../services/inccome-transactions.api.service';
import { GetIncomeTransactionGroupDto } from '../../models/IncomeTransactionDtos/GetIncomeTransactionGroupDto';
import { take } from 'rxjs';
import { CurrencyEnum } from '../../models/IncomeTransactionDtos/Money';

@Component({
  selector: 'app-income-transaction-modal',
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
  templateUrl: './create-income-transaction-modal.component.html',
  styleUrl: './create-income-transaction-modal.component.scss',
  standalone: true,
})
export class CreateIncomeTransactionModalComponent implements OnInit {
  transactionForm: FormGroup;
  groupOptions: GetIncomeTransactionGroupDto[] = [];
  currencyOptions = Object.keys(CurrencyEnum).filter((key) =>
    isNaN(Number(key))
  );

  constructor(
    private dialogRef: MatDialogRef<CreateIncomeTransactionModalComponent>,
    private fb: FormBuilder,
    private apiService: IncomeTransactionApiService
  ) {
    this.transactionForm = this.fb.group({
      name: new FormControl('', Validators.required),
      description: new FormControl(''),
      value: new FormControl(0, [Validators.required, Validators.min(0)]),
      currency: new FormControl('', Validators.required),
      dueDate: new FormControl(new Date()),
      group: new FormControl(''),
    });

    this.apiService
      .getAllIncomeTransactionGroups()
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
      this.apiService
        .createIncomeTransaction({
          name: this.transactionForm.get('name')?.value,
          description: this.transactionForm.get('description')?.value,
          value: {
            amount: this.transactionForm.get('value')?.value,
            currency: this.transactionForm.get('currency')!.value,
          },
          dueDate: this.transactionForm.get('dueDate')?.value,
          transactionGroupId: this.transactionForm.get('group')?.value.id,
        })
        .pipe(take(1))
        .subscribe(() => this.dialogRef.close(this.transactionForm.value));
    }
  }
}
