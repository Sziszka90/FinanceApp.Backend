import { Component, inject, PLATFORM_ID, HostListener } from '@angular/core';
import { RouterOutlet } from '@angular/router';
import { CommonModule, isPlatformServer } from '@angular/common';
import { NavBarComponent } from './shared/nav-bar/nav-bar.component';

@Component({
    selector: 'root',
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
