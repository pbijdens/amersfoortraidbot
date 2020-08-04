import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { RaidsDashboardComponent } from './raids-dashboard/raids-dashboard.component';
import { RaidsRaidComponent } from './raids-raid/raids-raid.component';

const routes: Routes = [
    { path: '', component: RaidsDashboardComponent },
    { path: ':id', component: RaidsRaidComponent },
];
@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class RaidsRoutingModule { }
