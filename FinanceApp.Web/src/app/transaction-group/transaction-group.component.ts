import { Component, inject } from '@angular/core';
import { MatDialog } from '@angular/material/dialog';
import { CreateTransactionGroupModalComponent } from '../create-transaction-group-modal/create-transaction-group-modal.component';
import { MatIconModule } from '@angular/material/icon';
import { MatButtonModule } from '@angular/material/button';
import { CommonModule } from '@angular/common';
import { MatTableModule } from '@angular/material/table';
import { TransactionApiService } from 'src/services/transactions.api.service';
import { Observable } from 'rxjs';
import { GetTransactionGroupDto } from 'src/models/TransactionGroupDtos/GetTransactionGroupDto';

@Component({
  selector: 'app-transaction-group',
  imports: [MatIconModule, MatButtonModule, CommonModule, MatTableModule],
  templateUrl: './transaction-group.component.html',
  styleUrl: './transaction-group.component.css'
})
export class TransactionGroupComponent {
  public matDialog = inject(MatDialog);
  public transactionApiService = inject(TransactionApiService);

    displayedColumns: string[] = [
    'name',
    'description',
    'icon',
    'value',
    'currency',
    'actions',
  ];

  public transactionGroups$: Observable<GetTransactionGroupDto[]> | undefined;

  ngOnInit(): void {
    this.transactionGroups$ = this.transactionApiService.getAllTransactionGroups();
    this.transactionGroups$.subscribe(value => {console.log(value);});
  }

  createTransactionGroup() {
    const dialogRef = this.matDialog.open(
      CreateTransactionGroupModalComponent,
      {
        width: '50rem',
      }
    );
  }
}
