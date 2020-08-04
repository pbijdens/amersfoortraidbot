import { NgModule } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { IsoDatestampEditorComponent } from './iso-datestamp-editor/iso-datestamp-editor.component';
import { LoadErrorComponent } from './load-error/load-error.component';
import { IsoDateAsLocalDateTimePipe } from './pipes/iso.date.as.local.datetime';
import { IsoDateAsRaidDateTimePipe } from './pipes/iso.date.as.raid.datetime';
import { CommaSeparatedListOfNamesPipe } from './pipes/comma.separated.list.of.names';

@NgModule({
    imports: [CommonModule, FormsModule],
    declarations: [IsoDatestampEditorComponent, LoadErrorComponent, IsoDateAsLocalDateTimePipe, IsoDateAsRaidDateTimePipe, CommaSeparatedListOfNamesPipe],
    exports: [IsoDatestampEditorComponent, LoadErrorComponent, IsoDateAsLocalDateTimePipe, IsoDateAsRaidDateTimePipe, CommaSeparatedListOfNamesPipe]
})
export class SharedModule { }
