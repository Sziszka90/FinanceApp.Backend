import { Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Money } from '../models/Money/Money';
import { CreateTransactionDto } from '../models/TransactionDtos/CreateTransactionDto';
import { GetTransactionDto } from 'src/models/TransactionDtos/GetTransactionDto';
import { UpdateTransactionDto } from 'src/models/TransactionDtos/UpdateTransactionDto';
import { GetTransactionGroupDto } from 'src/models/TransactionGroupDtos/GetTransactionGroupDto';
import { CreateTransactionGroupDto } from 'src/models/TransactionGroupDtos/CreateTransactionGroupDto';
import { UpdateTransactionGroupDto } from 'src/models/TransactionGroupDtos/UpdateTransactionGroupDto';

@Injectable({
  providedIn: 'root'
})
export class TransactionApiService {

  // API base URL
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  // Method to get data from the backend
  getAllTransactions(): Observable<GetTransactionDto[]> {
     return this.http.get<GetTransactionDto[]>(`api/transactions/`);
  }

  getAllTransactionsSummary(): Observable<Money> {
    return this.http.get<Money>(`api/transactions/summary`);
  }

  createTransaction(createTransactionDto:CreateTransactionDto): Observable<GetTransactionDto> {
    console.log(createTransactionDto);
    return this.http.post<GetTransactionDto>(`api/transactions/`, createTransactionDto);
  }

  updateTransaction(id: string, updateTransactionDto: UpdateTransactionDto): Observable<GetTransactionDto> {
    console.log(updateTransactionDto);
    return this.http.put<GetTransactionDto>(`api/transactions/`, updateTransactionDto);
  }

  deleteTransaction(id: string): Observable<any> {
    return this.http.delete<any>(`api/transactions/${id}`);
  }

  getAllTransactionGroups(): Observable<GetTransactionGroupDto[]> {
    return this.http.get<GetTransactionGroupDto[]>(`api/transactiongroups`);
  }

  getTransactionGroup(id: string): Observable<GetTransactionGroupDto> {
    return this.http.get<GetTransactionGroupDto>(`api/transactiongroups/${id}`);
  }

  createTransactionGroup(createTransactionGroupDto:CreateTransactionGroupDto): Observable<GetTransactionGroupDto> {
    return this.http.post<GetTransactionGroupDto>(`api/transactiongroups/`, createTransactionGroupDto);
  }

  updateTransactionGroup(updateTransactionGroupDto:UpdateTransactionGroupDto): Observable<GetTransactionGroupDto> {
    return this.http.put<GetTransactionGroupDto>(`api/transactiongroups/`, updateTransactionGroupDto);
  }

  deleteTransactionGroup(id: string): Observable<void> {
    return this.http.delete<void>(`api/transactiongroups/${id}`);
  }
}
