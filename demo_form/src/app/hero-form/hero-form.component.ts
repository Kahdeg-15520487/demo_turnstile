import { Component } from '@angular/core';
import { HttpClient, HttpHeaders } from '@angular/common/http';

import { Hero } from '../hero';

@Component({
  selector: 'app-hero-form',
  templateUrl: './hero-form.component.html',
  styleUrls: ['./hero-form.component.css']
})
export class HeroFormComponent {
  siteKey = '0x4AAAAAAAxaBIN0SULalYh_'
  powers = ['Really Smart', 'Super Flexible',
    'Super Hot', 'Weather Changer'];

  model = new Hero(18, 'Dr. IQ', this.powers[0], 'Chuck Overstreet');

  submitted = false;
  private captchaToken: string | null = null;

  constructor(private http: HttpClient) { }

  onSubmit() {
    if (this.captchaToken) {
      const payload = {
        hero: this.model
      };

      const headers = new HttpHeaders({
        'Turnstile-Token': this.captchaToken
      });

      this.http.post('http://localhost:5005/api/hero', payload, { headers })
        .subscribe({
          next: response => {
            this.submitted = true;
            console.log('Form submitted successfully:', response);
          },
          error: error => {
            console.error('Error submitting form:', error);
          }
        });
    } else {
      console.error('Captcha token is not available.');
    }
  }

  onResolved(response: string | null) {
    this.captchaToken = response;
    console.log('onResolved', response);
  }

  onErrored(errorCode: string | null) {
    console.log('onErrored', errorCode);
  }

  newHero() {
    this.model = new Hero(42, '', '');
  }

  skyDog(): Hero {
    const myHero = new Hero(42, 'SkyDog',
      'Fetch any object at any distance',
      'Leslie Rollover');
    console.log('My hero is called ' + myHero.name); // "My hero is called SkyDog"
    return myHero;
  }

  //////// NOT SHOWN IN DOCS ////////

  // Reveal in html:
  //   Name via form.controls = {{showFormControls(heroForm)}}
  showFormControls(form: any) {
    return form && form.controls.name &&
      form.controls.name.value; // Dr. IQ
  }

  /////////////////////////////

}
