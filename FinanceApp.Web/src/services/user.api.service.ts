import { Injectable } from '@angular/core';
import { environment } from '../environments/environment';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { GetUserDto } from '../models/UserDtos/GetUserDto';
import { UpdateUserDto } from '../models/UserDtos/UpdateUserDto';
import { CreateUserDto } from '../models/UserDtos/CreateUserDto';
import { UpdatePasswordDto } from 'src/models/UserDtos/UpdatePasswordDto';

@Injectable({
  providedIn: 'root'
})
export class UserApiService {

  // API base URL
  private apiUrl = environment.apiUrl;

  constructor(private http: HttpClient) { }

  register(createUserDto: CreateUserDto): Observable<GetUserDto> {
    return this.http.post<GetUserDto>(`${this.apiUrl}/users`, createUserDto);
  }

  getActiveUser(): Observable<GetUserDto> {
      return this.http.get<GetUserDto>(`${this.apiUrl}/users`);
  }

  updateUser(updatedUser: UpdateUserDto): Observable<GetUserDto> {
    return this.http.put<GetUserDto>(`${this.apiUrl}/users`, updatedUser);
  }

  updatePassword(updatePasswordDto: UpdatePasswordDto): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/users/update-password`, updatePasswordDto);
  }

  forgotPassword(email: string): Observable<void> {
    return this.http.post<void>(`${this.apiUrl}/users/forgot-password`, { email });
  }
}
