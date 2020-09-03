import { Component, OnInit } from '@angular/core';
import * as CanvasJs from '../canvasjs-2.3.2/canvasjs.min';
import { HttpClient, HttpHeaders } from "@angular/common/http";

interface eventsDay {
  // x: number;
  y: number;
}

interface injuredcount {
  // x: number;
  label: string
  y: number;
}

interface citycount {
  // x: number;
  name: string;
  y: number;
}



@Component({
  selector: 'app-monitor-charts',
  templateUrl: './monitor-charts.component.html',
  styleUrls: ['./monitor-charts.component.less']
})
export class MonitorChartsComponent implements OnInit {
  private readonly prefixUrlcosmos = "https://webfunc20200824154225.azurewebsites.net";
  
  private readonly httpOptions = { headers: new HttpHeaders({ "Content-Type": "application/json" }) };
  private readonly aggInjuredCount = this.prefixUrlcosmos + "/api/agg-injured--count"
  private readonly aggEventsDay = this.prefixUrlcosmos + "/api/agg-event-day"
  private readonly aggCityCount = this.prefixUrlcosmos + "/api/agg-city-count"

  public EventsDayCount:  eventsDay[] = [];
  public InjuredCount:  injuredcount[] = [];
  public CityCount:  citycount[] = [];

  constructor(private readonly http: HttpClient) {
    // this.http.post<JSON>(this.aggEventsDay, this.httpOptions).subscribe(eventsDaysArray => {
    //   console.log(eventsDaysArray)
    //   for (let key in eventsDaysArray){
    //     this.EventsDayCount.push({y: eventsDaysArray[key]["y"]})
    //   }
    //   console.log(this.EventsDayCount)
      // this.lonlatArr = lonlat["0"]["address"];
      //  this.CreateHistoryAlerts(lonlat["0"])
      // console.log(this.lonlatArr);
      // console.log(this.mapLonLatArr);

    // });

   }

  ngOnInit(): void {
    this.Charts()
  }
  
  Charts(){
    let chart_col = new CanvasJs.Chart("chartContainerCol", {
      animationEnabled: true,
      exportEnabled: true,
      title: {
        text: "Total events for each number of injured"
      },
      data: [{
        type: "column",
        dataPoints: this.InjuredCount
      }]
    });

    this.http.post<JSON>(this.aggInjuredCount, this.httpOptions).subscribe(InjuredCountArray => {
      console.log(InjuredCountArray)
      for (let key in InjuredCountArray){
        this.InjuredCount.push({y: InjuredCountArray[key]["y"], label: InjuredCountArray[key]["label"]})
        // ,x:  +(eventsDaysArray[key]["x"].substring(5,7).concat(eventsDaysArray[key]["x"].substring(8,10)))})
      }
      console.log(this.InjuredCount)
      chart_col.render();
    });
    

    let chart_cake = new CanvasJs.Chart("chartContainerCake", {
      theme: "light2",
      animationEnabled: true,
      exportEnabled: true,
      title:{
        text: "City total events cake charts"
      },
      data: [{
        type: "pie",
        showInLegend: true,
        toolTipContent: "<b>{name}</b>: ${y} (#percent%)",
        indexLabel: "{name} - #percent%",
        dataPoints: this.CityCount
      }]
    });

    this.http.post<JSON>(this.aggCityCount, this.httpOptions).subscribe(CityCountArray => {
      console.log(CityCountArray)
      for (let key in CityCountArray){
        this.CityCount.push({y: CityCountArray[key]["y"], name: CityCountArray[key]["name"]})
        // ,x:  +(eventsDaysArray[key]["x"].substring(5,7).concat(eventsDaysArray[key]["x"].substring(8,10)))})
      }
      console.log(this.CityCount)
      chart_cake.render();
    });

    

      

    let chart_plot = new CanvasJs.Chart("chartContainerPlot", {
      exportEnabled: true,
		  title:{
			  text:"Number of events during last month"
		  },
		  data: [{
			  type: "spline",
			  dataPoints : this.EventsDayCount,
		  }]
	  });
	
    this.http.post<JSON>(this.aggEventsDay, this.httpOptions).subscribe(eventsDaysArray => {
      console.log(eventsDaysArray)
      for (let key in eventsDaysArray){
        this.EventsDayCount.push({y: eventsDaysArray[key]["y"]})
        // ,x:  +(eventsDaysArray[key]["x"].substring(5,7).concat(eventsDaysArray[key]["x"].substring(8,10)))})
      }
      console.log(this.EventsDayCount)
      chart_plot.render();
    });
  }

}
