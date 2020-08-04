import { Component, OnInit, OnDestroy } from '@angular/core';
import { McRaidDetails } from '../../../api/mc.raid.details';
import { DataLoader } from '../../utils/data.loader';
import { ResultOrError } from '../../utils/result.or.error';
import { ActivatedRoute, ParamMap } from '@angular/router';
import { BotsService } from '../../services/bots.service';
import { Subscription } from 'rxjs';
import { McRaidDescription } from '../../../api/mc.raid.description';

const PageSize: number = 50;

@Component({
    selector: 'app-bots-raids',
    templateUrl: './bots-raids.component.html',
    styleUrls: ['./bots-raids.component.less']
})
export class BotsRaidsComponent implements OnInit, OnDestroy {
    public botID: string;
    public query: string = null;
    public selectedQuery: string = null;
    public dataLoader: DataLoader<ResultOrError<McRaidDetails[]>>;
    private sub: Subscription[] = [];
    private data: McRaidDetails[] = [];
    public actionResult: ResultOrError<McRaidDetails[]>;

    constructor(private route: ActivatedRoute, private botsService: BotsService) {
    }

    refresh() {
        this.data = [];
        this.dataLoader = new DataLoader<ResultOrError<McRaidDetails[]>>(this.botsService.raids(this.botID, this.data.length, PageSize, this.selectedQuery));
        this.dataLoader.data$.toPromise().then((re: ResultOrError<McRaidDetails[]>) => {
            this.actionResult = re;
            if (!re.isError && re.data && re.data.length > 0) {
                re.data.forEach(x => this.data.push(this.postProcess(x)));
            }
        }).catch(err => { });
    }

    loadMore() {
        var dataLoader = new DataLoader<ResultOrError<McRaidDetails[]>>(this.botsService.raids(this.botID, this.data.length, PageSize, this.selectedQuery));
        dataLoader.data$.toPromise().then((re: ResultOrError<McRaidDetails[]>) => {
            this.actionResult = re;
            if (!re.isError && re.data && re.data.length > 0) {
                re.data.forEach(x => this.data.push(this.postProcess(x)));
            }
        }).catch(err => { });
    }

    postProcess(x: McRaidDetails): McRaidDetails {
        x.NumberOfParticipants = x.Participants ? (x.Participants.Instinct!.length + x.Participants.Valor!.length + x.Participants.Mystic!.length + x.Participants.Unknown!.length) : 0;
        return x;
    }

    search() {
        this.selectedQuery = this.query;
        this.refresh();
    }

    ngOnInit() {
        this.sub.push(this.route.paramMap.subscribe((paramMap: ParamMap) => {
            this.botID = paramMap.get('id');
            this.refresh();
        }));
    }

    ngOnDestroy() {
        if (this.sub) {
            this.sub.forEach(x => x.unsubscribe());
            this.sub = [];
        }
    }

}
