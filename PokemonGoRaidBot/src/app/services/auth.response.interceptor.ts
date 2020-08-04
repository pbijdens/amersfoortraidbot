import { Injectable, Injector } from "@angular/core";
import { Router, NavigationExtras } from "@angular/router";
import {
    HttpClient,
    HttpHandler, HttpEvent, HttpInterceptor,
    HttpRequest, HttpResponse, HttpErrorResponse
} from "@angular/common/http";
import { AuthService } from "./auth.service";
import { Observable } from "rxjs";

@Injectable()
export class AuthResponseInterceptor implements HttpInterceptor {

    currentRequest: HttpRequest<any>;
    auth: AuthService;

    constructor(private injector: Injector, private router: Router) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        this.auth = this.injector.get(AuthService);
        var token = (this.auth.isLoggedIn()) ? this.auth.getAuth()!.token : null;

        // store the current request
        this.currentRequest = request;

        if (token) { // there is a token
            // save current request

            return next.handle(request)
                .do((event: HttpEvent<any>) => {
                    if (event instanceof HttpResponse) {
                        // do nothing
                    }
                })
                .catch(error => { // despite adding the token, the call still failed
                    console.warn('ari: call failed, is token still valid?');
                    return this.handleError(error, next)
                });
        }
        else { // there is no current token, so do nothing
            return next.handle(request)
                .do((event: HttpEvent<any>) => {
                    if (event instanceof HttpResponse) {
                        // do nothing
                    }
                })
                .catch(error => {
                    if (error instanceof HttpErrorResponse) {
                        console.warn(`ari: there is no token, and error ${error.status} was returned`);
                        if (error.status === 401 && !request.url.endsWith('api/token/auth')) {
                            console.error(`401 on service request ${request.urlWithParams}, redirecting to login page`);
                            this.router.navigate(['/misc/login'], <NavigationExtras>{
                                queryParams: {
                                    r: window.location.pathname + window.location.search
                                }
                            });
                            return next.handle(this.currentRequest);
                        }
                        else if (error.status === 401) {
                            console.error(`401 on auth service request ${request.urlWithParams}, not redirecting`);
                            return next.handle(this.currentRequest);
                        }
                    }
                });
        }
    }

    handleError(err: any, next: HttpHandler) {
        if (err instanceof HttpErrorResponse) {
            if (err.status === 401) {
                // JWT token might be expired: try to get a new one using refresh token
                console.log("Token expired. Attempting refresh...");

                // store current request into a local variable
                var previousRequest = this.currentRequest;

                // thanks to @mattjones61 for the following code
                return this.auth.refreshToken()
                    .flatMap((refreshed) => {
                        if (refreshed) {
                            var token = (this.auth.isLoggedIn()) ? this.auth.getAuth()!.token : null;
                            if (token) {
                                previousRequest = previousRequest.clone({
                                    setHeaders: { Authorization: `Bearer ${token}` }
                                });
                                console.log("header token reset");
                            }
                        }
                        return next.handle(previousRequest);
                    });
            }
        }

        return Observable.throw(err);
    }
}
