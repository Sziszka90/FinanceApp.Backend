import { Component, inject, Inject, OnInit } from '@angular/core';
import {
  FormBuilder,
  FormControl,
  FormGroup,
  ReactiveFormsModule,
  Validators,
} from '@angular/forms';
import { MatSelectModule } from '@angular/material/select';
import {
  MAT_DIALOG_DATA,
  MatDialogRef,
} from '@angular/material/dialog';
import { CommonModule } from '@angular/common';
import { TransactionApiService } from '../../services/transactions.api.service';
import { CurrencyEnum, Money } from '../../models/Money/Money';
import { groupIconOptions } from 'src/models/Constants/group-icon-options.const';
import { GetTransactionGroupDto } from 'src/models/TransactionGroupDtos/GetTransactionGroupDto';

@Component({
  selector: 'app-transaction-modal',
  imports: [
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
  public groupIconOptions: string[] = groupIconOptions;
  currencyOptions = Object.keys(CurrencyEnum).filter((key) =>
    isNaN(Number(key))
  );
  public selectedIcon: string = "";

  private dialogRef = inject(MatDialogRef<UpdateTransactionGroupModalComponent>);
  private fb = inject(FormBuilder);
  private transactionApiService = inject(TransactionApiService);
  public data = inject<GetTransactionGroupDto>(MAT_DIALOG_DATA);

  constructor() {
    this.transactionForm = this.fb.group({
      name: new FormControl(this.data.name, Validators.required),
      description: new FormControl(this.data.description),
      value: new FormControl(this.data.limit?.amount),
      currency: new FormControl(this.data.limit?.currency),
      groupIcon: new FormControl(this.data.groupIcon)
    });
  }
  ngOnInit(): void {
    this.selectedIcon = this.transactionForm.get('groupIcon')?.value || '';
  }

  onClose(): void {
    this.dialogRef.close();
  }

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
        .updateTransactionGroup({
          id: this.data.id,
          name: this.transactionForm.get('name')?.value,
          description: this.transactionForm.get('description')?.value,
          limit: limit,
          groupIcon: this.transactionForm.get('groupIcon')?.value
        }).subscribe(() => {
          this.dialogRef.close(this.transactionForm.value);
      });
    }
  }

  compareCategoryObjects(object1: any, object2: any) {
    return object1 && object2 && object1.id == object2.id;
  }
}
