import { Component, OnInit, ViewChild } from '@angular/core';
import { DataManager, UrlAdaptor } from '@syncfusion/ej2-data';
import { GridComponent, PageSettingsModel, ToolbarItems } from '@syncfusion/ej2-angular-grids';
import { AuthService } from '../../core/auth.service';

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
    <ejs-grid #grid [dataSource]="data" [allowPaging]="true" [pageSettings]="pageSettings" [allowSorting]="true" [toolbar]="toolbar">
      <e-columns>
        <e-column field="name" headerText="客户名称" width="180"></e-column>
        <e-column field="phone" headerText="电话" width="140"></e-column>
        <e-column field="email" headerText="邮箱" width="200"></e-column>
        <e-column field="city" headerText="城市" width="120"></e-column>
        <e-column field="createdAt" headerText="创建时间" width="180" format="yMd"></e-column>
        <e-column headerText="操作" width="160" [visible]="auth.hasRole('Dispatcher','Admin')" [commands]="[{ buttonOption: { content: '编辑', cssClass: 'e-flat' } }]"></e-column>
      </e-columns>
    </ejs-grid>
  `
})
export class CustomersPageComponent implements OnInit {
  @ViewChild('grid', { static: true }) grid!: GridComponent;
  data = new DataManager({ url: '/api/customers?requiresCounts=true', adaptor: new UrlAdaptor() });
  pageSettings: PageSettingsModel = { pageSize: 10, pageCount: 5 };
  toolbar: ToolbarItems[] = ['Search'];
  constructor(public auth: AuthService) {}
  ngOnInit(): void {}
}