import { Component, OnInit, OnDestroy } from '@angular/core';
import { DataLoader } from '../../utils/data.loader';
import { ResultOrError } from '../../utils/result.or.error';
import { McRaidDetails } from '../../../api/mc.raid.details';
import { Subscription } from 'rxjs';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { RaidsService } from '../../services/raids.service';

@Component({
    selector: 'app-raids-raid',
    templateUrl: './raids-raid.component.html',
    styleUrls: ['./raids-raid.component.less']
})
export class RaidsRaidComponent implements OnInit, OnDestroy {

    public dataLoader: DataLoader<ResultOrError<McRaidDetails>>;
    public raid: McRaidDetails;
    public times: Date[] = [];
    public extra: number = 0;
    public total: number;

    private sub: Subscription[] = [];
    constructor(private route: ActivatedRoute, private raidsService: RaidsService) {
        this.sub.push(this.route.paramMap.subscribe((paramMap: ParamMap) => {
            this.dataLoader = new DataLoader<ResultOrError<McRaidDetails>>(this.raidsService.raid(paramMap.get('id')));
            this.dataLoader.data$.subscribe((x: ResultOrError<McRaidDetails>) => {
                this.handleIncomingData(x);
            });
        }));
    }

    reset() {
        var paramMap: ParamMap = this.route.snapshot.paramMap;
        this.dataLoader = new DataLoader<ResultOrError<McRaidDetails>>(this.raidsService.raid(paramMap.get('id')));
        this.dataLoader.data$.subscribe((x: ResultOrError<McRaidDetails>) => {
            this.handleIncomingData(x);
        });
    }

    ngOnInit() {
    }

    ngOnDestroy() {
        if (this.sub) {
            this.sub.forEach(x => x.unsubscribe());
            this.sub = [];
        }
    }

    handleIncomingData(x: ResultOrError<McRaidDetails>) {
        this.times = [];
        this.raid = x.data;

        if (!x.data) return;

        var dtStart = new Date(x.data.Raid.RaidUnlockTime);
        var dtEnd = new Date(x.data.Raid.RaidEndTime)
        var dtNow = new Date();

        var offset = (5.0 * Math.ceil(dtStart.getMinutes() / 5.0)) - dtStart.getMinutes();
        dtStart.setMinutes(offset + dtStart.getMinutes());

        while (dtStart.getTime() < dtEnd.getTime()) {
            if (dtStart.getTime() >= dtNow.getTime()) {
                this.times.push(new Date(dtStart));
            }
            dtStart.setMinutes(5 + dtStart.getMinutes());
        }

        this.total = 0;
        var participants = (x.data.Participants.Unknown || []).concat(x.data.Participants.Valor || []).concat(x.data.Participants.Mystic || []).concat(x.data.Participants.Instinct || []);
        participants.forEach(x => this.total += (1 + x.Extra));
    }

    public join(when: Date = null) {
        var paramMap: ParamMap = this.route.snapshot.paramMap;
        this.dataLoader = new DataLoader<ResultOrError<McRaidDetails>>(this.raidsService.join(paramMap.get('id'), when, this.extra));
        this.dataLoader.data$.subscribe((x: ResultOrError<McRaidDetails>) => {
            this.handleIncomingData(x);
        });
    }

    public maybe() {
        var paramMap: ParamMap = this.route.snapshot.paramMap;
        this.dataLoader = new DataLoader<ResultOrError<McRaidDetails>>(this.raidsService.maybe(paramMap.get('id')));
        this.dataLoader.data$.subscribe((x: ResultOrError<McRaidDetails>) => {
            this.handleIncomingData(x);
        });
    }

    public done() {
        var paramMap: ParamMap = this.route.snapshot.paramMap;
        this.dataLoader = new DataLoader<ResultOrError<McRaidDetails>>(this.raidsService.done(paramMap.get('id')));
        this.dataLoader.data$.subscribe((x: ResultOrError<McRaidDetails>) => {
            this.handleIncomingData(x);
        });
    }

    public no() {
        var paramMap: ParamMap = this.route.snapshot.paramMap;
        this.dataLoader = new DataLoader<ResultOrError<McRaidDetails>>(this.raidsService.no(paramMap.get('id')));
        this.dataLoader.data$.subscribe((x: ResultOrError<McRaidDetails>) => {
            this.handleIncomingData(x);
        });
    }
}
