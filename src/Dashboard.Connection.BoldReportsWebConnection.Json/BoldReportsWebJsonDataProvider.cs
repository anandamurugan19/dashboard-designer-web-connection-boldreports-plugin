namespace Dashboard.Connection.BoldReportsWebConnection.Json
{
    using Dashboard.Connection.BoldReports.Model;
    //using Model;
    using Syncfusion.Dashboard.Core;
    using Syncfusion.Dashboard.Core.JsonRepository;

    public class BoldReportsWebJsonDataProvider : IJsonProvider
    {
        public T ToJson<T, U>(U model)
            where T : class
        {
            return (model as BoldReportsWebConnection)?.ToJson() as T;
        }

        public T ToModel<T, U>(U json, DashboardContainer dashboard)
            where T : class
        {
            return (json as JsonBoldReportsConnection)?.ToModelIsBoldReportsWebConnection(dashboard) as T;
        }
    }
}
