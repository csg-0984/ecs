import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface WorkOrder {
  id: string;
  customerId: string;
  applianceId: string;
  assignedTechnicianId?: string;
  title: string;
  description: string;
  status: number;
  priority: number;
  createdAt: string;
  scheduledAt?: string;
  completedAt?: string;
}

@Component({
  selector: 'app-workorders-page',
  template: `
    <ejs-grid [dataSource]="workorders" [allowPaging]="true" [pageSettings]="{ pageSize: 10 }" [toolbar]="['Search']">
      <e-columns>
        <e-column field="title" headerText="标题" width="200"></e-column>
        <e-column field="status" headerText="状态" width="120"></e-column>
        <e-column field="priority" headerText="优先级" width="120"></e-column>
        <e-column field="createdAt" headerText="创建时间" width="180" format="yMd"></e-column>
      </e-columns>
    </ejs-grid>
  `
})
export class WorkordersPageComponent implements OnInit {
  workorders: WorkOrder[] = [];
  constructor(private http: HttpClient) {}
  ngOnInit(): void {
    this.http.get<WorkOrder[]>('/api/workorders').subscribe(res => this.workorders = res);
  }
}