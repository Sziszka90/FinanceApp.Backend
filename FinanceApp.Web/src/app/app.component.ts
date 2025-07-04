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
  isChatWindowOpen: boolean = false;

  private platformId = inject<object>(PLATFORM_ID);

  constructor() {
    this.isServer = isPlatformServer(this.platformId);
    }

  toggleChatWindow(): void {
    this.isChatWindowOpen = !this.isChatWindowOpen;
  }

  sendMessage(): void {
    console.log('Message sent!');
  }

  isDragging: boolean = false;
  dragStartX: number = 0;
  dragStartY: number = 0;

  startDrag(event: MouseEvent): void {
      const chatBubble = event.target as HTMLElement;

      if (!chatBubble.style.left || !chatBubble.style.top) {
          const rect = chatBubble.getBoundingClientRect();
          chatBubble.style.left = `${rect.left}px`;
          chatBubble.style.top = `${rect.top}px`;
      }

      this.isDragging = true;
      this.dragStartX = event.clientX;
      this.dragStartY = event.clientY;
  }

  stopDrag(): void {
      this.isDragging = false;
  }

  drag(event: MouseEvent): void {
    if (this.isDragging && event.buttons === 1) {
        const chatBubble = event.target as HTMLElement;
        const deltaX = event.clientX - this.dragStartX;
        const deltaY = event.clientY - this.dragStartY;

        const currentLeft = parseInt(chatBubble.style.left || '20', 10);
        const currentTop = parseInt(chatBubble.style.top || '20', 10);

        const newLeft = Math.max(0, Math.min(window.innerWidth - chatBubble.offsetWidth, currentLeft + deltaX));
        const newTop = Math.max(0, Math.min(window.innerHeight - chatBubble.offsetHeight, currentTop + deltaY));

        chatBubble.style.left = `${newLeft}px`;
        chatBubble.style.top = `${newTop}px`;

        this.dragStartX = event.clientX;
        this.dragStartY = event.clientY;
    }
  }

  startTouch(event: TouchEvent): void {
    const chatBubble = event.target as HTMLElement;

    if (!chatBubble.style.left || !chatBubble.style.top) {
        const rect = chatBubble.getBoundingClientRect();
        chatBubble.style.left = `${rect.left}px`;
        chatBubble.style.top = `${rect.top}px`;
    }

    this.isDragging = true;
    this.dragStartX = event.touches[0].clientX;
    this.dragStartY = event.touches[0].clientY;
  }

  moveTouch(event: TouchEvent): void {
    if (this.isDragging && event.touches.length === 1) {
        const chatBubble = event.target as HTMLElement;
        const deltaX = event.touches[0].clientX - this.dragStartX;
        const deltaY = event.touches[0].clientY - this.dragStartY;

        const currentLeft = parseInt(chatBubble.style.left || '20', 10);
        const currentTop = parseInt(chatBubble.style.top || '20', 10);

        const newLeft = Math.max(0, Math.min(window.innerWidth - chatBubble.offsetWidth, currentLeft + deltaX));
        const newTop = Math.max(0, Math.min(window.innerHeight - chatBubble.offsetHeight, currentTop + deltaY));

        chatBubble.style.left = `${newLeft}px`;
        chatBubble.style.top = `${newTop}px`;

        this.dragStartX = event.touches[0].clientX;
        this.dragStartY = event.touches[0].clientY;
    }
  }

  endTouch(): void {
      this.isDragging = false;
  }

  @HostListener('document:click', ['$event'])
  closeChatOnOutsideClick(event: MouseEvent): void {
    const target = event.target as HTMLElement;
    if (!target.closest('.chat-bubble') && !target.closest('.chat-window')) {
        this.isChatWindowOpen = false;
    }
  }
}
