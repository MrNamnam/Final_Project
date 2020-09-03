/// <reference types="@types/googlemaps" />
import { Component, ViewEncapsulation, ViewChild } from "@angular/core";
import * as signalR from "@aspnet/signalr";
import { HttpClient, HttpHeaders } from "@angular/common/http";
import { map, switchMap } from "rxjs/operators";
import {MatSelectionList, MatListOption} from '@angular/material/list';
import {FlatTreeControl} from '@angular/cdk/tree';
import {MatTreeFlatDataSource, MatTreeFlattener} from '@angular/material/tree';
import {MatTableModule} from '@angular/material/table';
import {MatIconModule} from '@angular/material/icon';
import { range } from 'rxjs';
import { stringify } from 'querystring';
import { OnInit } from '@angular/core'
import { AfterViewInit, ElementRef } from '@angular/core';
import { } from 'googlemaps';
import { AgmCoreModule } from '@agm/core';


interface SomkingDetector {
  id: number
  latitude: number
  longitude: number
  address: string
  owner_name: string
  owner_phone_num: number 
}

interface Events {
  city: string
  country: string
  device_id: string
  email: string
  event_details: string
  id: number
  is_false_alarm: string
  latitude: string
  longitude: string
  num_of_injured: string
  time: string
  

}

interface SignalRConnection {
  url: string;
  accessToken: string;
}


interface CurrentAlertsObject {
  partitionKey: string;
  rowKey: string;
  latitude: string;
  longitude: string;
  time: string;
}

interface ClientsObject {
  partitionKey: string;
  rowKey: string;
  email: string;
  name: string;
  password: string;
  phone_number: string;
}


interface AlertNode {
  alert_obj: CurrentAlertsObject;
  client_obj: ClientsObject;
  chat_box_open: boolean;
  country: string;
  city: string;
  alert_message: string;
  details: string;
  injured: number;
  checked: boolean;
}






@Component({
  selector: "app-root",
  templateUrl: "./app.component.html",
  styleUrls: ["./app.component.css", "./app.component.less"],
  encapsulation: ViewEncapsulation.None,

})





export class AppComponent{






  ////////// for map
  public dis: String ;
  public time: number ;
  public h: String ;
  public m: String ;
  public lat: Number = 32.2936086;
  public lng: Number = 34.9367102;
  public origin: any;
  public destination: any;

  panelOpenState = false;


  private readonly prefixUrlcosmos = "https://webfunc20200824154225.azurewebsites.net";
  private readonly prefixUrltable = "https://webfunc20200824170629.azurewebsites.net"
  
  private readonly connectionStringStorage = "?sv=2019-12-12&ss=bfqt&srt=sco&sp=rwdlacupx&se=2020-09-16T20:26:30Z&st=2020-08-06T12:26:30Z&spr=https,http&sig=NO59mo1wI95CNGVWrXozkF7Nt7Dp%2BVfMPvDqL%2BYL0%2FQ%3D"
  private readonly httpOptions = { headers: new HttpHeaders({ "Content-Type": "application/json" }) };
  private readonly negotiateUrl = this.prefixUrlcosmos + "/api/negotiate";
  private readonly getActiveEvents = this.prefixUrltable + "/api/get_current_alerts";
  private readonly getClientData = this.prefixUrltable + "/api/match-alert-to-client/"
  private readonly addEvent = this.prefixUrlcosmos + "/api/add-event"
  private readonly ClientDeleteMessage = this.prefixUrlcosmos + "/api/client-delete-message/";
  private readonly deleteCurrent = this.prefixUrltable + "/api/delete-alert"
  private readonly getHistoryEvents = this.prefixUrlcosmos + "/api/get_history_events"



 
  private readonly counterId = 1;

  private hubConnection: signalR.HubConnection;
  public counter: number = 0;
  public devices: string[] = ["bla", "bla1"];
  public events: JSON;
  public page: string;
  public maptype: number = -1;
  public station: string = 'Jaffa';

