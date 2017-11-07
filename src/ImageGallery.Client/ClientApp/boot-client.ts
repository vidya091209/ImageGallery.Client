import { enableProdMode } from '@angular/core';
import { platformBrowserDynamic } from '@angular/platform-browser-dynamic';
import { AppModule } from './app/app.module';

import './modernizr.js'; // 'npm run modernizr' to create this file

// Enable either Hot Module Reloading or production mode
/* tslint:disable */
if (module['hot']) {
    module['hot'].accept();
    module['hot'].dispose(() => { });
} else {
    enableProdMode();
}

let a = platformBrowserDynamic().bootstrapModule(AppModule);
a.then(() => { (<any>window).appBootstrap && (<any>window).appBootstrap(); })
