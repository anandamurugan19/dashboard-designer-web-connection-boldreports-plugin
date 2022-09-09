namespace Dashboard.Connection.BoldReportsWebConnection.Json
{
    using Dashboard.Connection.BoldReports.Model;
    using System;
    //using Model;
    using Core.JsonRepository;
    using Syncfusion.Dashboard.Core;
    using Syncfusion.Dashboard.Core.Helpers;

    internal static class Extension
    {
        public static JsonBoldReportsConnection ToJson(this BoldReportsWebConnection model)
        {
            return model == null ? null : new JsonBoldReportsConnection
            {
                PluginUID = model.PluginUID,
                IsBoldReportsConnection = model.IsBoldReportsConnection,
                Id = model.Id,
                JsonData = model.JsonData,
                Name = model.Name,
                OAuthProviderInfo = model.OAuthProviderInfo,
                TargetProviderType = model.TargetProviderType,
                TargetConnectionString = model.TargetConnectionString
            };
        }

        public static BoldReportsWebConnection ToModelIsBoldReportsWebConnection(this JsonBoldReportsConnection model, DashboardContainer container)
        {
            return model == null ? null : new BoldReportsWebConnection(container, new Guid("dc8b01ad-9970-4dab-a066-499ec13a6c26"), model.Id, model.Name)
            {
                IsBoldReportsConnection = model.IsBoldReportsConnection,
                JsonData = model.JsonData,
                Name = model.Name,
                OAuthProviderInfo = model.OAuthProviderInfo,
                TargetProviderType = model.TargetProviderType,
                TargetConnectionString = model.TargetConnectionString
            };
        }
    }
}
