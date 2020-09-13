# APP API

The API of the application consist of 4 main parts

## 1. Azure Tables

__There are 4 tables which defined as CS classes with all the relevant methods__
__All the tables are stored in our storage account in azure__
 

1. Client (Azure table)
* This table holds all the client details
* New row is created once a new user register
*fields:
PartitionKey = email
RowKey = email
other fields: password, name, phone number, favorite color

2. DetectorsEntities (Azure table)
* This table holds all the devices details (each client can have as many devices as he wants)
* New row is created once a user register add a device
*fields:
_PartitionKey_: email (of the owner)
_RowKey_: device_id (of the alerting device)
_Other fields_: device_name, address, version, longitude, latitude (To help allocte the device)

3. CurrentAlerts (Azure table)
* This table holds all the current alerts (i.e. an alert from device that not yet been closed)
* New row is created once a device detect smoke and trigger the EventHub
*fields:
_PartitionKey_: email (of the owner)
_RowKey_: device_id (of the alerting device)
_Other fields_: dlongitude, latitude (To help allocte the allert), time

4. DetectorEvents ([CosmosDB] (https://azure.microsoft.com/en-us/free/cosmos-db/search/?&ef_id=CjwKCAjw4_H6BRALEiwAvgfzqw9WW9fxTSj6UA5UpO5xQJisE-lBGOvZMojWhLVZLKxbpPsr8U12nRoCYIEQAvD_BwE:G:s&OCID=AID2100061_SEM_CjwKCAjw4_H6BRALEiwAvgfzqw9WW9fxTSj6UA5UpO5xQJisE-lBGOvZMojWhLVZLKxbpPsr8U12nRoCYIEQAvD_BwE:G:s))
* This table holds all the closed events (i.e. after the fire force took over and closed the event)
* New row is created once a fireman closes the alert 
* This table is a cosmusDB table for better analysis
*fields:
_PropertyName_: id (of the event)
_Other fields_: device_id, email (of the owner), country (locate by [Google.Maps API] (https://cloud.google.com/maps-platform/?utm_source=google&utm_medium=cpc&utm_campaign=FY18-Q2-global-demandgen-paidsearchonnetworkhouseads-cs-maps_contactsal_saf&utm_content=text-ad-none-none-DEV_c-CRE_267331616158-ADGP_Hybrid+%7C+AW+SEM+%7C+BKWS+~+EXA_%5BM:1%5D_EMEAOt_EN_Google+Maps+Brand-KWID_43700020520504203-aud-903284319780:kwd-298247230465-userloc_1008006&utm_term=KW_google%20maps%20api-ST_google+maps+api&gclid=CjwKCAjw4_H6BRALEiwAvgfzq4FvqWChDeNaNZrtkbuqI1qsNkDKQcJzvXInzEMXk86pCc0G7QRlLBoCMgUQAvD_BwE)
				is_false_alarm, event_details, num_of_injured, time

## 2. Azure Functions

__For each of the tables above, there are some azure HTTP Triggred Functions which__
__allow accessing the data, manipulate it and provide connection__

1. ClientFunctions
* Register - register a new client and add his details to the table
  Checks that all details are valid
* SignIn - Allowing a client to eneter the app and access his devices
  Check confirmation details by comparing queries
* ListDevices (Called after SignIn) - List all the devices of the owner (using _email_ as a PartitionKey)
* CheckColor - If forot password, this compare the color to the color
  the user had entered when register.
  Allow the user to recreate it's passwors
* UpdatePassword (Called if CheckColor is True) - changing in the _client_ table the _password_ field)

2. DetectorsEntitiesFunctions
* AddDevice - Adding a device to devices table when a user choose to
* GetDevice - Getting a specific device's details by it's id and owner's email
* FalseAlarm - Sending an __Azure SignalR__ massage to firmen that the alarm is false.
  User uses it from the application when he knows that the alarm is false
  In order that the firemen won't have to come
* Chat - Start chating (by __Azure SignalR__) with the firemen
* DeleteDevice - Deleting a device from the table

3. CurrentAlertsFunctions
* ClientGetCurrentAlerts - Showing the client all of the current alerts of his devices
  This function is being called by __Azure SignalR__ when a new alert is once
  Then the client can watch it on map and mark as false alarm
  
4. DetectorEventsFunctions
* ListEvents - For each device, a user can list all of its events and
  watch all of their deatils. Help him understand what are his more problematic areas
* GetEventsByEmailAndDeviceId - This is not an HTTP Function but a function
  That helps as query the table from __cosmosDB__ and make statistics

## 3. SignalR connection
__We used SignalR for many cases (as described in slides)__
__Here are some files that helps creating the connection__
* AzureSignalR - This _class_ define the azure signalR, the connection details
  (by parsing and find endpoints), accessToken exc..
* ConnectionRequest - this class represent a single connection		
  Fields: DeviceID, UserID
* SignalRFunctions - Create a __AzureSignalR__ object and -
    * negotiate - Creating the first connection for negotiation

## 4. Settings
* gitIgnore file
* local.settings.json - define the endpoints in our azure account
   * AzureWebJobsStorage
   * AzureWebJobsDashboard
   * EventHub
   * AzureSignalRConnectionString


