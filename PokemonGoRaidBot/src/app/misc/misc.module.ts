import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';

import { MiscRoutingModule } from './misc-routing.module';
import { SettingsDashboardComponent } from './settings-dashboard/settings-dashboard.component';
import { PageNotFoundComponent } from './page-not-found/page-not-found.component';
import { LoginComponent } from './login/login.component';


@NgModule({
    imports: [
        CommonModule,
        MiscRoutingModule,
        FormsModule,
        ReactiveFormsModule,
    ],
    declarations: [SettingsDashboardComponent, PageNotFoundComponent, LoginComponent]
})
export class MiscModule { }
