import { Observable } from 'rxjs/Observable';
import { merge } from 'rxjs/observable/merge';
import { catchError, shareReplay } from 'rxjs/operators';
import { Subject } from 'rxjs/Subject';
import { of } from 'rxjs/observable/of';
import { HttpErrorResponse } from '@angular/common/http';
import { JsonError } from '../../api/json.error';
import { DataLoaderError } from '../../api/data.loader.error';

export class DataLoader<T> {
    private readonly _error$ = new Subject<DataLoaderError>();
    readonly error$: Observable<DataLoaderError> = this._error$.pipe(shareReplay(1));
    readonly data$: Observable<{} | T>;

    constructor(data: Observable<T>) {
        this.data$ = data.pipe(
            shareReplay(1),

            // The catcherror clause should actually never be used.
            // Errors are a perfectly normal part of the data flow in this application, and are treated as such, so results are normally
            // wrapped in a ResultOrError object and returned as a normal result.
            catchError((error: any) => {
                var result: DataLoaderError = <DataLoaderError>{ status: -1, error: "Internal error", message: "Internal error" };
                if (error instanceof HttpErrorResponse) {
                    console.log(`Error accessing ${error.url}: ${error.status}: (${error.message}) ${error.error}`);
                    var err = error.error as JsonError;
                    if (err && err.success === false) {
                        console.error(`Message: ${err.message}, Error: ${err.error}`);
                        result = <DataLoaderError>{ status: error.status, error: err.error, message: err.message }
                    }
                    else {
                        result = <DataLoaderError>{ status: error.status, error: error.error, message: error.message }
                    }
                }
                else {
                    console.error(error);
                }
                this._error$.next(result);
                return of();
            })
        );
    }

}
