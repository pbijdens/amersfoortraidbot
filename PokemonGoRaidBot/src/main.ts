import { enableProdMode } from '@angular/core';
import { TRANSLATIONS, TRANSLATIONS_FORMAT } from '@angular/core';
import { MissingTranslationStrategy } from '@angular/core'
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';

import { AppModule } from './app/app.module';
import { environment } from './environments/environment';

if (environment.production) {
  enableProdMode();
}

const GetCurrentTranslation = () => {
    let currentLanguage = `${window.location.pathname}`.replace(/^[/]*/, '').split('/').concat('nl-NL')[0];
    return environment.translations[currentLanguage];
}

platformBrowserDynamic().bootstrapModule(AppModule, {
    missingTranslation: MissingTranslationStrategy.Warning,
    providers: environment.production ? [] : [
        { provide: TRANSLATIONS, useValue: GetCurrentTranslation() },
        { provide: TRANSLATIONS_FORMAT, useValue: 'xlf' }
    ]
}).catch(err => console.log(err));
