import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { GridModule } from '@syncfusion/ej2-angular-grids';
import { CustomersPageComponent } from './customers.page';

const routes: Routes = [
  { path: '', component: CustomersPageComponent }
];

@NgModule({
  declarations: [CustomersPageComponent],
  imports: [CommonModule, GridModule, RouterModule.forChild(routes)]
})
export class CustomersModule {}