import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { LoginRequestDto } from '../models/LoginDtos/login-request.dto';
import { Observable, Subject } from 'rxjs';
import { LoginResponseDto } from '../models/LoginDtos/login-response.dto';
import { isPlatformBrowser } from '@angular/common';
import { jwtDecode } from 'jwt-decode';
import { AuthenticationApiService } from './authentication.api.service';
import { TOKEN_KEY } from 'src/models/Constants/token.const';

@Injectable({
  providedIn: 'root',
})
export class AuthenticationService {
  private readonly tokenKey: string = TOKEN_KEY; // Define the key for local storage
  public userLoggedIn: Subject<boolean> = new Subject<boolean>();


  constructor(
    private authApiService: AuthenticationApiService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  saveToken(token: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.tokenKey, token);
    }
  }

  getToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem(this.tokenKey);
    }
    return null;
  }

  logout(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem(this.tokenKey);
    }
    this.userLoggedIn.next(false);
  }

  login(loginRequestDto: LoginRequestDto): Observable<LoginResponseDto> {
    return this.authApiService.login(loginRequestDto);
  }


  isAuthenticated(): boolean {
    return this.validateToken();
  }

  validateToken(): boolean {
    const token = this.getToken();
    if (!token) {
      return false;
    }

    try {
      const decodedToken: any = jwtDecode(token);

      const currentTime = Math.floor(Date.now() / 1000);
      if (decodedToken.exp < currentTime) {
        console.log('Token has expired.');
        return false;
      }
      return true;
    } catch (error) {
      console.error('Invalid token format:', error);
      return false;
    }
  }
}
