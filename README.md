# A .NET wrapper for TimeTagger REST API

Allows to easily integrate TimeTagger into your .NET application. Supports all official endpoints offered by the TimeTagger REST API.

## Endpoints:

- `GET ./records?timerange=timestamp-timestamp`: `ITimeTaggerClient.FetchRecords`
- `PUT ./records`: `ITimeTaggerClient.UpdateRecords`
- `GET ./settings`: `ITimeTaggerClient.FetchSettings`
- `PUT ./settings`: `ITimeTaggerClient.UpdateSettings`
- `GET ./updates?since=timestamp`: `ITimeTaggerClient.FetchNew`

## Usage

Initialize library with your TimeTagger API address and your accounts API Key:

```csharp
using var client = new TimeTaggerClient("http://localhost/timetagger/api/v2/", "api-key");
```

