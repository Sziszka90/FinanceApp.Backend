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

  constructor(private http: HttpClient) { }

  register(createUserDto: CreateUserDto): Observable<GetUserDto> {
    return this.http.post<GetUserDto>(`api/users`, createUserDto);
  }

  getActiveUser(): Observable<GetUserDto> {
      return this.http.get<GetUserDto>(`api/users`);
  }

  updateUser(updatedUser: UpdateUserDto): Observable<GetUserDto> {
    return this.http.put<GetUserDto>(`api/users`, updatedUser);
  }

  updatePassword(updatePasswordDto: UpdatePasswordDto): Observable<void> {
    return this.http.post<void>(`api/users/update-password`, updatePasswordDto);
  }

  forgotPassword(email: string): Observable<void> {
    return this.http.post<void>(`api/users/forgot-password`, { email });
  }
}
