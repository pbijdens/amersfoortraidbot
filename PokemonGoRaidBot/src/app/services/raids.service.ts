import { EventEmitter, Inject, Injectable, PLATFORM_ID } from "@angular/core";
import { HttpClient, HttpHeaders, HttpEvent, HttpResponse } from "@angular/common/http";
import { Observable, ReplaySubject, BehaviorSubject, Subscription } from "rxjs";
import 'rxjs/Rx';
import 'rxjs/add/operator/map';
import { ResultOrError } from "../utils/result.or.error";
import { TokenResponse } from "../../api/token.response";
import { McBot } from "../../api/mc.bot";
import { McRaidDetails } from "../../api/mc.raid.details";
import { McRaidDescription } from "../../api/mc.raid.description";

@Injectable()
export class RaidsService {

    constructor(private http: HttpClient) {
    }

    active(): Observable<ResultOrError<McRaidDescription[]>> {
        var url: string = `/api/raids/active`;
        return this.http.get<McRaidDescription[]>(url).map((value: McRaidDescription[]) => {
            return ResultOrError.fromData<McRaidDescription[]>(value);
        }).catch((error: any) => {
            return new BehaviorSubject<ResultOrError<McRaidDescription[]>>(ResultOrError.fromError<McRaidDescription[]>(error));
        });
    }

    raid(id: string): Observable<ResultOrError<McRaidDetails>> {
        var url: string = `/api/raids/raid?id=${id}`;
        return this.http.get<McRaidDetails>(url, {}).map((value: McRaidDetails) => {
            return ResultOrError.fromData<McRaidDetails>(value);
        }).catch((error: any) => {
            return new BehaviorSubject<ResultOrError<McRaidDetails>>(ResultOrError.fromError<McRaidDetails>(error));
        });
    }

    join(id: string, when: Date, extra: number): Observable<ResultOrError<McRaidDetails>> {
        var url: string = `/api/raids/join?id=${id}`;
        if (when) { url = url + `&when=${when.toISOString()}`; }
        url = url + `&extra=${extra}`;
        return this.http.post<McRaidDetails>(url, {}).map((value: McRaidDetails) => {
            return ResultOrError.fromData<McRaidDetails>(value);
        }).catch((error: any) => {
            return new BehaviorSubject<ResultOrError<McRaidDetails>>(ResultOrError.fromError<McRaidDetails>(error));
        });
    }

    maybe(id: string): Observable<ResultOrError<McRaidDetails>> {
        var url: string = `/api/raids/maybe?id=${id}`;
        return this.http.post<McRaidDetails>(url, {}).map((value: McRaidDetails) => {
            return ResultOrError.fromData<McRaidDetails>(value);
        }).catch((error: any) => {
            return new BehaviorSubject<ResultOrError<McRaidDetails>>(ResultOrError.fromError<McRaidDetails>(error));
        });
    }

    done(id: string): Observable<ResultOrError<McRaidDetails>> {
        var url: string = `/api/raids/done?id=${id}`;
        return this.http.post<McRaidDetails>(url, {}).map((value: McRaidDetails) => {
            return ResultOrError.fromData<McRaidDetails>(value);
        }).catch((error: any) => {
            return new BehaviorSubject<ResultOrError<McRaidDetails>>(ResultOrError.fromError<McRaidDetails>(error));
        });
    }

    no(id: string): Observable<ResultOrError<McRaidDetails>> {
        var url: string = `/api/raids/no?id=${id}`;
        return this.http.post<McRaidDetails>(url, {}).map((value: McRaidDetails) => {
            return ResultOrError.fromData<McRaidDetails>(value);
        }).catch((error: any) => {
            return new BehaviorSubject<ResultOrError<McRaidDetails>>(ResultOrError.fromError<McRaidDetails>(error));
        });
    }
} 
