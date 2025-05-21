import { Inject, Injectable, PLATFORM_ID } from '@angular/core';
import { LoginRequestDto } from '../models/LoginDtos/LoginRequestDto';
import { Observable } from 'rxjs';
import { LoginResponseDto } from '../models/LoginDtos/LoginResponseDto';
import { isPlatformBrowser } from '@angular/common';
import { jwtDecode } from 'jwt-decode';
import { AuthenticationApiService } from './authentication.api.service';

@Injectable({
  providedIn: 'root',
})
export class AuthenticationService {
  private readonly tokenKey: string = 'authToken'; // Define the key for local storage

  constructor(
    private authApiService: AuthenticationApiService,
    @Inject(PLATFORM_ID) private platformId: Object
  ) {}

  // Save token to localStorage
  saveToken(token: string): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.setItem(this.tokenKey, token);
    }
  }

  // Retrieve token from localStorage
  getToken(): string | null {
    if (isPlatformBrowser(this.platformId)) {
      return localStorage.getItem(this.tokenKey);
    }
    return null;
  }

  // Remove token from localStorage (e.g., on logout)
  logout(): void {
    if (isPlatformBrowser(this.platformId)) {
      localStorage.removeItem(this.tokenKey);
    }
  }

  login(loginRequestDto: LoginRequestDto): Observable<LoginResponseDto> {
    return this.authApiService.login(loginRequestDto);
  }

  // Check if the token exists in localStorage
  isAuthenticated(): boolean {
    return this.validateToken();
  }

  validateToken(): boolean {
    const token = this.getToken();
    if (!token) {
      return false; // If no token, return false
    }

    try {
      // Decode the JWT token
      const decodedToken: any = jwtDecode(token);

      // Check if the token has expired
      const currentTime = Math.floor(Date.now() / 1000); // Current time in seconds
      if (decodedToken.exp < currentTime) {
        console.log('Token has expired.');
        return false; // Token is expired
      }

      // Optionally: Validate other claims (e.g., issuer, audience)
      // if (decodedToken.iss !== 'expectedIssuer') {
      //   console.log('Invalid issuer.');
      //   return false;
      // }

      return true; // Token is valid
    } catch (error) {
      console.error('Invalid token format:', error);
      return false; // Invalid token format or decoding error
    }
  }
}
