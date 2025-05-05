import { Component } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { Router, RouterLink } from '@angular/router';
import { AuthenticationService } from '../../services/authentication.service';
import { MatButtonModule } from '@angular/material/button';

@Component({
  selector: 'app-logged-in',
  templateUrl: './logged-in.component.html',
  styleUrls: ['./logged-in.component.css'],
  imports: [
    MatToolbarModule,
    MatButtonModule,
    RouterLink
  ]
})
export class LoggedInComponent {
  constructor(private router: Router, private authService: AuthenticationService) {}

  logout() {
    this.authService.logout();
    this.router.navigate(['/login']);
  }
}