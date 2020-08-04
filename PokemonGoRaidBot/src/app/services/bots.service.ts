import { EventEmitter, Inject, Injectable, PLATFORM_ID } from "@angular/core";
import { HttpClient, HttpHeaders, HttpEvent, HttpResponse } from "@angular/common/http";
import { Observable, ReplaySubject, BehaviorSubject, Subscription } from "rxjs";
import 'rxjs/Rx';
import 'rxjs/add/operator/map';
import { ResultOrError } from "../utils/result.or.error";
import { TokenResponse } from "../../api/token.response";
import { McBot } from "../../api/mc.bot";
import { McRaidDetails } from "../../api/mc.raid.details";

@Injectable()
export class BotsService {

    constructor(private http: HttpClient) {
    }

    list(start: number = 0, num: number = 65536, query = null, includeDeleted = false): Observable<ResultOrError<McBot[]>> {
        var url: string = `/api/bots/list?start=${start}&num=${num}&query=${query || ''}&includeDisabled=${includeDeleted}`;
        return this.http.get<McBot[]>(url).map((value: McBot[]) => {
            return ResultOrError.fromData<McBot[]>(value);
        }).catch((error: any) => {
            return new BehaviorSubject<ResultOrError<McBot[]>>(ResultOrError.fromError<McBot[]>(error));
        });
    }

    start(id: string): Observable<ResultOrError<McBot>> {
        var url: string = `/api/bots/start?id=${id}`;
        return this.http.post<McBot>(url, {}).map((value: McBot) => {
            return ResultOrError.fromData<McBot>(value);
        }).catch((error: any) => {
            return new BehaviorSubject<ResultOrError<McBot>>(ResultOrError.fromError<McBot>(error));
        });
    }

    stop(id: string): Observable<ResultOrError<McBot>> {
        var url: string = `/api/bots/stop?id=${id}`;
        return this.http.post<McBot>(url, {}).map((value: McBot) => {
            return ResultOrError.fromData<McBot>(value);
        }).catch((error: any) => {
            return new BehaviorSubject<ResultOrError<McBot>>(ResultOrError.fromError<McBot>(error));
        });
    }

    raids(botID: string, start: number = 0, num: number = 65536, query = null): Observable<ResultOrError<McRaidDetails[]>> {
        var url: string = `/api/bots/raids?botID=${botID}&start=${start}&num=${num}&query=${query || ''}`;
        return this.http.get<McRaidDetails[]>(url).map((value: McRaidDetails[]) => {
            var x = ResultOrError.fromData<McRaidDetails[]>(value);
            return x;
        }).catch((error: any) => {
            return new BehaviorSubject<ResultOrError<McRaidDetails[]>>(ResultOrError.fromError<McRaidDetails[]>(error));
        });
    }
} 
