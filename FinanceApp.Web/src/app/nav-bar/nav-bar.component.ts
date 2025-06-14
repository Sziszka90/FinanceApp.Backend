import { Component, ElementRef, HostListener } from '@angular/core';
import { MatIconModule } from '@angular/material/icon';
import { TranslateModule } from '@ngx-translate/core';
import { Router, RouterLink } from '@angular/router';
import { AuthenticationService } from '../../services/authentication.service';
import { CommonModule } from '@angular/common';

@Component({
    selector: 'app-nav-bar',
    imports: [
      CommonModule,
      MatIconModule,
      TranslateModule,
      RouterLink
    ],
    templateUrl: './nav-bar.component.html',
    styleUrl: './nav-bar.component.scss',
    standalone: true
})
export class NavBarComponent {
  showMenu = false;
  constructor(
    private authService: AuthenticationService,
    private router: Router,
    private elementRef: ElementRef){}
    
  login(){
    if(this.authService.isAuthenticated()){
      this.router.navigateByUrl('/logged-in');
    } else {
      this.router.navigateByUrl('/login');
    }
  }

  menuToggle() {
    this.showMenu = !this.showMenu;
  }

  @HostListener('document:click', ['$event'])
  onDocumentClick(event: MouseEvent) {
    // Close menu if click is outside the dropdown
    if (
      this.showMenu &&
      !this.elementRef.nativeElement.querySelector('.custom-dropdown-minimized')?.contains(event.target as Node)
    ) {
      this.showMenu = false;
    }
  }
}
