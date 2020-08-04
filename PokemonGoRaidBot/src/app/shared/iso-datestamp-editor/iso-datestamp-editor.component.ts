import { Component, OnInit, Input } from '@angular/core';

@Component({
    selector: 'mc-datestamp-editor',
    templateUrl: './iso-datestamp-editor.component.html',
    styleUrls: ['./iso-datestamp-editor.component.less']
})
export class IsoDatestampEditorComponent implements OnInit {

    @Input()
    public id: string;

    @Input()
    public date: string;

    constructor() { }

    ngOnInit() {
    }

}