  // lina:
  public displayedColumns: string[] = ['id', 'city', 'country', 'lat', 'lon','details','bool','number'];
  public dataSource: Event[]= [];
 
  public ALERTS_DATA:  AlertNode[] = [];
  public errorSubmit: string; 
  public url: string;
  public mySidenav: string = "close-menu";

   


  constructor(private readonly http: HttpClient) {
    const negotiateBody = { UserId: "FireWeb" };
   
   let consignal =  this.http
      .post<SignalRConnection>(this.negotiateUrl, JSON.stringify(negotiateBody), this.httpOptions)
      .pipe(
        map(connectionDetails =>
          new signalR.HubConnectionBuilder().withUrl(`${connectionDetails.url}`, { accessTokenFactory: () => connectionDetails.accessToken }).build(),
        )
      )

      consignal.subscribe(hub => {
        this.hubConnection = hub;
        hub.on("NewAlertWeb", data => {
          console.log(data)
          let alert : CurrentAlertsObject =   {
            partitionKey:data.PartitionKey,
            rowKey: data.RowKey,
            latitude: data.latitude,
            longitude: data.longitude,
            time: data.time
          }
          console.log("SignalR was activated")
          this.SignalrUpdatingNewCurrentAlertAdd(alert);
        });
        hub.start();
      });


      consignal.subscribe(hub => {
        this.hubConnection = hub;
        hub.on("FalseAlarm", data => {
          console.log(data)
          this.FalseAlarm(data); 
        });
        hub.start();
      });


   this.GetCurrentAlerts()

    this.http.get<JSON>(this.getHistoryEvents, this.httpOptions).subscribe(History => {
      console.log(history)
      this.CreateHistoryAlerts(History)
      console.log(this.dataSource)

    }); 

  }
  
 




  public GetCurrentAlerts(): void{
    this.http.get<JSON>(this.getActiveEvents, this.httpOptions).subscribe(Alerts => { 
      console.log(Alerts)
      
      for (let key in Alerts){
        let rowkey =  Alerts[key].partitionKey // partionkey of alerts is the email  
        console.log(rowkey);
        let clientdata = null;
        this.http.get<JSON>(this.getClientData + rowkey, this.httpOptions).subscribe(clientsData => {
          console.log(clientsData);
          if(clientsData != null){
            clientdata = clientsData
            this.CreateAlerts(Alerts[key], clientsData);
          }
        });
      }
      console.log(this.ALERTS_DATA)

    }); 
  }


  public CreateAlerts(alertElement, clientElement): void {
    console.log(alertElement)
    console.log(clientElement) 
    let parti
    let rowky
    if(alertElement.PartitionKey !== undefined){
      parti = alertElement.PartitionKey
      rowky = alertElement.RowKey
    }
    else{
      parti = alertElement.partitionKey
      rowky = alertElement.rowKey
    }

    let alertObj: CurrentAlertsObject =   {
      partitionKey: parti,
      rowKey: alertElement.rowKey,
      latitude: alertElement.latitude,
      longitude: alertElement.longitude,
      time: alertElement.time
    }


    this.findAdress(alertObj.latitude, alertObj.longitude).then((addressarray) =>{
      console.log(addressarray)
      if(addressarray != null){
        this.ALERTS_DATA = [...this.ALERTS_DATA, { alert_obj:alertObj, client_obj: clientElement, chat_box_open: false, country: addressarray[1], city: addressarray[0], alert_message: "is alerting", details: "null", injured: 0, checked: false }]
      }
      else{
        this.ALERTS_DATA = [...this.ALERTS_DATA, { alert_obj: alertElement, client_obj: clientElement, chat_box_open: false, country: "", city: "", alert_message: "is alerting",  details: "null", injured: 0, checked: false}]
      }

      });

  }


