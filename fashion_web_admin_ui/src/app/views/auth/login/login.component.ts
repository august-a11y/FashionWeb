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
import { ActivatedRoute, Router, RouterLink } from '@angular/router';
import { finalize } from 'rxjs';
import {
  AdminApiAuthenticationApiClient,
  AuthenticatedResponseApiResponse,
  LoginRequestDTO,
  SwaggerException
} from '../../../api/admin-api.service.generated';
import { AlertService } from '../../../shared/services/alert.service';
import { AuthSessionService } from '../../../shared/services/auth-session.service';

@Component({
  selector: 'app-login',
  templateUrl: './login.component.html',
  styleUrls: ['./login.component.scss'],
  imports: [FormDirective, InputGroupComponent, InputGroupTextDirective, IconDirective, FormControlDirective, ButtonDirective, ReactiveFormsModule, RouterLink, NgIf]
})
export class LoginComponent {
  loginForm: FormGroup;
  isSubmitting = false;
  submitted = false;

  constructor(
    private fb: FormBuilder,
    private authApiClient: AdminApiAuthenticationApiClient,
    private authSessionService: AuthSessionService,
    private alertService: AlertService,
    private router: Router,
    private route: ActivatedRoute
  ) {
    this.loginForm = this.fb.group({
      username: new FormControl('', Validators.required),
      password: new FormControl('', Validators.required)
    });
  }

  get usernameControl(): FormControl {
    return this.loginForm.get('username') as FormControl;
  }

  get passwordControl(): FormControl {
    return this.loginForm.get('password') as FormControl;
  }

  login(): void {
    this.submitted = true;
    if (this.loginForm.invalid || this.isSubmitting) {
      this.loginForm.markAllAsTouched();
      return;
    }

    const request: LoginRequestDTO = new LoginRequestDTO({
      username: this.loginForm.controls['username']?.value,
      password: this.loginForm.controls['password']?.value
    });

    this.isSubmitting = true;
    this.authApiClient.login(request)
      .pipe(finalize(() => {
        this.isSubmitting = false;
      }))
      .subscribe({
        next: (response: AuthenticatedResponseApiResponse) => {
          if (!response.data?.accessToken) {
            this.alertService.error('Invalid login response from server');
            return;
          }

          this.authSessionService.setSession(response.data);
          this.alertService.success('Login successful');

          const returnUrl = this.route.snapshot.queryParamMap.get('returnUrl');
          const targetUrl = returnUrl && returnUrl.startsWith('/') ? returnUrl : '/dashboard';
          this.router.navigateByUrl(targetUrl);
        },
        error: (error: unknown) => {
          this.alertService.error(this.getLoginErrorMessage(error));
        }
      });
  }

  private getLoginErrorMessage(error: unknown): string {
    if (SwaggerException.isSwaggerException(error)) {
      try {
        const parsedResponse = JSON.parse(error.response) as { message?: string };
        if (parsedResponse.message) {
          return parsedResponse.message;
        }
      } catch {
        // Keep the fallback below when response body is not JSON.
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

    return 'Login failed';
  }
}
