import { Component, OnInit, Input } from '@angular/core';
import { ResultOrError } from '../../utils/result.or.error';

@Component({
    selector: 'load-error',
    templateUrl: './load-error.component.html',
    styleUrls: ['./load-error.component.less']
})
export class LoadErrorComponent implements OnInit {

    @Input()
    public loaderResult: ResultOrError<any>;

    constructor() { }

    ngOnInit() {
    }

}
