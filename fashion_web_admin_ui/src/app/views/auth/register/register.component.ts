import { Component } from '@angular/core';
import { HttpErrorResponse } from '@angular/common/http';
import { NgIf } from '@angular/common';
import { IconDirective } from '@coreui/icons-angular';
import {
  ButtonDirective,
  FormControlDirective,
  FormDirective,
  InputGroupComponent,
  InputGroupTextDirective,
} from '@coreui/angular';
import { FormBuilder, FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import {
  AdminApiAuthenticationApiClient,
  RegisterRequestDTO,
  SwaggerException
} from '../../../api/admin-api.service.generated';
import { AlertService } from '../../../shared/services/alert.service';

@Component({
  selector: 'app-register',
  templateUrl: './register.component.html',
  styleUrls: ['./register.component.scss'],
  imports: [
    FormDirective,
    InputGroupComponent,
    InputGroupTextDirective,
    IconDirective,
    FormControlDirective,
    ButtonDirective,
    ReactiveFormsModule,
    NgIf,
    RouterLink
  ]
})
export class RegisterComponent {
  registerForm: FormGroup;
  isSubmitting = false;
  submitted = false;

  constructor(
    private fb: FormBuilder,
    private authApiClient: AdminApiAuthenticationApiClient,
    private alertService: AlertService,
    private router: Router
  ) {
    this.registerForm = this.fb.group({
      username: new FormControl('', [Validators.required, Validators.maxLength(100)]),
      email: new FormControl('', [Validators.required, Validators.email]),
      firstName: new FormControl('', [Validators.required, Validators.maxLength(100)]),
      lastName: new FormControl('', [Validators.required, Validators.maxLength(100)]),
      phoneNumber: new FormControl('', [Validators.maxLength(20)]),
      password: new FormControl('', [Validators.required, Validators.minLength(6)]),
      confirmPassword: new FormControl('', [Validators.required])
    });
  }

  get usernameControl(): FormControl {
    return this.registerForm.get('username') as FormControl;
  }

  get emailControl(): FormControl {
    return this.registerForm.get('email') as FormControl;
  }

  get firstNameControl(): FormControl {
    return this.registerForm.get('firstName') as FormControl;
  }

  get lastNameControl(): FormControl {
    return this.registerForm.get('lastName') as FormControl;
  }

  get phoneNumberControl(): FormControl {
    return this.registerForm.get('phoneNumber') as FormControl;
  }

  get passwordControl(): FormControl {
    return this.registerForm.get('password') as FormControl;
  }

  get confirmPasswordControl(): FormControl {
    return this.registerForm.get('confirmPassword') as FormControl;
  }

  get isPasswordMismatch(): boolean {
    const password = String(this.passwordControl.value ?? '');
    const confirmPassword = String(this.confirmPasswordControl.value ?? '');
    return password.length > 0 && confirmPassword.length > 0 && password !== confirmPassword;
  }

  register(): void {
    this.submitted = true;
    if (this.registerForm.invalid || this.isPasswordMismatch || this.isSubmitting) {
      this.registerForm.markAllAsTouched();
      return;
    }

    const request = new RegisterRequestDTO({
      username: String(this.usernameControl.value ?? '').trim(),
      email: String(this.emailControl.value ?? '').trim(),
      firstName: String(this.firstNameControl.value ?? '').trim(),
      lastName: String(this.lastNameControl.value ?? '').trim(),
      phoneNumber: String(this.phoneNumberControl.value ?? '').trim(),
      password: String(this.passwordControl.value ?? '')
    });

    this.isSubmitting = true;
    this.authApiClient.register(request)
      .pipe(finalize(() => {
        this.isSubmitting = false;
      }))
      .subscribe({
        next: (response) => {
          this.alertService.success(response.message ?? 'Dang ky tai khoan thanh cong.');
          this.router.navigate(['/auth/login']);
        },
        error: (error: unknown) => {
          this.alertService.error(this.getRegisterErrorMessage(error));
        }
      });
  }

  private getRegisterErrorMessage(error: unknown): string {
    if (SwaggerException.isSwaggerException(error)) {
      try {
        const parsedResponse = JSON.parse(error.response) as { message?: string };
        if (parsedResponse.message) {
          return parsedResponse.message;
        }
      } catch {
        // Fallback below when response is not JSON.
      }

      if (error.message) {
        return error.message;
      }
    }

    if (error instanceof HttpErrorResponse) {
      if (typeof error.error === 'string' && error.error.trim().length > 0) {
        return error.error;
      }

      if (error.error && typeof error.error === 'object' && 'message' in error.error) {
        const candidate = (error.error as { message?: unknown }).message;
        if (typeof candidate === 'string' && candidate.trim().length > 0) {
          return candidate;
        }
      }
    }

    return 'Dang ky that bai';
  }
}
