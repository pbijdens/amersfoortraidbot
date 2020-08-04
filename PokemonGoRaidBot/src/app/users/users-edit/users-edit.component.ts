import { Component, OnInit, OnDestroy } from '@angular/core';
import { DataLoader } from '../../utils/data.loader';
import { ResultOrError } from '../../utils/result.or.error';
import { McUserEditorData } from '../../../api/mc.user.editor.data';
import { UserService } from '../../services/user.service';
import { switchMap } from 'rxjs/operator/switchMap';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { Observable } from 'rxjs/Observable';
import { Subscription } from 'rxjs';
import { IdentityError } from '../../../api/identity.error';

@Component({
    selector: 'app-users-edit',
    templateUrl: './users-edit.component.html',
    styleUrls: ['./users-edit.component.less']
})
export class UsersEditComponent implements OnInit, OnDestroy {

    public userLoader: DataLoader<ResultOrError<McUserEditorData>>; // subscribe to user-data
    public confirmPassword: string; // backs confirm password box
    public updateReturned: boolean; // true when the update method returned, errors or not
    public validationErrors: string[] = []; // list of validatione errors, copied from update result or set locally
    public identityErrors: IdentityError[] = []; // list of identity errors, copied from update result

    constructor(private route: ActivatedRoute, private userService: UserService) { }

    private sub: Subscription[] = [];
    ngOnInit() {
        this.sub.push(this.route.paramMap.subscribe((paramMap: ParamMap) => {
            this.userLoader = new DataLoader<ResultOrError<McUserEditorData>>(this.userService.getUser(paramMap.get('id')));
        }));
    }

    ngOnDestroy() {
        if (this.sub) {
            this.sub.forEach(x => x.unsubscribe());
            this.sub = [];
        }
    }

    save(user: McUserEditorData) {
        this.updateReturned = false;
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
            this.userService.updateUser(user).subscribe((result: ResultOrError<McUserEditorData>) => {
                this.updateReturned = true;
                var success: boolean = true;
                if (result.validationErrors && result.validationErrors.length > 0) {
                    this.validationErrors = result.validationErrors;
                    success = false;
                }
                if (result.identityErrors && result.identityErrors.length > 0) {
                    this.identityErrors = result.identityErrors;
                    success = false;
                }
                if (success) {
                    this.reset();
                }
            });
        }
    }

    reset() {
        var paramMap: ParamMap = this.route.snapshot.paramMap;
        this.userLoader = new DataLoader<ResultOrError<McUserEditorData>>(this.userService.getUser(paramMap.get('id')));
    }

    isValidEmail(email: string): boolean {
        var re = /^(([^<>()\[\]\\.,;:\s@"]+(\.[^<>()\[\]\\.,;:\s@"]+)*)|(".+"))@((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+[a-zA-Z]{2,}))$/;
        return re.test(`${email}`.toLowerCase());
    }

    isNullOrWhitespace(value: string): boolean {
        var re = /^\s*$/;
        return (!value) || re.test(`${value}`.toLowerCase());
    }
}
