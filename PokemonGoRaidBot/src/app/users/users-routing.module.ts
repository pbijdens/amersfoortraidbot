import { NgModule } from '@angular/core';
import { Routes, RouterModule } from '@angular/router';
import { UsersDashboardComponent } from './users-dashboard/users-dashboard.component';
import { UsersEditComponent } from './users-edit/users-edit.component';
import { UsersNewComponent } from './users-new/users-new.component';

const routes: Routes = [
    { path: '', component: UsersDashboardComponent },
    { path: 'edit/:id', component: UsersEditComponent },
    { path: 'new', component: UsersNewComponent},
];

@NgModule({
  imports: [RouterModule.forChild(routes)],
  exports: [RouterModule]
})
export class UsersRoutingModule { }
