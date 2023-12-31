# A .NET wrapper for TimeTagger REST API

![build](https://github.com/michal-b-kaminski/TimeTaggerClient/actions/workflows/build-and-test.yml/badge.svg) ![publish](https://github.com/michal-b-kaminski/TimeTaggerClient/actions/workflows/publish.yml/badge.svg)

Allows to easily integrate TimeTagger into your .NET application. Supports all official endpoints offered by the TimeTagger REST API.

## Usage

Initialize library with your TimeTagger API address and your accounts API Key:

```csharp
using var client = new TimeTaggerClient("http://localhost/timetagger/api/v2/", "api-key");
```

## Supported endpoints

- Fetch records: `GET ./records?timerange=timestamp-timestamp`: `ITimeTaggerClient.FetchRecords`
- Create/update records: `PUT ./records`: `ITimeTaggerClient.UpdateRecords`
- Fetch settings: `GET ./settings`: `ITimeTaggerClient.FetchSettings`
- Create/update records: `PUT ./settings`: `ITimeTaggerClient.UpdateSettings`
- Incremental fetch of new/modified records and settings since last check (by server time): `GET ./updates?since=timestamp`: `ITimeTaggerClient.FetchNew`

## Models

### `TimeTaggerRecord`

Models TimeTagger record.

#### Properties

- `Key`: identifier of the record, generated by user, must be globally unique
- `Start`: start of the record
- `End`: end of the record, if it is equal to `Start` then the record is treated as in progress
- `Description`: full description of records, with all tags
- `Tags`: (calculated) list of tags assigned to the record
- `ModifiedTime`: modification time of the record, assigned by the user
- `ServerTime`: server time of the record, set by the server. Should be left empty for new records
- `Duration`: (calculated) duration of the record
- `DurationS`: (calculated) duration of the record in seconds

### Setting

Models TimeTagger setting. Settings can be used by clients to store arbitrary data on the server. Good policy is to use name of the client as part of the setting key.

#### Properties

- `Key`: identifier of the setting, generated by user, must be globally unique; should be prefixed with client name
- `Value`: value of the setting
- `ModifiedTime`: modification time of the setting, assigned by the user
- `ServerTime`: server time of the setting, set by the server. Should be left empty for new settings