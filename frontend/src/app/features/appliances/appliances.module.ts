import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { RouterModule, Routes } from '@angular/router';
import { GridModule } from '@syncfusion/ej2-angular-grids';
import { AppliancesPageComponent } from './appliances.page';

const routes: Routes = [
  { path: '', component: AppliancesPageComponent }
];

@NgModule({
  declarations: [AppliancesPageComponent],
  imports: [CommonModule, GridModule, RouterModule.forChild(routes)]
})
export class AppliancesModule {}