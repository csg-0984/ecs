import { Component, OnInit, ViewChild } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { DataManager, UrlAdaptor } from '@syncfusion/ej2-data';
import { GridComponent, PageSettingsModel, ToolbarItems } from '@syncfusion/ej2-angular-grids';
import { AuthService } from '../../core/auth.service';

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
    <div style="display:grid; gap:12px;">
      <ejs-grid #grid [dataSource]="data" [allowPaging]="true" [pageSettings]="pageSettings" [allowSorting]="true" [toolbar]="toolbar" (rowSelected)="onRowSelected($event)">
        <e-columns>
          <e-column field="title" headerText="标题" width="200"></e-column>
          <e-column field="status" headerText="状态" width="120"></e-column>
          <e-column field="priority" headerText="优先级" width="120"></e-column>
          <e-column field="createdAt" headerText="创建时间" width="180" format="yMd"></e-column>
          <e-column headerText="操作" width="180" [visible]="auth.hasRole('Dispatcher','Admin')" [commands]="[{ buttonOption: { content: '编辑', cssClass: 'e-flat' } }, { buttonOption: { content: '派工', cssClass: 'e-flat' } }]"></e-column>
        </e-columns>
      </ejs-grid>

      <div *ngIf="selectedId">
        <h4>附件</h4>
        <div *ngIf="auth.hasRole('Dispatcher','Admin')">
          <input type="file" multiple (change)="uploadAttachments($event)" />
        </div>
        <div style="display:flex; gap:8px; flex-wrap:wrap; margin-top:8px;">
          <img *ngFor="let img of attachments" [src]="img" style="width:120px; height:90px; object-fit:cover; border:1px solid #ddd;" />
        </div>
      </div>
    </div>
  `
})
export class WorkordersPageComponent implements OnInit {
  @ViewChild('grid', { static: true }) grid!: GridComponent;
  data = new DataManager({ url: '/api/workorders?requiresCounts=true', adaptor: new UrlAdaptor() });
  pageSettings: PageSettingsModel = { pageSize: 10, pageCount: 5 };
  toolbar: ToolbarItems[] = ['Search'];
  selectedId: string | null = null;
  attachments: string[] = [];
  constructor(private http: HttpClient, public auth: AuthService) {}
  ngOnInit(): void {}
  onRowSelected(args: any) {
    const row = args?.data as WorkOrder; this.selectedId = row?.id || null;
    if (this.selectedId) {
      this.http.get<string[]>(`/api/workorders/${this.selectedId}/attachments`).subscribe(res => this.attachments = res);
    }
  }
  uploadAttachments(event: Event) {
    if (!this.selectedId) return;
    const input = event.target as HTMLInputElement;
    if (!input.files || input.files.length === 0) return;
    const form = new FormData();
    Array.from(input.files).forEach(f => form.append('files', f));
    this.http.post(`/api/workorders/${this.selectedId}/attachments`, form).subscribe(() => {
      this.http.get<string[]>(`/api/workorders/${this.selectedId}/attachments`).subscribe(res => this.attachments = res);
    });
  }
}