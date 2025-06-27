import { Component, inject } from '@angular/core';
import { FormBuilder, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { UserApiService } from 'src/services/user.api.service';

@Component({
  selector: 'app-reset-password-request-modal',
  imports: [],
  templateUrl: './reset-password-request-modal.component.html',
  styleUrl: './reset-password-request-modal.component.css'
})
export class ResetPasswordRequestModalComponent {
  private userApiService = inject(UserApiService);
  private matDialogRef = inject(MatDialogRef<ResetPasswordRequestModalComponent>);
  private fb = inject(FormBuilder);
  private form = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
  });

  onCancel(): void {
    this.matDialogRef.close();
  }

  onConfirm(): void {
    if (this.form.valid) {
      this.userApiService.forgotPassword(this.form.value.email ?? "").subscribe(() =>{
        this.matDialogRef.close();
      });
    }
  }
}
