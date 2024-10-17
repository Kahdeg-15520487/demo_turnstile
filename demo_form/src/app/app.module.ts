import { NgModule } from '@angular/core';
import { BrowserModule } from '@angular/platform-browser';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { NgxTurnstileModule } from "ngx-turnstile";
import { HttpClientModule } from '@angular/common/http';

import { AppComponent } from './app.component';
import { HeroFormComponent } from './hero-form/hero-form.component';

@NgModule({
  imports: [
    BrowserModule,
    CommonModule,
    FormsModule,
    NgxTurnstileModule,
    HttpClientModule
  ],
  declarations: [
    AppComponent,
    HeroFormComponent
  ],
  providers: [],
  bootstrap: [AppComponent]
})
export class AppModule { }
