import { Component, inject, OnDestroy, OnInit } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { Observable, Subject, takeUntil } from 'rxjs';
import { TransactionApiService } from 'src/services/transactions.api.service';
import { CurrencyEnum, Money } from 'src/models/Money/Money';
import { MatDialog } from '@angular/material/dialog';
import { CreateTransactionModalComponent } from '../create-transaction-modal/create-transaction-modal.component';
import { GetTransactionDto } from 'src/models/TransactionDtos/GetTransactionDto';
import { UpdateTransactionModalComponent } from '../update-transaction-modal/update-transaction-modal.component';
import { MatTableModule } from '@angular/material/table';

@Component({
  selector: 'app-transaction',
  imports: [MatIconModule, MatButtonModule, CommonModule, MatTableModule],
  templateUrl: './transaction.component.html',
  styleUrl: './transaction.component.scss',
  standalone: true,
})

export class TransactionComponent implements OnInit, OnDestroy {
  public transactionApiService = inject(TransactionApiService);
  public matDialog = inject(MatDialog);

  public summary$: Observable<Money> | undefined;
  public transactions$: Observable<GetTransactionDto[]> | undefined;
  public allTransactions: GetTransactionDto[] = [];
  public total: Money = {amount: 0, currency: CurrencyEnum.EUR};

  showSlide1: boolean = true;
  showSlide2: boolean = false;
  touchStartX = 0;

  displayedColumnsFull: string[] = [
    'name',
    'description',
    'value',
    'currency',
    'transactionDate',
    'transactionType',
    'group',
    'actions',
  ];

  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.transactions$ = this.transactionApiService.getAllTransactions();
    this.transactionApiService.getAllTransactions().pipe(takeUntil(this.destroy$)).subscribe((value) => {
      this.allTransactions = value;
    });
    this.summary$ = this.transactionApiService.getAllTransactionsSummary();
  }

  deleteTransaction(transactionDto: GetTransactionDto) {
    this.transactionApiService.deleteTransaction(transactionDto.id).subscribe(() => {
      this.allTransactions = this.allTransactions?.filter((t) => t.id !== transactionDto.id);
    });
  }

  editTransaction(transactionDto: GetTransactionDto) {
    const dialogRef = this.matDialog.open(
      UpdateTransactionModalComponent,
      {
        width: '70vw',
        height: '90vh',
        data: transactionDto,
      }
    );

    dialogRef.afterClosed()
    .pipe(takeUntil(this.destroy$))
    .subscribe((updatedTransaction: GetTransactionDto) => {
    if (updatedTransaction) {
          this.allTransactions = this.allTransactions?.map((transaction: GetTransactionDto) => {
            if (transaction.id === updatedTransaction.id) {
              return {
                ...transaction,
                name: updatedTransaction.name,
                description: updatedTransaction.description,
                value: updatedTransaction.value,
                transactionDate: updatedTransaction.transactionDate,
                transactionType: updatedTransaction.transactionType,
                transactionGroup: updatedTransaction.transactionGroup,
              };
            }
            return transaction;
          });
        }
    });
  }

  createTransaction() {
    const dialogRef = this.matDialog.open(
      CreateTransactionModalComponent,
      {
        width: '70vw',
        height: '90vh'
      }
    )
    dialogRef.afterClosed()
      .pipe(takeUntil(this.destroy$))
      .subscribe((createdTransaction) => {
        if (createdTransaction) {
          this.allTransactions = [...this.allTransactions, createdTransaction];
        }
      });
  };

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
