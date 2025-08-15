import { Component } from '@angular/core';
import { FormBuilder, Validators, FormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { AuthService } from '../../core/auth.service';

@Component({
  selector: 'app-login-page',
  template: `
    <div style="max-width: 360px; margin: 40px auto;">
      <h3>登录</h3>
      <form [formGroup]="form" (ngSubmit)="onSubmit()" style="display: grid; gap: 12px;">
        <input class="e-input" placeholder="邮箱" formControlName="email" type="email" />
        <input class="e-input" placeholder="密码" formControlName="password" type="password" />
        <button ejs-button cssClass="e-primary" type="submit" [disabled]="form.invalid || loading">登录</button>
      </form>
      <div *ngIf="error" style="color:#c00; margin-top:8px;">{{error}}</div>
    </div>
  `
})
export class LoginPageComponent {
  loading = false;
  error = '';
  form: FormGroup;

  constructor(private fb: FormBuilder, private auth: AuthService, private router: Router) {
    this.form = this.fb.group({
      email: ['', [Validators.required, Validators.email]],
      password: ['', Validators.required]
    });
  }

  onSubmit() {
    if (this.form.invalid) return;
    this.loading = true;
    const { email, password } = this.form.value as { email: string; password: string };
    this.auth.login(email, password).subscribe({
      next: () => { this.loading = false; this.router.navigateByUrl('/customers'); },
      error: (err) => { this.loading = false; this.error = err?.error?.message || '登录失败'; }
    });
  }
}