import { Component, OnInit } from '@angular/core';
import { CheckForUpdateService } from '../notify.service'

@Component({
  selector: 'app-root',
  templateUrl: './app.component.html',
  styleUrls: ['./app.component.css']
})
export class AppComponent {
  title = 'ClientApp';
  tk='';
  constructor(){
    this.ngOnInit();
  }
  ngOnInit(): void {
  }
}
