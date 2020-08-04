import { EventEmitter, Inject, Injectable, PLATFORM_ID } from "@angular/core";
import { HttpClient, HttpHeaders, HttpEvent, HttpResponse } from "@angular/common/http";
import { Observable, ReplaySubject, BehaviorSubject, Subscription } from "rxjs";
import 'rxjs/Rx';
import 'rxjs/add/operator/map';
import { McUserInfo } from "../../api/mc.user.info";
import { McUser } from "../../api/mc.user";
import { AuthService } from "./auth.service";
import { ResultOrError } from "../utils/result.or.error";
import { TokenResponse } from "../../api/token.response";
import { McUserEditorData } from "../../api/mc.user.editor.data";

@Injectable()
export class UserService {

    constructor(private http: HttpClient, private auth: AuthService) {
    }

    // This method actually caches the result using a replay subject
    // It also subscribes to updates fom the authentication service, causing this data to be automatically refreshed when a token operation takes place.
    private _meTokenStreamSubscription: Subscription;
    private _meCache: ReplaySubject<ResultOrError<McUserInfo>> = new ReplaySubject<ResultOrError<McUserInfo>>(1);
    me(allowCached: boolean = true): Observable<ResultOrError<McUserInfo>> {
        if (!this._meTokenStreamSubscription) {
            // subscribe to the token updates: every time the token is updated, this subscription is triggered causing a refresh of subscribed clients
            this._meTokenStreamSubscription = this.auth.tokenStream.subscribe((x: TokenResponse) => {
                var url: string = `/api/user/me`;
                this.http.get<McUserInfo>(url).subscribe((value: McUserInfo) => {
                    this._meCache.next(ResultOrError.fromData<McUserInfo>(value));
                }, (error: any) => {
                    this._meCache.next(ResultOrError.fromError<McUserInfo>(error));
                });
            });
        }

        return this._meCache.asObservable();
    }

    private _rolesTokenStreamSubscription: Subscription;
    private _rolesCache: ReplaySubject<ResultOrError<string[]>> = new ReplaySubject<ResultOrError<string[]>>(1);
    roles(allowCached: boolean = true): Observable<ResultOrError<string[]>> {
        if (!this._rolesTokenStreamSubscription) {
            // subscribe to the token updates: every time the token is updated, this subscription is triggered causing a refresh of subscribed clients
            this._rolesTokenStreamSubscription = this.auth.tokenStream.subscribe((x: TokenResponse) => {
                var url: string = `/api/user/roles`;
                this.http.get<string[]>(url).subscribe((value: string[]) => {
                    this._rolesCache.next(ResultOrError.fromData<string[]>(value));
                }, (error: any) => {
                    this._rolesCache.next(ResultOrError.fromError<string[]>(error));
                });
            });
        }

        return this._rolesCache.asObservable();
    }

    list(start: number = 0, num: number = 65536, query: string = null, includeDeleted: boolean = false): Observable<ResultOrError<McUser[]>> {
        var url: string = `/api/user/list?start=${start}&num=${num}&query=${query || ''}&includeDeleted=${includeDeleted}`;
        return this.http.get<McUser[]>(url).map((value: McUser[]) => {
            return ResultOrError.fromData<McUser[]>(value);
        }).catch((error: any) => {
            return new BehaviorSubject<ResultOrError<McUser[]>>(ResultOrError.fromError<McUser[]>(error));
        });
    }

    getUser(id: string): Observable<ResultOrError<McUserEditorData>> {
        var url: string = `/api/user/user?id=${id}`;
        return this.http.get<McUserEditorData>(url).map((value: McUserEditorData) => {
            return ResultOrError.fromData<McUserEditorData>(value);
        }).catch((error: any) => {
            return new BehaviorSubject<ResultOrError<McUserEditorData>>(ResultOrError.fromError<McUserEditorData>(error));
        });
    }

    createUser(user: McUserEditorData) {
        var url: string = `/api/user/user`;
        return this.http.put<McUserEditorData>(url, user).map((value: McUserEditorData) => {
            return ResultOrError.fromData<McUserEditorData>(value);
        }).catch((error: any) => {
            return new BehaviorSubject<ResultOrError<McUserEditorData>>(ResultOrError.fromError<McUserEditorData>(error));
        });
    }

    updateUser(user: McUserEditorData) {
        var url: string = `/api/user/user`;
        return this.http.post<McUserEditorData>(url, user).map((value: McUserEditorData) => {
            return ResultOrError.fromData<McUserEditorData>(value);
        }).catch((error: any) => {
            return new BehaviorSubject<ResultOrError<McUserEditorData>>(ResultOrError.fromError<McUserEditorData>(error));
        });
    }
} 
