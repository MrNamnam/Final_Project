# APP UI

__Each page in thhe app consist of two files__:
* xaml file - define the UI of the page
* xaml.cs file - define the logic behind the page

## Pages 
__Those pages uses the API by calling their enpoints__

### 1. Chat
* The chat paged is being opend once the user press the button
  Of chating the firemen after ther is an alert on one of his devices
  However, it can also be opened when the firemen send him a massage
* There are _Chat_ objects which consist of _Massage_ objects
* Uses [__AzureSignalR__] (https://azure.microsoft.com/en-us/services/signalr-service/)

### 2. EventsHistory
* The user can see for each device the history of it's events
  This page is opened when the user choose a device and press _History_
* The history is being taken from the _events table_
* There are _EventHistory_ objects which consist of _Event_ objects

### 3. LoginPage
* User can enter its details and then by checking on _clients table_ 
  he enter the UserListPage
* There is also a hidden page here of _foret password_

  
### 4. Registraion page
* Here the user can fill his details and then it check in the __clients table__
  In order to register him
* Also validate the fields

### 5. UserListPage
* This page is the most important page for the user.
  He can:
  * Manage devices
  * Wacth alerts, start chating and mark false alarm
  * Show devices on map
* This is the main page after reopening the app

## Maps
* This allow to show the devices on map
* Uses google api for nevigationg and showing

## MainPage
* The main page of the app
* If the user exit the app and then back without closing it,
  it will not return here but to the page of devices for better listening
* Can chhose login/ register

## Android + IOS
* Those directories contain all the relevant files to allow the user
  use the app from those operating ststems
* The IOS does not support Maps because it nedded another account