import { Component } from '@angular/core';
import { MatToolbarModule } from '@angular/material/toolbar';
import { MatButtonModule } from '@angular/material/button';
import { MatIconModule } from '@angular/material/icon';  // Optional, for icons
import { TranslateModule } from '@ngx-translate/core';
import { MatMenuModule } from '@angular/material/menu';  // Import MatMenuModule
import { Router, RouterLink } from '@angular/router';
import { AuthenticationService } from '../../services/authentication.service';

@Component({
    selector: 'app-nav-bar',
    imports: [
        MatToolbarModule,
        MatButtonModule,
        MatIconModule,
        TranslateModule,
        MatMenuModule,
        RouterLink
    ],
    templateUrl: './nav-bar.component.html',
    styleUrl: './nav-bar.component.scss',
    standalone: true
})
export class NavBarComponent {
  constructor(private authService: AuthenticationService, private router: Router){}

  login(){
    if(this.authService.isAuthenticated()){
      this.router.navigateByUrl('/logged-in');    
    } else {
      this.router.navigateByUrl('/login');    
    }
  }
}
