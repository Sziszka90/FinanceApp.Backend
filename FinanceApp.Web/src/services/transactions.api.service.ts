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
  private apiUrl = environment?.apiUrl ?? '';

  constructor(private http: HttpClient) { }

  // Method to get data from the backend
  getAllTransactions(): Observable<GetTransactionDto[]> {
     return this.http.get<GetTransactionDto[]>(`${this.apiUrl}/api/transactions/`);
  }

  getAllTransactionsSummary(): Observable<Money> {
    return this.http.get<Money>(`${this.apiUrl}/api/transactions/summary`);
  }

  createTransaction(createTransactionDto:CreateTransactionDto): Observable<GetTransactionDto> {
    console.log(createTransactionDto);
    return this.http.post<GetTransactionDto>(`${this.apiUrl}/api/transactions/`, createTransactionDto);
  }

  updateTransaction(id: string, updateTransactionDto: UpdateTransactionDto): Observable<GetTransactionDto> {
    console.log(updateTransactionDto);
    return this.http.put<GetTransactionDto>(`${this.apiUrl}/api/transactions`, updateTransactionDto);
  }

  deleteTransaction(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/api/transactions/${id}`);
  }

  getAllTransactionGroups(): Observable<GetTransactionGroupDto[]> {
    return this.http.get<GetTransactionGroupDto[]>(`${this.apiUrl}/api/transactiongroups`);
  }

  getTransactionGroup(id: string): Observable<GetTransactionGroupDto> {
    return this.http.get<GetTransactionGroupDto>(`${this.apiUrl}/api/transactiongroups/${id}`);
  }

  createTransactionGroup(createTransactionGroupDto:CreateTransactionGroupDto): Observable<GetTransactionGroupDto> {
    return this.http.post<GetTransactionGroupDto>(`${this.apiUrl}/api/transactiongroups/`, createTransactionGroupDto);
  }

  updateTransactionGroup(updateTransactionGroupDto:UpdateTransactionGroupDto): Observable<GetTransactionGroupDto> {
    return this.http.put<GetTransactionGroupDto>(`${this.apiUrl}/api/transactiongroups/`, updateTransactionGroupDto);
  }

  deleteTransactionGroup(id: string): Observable<void> {
    return this.http.delete<void>(`${this.apiUrl}/api/transactiongroups/${id}`);
  }
}
