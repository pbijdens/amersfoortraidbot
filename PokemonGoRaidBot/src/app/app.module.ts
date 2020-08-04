import { BrowserModule } from '@angular/platform-browser';
import { NgModule } from '@angular/core';
import { LOCALE_ID } from '@angular/core';
import { RouterModule, Routes } from '@angular/router';
import { FormsModule, ReactiveFormsModule } from '@angular/forms';
import { HttpClientModule, HTTP_INTERCEPTORS } from '@angular/common/http';

// Application
import { AppComponent } from './app.component';
import { AuthInterceptor } from './services/auth.interceptor';

// Services
import { AuthService } from './services/auth.service';
import { UserService } from './services/user.service';
import { AuthResponseInterceptor } from './services/auth.response.interceptor';
import { BotsService } from './services/bots.service';
import { RaidsService } from './services/raids.service';

const appRoutes: Routes = [
    { path: 'misc', loadChildren: 'app/misc/misc.module#MiscModule' },
    { path: 'users', loadChildren: 'app/users/users.module#UsersModule' },
    { path: 'bots', loadChildren: 'app/bots/bots.module#BotsModule' },
    { path: 'raids', loadChildren: 'app/raids/raids.module#RaidsModule' },
    { path: '', redirectTo: '/raids', pathMatch: 'full' },
    { path: 'login', redirectTo: '/misc/login', pathMatch: 'full' },
    { path: '**', redirectTo: '/misc/404' }
];

@NgModule({
    declarations: [
        AppComponent
    ],
    imports: [
        RouterModule.forRoot(appRoutes),
        BrowserModule,
        FormsModule,
        ReactiveFormsModule,
        HttpClientModule,
    ],
    providers: [
        { provide: LOCALE_ID, useValue: `${window.location.pathname}`.replace(/^[/]*/, '').split('/').concat('default')[0] },
        { provide: HTTP_INTERCEPTORS, useClass: AuthInterceptor, multi: true },
        { provide: HTTP_INTERCEPTORS, useClass: AuthResponseInterceptor, multi: true },
        { provide: 'BASE_URL', useFactory: getBaseUrl },

        AuthService,
        UserService,
        BotsService,
        RaidsService,
    ],
    bootstrap: [AppComponent]
})
export class AppModule { }

export function getBaseUrl() {
    return document.getElementsByTagName('base')[0].href;
}
