import { Component, OnInit } from '@angular/core';
import { McBot } from '../../../api/mc.bot';
import { DataLoader } from '../../utils/data.loader';
import { ResultOrError } from '../../utils/result.or.error';
import { Router } from '@angular/router';
import { RaidsService } from '../../services/raids.service';
import { McRaidDescription } from '../../../api/mc.raid.description';

@Component({
    selector: 'app-raids-dashboard',
    templateUrl: './raids-dashboard.component.html',
    styleUrls: ['./raids-dashboard.component.less']
})
export class RaidsDashboardComponent implements OnInit {

    public actionResult: ResultOrError<McRaidDescription>;
    public dataLoader: DataLoader<ResultOrError<McRaidDescription[]>>;
    public includeDeletedItems: boolean = false;
    public query: string = null;

    constructor(private router: Router, private raidsService: RaidsService) {
        this.refresh();
    }

    public refresh() {
        this.dataLoader = new DataLoader<ResultOrError<McRaidDescription[]>>(this.raidsService.active());
        this.dataLoader.data$.subscribe((x: ResultOrError<McRaidDescription[]>) => {
            this.handleIncomingData(x);
        });
    }

    goto(row: McRaidDescription) {
        this.router.navigate(['/raids', row.PublicID]);
    }

    handleIncomingData(x: ResultOrError<McRaidDescription[]>) {
        if (!x || !x.data) return;
        x.data.forEach(row => row.Total = row.Mystic + row.Valor + row.Instinct + row.Unknown);
    }

    ngOnInit() {
    }

}
