# A .NET wrapper for TimeTagger REST API

![build](https://github.com/michal-b-kaminski/TimeTaggerClient/actions/workflows/build-and-test.yml/badge.svg) ![publish](https://github.com/michal-b-kaminski/TimeTaggerClient/actions/workflows/publish.yml/badge.svg)

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

