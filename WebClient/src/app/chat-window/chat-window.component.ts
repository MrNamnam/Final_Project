import { Component, OnInit, Input, EventEmitter, Output } from '@angular/core';
import { HttpClient, HttpHeaders } from "@angular/common/http";
import * as signalR from "@aspnet/signalr";
import { map, switchMap } from "rxjs/operators";
import { Time } from '@angular/common';

interface SignalRConnection {
  url: string;
  accessToken: string;
}

interface CurrentAlertsObject {
  PartitionKey: string;
  RowKey: string;
  latitude: string;
  longitude: string;
}
interface Message {
  msg: string;
  time: string;
  sender: string;
}

@Component({
  selector: 'chat-window',
  templateUrl: './chat-window.component.html',
  styleUrls: ['./chat-window.component.less']
})
export class ChatWindowComponent implements OnInit {
  @Input() user: string;
  @Output() public closechatfunck: EventEmitter<any> = new EventEmitter();
  private readonly prefixUrlcosmos = "https://webfunc20200824154225.azurewebsites.net";
  private readonly httpOptions = { headers: new HttpHeaders({ "Content-Type": "application/json" }) };
  private readonly negotiateUrl = this.prefixUrlcosmos + "/api/negotiate";
  private readonly WebSendMessage = this.prefixUrlcosmos + "/api/web-send-message/";
  private hubConnection: signalR.HubConnection;
  private messages: Message[] = [];
  private webcompmessage: string = "";
  public minimized: boolean = false;
  constructor(private readonly http: HttpClient) {
    const negotiateBody = { UserId: "FireWebChat" };
    
   
      this.http
      .post<SignalRConnection>(this.negotiateUrl, JSON.stringify(negotiateBody), this.httpOptions)
      .pipe(
        map(connectionDetails =>
          new signalR.HubConnectionBuilder().withUrl(`${connectionDetails.url}`, { accessTokenFactory: () => connectionDetails.accessToken }).build(),
        )
    ).subscribe(hub => {
        this.hubConnection = hub;
        hub.on(this.user, data => {
          console.log(data)
          const date: Date = new Date()
          const time:string = date.getHours() +":" + this.Pad(date.getMinutes())
          const message : Message = {msg: data, time: time, sender: this.user}
          this.messages = [...this.messages, message]
          console.log("SignalR was activated")
          ;
        });
        hub.start();
      });

   }

  ngOnInit(): void {
  }

  public SendMessage(): void {
    if(this.webcompmessage === ""){
      return;
    }
    else{
      this.http.get<JSON>(this.WebSendMessage + this.user + "/" + this.webcompmessage, this.httpOptions).subscribe();
      const date: Date = new Date()
      const time:string = date.getHours() +":" + this.Pad(date.getMinutes())
      const message : Message = {msg: this.webcompmessage, time: time, sender: "me"}
      this.messages = [...this.messages, message]
      this.webcompmessage = ""
      console.log(message)
    }
}

  public Pad(d): number {
    return (d < 10) ? '0' + d.toString() : d.toString();
}

public closeChat(){
  this.hubConnection.stop;
  console.log(this.user)
  this.closechatfunck.emit(this.user)
}
  
}


