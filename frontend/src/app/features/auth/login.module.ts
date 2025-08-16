import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { ReactiveFormsModule } from '@angular/forms';
import { ButtonModule } from '@syncfusion/ej2-angular-buttons';
import { LoginPageComponent } from './login.page';

const routes: Routes = [
  { path: '', component: LoginPageComponent }
];

@NgModule({
  declarations: [LoginPageComponent],
  imports: [CommonModule, ReactiveFormsModule, ButtonModule, RouterModule.forChild(routes)]
})
export class LoginModule {}