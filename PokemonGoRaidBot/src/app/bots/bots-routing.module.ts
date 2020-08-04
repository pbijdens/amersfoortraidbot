import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { BotsDashboardComponent } from './bots-dashboard/bots-dashboard.component';
import { BotsRaidsComponent } from './bots-raids/bots-raids.component';

const routes: Routes = [
    { path: '', component: BotsDashboardComponent },
    { path: ':id/raids', component: BotsRaidsComponent },
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule]
})
export class BotsRoutingModule { }
