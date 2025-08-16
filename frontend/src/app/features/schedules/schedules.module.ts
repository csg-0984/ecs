import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { SchedulesPageComponent } from './schedules.page';
import { ScheduleModule, DayService, WeekService, WorkWeekService, MonthService, AgendaService } from '@syncfusion/ej2-angular-schedule';

const routes: Routes = [
  { path: '', component: SchedulesPageComponent }
];

@NgModule({
  declarations: [SchedulesPageComponent],
  imports: [CommonModule, RouterModule.forChild(routes), ScheduleModule],
  providers: [DayService, WeekService, WorkWeekService, MonthService, AgendaService]
})
export class SchedulesModule {}