import { Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { Money } from '../models/Money/Money';
import { CreateTransactionDto } from '../models/TransactionDtos/CreateTransactionDto';
import { GetTransactionDto } from 'src/models/TransactionDtos/GetTransactionDto';
import { UpdateTransactionDto } from 'src/models/TransactionDtos/UpdateTransactionDto';
import { GetTransactionGroupDto } from 'src/models/TransactionGroupDtos/GetTransactionGroupDto';

@Injectable({
  providedIn: 'root'
})
export class TransactionApiService {

  // API base URL
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  // Method to get data from the backend
  getAllTransactions(): Observable<GetTransactionDto[]> {
     return this.http.get<GetTransactionDto[]>(`${this.apiUrl}/transactions/`);
  }

  getAllTransactionsSummary(): Observable<Money> {
    return this.http.get<Money>(`${this.apiUrl}/transactions/summary`);
  }

  createTransaction(createTransactionDto:CreateTransactionDto): Observable<GetTransactionDto[]> {
    console.log(createTransactionDto);
    return this.http.post<GetTransactionDto[]>(`${this.apiUrl}/transactions/`, createTransactionDto);
  }

  updateTransaction(id: string, updateTransactionDto: UpdateTransactionDto): Observable<GetTransactionDto[]> {
    console.log(updateTransactionDto);
    return this.http.put<GetTransactionDto[]>(`${this.apiUrl}/transactions/`, updateTransactionDto);
  }

  deleteTransaction(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/transactions/${id}`);
  }

  getAllTransactionGroups(): Observable<GetTransactionGroupDto[]> {
    return this.http.get<GetTransactionGroupDto[]>(`${this.apiUrl}/transactiongroups`);
  }
}
