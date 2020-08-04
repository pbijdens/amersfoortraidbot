import { Component, OnInit } from '@angular/core';

import { AuthService } from '../../services/auth.service';
import { DataLoader } from '../../utils/data.loader';
import { ResultOrError } from '../../utils/result.or.error';
import { McUserInfo } from '../../../api/mc.user.info';
import { UserService } from '../../services/user.service';

@Component({
    selector: 'app-settings-dashboard',
    templateUrl: './settings-dashboard.component.html',
    styleUrls: ['./settings-dashboard.component.less']
})
export class SettingsDashboardComponent implements OnInit {

    public meLoader: DataLoader<ResultOrError<McUserInfo>>;


    constructor(private auth: AuthService, private userService: UserService) {
        this.meLoader = new DataLoader<ResultOrError<McUserInfo>>(this.userService.me());
    }

    ngOnInit() {
    }

}
