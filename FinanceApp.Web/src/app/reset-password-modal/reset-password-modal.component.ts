import { Component, inject } from '@angular/core';
import { AbstractControl, FormBuilder, ValidationErrors, Validators } from '@angular/forms';
import { MatDialogRef } from '@angular/material/dialog';
import { ActivatedRoute } from '@angular/router';
import { UserApiService } from 'src/services/user.api.service';

@Component({
  selector: 'app-reset-password-modal',
  imports: [],
  templateUrl: './reset-password-modal.component.html',
  styleUrl: './reset-password-modal.component.css'
})
export class ResetPasswordModalComponent {
  private dialogRef = inject(MatDialogRef<ResetPasswordModalComponent>);
  private userApiService = inject(UserApiService);
  private route = inject(ActivatedRoute);
  private fb = inject(FormBuilder);

  private token: string = "";

  private resetPasswordForm = this.fb.group({
    password: [
      '',
      [
        Validators.required,
        Validators.pattern('^(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{8,}$'),
        Validators.minLength(8),
      ],
    ],
    confirmPassword: [
      '',
      [
        Validators.required,
        Validators.pattern('^(?=.*[A-Z])(?=.*\\d)(?=.*[^A-Za-z0-9]).{8,}$'),
        Validators.minLength(8),
      ],
    ]
  },
  { validators: this.passwordsMatchValidator }
  );

  passwordsMatchValidator(control: AbstractControl): ValidationErrors | null {
    const password = control.get('password')?.value;
    const confirmPassword = control.get('confirmPassword')?.value;

    return password === confirmPassword ? null : { passwordsMismatch: true };
  }

  ngOnInit(): void {
    this.route.queryParamMap.subscribe((params) => {
      this.token = params.get('token') ?? "";
    });
  }

  onClose() {
    this.dialogRef.close();
  }

  onSubmit() {
    if (this.resetPasswordForm.valid) {
      const password = this.resetPasswordForm.get('password')?.value ?? "";
      this.userApiService.updatePassword({password: password, token: this.token}).subscribe(() => {
        this.dialogRef.close();
      });
    }
  }
}
