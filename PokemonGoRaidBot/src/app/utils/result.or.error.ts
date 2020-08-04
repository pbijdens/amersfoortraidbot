import { HttpErrorResponse } from "@angular/common/http";
import { JsonError } from "../../api/json.error";
import { IdentityError } from "../../api/identity.error";

export class ResultOrError<T> {
    data: T;
    isError: boolean;
    errorCode: number;
    errorMessage: string;
    errorDetails: string;
    validationErrors: string[];
    identityErrors: IdentityError[];

    public static fromData<T>(data: T): ResultOrError<T> {
        return <ResultOrError<T>>{
            data: data,
            isError: false
        };
    }

    public static fromError<T>(error: any): ResultOrError<T> {
        var result = <ResultOrError<T>>{
            isError: true
        };

        if (error instanceof HttpErrorResponse) {
            result.errorCode = error.status;

            var err = error.error as JsonError;
            if (err && err.success === false) {
                result.errorMessage = err.message;
                result.errorDetails = err.error;
                result.validationErrors = err.validationErrors;
                result.identityErrors = err.identityErrors;
            }
            else {
                result.errorMessage = `HTTP error`;
                result.errorDetails = `HTTP error ${error.type} on ${error.url} returned ${error.status}: ${error.statusText}`;
            }
        }
        else {
            result.errorMessage = `General error`;
            result.errorDetails = `No further details are avalable, check debug console for more information`;
        }

        console.error(`Error ${result.errorCode}: ${result.errorMessage} (ve: ${result.validationErrors && result.validationErrors.length}, ie: ${result.identityErrors && result.identityErrors.length}) \nDetails: ${result.errorDetails}`)
        return result;
    }
}
