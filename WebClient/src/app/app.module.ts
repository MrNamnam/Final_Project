import { BrowserModule } from "@angular/platform-browser";
import { NgModule } from "@angular/core";
import { HttpClientModule } from "@angular/common/http";
import {MatToolbarModule} from '@angular/material/toolbar';
import { AppComponent } from "./app.component";
import {MatMenuModule} from '@angular/material/menu';
import {MatTableModule} from '@angular/material/table';
import {MatListModule} from '@angular/material/list';
import {BrowserAnimationsModule} from '@angular/platform-browser/animations';
import {MatSelectionList, MatListOption} from '@angular/material/list';
import {CommonModule} from '@angular/common';
import {MatTreeModule} from '@angular/material/tree';
import {MatIconModule} from '@angular/material/icon';
import {FlatTreeControl} from '@angular/cdk/tree';
import {MatTreeFlatDataSource, MatTreeFlattener} from '@angular/material/tree';
import {MatButtonModule} from '@angular/material/button';
import {MatExpansionModule} from '@angular/material/expansion';
import {MatCheckboxModule} from '@angular/material/checkbox';
import {MatInputModule} from '@angular/material/input';
import {MatFormFieldModule} from '@angular/material/form-field';
import { FormsModule } from '@angular/forms';


import { Pipe, PipeTransform } from '@angular/core';
import { DomSanitizer } from '@angular/platform-browser';
import { MonitorChartsComponent } from './monitor-charts/monitor-charts.component';
import {MatPaginatorModule} from '@angular/material/paginator';
import { AgmCoreModule } from '@agm/core';

import { AgmDirectionModule } from 'agm-direction';
import { ChatWindowComponent } from './chat-window/chat-window.component';


@Pipe({
  name: 'safe'
})
export class SafePipe implements PipeTransform {

  constructor(private sanitizer: DomSanitizer) { }
  transform(url) {
    return this.sanitizer.bypassSecurityTrustResourceUrl(url);
  }

} 



@NgModule({
  declarations: [AppComponent, SafePipe, MonitorChartsComponent, ChatWindowComponent],
  imports: [BrowserModule, HttpClientModule, MatMenuModule, BrowserAnimationsModule, MatTableModule,
     MatListModule, CommonModule, MatTreeModule, MatIconModule, MatButtonModule, MatPaginatorModule, MatExpansionModule,
    MatCheckboxModule, MatInputModule, MatFormFieldModule, FormsModule, AgmCoreModule.forRoot({
      apiKey: 'AIzaSyATDm1xNWVektHBJAAixT_j-b_4dqynlrw'
    }),
    AgmDirectionModule],
  exports: [MatSelectionList, MatListOption],
  providers: [],
  bootstrap: [AppComponent],
})
export class AppModule {}



