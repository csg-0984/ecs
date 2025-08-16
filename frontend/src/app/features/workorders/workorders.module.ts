import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { GridModule } from '@syncfusion/ej2-angular-grids';
import { WorkordersPageComponent } from './workorders.page';

const routes: Routes = [
  { path: '', component: WorkordersPageComponent }
];

@NgModule({
  declarations: [WorkordersPageComponent],
  imports: [CommonModule, GridModule, RouterModule.forChild(routes)]
})
export class WorkordersModule {}