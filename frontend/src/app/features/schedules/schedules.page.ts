import { Component, OnInit, ViewChild } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { ScheduleComponent, EventSettingsModel, View } from '@syncfusion/ej2-angular-schedule';

interface ScheduleEvent {
  Id: string;
  Subject: string;
  StartTime: Date;
  EndTime: Date;
}

@Component({
  selector: 'app-schedules-page',
  template: `
    <ejs-schedule #schedule width='100%' height='650px' [selectedDate]="selectedDate" [currentView]="currentView" [eventSettings]="eventSettings"></ejs-schedule>
  `
})
export class SchedulesPageComponent implements OnInit {
  @ViewChild('schedule', { static: true }) schedule!: ScheduleComponent;
  selectedDate: Date = new Date();
  currentView: View = 'Week';
  eventSettings: EventSettingsModel = { dataSource: [] };

  constructor(private http: HttpClient) {}

  ngOnInit(): void {
    this.loadEvents(this.selectedDate);
  }

  private loadEvents(center: Date) {
    const start = new Date(center);
    start.setDate(start.getDate() - 7);
    const end = new Date(center);
    end.setDate(end.getDate() + 21);

    const params = new HttpParams().set('start', start.toISOString()).set('end', end.toISOString());
    this.http.get<any[]>(`/api/schedules`, { params }).subscribe(list => {
      const events = list.map(x => ({
        Id: x.id,
        Subject: `工单 ${x.workOrderId}`,
        StartTime: new Date(x.startTime),
        EndTime: new Date(x.endTime)
      }));
      this.eventSettings = { dataSource: events };
    });
  }
}