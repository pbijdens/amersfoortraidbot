import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { BotsRoutingModule } from './bots-routing.module';
import { BotsDashboardComponent } from './bots-dashboard/bots-dashboard.component';
import { FormsModule } from '@angular/forms';
import { SharedModule } from '../shared/shared.module';
import { BotsRaidsComponent } from './bots-raids/bots-raids.component';

@NgModule({
    imports: [
        CommonModule,
        FormsModule,
        SharedModule,
        BotsRoutingModule
    ],
    declarations: [BotsDashboardComponent, BotsRaidsComponent]
})
export class BotsModule { }
