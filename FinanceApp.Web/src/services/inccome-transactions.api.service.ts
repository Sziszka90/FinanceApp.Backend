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
import { Money } from '../models/IncomeTransactionDtos/Money';
import { CreateIncomeTransactionDto } from '../models/IncomeTransactionDtos/CreateIncomeTransactionDto';

@Injectable({
  providedIn: 'root'
})
export class IncomeTransactionApiService {

   // API base URL
   private apiUrl = environment.apiUrl;

   constructor(private http: HttpClient) { }
 
   // Method to get data from the backend
   getAllIncomeTransactions(): Observable<GetIncomeTransactionDto[]> {
     return this.http.get<GetIncomeTransactionDto[]>(`${this.apiUrl}/incometransactions`);
   }

   getAllIncomeTransactionsSummary(): Observable<Money> {
    return this.http.get<Money>(`${this.apiUrl}/incometransactions/summary`);
  }

  createIncomeTransaction(createIncomeTransactionDto:CreateIncomeTransactionDto): Observable<GetIncomeTransactionDto[]> {
    console.log(createIncomeTransactionDto);
    return this.http.post<GetIncomeTransactionDto[]>(`${this.apiUrl}/incometransactions`, createIncomeTransactionDto);
  }

   updateIncomeTransaction(id: string, updateIncomeTransactionDto: UpdateIncomeTransactionDto): Observable<GetIncomeTransactionDto[]> {
    console.log(updateIncomeTransactionDto);
    return this.http.put<GetIncomeTransactionDto[]>(`${this.apiUrl}/incometransactions`, updateIncomeTransactionDto);
  }

  deleteIncomeTransaction(id: string): Observable<any> {
    return this.http.delete<any>(`${this.apiUrl}/incometransactions/${id}`);
  }

   getAllIncomeTransactionGroups(): Observable<GetIncomeTransactionGroupDto[]> {
    return this.http.get<GetIncomeTransactionGroupDto[]>(`${this.apiUrl}/incometransactiongroups`);
  }

  login(loginRequestDto: LoginRequestDto): Observable<LoginResponseDto> {
      return this.http.post<LoginResponseDto>(`${this.apiUrl}/auth/login`, loginRequestDto);
  }

  register(createUserDto: CreateUserDto): Observable<GetUserDto> {
    return this.http.post<GetUserDto>(`${this.apiUrl}/users`, createUserDto);
  }
}
