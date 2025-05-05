import { Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LoginResponseDto } from '../models/LoginDtos/LoginResponseDto';
import { LoginRequestDto } from '../models/LoginDtos/LoginRequestDto';
import { CreateUserDto } from '../models/RegisterDtos/CreateUserDto';
import { GetUserDto } from '../models/RegisterDtos/GetUserDto';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationApiService {

   // API base URL
   private apiUrl = environment.apiUrl;

   constructor(private http: HttpClient) { }
 
  login(loginRequestDto: LoginRequestDto): Observable<LoginResponseDto> {
      return this.http.post<LoginResponseDto>(`${this.apiUrl}/auth/login`, loginRequestDto);
  }
}
