import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface Technician {
  id: string;
  name: string;
  phone: string;
  email: string;
  skills: string;
  isActive: boolean;
}

@Component({
  selector: 'app-technicians-page',
  template: `
    <ejs-grid [dataSource]="technicians" [allowPaging]="true" [pageSettings]="{ pageSize: 10 }" [toolbar]="['Search']">
      <e-columns>
        <e-column field="name" headerText="姓名" width="160"></e-column>
        <e-column field="phone" headerText="电话" width="140"></e-column>
        <e-column field="email" headerText="邮箱" width="200"></e-column>
        <e-column field="skills" headerText="技能" width="200"></e-column>
        <e-column field="isActive" headerText="启用" width="100"></e-column>
      </e-columns>
    </ejs-grid>
  `
})
export class TechniciansPageComponent implements OnInit {
  technicians: Technician[] = [];
  constructor(private http: HttpClient) {}
  ngOnInit(): void {
    this.http.get<Technician[]>('/api/technicians').subscribe(res => this.technicians = res);
  }
}