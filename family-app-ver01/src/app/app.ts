import { Component, signal } from '@angular/core';
import { Router, RouterOutlet } from '@angular/router';
import { httpResource } from '@angular/common/http';
import { LoginUser } from './authentication/LoginUser';

@Component({
  selector: 'app-root',
  imports: [RouterOutlet],
  templateUrl: './app.html',
  styleUrl: './app.css'
})
export class App {
  protected readonly title = signal('family-app-ver01');
  public href: string = "";
  user = httpResource<LoginUser>(() => `/authapi/bff/user`); // A reactive function as argument

  ngOnInit() {
    this.href = window.location.href;

  }
}
