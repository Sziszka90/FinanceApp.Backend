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
  public allTransactions: GetTransactionDto[] | undefined;
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

  displayedColumnsSlide1: string[] = [
    'name',
    'description',
    'value',
    'currency',
  ];

  displayedColumnsSlide2: string[] = [
    'transactionDate',
    'transactionType',
    'group',
    'actions',
  ];

  private destroy$ = new Subject<void>();

  ngOnInit(): void {
    this.transactions$ = this.transactionApiService.getAllTransactions();
    this.summary$ = this.transactionApiService.getAllTransactionsSummary();
  }

  deleteTransaction(row: any) {
    this.transactionApiService.deleteTransaction(row.id).subscribe(() => {
      this.transactions$ = this.transactionApiService.getAllTransactions();
      this.summary$ = this.transactionApiService.getAllTransactionsSummary();
    });
  }

  editTransaction(row: any) {
    const dialogRef = this.matDialog.open(
      UpdateTransactionModalComponent,
      {
        width: '50rem',
        data: row,
      }
    );

    dialogRef.afterClosed()
    .pipe(takeUntil(this.destroy$))
    .subscribe(() => {
      this.transactions$ = this.transactionApiService.getAllTransactions();
      this.summary$ = this.transactionApiService.getAllTransactionsSummary();
    })
  }

  slideRight() {
    this.showSlide1 = false;
    this.showSlide2 = true;
  }

  slideLeft() {
    this.showSlide1 = true;
    this.showSlide2 = false;
  }

  onTouchStart(event: TouchEvent) {
    this.touchStartX = event.touches[0].clientX;
  }

  onTouchEnd(event: TouchEvent) {
    const deltaX = event.changedTouches[0].clientX - this.touchStartX;
    if (deltaX < -50) {
      this.slideRight();
    } else if (deltaX > 50) {
      this.slideLeft();
    }
  }

  createTransaction() {
    const dialogRef = this.matDialog.open(
      CreateTransactionModalComponent,
      {
        width: '50rem',
      }
    )
    dialogRef.afterClosed()
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.summary$ = this.transactionApiService.getAllTransactionsSummary();
        this.transactions$ = this.transactionApiService.getAllTransactions();
    })
  };

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