  public findAdress(lat, lng): Promise<string[]>{
    let array: string[] = ["null", "null"]
    return new Promise(function(resolve, reject) {
      let geocoder = new google.maps.Geocoder();
      let justcheck = { lat: lat, lng: lng };
      var latlngaddress = new google.maps.LatLng(justcheck.lat, justcheck.lng);
      geocoder.geocode( { 'location': latlngaddress}, function(results, status) {
        if (status == 'OK'){
          console.log(results[0].address_components[2].short_name)
          resolve([results[0].address_components[2].short_name, results[0].address_components[4].short_name])
        } 
        else {
          reject(new Error('Couldnt\'t find the location ' + status))
        }
      })
    })
  }



  public SignalrUpdatingNewCurrentAlertAdd(alert){
    console.log("signalR")
    console.log(alert)
    let cureentalert = this.ALERTS_DATA.filter((currAlert) => ((currAlert.alert_obj.partitionKey === alert.partitionKey) && (currAlert.alert_obj.rowKey === alert.rowKey)))[0]
    console.log(cureentalert)
    if(cureentalert !== undefined){
      console.log(cureentalert)
      return
    }

    console.log(alert)
    this.http.get<JSON>(this.getClientData + alert.partitionKey, this.httpOptions).subscribe(clientsData => {
      console.log(clientsData);
      if(clientsData != null){
        this.CreateAlerts(alert, clientsData);
      }
    }); 
}


public DeleteAlertFromArray(PartitionKey, RowKey){
  console.log(PartitionKey + "------" + RowKey)
  this.ALERTS_DATA = this.ALERTS_DATA.filter((alert) => !((alert.alert_obj.partitionKey === PartitionKey) && (alert.alert_obj.rowKey === RowKey)))
}


  public UpdateHistory(event): void{
    console.log("update" + event)
    this.dataSource = [...this.dataSource, event]
    console.log(this.dataSource[this.dataSource.length - 1])
  }
  
  public CreateHistoryAlerts(event): void{
    console.log(event)
    this.dataSource = event;
    console.log(this.dataSource)
  }
  

  public changeToPage(pageName: string): void {
    this.page = pageName
    this.maptype= -1
    console.log(this.maptype)
    this.menuClick()

  }

  public changeToOnlyMapPage(pageName: string): void {
    this.page = pageName
    console.log(this.maptype)

  }

  public chatBoxFunc(i: number): void {
    console.log(this.ALERTS_DATA[i].chat_box_open)
    let chat_box = this.ALERTS_DATA.filter((alert) => ((this.ALERTS_DATA[i].alert_obj.partitionKey === alert.alert_obj.partitionKey) && (alert.chat_box_open === true)))
    if(chat_box.length == 0){
      this.ALERTS_DATA[i].chat_box_open = true;
      console.log(this.ALERTS_DATA)
    }
    else{
      console.log("already has open chat")
    }
  }

  public closeChatFunc(chatalert: AlertNode): void {
    console.log(chatalert)
    this.ALERTS_DATA.map((alert) => {
      console.log(alert.alert_obj)
      if(chatalert.alert_obj.partitionKey === alert.alert_obj.partitionKey){
        alert.chat_box_open = false;
        console.log(alert.chat_box_open)
        console.log("closed chat")
      }
    });
    
  }


  public whichMap(i: number): void {
    this.maptype = i
    console.log(this.maptype)
  }
  

