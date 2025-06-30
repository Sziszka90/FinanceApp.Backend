import { Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { LoginResponseDto } from '../models/LoginDtos/LoginResponseDto';
import { LoginRequestDto } from '../models/LoginDtos/LoginRequestDto';

@Injectable({
  providedIn: 'root'
})
export class AuthenticationApiService {


   constructor(private http: HttpClient) { }

  login(loginRequestDto: LoginRequestDto): Observable<LoginResponseDto> {
      return this.http.post<LoginResponseDto>(`api/auth/login`, loginRequestDto);
  }
}
