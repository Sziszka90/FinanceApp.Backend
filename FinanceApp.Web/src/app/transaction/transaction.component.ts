import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { combineLatest, Observable, Subject, takeUntil } from 'rxjs';
import { TransactionApiService } from 'src/services/transactions.api.service';
import { CurrencyEnum, Money } from 'src/models/Money/Money';
import { GetIncomeTransactionDto } from 'src/models/IncomeTransactionDtos/GetIncomeTransactionDto';
import { GetExpenseTransactionDto } from 'src/models/ExpenseTransactionDtos/GetExpenseTransactionDto';
import { UpdateIncomeTransactionModalComponent } from '../update-income-transaction-modal/update-income-transaction-modal.component';
import { MatDialog } from '@angular/material/dialog';
import { CreateIncomeTransactionModalComponent } from '../create-income-transaction-modal/create-income-transaction-modal.component';

@Component({
  selector: 'app-transaction',
  imports: [MatIconModule, MatButtonModule, CommonModule],
  templateUrl: './transaction.component.html',
  styleUrl: './transaction.component.css',
  standalone: true,
})

export class TransactionComponent implements OnInit, OnDestroy {
  public transactionApiService = inject(TransactionApiService);
  public matDialog = inject(MatDialog);

  public incomeSummary$: Observable<Money> | undefined;
  public expenseSummary$: Observable<Money> | undefined;
  public incomeTransactions$: Observable<GetIncomeTransactionDto[]> | undefined;
  public expenseTransactions$: Observable<GetExpenseTransactionDto[]> | undefined;
  public allTransactions: GetExpenseTransactionDto[]> | undefined;
  public total: Money = {amount: 0, currency: CurrencyEnum.EUR};

  displayedColumns: string[] = [
    'name',
    'description',
    'value',
    'currency',
    'dueDate',
    'group',
    'actions',
  ];

  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.incomeSummary$ = this.transactionApiService.getAllIncomeTransactionsSummary();
    this.expenseSummary$ = this.transactionApiService.getAllExpenseTransactionsSummary();
    this.incomeTransactions$ = this.transactionApiService.getAllIncomeTransactions();
    this.expenseTransactions$ = this.transactionApiService.getAllExpenseTransactions();

    combineLatest([this.incomeSummary$, this.expenseSummary$])
    .pipe(takeUntil(this.destroy$))
    .subscribe(([incomeSum, expenseSum]) => {
      this.total = { amount: incomeSum.amount - expenseSum.amount, currency: incomeSum.currency }
    })

    combineLatest([this.incomeTransactions$, this.expenseTransactions$])
    .pipe(takeUntil(this.destroy$))
    .subscribe([incomeTransactions, expenseTransaction]) => {

    }
  }

  deleteIncomeTransaction(row: any) {
    this.transactionApiService.deleteIncomeTransaction(row.id).subscribe(() => {
      this.incomeSummary$ = this.transactionApiService.getAllIncomeTransactionsSummary();
      this.incomeTransactions$ = this.transactionApiService.getAllIncomeTransactions();
    });
  }

  deleteExpenseTransaction(row: any) {
    this.transactionApiService.deleteExpenseTransaction(row.id).subscribe(() => {
      this.expenseSummary$ = this.transactionApiService.getAllExpenseTransactionsSummary();
      this.expenseTransactions$ = this.transactionApiService.getAllExpenseTransactions();
    });
  }

  editIncomeTransaction(row: any) {
    const dialogRef = this.matDialog.open(
      UpdateIncomeTransactionModalComponent,
      {
        width: '50rem',
        data: row,
      }
    );

    dialogRef.afterClosed()
    .pipe(takeUntil(this.destroy$))
    .subscribe(() => {
      this.incomeSummary$ = this.transactionApiService.getAllIncomeTransactionsSummary();
      this.incomeTransactions$ = this.transactionApiService.getAllIncomeTransactions();
    })
  }

  createIncomeTransaction() {
    const dialogRef = this.matDialog.open(
      CreateIncomeTransactionModalComponent,
      {
        width: '50rem',
      }
    );

    dialogRef.afterClosed()
    .pipe(takeUntil(this.destroy$))
    .subscribe(() => {
      this.incomeSummary$ = this.transactionApiService.getAllIncomeTransactionsSummary();
      this.incomeTransactions$ = this.transactionApiService.getAllIncomeTransactions();
    })
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
