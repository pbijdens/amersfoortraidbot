import { Component, OnInit, OnDestroy } from '@angular/core';
import { DataLoader } from '../../utils/data.loader';
import { ResultOrError } from '../../utils/result.or.error';
import { McUserEditorData } from '../../../api/mc.user.editor.data';
import { UserService } from '../../services/user.service';
import { ActivatedRoute, ParamMap, Router } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { Subscription } from 'rxjs';
import { IdentityError } from '../../../api/identity.error';

@Component({
    selector: 'app-users-new',
    templateUrl: './users-new.component.html',
    styleUrls: ['./users-new.component.less']
})
export class UsersNewComponent implements OnInit {

    public confirmPassword: string;
    public data: McUserEditorData = new McUserEditorData();
    public createReturned: boolean;
    public validationErrors: string[] = [];
    public identityErrors: IdentityError[] = [];
    public newUserLink: string[];

    constructor(private router: Router, private userService: UserService) { }

    private sub: Subscription[] = [];
    ngOnInit() {
    }

    ngOnDestroy() {
        if (this.sub) {
            this.sub.forEach(x => x.unsubscribe());
            this.sub = [];
        }
    }

    save(user: McUserEditorData) {
        this.createReturned = false;
        this.validationErrors = [];
        this.identityErrors = [];

        if (!user) this.validationErrors.push("INTERNAL_ERROR");
        if (!this.isValidEmail(user!.Email)) this.validationErrors.push("EMAIL_INVALID");
        if (this.isNullOrWhitespace(user!.Id)) this.validationErrors.push("ID_INVALID");
        if (this.isNullOrWhitespace(user!.UserName)) this.validationErrors.push("USERNAME_INVALID");
        if ((!this.isNullOrWhitespace(user!.Password) || !this.isNullOrWhitespace(this.confirmPassword)) && (user!.Password !== this.confirmPassword)) {
            this.validationErrors.push("PASSWORDS_DO_NOT_MATCH");
        }
        if (this.isNullOrWhitespace(user!.DisplayName)) this.validationErrors.push("NAME_INVALID");

        if (this.validationErrors.length == 0) {
            this.userService.createUser(user).subscribe((result: ResultOrError<McUserEditorData>) => {
                this.createReturned = true;
                if (result.validationErrors && result.validationErrors.length > 0) {
                    this.validationErrors = result.validationErrors;
                }
                if (result.identityErrors && result.identityErrors.length > 0) {
                    this.identityErrors = result.identityErrors;
                }
                this.newUserLink = ['/', 'users', 'edit', result.data!.Id];
                this.reset();
            });
        }
    }

    reset() {
        this.data = new McUserEditorData();
    }

    isValidEmail(email: string): boolean {
        var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return re.test(`${email}`.toLowerCase());
    }

    isNullOrWhitespace(value: string): boolean {
        var re = /^\s*$/;
        return re.test(`${value}`.toLowerCase());
    }
}
