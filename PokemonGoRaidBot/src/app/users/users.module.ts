import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';

import { UsersRoutingModule } from './users-routing.module';
import { UsersDashboardComponent } from './users-dashboard/users-dashboard.component';
import { UsersNewComponent } from './users-new/users-new.component';
import { UsersEditComponent } from './users-edit/users-edit.component';
import { FormsModule } from '@angular/forms';

@NgModule({
    imports: [
        CommonModule,
        UsersRoutingModule,
        FormsModule
    ],
    declarations: [UsersDashboardComponent, UsersNewComponent, UsersEditComponent]
})
export class UsersModule { }
