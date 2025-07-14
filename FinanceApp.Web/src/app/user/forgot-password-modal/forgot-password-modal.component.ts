import { CommonModule } from '@angular/common';
import { Component, inject } from '@angular/core';
import { FormBuilder, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { Subject, takeUntil } from 'rxjs';
import { UserApiService } from 'src/services/user.api.service';

@Component({
  selector: 'forgot-password-modal',
  imports: [CommonModule, ReactiveFormsModule],
  templateUrl: './forgot-password-modal.component.html',
  styleUrl: './forgot-password-modal.component.scss'
})
export class ForgotPasswordRequestModalComponent {
  private userApiService = inject(UserApiService);
  private matDialogRef = inject(MatDialogRef<ForgotPasswordRequestModalComponent>);
  private fb = inject(FormBuilder);

  public emailForm: FormGroup = this.fb.group({
    email: ['', [Validators.required, Validators.email]]
  });

  private $onDestroy = new Subject<void>();

  onSubmit(): void {
    if (this.emailForm.valid) {
      this.userApiService.forgotPassword(this.emailForm.value.email ?? "")
      .pipe(takeUntil(this.$onDestroy)).subscribe(() =>{
        this.matDialogRef.close();
      });
    }
  }

  onCancel(): void {
    this.matDialogRef.close();
  }

  ngOnDestroy(): void {
    this.$onDestroy.next();
    this.$onDestroy.complete();
  }
}
