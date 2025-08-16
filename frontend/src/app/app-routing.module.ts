import { NgModule } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { AuthGuard } from './core/auth.guard';

const routes: Routes = [
  { path: '', redirectTo: 'customers', pathMatch: 'full' },
  { path: 'login', loadChildren: () => import('./features/auth/login.module').then(m => m.LoginModule) },
  { path: 'customers', canActivate: [AuthGuard], loadChildren: () => import('./features/customers/customers.module').then(m => m.CustomersModule) },
  { path: 'appliances', canActivate: [AuthGuard], loadChildren: () => import('./features/appliances/appliances.module').then(m => m.AppliancesModule) },
  { path: 'workorders', canActivate: [AuthGuard], loadChildren: () => import('./features/workorders/workorders.module').then(m => m.WorkordersModule) },
  { path: 'technicians', canActivate: [AuthGuard], loadChildren: () => import('./features/technicians/technicians.module').then(m => m.TechniciansModule) },
  { path: 'schedules', canActivate: [AuthGuard], loadChildren: () => import('./features/schedules/schedules.module').then(m => m.SchedulesModule) },
];

@NgModule({
  imports: [RouterModule.forRoot(routes)],
  exports: [RouterModule]
})
export class AppRoutingModule { }
