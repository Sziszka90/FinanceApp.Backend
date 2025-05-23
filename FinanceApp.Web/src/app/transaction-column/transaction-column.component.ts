import { CommonModule } from '@angular/common';
import { Component, input } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { Observable } from 'rxjs';
import { GetIncomeTransactionDto } from 'src/models/IncomeTransactionDtos/GetIncomeTransactionDto';
import { toObservable } from '@angular/core/rxjs-interop';
import { GetExpenseTransactionDto } from 'src/models/ExpenseTransactionDtos/GetExpenseTransactionDto';

@Component({
  selector: 'app-transaction-column',
  imports: [MatTableModule, CommonModule],
  templateUrl: './transaction-column.component.html',
  styleUrl: './transaction-column.component.css'
})
export class TransactionColumnComponent {
  public incomeTransactions = input<Observable<GetIncomeTransactionDto[]>>();
  incomeTransactions$ = toObservable(this.incomeTransactions);

  displayedColumns: string[] = [
    'name',
    'description',
    'value',
    'currency',
    'dueDate',
    'group',
    'actions',
  ];
}
