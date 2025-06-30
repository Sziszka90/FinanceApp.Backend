import { Component, inject, OnDestroy, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { Observable, of, Subject, takeUntil } from 'rxjs';
import { TransactionApiService } from 'src/services/transactions.api.service';
import { CurrencyEnum, Money } from 'src/models/Money/Money';
import { MatDialog } from '@angular/material/dialog';
import { CreateTransactionModalComponent } from '../create-transaction-modal/create-transaction-modal.component';
import { GetTransactionDto } from 'src/models/TransactionDtos/GetTransactionDto';
import { UpdateTransactionModalComponent } from '../update-transaction-modal/update-transaction-modal.component';
import { MatTableDataSource, MatTableModule } from '@angular/material/table';
import { FormBuilder, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TransactionTypeEnum } from 'src/models/Enums/TransactionType.enum';
import { MatSelectModule } from '@angular/material/select';
import { formatDate } from '@angular/common';
import { MatSort, MatSortModule } from '@angular/material/sort';
import { BsDatepickerModule } from 'ngx-bootstrap/datepicker';

@Component({
  selector: 'app-transaction',
  imports: [
    MatIconModule,
    MatSortModule,
    MatButtonModule,
    CommonModule,
    MatTableModule,
    MatSelectModule,
    ReactiveFormsModule,
    BsDatepickerModule,
  ],
  templateUrl: './transaction.component.html',
  styleUrl: './transaction.component.scss',
  standalone: true,
})

export class TransactionComponent implements OnInit, OnDestroy {
  public transactionApiService = inject(TransactionApiService);
  public matDialog = inject(MatDialog);
  public fb = inject(FormBuilder);
  private cdr = inject(ChangeDetectorRef);

  public summary$: Observable<Money> | undefined;
  public transactions$: Observable<GetTransactionDto[]> | undefined;
  public allTransactions: GetTransactionDto[] = [];
  public total: Money = {amount: 0, currency: CurrencyEnum.EUR};

  public showSummary = false;
  public summary: Money | null = null;

  typeOptions: {name: string, value: TransactionTypeEnum}[] = [{name: "Expense", value: TransactionTypeEnum.Expense}, {name: "Income", value: TransactionTypeEnum.Income}];

  filterForm: FormGroup;

  displayedColumns: string[] = [
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

  constructor() {
    this.filterForm = this.fb.group({
      name: [''],
      date: [''],
      type: []
    });
  }

  ngOnInit(): void {
    this.dataSource.sortingDataAccessor = (item, property) => {
      switch (property) {
        case 'value': return item.value.amount;
        case 'currency': return item.value.currency;
        case 'transactionDate': return new Date(item.transactionDate);
        case 'group': return item.transactionGroup?.name ?? '';
        default: return (item as any)[property];
      }
    };

    this.transactions$ = this.transactionApiService.getAllTransactions();
    this.transactionApiService.getAllTransactions().pipe(takeUntil(this.destroy$)).subscribe((value) => {
      this.allTransactions = value;
      this.dataSource.data = value;
      this.setupCustomFilterPredicate();
    });

    this.filterForm.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.applyFilters());
  }

  setupCustomFilterPredicate() {
    this.dataSource.filterPredicate = (data: GetTransactionDto, filter: string) => {
      const filterObj = JSON.parse(filter);
      const { name, date, type } = filterObj;

      return (!name || data.name.toLowerCase().includes(name.toLowerCase())) &&
             (!date || (
               data.transactionDate &&
               new Date(data.transactionDate).toISOString().slice(0, 10) === date
             )) &&
             (!type || data.transactionType === type);
    };
  }

  dataSource = new MatTableDataSource<GetTransactionDto>([]);

  @ViewChild(MatSort) sort!: MatSort;

  ngAfterViewChecked() {
    if (this.sort && this.dataSource.sort !== this.sort) {
      this.dataSource.sort = this.sort;
    }
  }

  applyFilters() {
    const { name, date, type } = this.filterForm.value;
    const formattedDate = date ? formatDate(date, 'yyyy-MM-dd', 'en-US') : '';

    this.dataSource.filter = JSON.stringify({ name, date: formattedDate, type });

    if (this.sort.active && this.sort.direction !== '') {
      this.dataSource.data = [...this.dataSource.filteredData];
    }
  }

  deleteTransaction(transactionDto: GetTransactionDto) {
    this.transactionApiService.deleteTransaction(transactionDto.id).subscribe(() => {
      this.allTransactions = this.allTransactions?.filter((t) => t.id !== transactionDto.id);
      this.dataSource.data = this.allTransactions
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
      this.dataSource.data = this.allTransactions;
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
          this.dataSource.data = this.allTransactions;
        }
      });
  };

  resetFilters() {
    this.filterForm.reset();
    this.dataSource.filter = "";
  }

  getSummary(): void {
    this.transactionApiService.getAllTransactionsSummary().subscribe((data) => {
      this.summary = data;
      this.showSummary = true;

      // Hide the summary after 5 seconds
      setTimeout(() => {
        this.showSummary = false;
      }, 5000);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
