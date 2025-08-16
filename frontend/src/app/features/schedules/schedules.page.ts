import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface Schedule {
  id: string;
  technicianId: string;
  workOrderId: string;
  startTime: string;
  endTime: string;
  status: number;
}

@Component({
  selector: 'app-schedules-page',
  template: `
    <ejs-grid [dataSource]="schedules" [allowPaging]="true" [pageSettings]="{ pageSize: 10 }" [toolbar]="['Search']">
      <e-columns>
        <e-column field="technicianId" headerText="技师" width="200"></e-column>
        <e-column field="workOrderId" headerText="工单" width="200"></e-column>
        <e-column field="startTime" headerText="开始" width="180" format="yMd"></e-column>
        <e-column field="endTime" headerText="结束" width="180" format="yMd"></e-column>
        <e-column field="status" headerText="状态" width="120"></e-column>
      </e-columns>
    </ejs-grid>
  `
})
export class SchedulesPageComponent implements OnInit {
  schedules: Schedule[] = [];
  constructor(private http: HttpClient) {}
  ngOnInit(): void {
    this.http.get<Schedule[]>('/api/schedules').subscribe(res => this.schedules = res);
  }
}