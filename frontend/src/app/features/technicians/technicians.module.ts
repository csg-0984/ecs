import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { GridModule } from '@syncfusion/ej2-angular-grids';
import { TechniciansPageComponent } from './technicians.page';

const routes: Routes = [
  { path: '', component: TechniciansPageComponent }
];

@NgModule({
  declarations: [TechniciansPageComponent],
  imports: [CommonModule, GridModule, RouterModule.forChild(routes)]
})
export class TechniciansModule {}