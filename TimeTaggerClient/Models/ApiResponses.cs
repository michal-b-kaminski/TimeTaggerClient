using static System.Runtime.InteropServices.JavaScript.JSType;

namespace TimeTaggerClient.Models
{
#pragma warning disable IDE1006 // Naming Styles
    internal record ApiRecord( string key, long t1, long t2, string? ds, long mt, double st ) { }
    internal record ApiSetting( string key, object value, long mt, long st ) { }

    internal record ApiFetchRecordsResponse( ApiRecord[] records )
    {
        public override string ToString() => string.Join<ApiRecord>( Environment.NewLine, records );
    }

    internal record ApiFetchSettingsResponse( ApiSetting[] settings )
    {
        public override string ToString() => string.Join<ApiSetting>( Environment.NewLine, settings );
    }

    internal record ApiFetchNewResponse( long server_time, short reset, ApiRecord[] records, ApiSetting[] settings )
    {
        public override string ToString() => $"ST: {server_time}, Reset: {reset}{Environment.NewLine}Records:{Environment.NewLine}{string.Join<ApiRecord>( Environment.NewLine, records )}{Environment.NewLine}Settings:{Environment.NewLine}{string.Join<ApiSetting>( Environment.NewLine, settings )}";
    }

    internal record ApiUpdateResponse( string[] accepted, string[] failed, string[] errors )
    {
        public override string ToString() => $"accepted:{Environment.NewLine}{string.Join( Environment.NewLine, accepted )}{Environment.NewLine}failed:{Environment.NewLine}{string.Join( Environment.NewLine, failed )}{Environment.NewLine}errors:{Environment.NewLine}{string.Join( Environment.NewLine, errors )}{Environment.NewLine}";
    }
#pragma warning restore IDE1006 // Naming Styles
}
