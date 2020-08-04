import { Component, OnInit } from '@angular/core';
import { UserService } from '../../services/user.service';
import { DataLoader } from '../../utils/data.loader';
import { McUser } from '../../../api/mc.user';
import { Router } from '@angular/router';
import { McUserInfo } from '../../../api/mc.user.info';
import { ResultOrError } from '../../utils/result.or.error';

@Component({
    selector: 'app-users-dashboard',
    templateUrl: './users-dashboard.component.html',
    styleUrls: ['./users-dashboard.component.less']
})
export class UsersDashboardComponent implements OnInit {

    public usersLoader: DataLoader<ResultOrError<McUser[]>>;
    public query: string;
    public includeDeletedItems: boolean = false;

    constructor(private router: Router, private userService: UserService) {
        this.refresh();
    }

    public openUser(user: McUser) {
        this.router.navigate(['users', 'edit', user.Id])
    }

    public toggleIncludeDeletedItems() {
        this.includeDeletedItems = !this.includeDeletedItems;
        this.refresh();
    }

    public refresh() {
        this.usersLoader = new DataLoader<ResultOrError<McUser[]>>(this.userService.list(0, 65536, this.query, this.includeDeletedItems));
    }

    ngOnInit() {
    }
}
