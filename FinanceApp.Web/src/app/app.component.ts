import { Component, inject, PLATFORM_ID, HostListener } from '@angular/core';
import { NavBarComponent } from './nav-bar/nav-bar.component';
import { RouterOutlet } from '@angular/router';
import { CommonModule, isPlatformServer } from '@angular/common';

@Component({
    selector: 'app-root',
    imports: [
        NavBarComponent,
        RouterOutlet,
        CommonModule
    ],
    templateUrl: './app.component.html',
    styleUrl: './app.component.scss',
    standalone: true,
})
export class AppComponent {
  title: string = "Finance App"
  isServer: boolean = false;
}
