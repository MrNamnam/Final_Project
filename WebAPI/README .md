# WEB API

The API of the application consist of 4 main parts

## 1. Azure Tables (same as APP API)


## 2. Azure Functions

__For each of the tables above, there are some azure HTTP Triggred Functions which__
__allow accessing the data, manipulate it and provide connection__

1. ClientFunctions
*  findMatchClientToAlert - get an email of a client and return its data from Client table


3. CurrentAlertsFunctions
*  GetCurrentAlerts - return all the alerts in the table CurrentAlerts
*  UpdateAlertTable - delete an alert which was submitted
  
4. DetectorEventsFunctions
*  GetHistoryEvents - return all events ever submitted
*  UpdateEventTable - add a new enent to Events table
*  AggEventsEachDay - return an array of EventdayCount. EventdayCount.x is the day and EventdayCount.y is number of event submitted
*  NumOfInjuredCount - return array of Objects that contain number of injured and how many events happened with this number of injured
*  NumOfEventsPerCity - return number of events in each city

## 3. SignalR connection
__We used SignalR for many cases (as described in slides)__
__Here are some files that helps creating the connection__
* AzureSignalR - This _class_ define the azure signalR, the connection details
  (by parsing and find endpoints), accessToken exc..
* ConnectionRequest - this class represent a single connection		
  Fields: DeviceID, UserID
* SignalRFunctions - Create a __AzureSignalR__ object and -
    * negotiate - Creating the first connection for negotiation
*  clientDeleteMessage - send signalR which update client alert was submitted.

## 4. Settings
* gitIgnore file
* local.settings.json - define the endpoints in our azure account
   * AzureWebJobsStorage
   * AzureWebJobsDashboard
   * EventHub
   * AzureSignalRConnectionString


