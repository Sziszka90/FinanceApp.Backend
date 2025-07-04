import { Component, inject, OnDestroy, OnInit, signal } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { CreateTransactionGroupModalComponent } from '../create-transaction-group-modal/create-transaction-group-modal.component';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { TransactionApiService } from 'src/services/transactions.api.service';
import { Observable, Subject, takeUntil } from 'rxjs';
import { GetTransactionGroupDto } from 'src/models/TransactionGroupDtos/GetTransactionGroupDto';
import { UpdateTransactionGroupModalComponent } from '../update-transaction-group-modal/update-transaction-group-modal.component';
import { trigger, transition, style, animate } from '@angular/animations';

@Component({
  selector: 'app-transaction-group',
  imports: [MatIconModule, MatButtonModule, CommonModule, MatTableModule],
  templateUrl: './transaction-group.component.html',
  styleUrl: './transaction-group.component.scss',
  animations: [
    trigger('slideInOut', [
      transition(':enter', [
        style({ opacity: 0, transform: 'translateX(-40px)' }),
        animate('300ms ease', style({ opacity: 1, transform: 'translateX(0)' })),
      ]),
      transition(':leave', [
        animate('300ms ease', style({ opacity: 0, transform: 'translateX(40px)' })),
      ]),
    ]),
  ]
})

export class TransactionGroupComponent implements OnInit, OnDestroy {
  public matDialog = inject(MatDialog);
  public transactionApiService = inject(TransactionApiService);

  displayedColumnsFull: string[] = [
    'name',
    'description',
    'icon',
    'actions',
  ];

  public transactionGroups$: Observable<GetTransactionGroupDto[]> | undefined;
  public allTransactionGroups = signal<GetTransactionGroupDto[]>([]);

  private destroy$ = new Subject<void>();

  touchStartX = 0;

  ngOnInit(): void {
    this.transactionGroups$ = this.transactionApiService.getAllTransactionGroups();
    this.transactionGroups$.pipe(takeUntil(this.destroy$)).subscribe(value => this.allTransactionGroups.set(value));
  }

  createTransactionGroup() {
    const dialogRef = this.matDialog.open(
      CreateTransactionGroupModalComponent,
      {
        width: '80vw',
        height: '90vh',
      }
    );

    dialogRef.afterClosed()
    .pipe(takeUntil(this.destroy$))
    .subscribe((createdTransactionGroup) => {
      if (createdTransactionGroup) {
        this.allTransactionGroups.update(groups => [...groups, createdTransactionGroup]);
      }
    });
  }

  deleteTransactionGroup(transactionGroup: GetTransactionGroupDto) {
    this.transactionApiService.deleteTransactionGroup(transactionGroup.id)
      .pipe(takeUntil(this.destroy$))
      .subscribe(() => {
        this.allTransactionGroups.update(groups => groups.filter(
          (group) => group.id !== transactionGroup.id
        ));
      });
  }

  editTransactionGroup(transactionGroup: GetTransactionGroupDto) {
    const dialogRef = this.matDialog.open(
      UpdateTransactionGroupModalComponent,
      {
        width: '80vw',
        height: '90vh',
        data: transactionGroup,
      }
    );

    dialogRef.afterClosed()
    .pipe(takeUntil(this.destroy$))
    .subscribe((updatedTransactionGroup) => {
      if (updatedTransactionGroup) {
        this.allTransactionGroups.update(groups => groups.map((transactionGroup) => {
          if (transactionGroup.id === updatedTransactionGroup.id) {
            return {
              ...transactionGroup,
              name: updatedTransactionGroup.name,
              description: updatedTransactionGroup.description,
              groupIcon: updatedTransactionGroup.groupIcon,
            };
          }
          return transactionGroup;
        }));
      }
    });
  }

  ngOnDestroy(): void {
    this.destroy$.next();
    this.destroy$.complete();
  }
}
