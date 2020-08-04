import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';

import { PageNotFoundComponent } from './page-not-found/page-not-found.component';
import { SettingsDashboardComponent } from './settings-dashboard/settings-dashboard.component';
import { LoginComponent } from './login/login.component';

const routes: Routes = [
    { path: 'settings', component: SettingsDashboardComponent },
    { path: 'login', component: LoginComponent },
    { path: '**', component: PageNotFoundComponent }
];

@NgModule({
    imports: [RouterModule.forChild(routes)],
    exports: [RouterModule],
})
export class MiscRoutingModule { }
