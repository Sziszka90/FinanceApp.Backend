import { Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GetIncomeTransactionDto } from '../models/IncomeTransactionDtos/GetIncomeTransactionDto';
import { LoginResponseDto } from '../models/LoginDtos/LoginResponseDto';
import { LoginRequestDto } from '../models/LoginDtos/LoginRequestDto';
import { CreateUserDto } from '../models/RegisterDtos/CreateUserDto';
import { GetUserDto } from '../models/RegisterDtos/GetUserDto';
import { GetIncomeTransactionGroupDto } from '../models/IncomeTransactionDtos/GetIncomeTransactionGroupDto';
import { UpdateIncomeTransactionDto } from '../models/IncomeTransactionDtos/UpdateIncomeTransactionDto';
import { Money } from '../models/Money/Money';
import { CreateIncomeTransactionDto } from '../models/IncomeTransactionDtos/CreateIncomeTransactionDto';
import { GetExpenseTransactionDto } from 'src/models/ExpenseTransactionDtos/GetExpenseTransactionDto';
import { CreateExpenseTransactionDto } from 'src/models/ExpenseTransactionDtos/CreateExpenseTransactionDto';
import { UpdateExpenseTransactionDto } from 'src/models/ExpenseTransactionDtos/UpdateExpenseTransactionDto';
import { GetExpenseTransactionGroupDto } from 'src/models/ExpenseTransactionDtos/GetExpenseTransactionGroupDto';

@Injectable({
  providedIn: 'root'
})
export class TransactionApiService {

  // API base URL
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }
 
  // Method to get data from the backend
  getAllIncomeTransactions(): Observable<GetIncomeTransactionDto[]> {
     return this.http.get<GetIncomeTransactionDto[]>(`${this.apiUrl}/incometransactions`);
  }

  getAllExpenseTransactions(): Observable<GetExpenseTransactionDto[]> {
     return this.http.get<GetExpenseTransactionDto[]>(`${this.apiUrl}/expensetransactions`);
  }

  getAllIncomeTransactionsSummary(): Observable<Money> {
    return this.http.get<Money>(`${this.apiUrl}/incometransactions/summary`);
  }

  getAllExpenseTransactionsSummary(): Observable<Money> {
    return this.http.get<Money>(`${this.apiUrl}/expensetransactions/summary`);
  }

  createIncomeTransaction(createIncomeTransactionDto:CreateIncomeTransactionDto): Observable<GetIncomeTransactionDto[]> {
    console.log(createIncomeTransactionDto);
    return this.http.post<GetIncomeTransactionDto[]>(`${this.apiUrl}/incometransactions`, createIncomeTransactionDto);
  }

  createExpenseTransaction(createExpenseTransactionDto:CreateExpenseTransactionDto): Observable<GetExpenseTransactionDto[]> {
    console.log(createExpenseTransactionDto);
    return this.http.post<GetExpenseTransactionDto[]>(`${this.apiUrl}/expensetransactions`, createExpenseTransactionDto);
  }

  updateIncomeTransaction(id: string, updateIncomeTransactionDto: UpdateIncomeTransactionDto): Observable<GetIncomeTransactionDto[]> {
    console.log(updateIncomeTransactionDto);
    return this.http.put<GetIncomeTransactionDto[]>(`${this.apiUrl}/incometransactions`, updateIncomeTransactionDto);
  }

  updateExpenseTransaction(id: string, updateExpenseTransactionDto: UpdateExpenseTransactionDto): Observable<GetExpenseTransactionDto[]> {
    console.log(updateExpenseTransactionDto);
    return this.http.put<GetExpenseTransactionDto[]>(`${this.apiUrl}/expensetransactions`, updateExpenseTransactionDto);
  }

  deleteIncomeTransaction(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/incometransactions/${id}`);
  }

  deleteExpenseTransaction(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/expensetransactions/${id}`);
  }

  getAllIncomeTransactionGroups(): Observable<GetIncomeTransactionGroupDto[]> {
    return this.http.get<GetIncomeTransactionGroupDto[]>(`${this.apiUrl}/incometransactiongroups`);
  }

  getAllExpenseTransactionGroups(): Observable<GetExpenseTransactionGroupDto[]> {
    return this.http.get<GetExpenseTransactionGroupDto[]>(`${this.apiUrl}/expensetransactiongroups`);
  }
}
