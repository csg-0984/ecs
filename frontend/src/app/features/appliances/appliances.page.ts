import { Component, OnInit, ViewChild } from '@angular/core';
import { DataManager, UrlAdaptor } from '@syncfusion/ej2-data';
import { GridComponent, PageSettingsModel, ToolbarItems } from '@syncfusion/ej2-angular-grids';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../core/auth.service';

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
    <div style="display:grid; gap:12px;">
      <ejs-grid #grid [dataSource]="data" [allowPaging]="true" [pageSettings]="pageSettings" [allowSorting]="true" [toolbar]="toolbar" (rowSelected)="onRowSelected($event)">
        <e-columns>
          <e-column field="brand" headerText="品牌" width="150"></e-column>
          <e-column field="model" headerText="型号" width="150"></e-column>
          <e-column field="serialNumber" headerText="序列号" width="180"></e-column>
          <e-column field="purchaseDate" headerText="购买日期" width="150" format="yMd"></e-column>
        </e-columns>
      </ejs-grid>

      <div *ngIf="selectedId">
        <h4>图片 / 质保单据</h4>
        <div *ngIf="auth.hasRole('Dispatcher','Admin')" style="display:flex; gap:8px; align-items:center;">
          <input type="file" multiple (change)="uploadImages($event)" />
          <input type="file" (change)="uploadWarranty($event)" />
        </div>
        <div style="display:flex; gap:8px; flex-wrap:wrap; margin-top:8px;">
          <img *ngFor="let img of images" [src]="img" style="width:120px; height:90px; object-fit:cover; border:1px solid #ddd;" />
        </div>
      </div>
    </div>
  `
})
export class AppliancesPageComponent implements OnInit {
  @ViewChild('grid', { static: true }) grid!: GridComponent;
  data = new DataManager({ url: '/api/appliances?requiresCounts=true', adaptor: new UrlAdaptor() });
  pageSettings: PageSettingsModel = { pageSize: 10, pageCount: 5 };
  toolbar: ToolbarItems[] = ['Search'];
  selectedId: string | null = null;
  images: string[] = [];

  constructor(private http: HttpClient, public auth: AuthService) {}
  ngOnInit(): void {}

  onRowSelected(args: any) {
    const row = args?.data as Appliance;
    this.selectedId = row?.id || null;
    if (this.selectedId) {
      this.http.get<string[]>(`/api/appliances/${this.selectedId}/images`).subscribe(res => this.images = res);
    }
  }

  uploadImages(event: Event) {
    if (!this.selectedId) return;
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;
    const form = new FormData();
    Array.from(input.files).forEach(f => form.append('files', f));
    this.http.post(`/api/appliances/${this.selectedId}/images`, form).subscribe(() => {
      this.http.get<string[]>(`/api/appliances/${this.selectedId}/images`).subscribe(res => this.images = res);
    });
  }

  uploadWarranty(event: Event) {
    if (!this.selectedId) return;
    const input = event.target as HTMLInputElement;
    const file = input.files?.[0];
    if (!file) return;
    const form = new FormData();
    form.append('file', file);
    this.http.post<{ url: string }>(`/api/appliances/${this.selectedId}/warranty`, form).subscribe();
  }
}