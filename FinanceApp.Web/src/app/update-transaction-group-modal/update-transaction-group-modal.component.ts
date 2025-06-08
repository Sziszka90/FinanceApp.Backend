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
  templateUrl: './update-transaction-group-modal.component.html',
  styleUrl: './update-transaction-group-modal.component.scss',
  standalone: true,
})
export class UpdateTransactionGroupModalComponent implements OnInit {
  transactionForm: FormGroup;
  groupOptions: GetTransactionGroupDto[] = [];
  public groupIconOptions: string[] = groupIconOptions;
  currencyOptions = Object.keys(CurrencyEnum).filter((key) =>
    isNaN(Number(key))
  );
  public selectedIcon: string = "";

  constructor(
    private dialogRef: MatDialogRef<UpdateTransactionGroupModalComponent>,
    private fb: FormBuilder,
    @Inject(MAT_DIALOG_DATA) public data: any,
    private transactionApiService: TransactionApiService
  ) {
    this.transactionForm = this.fb.group({
      name: new FormControl(this.data.name, Validators.required),
      description: new FormControl(this.data.description),
      value: new FormControl(this.data.value.amount, [
        Validators.required,
        Validators.min(0),
      ]),
      currency: new FormControl(this.data.value.currency, Validators.required),
      dueDate: new FormControl(this.data.dueDate),
      group: new FormControl(
        this.data.transactionGroup != null
          ? this.data.transactionGroup.Name
          : ''
      ),
    });

    this.transactionForm.get('group')?.setValue(this.data.transactionGroup);
    this.transactionForm.get('currency')?.setValue(this.data.value.currency);

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
        .updateTransaction(this.data.id, {
          id: this.data.id,
          name: this.transactionForm.get('name')?.value,
          description: this.transactionForm.get('description')?.value,
          value: {
            amount: this.transactionForm.get('value')?.value,
            currency: this.transactionForm.get('currency')?.value,
          },
          transactionDate: this.transactionForm.get('transactionDate')?.value,
          transactionGroupId:
            this.transactionForm.get('group')?.value.id === ''
              ? null
              : this.transactionForm.get('group')?.value.id,
        })
        .pipe(take(1))
        .subscribe(() => this.dialogRef.close(this.transactionForm.value));
    }
  }

  compareCategoryObjects(object1: any, object2: any) {
    return object1 && object2 && object1.id == object2.id;
  }
}
