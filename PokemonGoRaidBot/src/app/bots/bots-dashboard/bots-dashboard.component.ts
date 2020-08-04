import { Component, OnInit } from '@angular/core';
import { McBot } from '../../../api/mc.bot';
import { DataLoader } from '../../utils/data.loader';
import { ResultOrError } from '../../utils/result.or.error';
import { Router } from '@angular/router';
import { BotsService } from '../../services/bots.service';

@Component({
    selector: 'app-bots-dashboard',
    templateUrl: './bots-dashboard.component.html',
    styleUrls: ['./bots-dashboard.component.less']
})
export class BotsDashboardComponent implements OnInit {

    public actionResult: ResultOrError<McBot>;
    public dataLoader: DataLoader<ResultOrError<McBot[]>>;
    public includeDeletedItems: boolean = false;
    public query: string = null;

    constructor(private router: Router, private botsService: BotsService) {
        this.refresh();
    }

    public start(bot: McBot) {
        this.actionResult = null;
        this.botsService.start(bot.Id).subscribe((x: ResultOrError<McBot>) => {
            this.actionResult = x;
            this.refresh();
        });
    }

    public stop(bot: McBot) {
        this.actionResult = null;
        this.botsService.stop(bot.Id).subscribe((x: ResultOrError<McBot>) => {
            this.actionResult = x;
            this.refresh();
        });
    }

    public toggleIncludeDeletedItems() {
        this.includeDeletedItems = !this.includeDeletedItems;
        this.refresh();
    }

    public refresh() {
        this.dataLoader = new DataLoader<ResultOrError<McBot[]>>(this.botsService.list(0, 65536, this.query, this.includeDeletedItems));
    }

    ngOnInit() {
    }

}
