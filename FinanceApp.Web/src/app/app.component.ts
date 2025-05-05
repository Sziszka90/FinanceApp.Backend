import { Component, Inject, PLATFORM_ID } from '@angular/core';
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
    styleUrl: './app.component.css',
    standalone: true,
})
export class AppComponent {
    title: string = "Finance App"
    isServer: boolean = false;

    constructor(@Inject(PLATFORM_ID) private platformId: Object) {
        this.isServer = isPlatformServer(this.platformId);
      }
}
