import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { GridModule } from '@syncfusion/ej2-angular-grids';
import { SchedulesPageComponent } from './schedules.page';

const routes: Routes = [
  { path: '', component: SchedulesPageComponent }
];

@NgModule({
  declarations: [SchedulesPageComponent],
  imports: [CommonModule, GridModule, RouterModule.forChild(routes)]
})
export class SchedulesModule {}