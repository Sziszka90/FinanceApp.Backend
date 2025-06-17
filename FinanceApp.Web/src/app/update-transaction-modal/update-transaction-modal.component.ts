import { Component, inject, Inject, OnInit } from '@angular/core';
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
import { TransactionApiService } from '../../services/transactions.api.service';
import { take } from 'rxjs';
import { CurrencyEnum } from '../../models/Money/Money';
import { GetTransactionGroupDto } from 'src/models/TransactionGroupDtos/GetTransactionGroupDto';
import { TransactionTypeEnum } from 'src/models/Enums/TransactionType.enum';
import { enumValidator } from 'src/helpers/helpers';
import { GetTransactionDto } from 'src/models/TransactionDtos/GetTransactionDto';

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
  templateUrl: './update-transaction-modal.component.html',
  styleUrl: './update-transaction-modal.component.scss',
  standalone: true,
})
export class UpdateTransactionModalComponent implements OnInit {
  transactionForm: FormGroup;
  groupOptions: GetTransactionGroupDto[] = [];
  typeOptions: {name: string, value: TransactionTypeEnum}[] = [{name: "Expense", value: TransactionTypeEnum.Expense}, {name: "Income", value: TransactionTypeEnum.Income}];
  currencyOptions = Object.keys(CurrencyEnum).filter((key) =>
    isNaN(Number(key))
  );

  private dialogRef = inject(MatDialogRef<UpdateTransactionModalComponent>);
  private fb = inject(FormBuilder);
  private transactionApiService = inject(TransactionApiService);
  public data = inject(MAT_DIALOG_DATA);

  constructor() {
    this.transactionForm = this.fb.group({
      name: new FormControl(this.data.name, Validators.required),
      description: new FormControl(this.data.description),
      value: new FormControl(this.data.value.amount, [
        Validators.required,
        Validators.min(0),
      ]),
      currency: new FormControl(this.data.value.currency, Validators.required),
      transactionDate: new FormControl(this.data.transactionDate),
      transactionType: new FormControl(
        null,
        [Validators.required, enumValidator(TransactionTypeEnum)]
      ),
      group: new FormControl(
        this.data.transactionGroup != null
          ? this.data.transactionGroup.Name
          : ''
      ),
    });

    this.transactionForm.get('group')?.setValue(this.data.transactionGroup);
    this.transactionForm.get('currency')?.setValue(this.data.value.currency);
    this.transactionForm.get('transactionType')?.setValue(null);
    this.transactionForm.get('transactionDate')?.setValue(new Date(this.data.transactionDate));

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

      var transactionDate = undefined;

      const date: Date = this.transactionForm.get('transactionDate')?.value;
      if (date && date.getFullYear() !== 1) {
        transactionDate = date;
      }

      this.transactionApiService
        .updateTransaction(this.data.id, {
          id: this.data.id,
          name: this.transactionForm.get('name')?.value,
          description: this.transactionForm.get('description')?.value,
          value: {
            amount: this.transactionForm.get('value')?.value,
            currency: this.transactionForm.get('currency')!.value,
          },
          transactionType: this.transactionForm.get('transactionType')!.value,
          transactionDate: transactionDate,
          transactionGroupId: this.transactionForm.get('group')?.value.id,
        })
        .pipe(take(1))
        .subscribe(() => this.dialogRef.close(this.transactionForm.value));
    }
  }

  onClose(): void {
    this.dialogRef.close();
  }

  compareCategoryObjects(object1: any, object2: any) {
    return object1 && object2 && object1.id == object2.id;
  }
}
