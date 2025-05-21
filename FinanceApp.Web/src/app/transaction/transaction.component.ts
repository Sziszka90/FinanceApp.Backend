import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { UpdateIncomeTransactionModalComponent } from '../update-income-transaction-modal/update-income-transaction-modal.component';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { GetIncomeTransactionDto } from '../../models/IncomeTransactionDtos/GetIncomeTransactionDto';
import { Observable, Subscription } from 'rxjs';
import { CreateIncomeTransactionModalComponent } from '../create-income-transaction-modal/create-income-transaction-modal.component';
import { Money } from '../../models/Money/Money';
import { CommonModule } from '@angular/common';
import { TransactionApiService } from 'src/services/transactions.api.service';
import { GetExpenseTransactionDto } from 'src/models/ExpenseTransactionDtos/GetExpenseTransactionDto';
import { TransactionColumnComponent } from '../transaction-column/transaction-column.component';

@Component({
  selector: 'app-transaction',
  imports: [MatIconModule, MatButtonModule, CommonModule, TransactionColumnComponent],
  templateUrl: './transaction.component.html',
  styleUrl: './transaction.component.css',
  standalone: true,
})
export class TransactionComponent implements OnDestroy, OnInit {
  private subscriptions = new Subscription();
  allIncomes$!: Observable<GetIncomeTransactionDto[]>;
  allIncomesSummary$!: Observable<Money>;
  allExpenses$!: Observable<GetExpenseTransactionDto[]>;
  allExpensesSummary$!: Observable<Money>;

  constructor(
    private matDialog: MatDialog,
    private transactionApiService: TransactionApiService
  ) {}

  ngOnInit(): void {
    this.allIncomes$ = this.transactionApiService.getAllIncomeTransactions();
    this.allIncomesSummary$ = this.transactionApiService.getAllIncomeTransactionsSummary();

    this.allExpenses$ = this.transactionApiService.getAllExpenseTransactions();
    this.allExpensesSummary$ = this.transactionApiService.getAllExpenseTransactionsSummary();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  deleteTransaction(row: any) {
    this.transactionApiService.deleteIncomeTransaction(row.id).subscribe(() => {
      this.allIncomes$ = this.transactionApiService.getAllIncomeTransactions();
      this.allIncomesSummary$ =
        this.transactionApiService.getAllIncomeTransactionsSummary();
    });
  }

  editTransaction(row: any) {
    const dialogRef = this.matDialog.open(
      UpdateIncomeTransactionModalComponent,
      {
        width: '50rem',
        data: row,
      }
    );

    this.subscriptions.add(
      dialogRef.afterClosed().subscribe(() => {
        this.allIncomes$ = this.transactionApiService.getAllIncomeTransactions();
        this.allIncomesSummary$ =
          this.transactionApiService.getAllIncomeTransactionsSummary();
      })
    );
  }

  createTransaction() {
    const dialogRef = this.matDialog.open(
      CreateIncomeTransactionModalComponent,
      {
        width: '50rem',
      }
    );

    this.subscriptions.add(
      dialogRef.afterClosed().subscribe(() => {
        this.allIncomes$ = this.transactionApiService.getAllIncomeTransactions();
        this.allIncomesSummary$ =
          this.transactionApiService.getAllIncomeTransactionsSummary();
      })
    );
  }
}
