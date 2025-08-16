import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface Customer {
  id: string;
  name: string;
  phone: string;
  email: string;
  address: string;
  city: string;
  province: string;
  postalCode: string;
  createdAt: string;
}

@Component({
  selector: 'app-customers-page',
  template: `
    <ejs-grid [dataSource]="customers" [allowPaging]="true" [pageSettings]="{ pageSize: 10 }" [toolbar]="['Search']">
      <e-columns>
        <e-column field="name" headerText="客户名称" width="180"></e-column>
        <e-column field="phone" headerText="电话" width="140"></e-column>
        <e-column field="email" headerText="邮箱" width="200"></e-column>
        <e-column field="city" headerText="城市" width="120"></e-column>
        <e-column field="createdAt" headerText="创建时间" width="180" format="yMd"></e-column>
      </e-columns>
    </ejs-grid>
  `
})
export class CustomersPageComponent implements OnInit {
  customers: Customer[] = [];
  constructor(private http: HttpClient) {}
  ngOnInit(): void {
    this.http.get<Customer[]>('/api/customers').subscribe(res => this.customers = res);
  }
}