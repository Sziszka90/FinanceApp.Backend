import { Component, Inject } from '@angular/core';
import { MatButtonModule } from '@angular/material/button';
import { MatDialogRef, MAT_DIALOG_DATA, MatDialogModule } from '@angular/material/dialog';

@Component({
  selector: 'app-error-modal',
  templateUrl: './error-modal.component.html',
  styleUrls: ['./error-modal.component.scss'],
  imports: [
    MatDialogModule,
    MatButtonModule
  ]
})
export class ErrorModalComponent {

  constructor(
    public dialogRef: MatDialogRef<ErrorModalComponent>,
    @Inject(MAT_DIALOG_DATA) public data: { message?: string, details:  { [key: string]: any } }
  ) {}

  onClose(): void {
    this.dialogRef.close();
  }
}
