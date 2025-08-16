import { Component, OnInit } from '@angular/core';
import { HttpClient } from '@angular/common/http';

interface Appliance {
  id: string;
  customerId: string;
  brand: string;
  model: string;
  serialNumber: string;
  purchaseDate?: string;
  warrantyExpiryDate?: string;
  notes: string;
}

@Component({
  selector: 'app-appliances-page',
  template: `
    <ejs-grid [dataSource]="appliances" [allowPaging]="true" [pageSettings]="{ pageSize: 10 }" [toolbar]="['Search']">
      <e-columns>
        <e-column field="brand" headerText="品牌" width="150"></e-column>
        <e-column field="model" headerText="型号" width="150"></e-column>
        <e-column field="serialNumber" headerText="序列号" width="180"></e-column>
        <e-column field="purchaseDate" headerText="购买日期" width="150" format="yMd"></e-column>
      </e-columns>
    </ejs-grid>
  `
})
export class AppliancesPageComponent implements OnInit {
  appliances: Appliance[] = [];
  constructor(private http: HttpClient) {}
  ngOnInit(): void {
    this.http.get<Appliance[]>('/api/appliances').subscribe(res => this.appliances = res);
  }
}