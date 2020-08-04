import { Component } from '@angular/core';
import * as $ from 'jquery';
import { AuthService } from './services/auth.service';
import { UserService } from './services/user.service';
import { DataLoader } from './utils/data.loader';
import { McUserInfo } from '../api/mc.user.info';
import { Router } from '@angular/router';
import { ResultOrError } from './utils/result.or.error';

@Component({
    selector: 'app-root',
    templateUrl: './app.component.html',
    styleUrls: ['./app.component.less']
})
export class AppComponent {
    title = 'app';

    public userLoader: DataLoader<ResultOrError<McUserInfo>>;

    constructor(public auth: AuthService, public userService: UserService, private router: Router) {
        this.userLoader = new DataLoader<ResultOrError<McUserInfo>>(this.userService.me());
    }

    public logout() {
        if (this.auth.logout()) {
            this.router.navigate(["misc", "login"]);
        }
    }
}
