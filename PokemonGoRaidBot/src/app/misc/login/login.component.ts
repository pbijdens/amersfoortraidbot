import { Component, Inject, OnInit } from "@angular/core";
import { FormGroup, FormControl, FormBuilder, Validators } from '@angular/forms';
import { Router, ActivatedRoute } from "@angular/router";
import { AuthService } from '../../services/auth.service';

@Component({
    selector: "login",
    templateUrl: './login.component.html',
    styleUrls: ['./login.component.less']
})
export class LoginComponent implements OnInit {
    username: string;
    password: string;
    error: string;
    busy: boolean;
    r: string;

    constructor(private router: Router, private authService: AuthService, @Inject('BASE_URL') private baseUrl: string, private route: ActivatedRoute) {
        this.route.queryParams.subscribe(params => {
            this.r = params['r'];
        });
    }

    ngOnInit() {
        if (this.authService.isLoggedIn()) {
            if (this.r) {
                this.router.navigateByUrl(this.r);
            }
            else {
                this.router.navigate(["/misc/settings"]);
            }
        }
    }

    reset() {
        this.username = "";
        this.password = "";
        this.error = "";
        this.busy = false;
    }

    login() {
        var url = this.baseUrl + "api/token/auth";

        this.busy = true;
        this.error = null;
        this.authService.login(this.username, this.password).subscribe(res => {
            this.busy = false;

            if (res) {
                console.log('Login okay, token acquired.');

                if (this.r) {
                    this.router.navigateByUrl(this.r);
                }
                else {
                    this.router.navigate(["/misc/settings"]);
                }
            }
            else {
                // login failed.
                console.log('Login failed.');
                this.error = "Gebruikersnaam of wachtwoord onjuist.";
            }
        }, err => {
            this.busy = false;

            this.error = "Gebruikersnaam of wachtwoord onjuist."

            console.log(err)
        });
    }
}
