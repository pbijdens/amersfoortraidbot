import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { RaidsRoutingModule } from './raids-routing.module';
import { RaidsDashboardComponent } from './raids-dashboard/raids-dashboard.component';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../shared/shared.module';
import { RaidsRaidComponent } from './raids-raid/raids-raid.component';

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        SharedModule,
        RaidsRoutingModule
    ],
    declarations: [RaidsDashboardComponent, RaidsRaidComponent]
})
export class RaidsModule { }
