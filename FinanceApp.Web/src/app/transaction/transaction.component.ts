import { Component, inject, OnDestroy, OnInit, ViewChild, AfterViewInit, ChangeDetectorRef, Signal, signal } from '@angular/core';
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

  public summary$: Observable<Money> | undefined;
  public transactions$: Observable<GetTransactionDto[]> | undefined;
  public allTransactions = signal<GetTransactionDto[]>([]);
  public total = signal<Money>({amount: 0, currency: CurrencyEnum.EUR});

  public showSummary = signal<boolean>(false);
  public summary = signal<Money | null>(null);

  dataSource = signal<MatTableDataSource<GetTransactionDto>>(new MatTableDataSource<GetTransactionDto>([]));

  typeOptions: {name: string, value: TransactionTypeEnum}[] = [{name: "Expense", value: TransactionTypeEnum.Expense}, {name: "Income", value: TransactionTypeEnum.Income}];

  filterForm: FormGroup = this.fb.group({
    name: [''],
    date: [''],
    type: []
  });

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

  @ViewChild(MatSort) sort!: MatSort;

  ngOnInit(): void {
    this.dataSource.update(ds => {
      ds.sortingDataAccessor = (item, property) => {
        switch (property) {
          case 'value': return item.value.amount;
          case 'currency': return item.value.currency;
          case 'transactionDate': return new Date(item.transactionDate);
          case 'group': return item.transactionGroup?.name ?? '';
          default: return (item as any)[property];
        }
      };
      return ds;
    });

    this.transactions$ = this.transactionApiService.getAllTransactions();
    this.transactionApiService.getAllTransactions().pipe(takeUntil(this.destroy$)).subscribe((value) => {
      this.allTransactions.set(value);
      this.dataSource.update(ds => {
        ds.data = value;
        return ds;
      });
      this.setupCustomFilterPredicate();
    });

    this.filterForm.valueChanges
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => this.applyFilters());
  }

  ngAfterViewChecked() {
    if (this.sort && this.dataSource().sort !== this.sort) {
      this.dataSource.update(ds => {
        ds.sort = this.sort;
        return ds;
      });
    }
  }

  setupCustomFilterPredicate() {
    this.dataSource.update(ds => {
      ds.filterPredicate = (data: GetTransactionDto, filter: string) => {
        const filterObj = JSON.parse(filter);
        const { name, date, type } = filterObj;

        return (!name || data.name.toLowerCase().includes(name.toLowerCase())) &&
              (!date || (
                data.transactionDate &&
                new Date(data.transactionDate).toISOString().slice(0, 10) === date
              )) &&
              (!type || data.transactionType === type);
      };
      return ds; // important: return the same object
    });
  }

  applyFilters() {
    const { name, date, type } = this.filterForm.value;
    const formattedDate = date ? formatDate(date, 'yyyy-MM-dd', 'en-US') : '';

    this.dataSource.update(ds => {
      ds.filter = JSON.stringify({ name, date: formattedDate, type });
      return ds;
    });

    if (this.sort.active && this.sort.direction !== '') {
      this.dataSource().data = [...this.dataSource().filteredData];
    }
  }

  deleteTransaction(transactionDto: GetTransactionDto) {
    this.transactionApiService.deleteTransaction(transactionDto.id).subscribe(() => {
      this.allTransactions.update(transactions => transactions.filter((t) => t.id !== transactionDto.id));
      this.dataSource.update(ds => {
        ds.data = this.allTransactions();
        return ds;
      });
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
        this.allTransactions?.update(transactions => transactions.map((transaction: GetTransactionDto) => {
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
        }));
      }
      this.dataSource.update(ds => {
        ds.data = this.allTransactions();
        return ds;
      });
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
          this.allTransactions.update(transactions => [...transactions, createdTransaction]);
          this.dataSource.update(ds => {
            ds.data = this.allTransactions();
            return ds;
          });
        }
      });
  };

  resetFilters() {
    this.filterForm.reset();
    this.dataSource.update(ds => {
      ds.filter = "";
      ds.data = this.allTransactions();
      return ds;
    });
  }

  getSummary(): void {
    this.transactionApiService.getAllTransactionsSummary().subscribe((data) => {
      this.summary.set(data);
      this.showSummary.set(true);

      // Hide the summary after 5 seconds
      setTimeout(() => {
        this.showSummary.set(false);
      }, 5000);
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
