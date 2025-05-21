import { CommonModule } from '@angular/common';
import { Component, input } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { Observable } from 'rxjs';
import { GetIncomeTransactionDto } from 'src/models/IncomeTransactionDtos/GetIncomeTransactionDto';
import { toObservable } from '@angular/core/rxjs-interop';

@Component({
  selector: 'app-transaction-column',
  imports: [MatTableModule, CommonModule],
  templateUrl: './transaction-column.component.html',
  styleUrl: './transaction-column.component.css'
})
export class TransactionColumnComponent {
  public transactions = input<Observable<GetIncomeTransactionDto[]>>();
  transactions$ = toObservable(this.transactions);

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
