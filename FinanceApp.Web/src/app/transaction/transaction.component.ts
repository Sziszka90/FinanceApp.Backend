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
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TransactionTypeEnum } from 'src/models/Enums/TransactionType.enum';
import { MatSelectModule } from '@angular/material/select';

@Component({
  selector: 'app-transaction',
  imports: [
    MatIconModule,
    MatButtonModule,
    CommonModule,
    MatTableModule,
    MatSelectModule,
    ReactiveFormsModule,
  ],
  templateUrl: './transaction.component.html',
  styleUrl: './transaction.component.scss',
  standalone: true,
})

export class TransactionComponent implements OnInit, OnDestroy {
  public transactionApiService = inject(TransactionApiService);
  public matDialog = inject(MatDialog);
  public fb = inject(FormBuilder);

  public summary$: Observable<Money> | undefined;
  public transactions$: Observable<GetTransactionDto[]> | undefined;
  public allTransactions: GetTransactionDto[] = [];
  public filteredTransactions: GetTransactionDto[] = [];
  public total: Money = {amount: 0, currency: CurrencyEnum.EUR};

  typeOptions: {name: string, value: TransactionTypeEnum}[] = [{name: "Expense", value: TransactionTypeEnum.Expense}, {name: "Income", value: TransactionTypeEnum.Income}];

  showSlide1: boolean = true;
  showSlide2: boolean = false;
  touchStartX = 0;

  filterForm: FormGroup;

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

  showDateAsc = true;
  showDateDesc = true;
  showValueAsc = true;
  showValueDesc = true;

  private destroy$ = new Subject<void>();

  constructor() {
    this.filterForm = this.fb.group({
      name: [''],
      date: [''],
      type: []
    });
  }

  ngOnInit(): void {
    this.transactions$ = this.transactionApiService.getAllTransactions();
    this.transactionApiService.getAllTransactions().pipe(takeUntil(this.destroy$)).subscribe((value) => {
      this.allTransactions = value;
      this.filteredTransactions = value;
    });
    this.summary$ = this.transactionApiService.getAllTransactionsSummary();

    this.filterForm.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.applyFilters());
  }

  deleteTransaction(transactionDto: GetTransactionDto) {
    this.transactionApiService.deleteTransaction(transactionDto.id).subscribe(() => {
      this.allTransactions = this.allTransactions?.filter((t) => t.id !== transactionDto.id);
      this.filteredTransactions = this.allTransactions
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
      this.filteredTransactions = this.allTransactions;
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
          this.filteredTransactions = this.allTransactions;
        }
      });
  };

  applyFilters() {
    const { name, date, type } = this.filterForm.value;
    this.filteredTransactions = this.allTransactions.filter(t =>
      (!name || t.name.toLowerCase().includes(name.toLowerCase())) &&
      (!date || (
        t.transactionDate &&
        new Date(t.transactionDate).toISOString().slice(0, 10) === date
      )) &&
      (!type || t.transactionType === type)
    );
  }

  resetFilters() {
    this.filterForm.reset();
    this.showDateAsc = true;
    this.showDateDesc = true;
    this.showValueAsc = true;
    this.showValueDesc = true;
    this.filteredTransactions = this.allTransactions;
  }

  sortByDate(direction: 'asc' | 'desc') {
    this.showValueAsc = true;
    this.showValueDesc = true;

    if(direction === 'asc') {
      this.showDateAsc = false;
      this.showDateDesc = true;
    }

    if(direction === 'desc') {
      this.showDateAsc = true;
      this.showDateDesc = false;
    }

    this.filteredTransactions = [...this.filteredTransactions].sort((a, b) => {
      const dateA = new Date(a.transactionDate).getTime();
      const dateB = new Date(b.transactionDate).getTime();
      return direction === 'asc' ? dateA - dateB : dateB - dateA;
    });
  }

  sortByValue(direction: 'asc' | 'desc') {
    this.showDateAsc = true;
    this.showDateDesc = true;

    if(direction === 'asc') {
      this.showValueAsc = false;
      this.showValueDesc = true;
    }

    if(direction === 'desc') {
      this.showValueAsc = true;
      this.showValueDesc = false;
    }

    this.filteredTransactions = [...this.filteredTransactions].sort((a, b) => {
      const valueA = a.value.amount || 0;
      const valueB = b.value.amount || 0;
      if (valueA < valueB) return direction === 'asc' ? -1 : 1;
      if (valueA > valueB) return direction === 'asc' ? 1 : -1;
      return 0;
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