  public getDirection() {
    if (this.station == 'Eilat') {
      this.origin = { lat: 29.5661306, lng: 34.948351 };
    }
    else if (this.station == 'Jaffa') {
      this.origin = { lat: 32.0483568, lng: 34.7537548 };
    }
    else if (this.station == 'Haifa') {
      this.origin = { lat: 32.801523, lng: 34.983553 };
    }
    else if (this.station == 'Jerusalem') {
      this.origin = { lat: 31.762427, lng: 35.199502 };
    }
    else if (this.station == "Beer Sheva") {
      this.origin = { lat: 31.263284, lng: 34.820021 };
    }
    
    console.log(this.ALERTS_DATA[this.maptype].alert_obj.latitude);
    var lat1: number = +this.ALERTS_DATA[this.maptype].alert_obj.latitude;
    var lon1: number = +this.ALERTS_DATA[this.maptype].alert_obj.longitude;
    this.destination = { lat: lat1, lng: lon1  };

    console.log(this.station)
    var _kCord = new google.maps.LatLng(this.origin.lat, this.origin.lng);
    var _pCord = new google.maps.LatLng(this.destination.lat, this.destination.lng);
    this.dis = google.maps.geometry.spherical.computeDistanceBetween(_kCord, _pCord).toFixed(3);
    this.time = (google.maps.geometry.spherical.computeDistanceBetween(_kCord, _pCord) / 50000);
    this.h = this.time.toFixed(3);
    this.m = ((this.time)*60).toFixed(3);
    console.log(this.dis);
    console.log(this.h);
    console.log(this.m);

  }



  changeDisabled(i: number): boolean{
    console.log(this.ALERTS_DATA[i].checked)
    this.ALERTS_DATA[i].checked = !this.ALERTS_DATA[i].checked;
    if(this.ALERTS_DATA[i].checked){
      this.ALERTS_DATA[i].details = "null"
      this.ALERTS_DATA[i].injured = 0
    }
    console.log(this.ALERTS_DATA[i].checked)
    return this.ALERTS_DATA[i].checked;
  }


  submitEvent(i: number): void{
      console.log(this.ALERTS_DATA[i] + "!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!!11")
      let alert = this.ALERTS_DATA[i]
      console.log(alert.alert_obj.rowKey)
      let is_false_alarm_str = this.ALERTS_DATA[i].checked.toString();
      this.url =  "/" + alert.alert_obj.rowKey + "/" + alert.country + "/" + alert.city + "/"
        + alert.alert_obj.partitionKey + "/" + alert.alert_obj.latitude + "/" + alert.alert_obj.longitude + "/" + alert.alert_obj.time + "/" + 
        is_false_alarm_str + "/" + alert.details.toString() + "/" + alert.injured;


      let event: Events = {
        city: alert.city,
        country: alert.country,
        device_id: alert.alert_obj.rowKey,
        email: alert.alert_obj.partitionKey,
        event_details: alert.details.toString(),
        id: +alert.alert_obj.rowKey,
        is_false_alarm: is_false_alarm_str,
        latitude: alert.alert_obj.latitude,
        longitude: alert.alert_obj.longitude,
        num_of_injured: alert.injured.toString(),
        time: Date.now.toString()
      }


      console.log(this.url)

      this.http.post(this.addEvent + this.url, this.httpOptions).toPromise()
      .catch(e =>
        this.errorSubmit = stringify(e));

      this.http.get<JSON>(this.deleteCurrent + "/" + alert.alert_obj.rowKey + "/" + alert.alert_obj.partitionKey
      , this.httpOptions) .subscribe(() => {
          console.log("delete alert");});

      this.http.get<JSON>(this.ClientDeleteMessage + alert.alert_obj.partitionKey + "/" + alert.alert_obj.rowKey
      , this.httpOptions) .subscribe(() => {
          console.log("delete alert client");});

      this.DeleteAlertFromArray(alert.alert_obj.partitionKey, alert.alert_obj.rowKey)

      this.UpdateHistory(event)
    }  
    
    public FalseAlarm(data: string): void {
      let dataarray = data.split('/')
      let email = dataarray[0]
      let deviceid = dataarray[1]
      console.log(email + deviceid)
      this.ALERTS_DATA.map((alert) => {
        if((alert.alert_obj.partitionKey === email) && (alert.alert_obj.rowKey === deviceid)){
            alert.alert_message = "false alarm!"
        }
      })
    }

    public menuClick(): void {
      if(this.mySidenav === "open-menu"){
        this.mySidenav = "close-menu";
      }
      else{
        this.mySidenav = "open-menu";
      }
    }
    

     
}







