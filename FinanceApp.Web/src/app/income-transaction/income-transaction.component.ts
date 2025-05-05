import { Component, OnDestroy, OnInit } from '@angular/core';
import { MatTableModule } from '@angular/material/table';
import { MatDialog } from '@angular/material/dialog';
import { UpdateIncomeTransactionModalComponent } from '../update-income-transaction-modal/update-income-transaction-modal.component';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { IncomeTransactionApiService } from '../../services/inccome-transactions.api.service';
import { GetIncomeTransactionDto } from '../../models/IncomeTransactionDtos/GetIncomeTransactionDto';
import { Observable, Subscription } from 'rxjs';
import { CreateIncomeTransactionModalComponent } from '../create-income-transaction-modal/create-income-transaction-modal.component';
import { Money } from '../../models/IncomeTransactionDtos/Money';
import { CommonModule } from '@angular/common';

@Component({
  selector: 'app-income-transaction',
  imports: [MatTableModule, MatIconModule, MatButtonModule, CommonModule],
  templateUrl: './income-transaction.component.html',
  styleUrl: './income-transaction.component.scss',
  standalone: true,
})
export class IncomeTransactionComponent implements OnDestroy, OnInit {
  private subscriptions = new Subscription();
  allIncomes$!: Observable<GetIncomeTransactionDto[]>;
  allIncomesSummary$!: Observable<Money>;
  displayedColumns: string[] = [
    'name',
    'description',
    'value',
    'currency',
    'dueDate',
    'group',
    'actions',
  ];

  constructor(
    private matDialog: MatDialog,
    private apiService: IncomeTransactionApiService
  ) {}

  ngOnInit(): void {
    this.allIncomes$ = this.apiService.getAllIncomeTransactions();
    this.allIncomesSummary$ = this.apiService.getAllIncomeTransactionsSummary();
  }

  ngOnDestroy(): void {
    this.subscriptions.unsubscribe();
  }

  deleteTransaction(row: any) {
    this.apiService.deleteIncomeTransaction(row.id).subscribe(() => {
      this.allIncomes$ = this.apiService.getAllIncomeTransactions();
      this.allIncomesSummary$ =
        this.apiService.getAllIncomeTransactionsSummary();
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
        this.allIncomes$ = this.apiService.getAllIncomeTransactions();
        this.allIncomesSummary$ =
          this.apiService.getAllIncomeTransactionsSummary();
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
        this.allIncomes$ = this.apiService.getAllIncomeTransactions();
        this.allIncomesSummary$ =
          this.apiService.getAllIncomeTransactionsSummary();
      })
    );
  }
}
