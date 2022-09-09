namespace Dashboard.Connection.BoldReportsWebConnection
{
    using System;
    using Dashboard.Connection.BoldReports.Model;
    using Json;
    //using Model;
    using Syncfusion.Dashboard.Core;
    using Syncfusion.Dashboard.Core.Connection;
    using Syncfusion.Dashboard.Core.Plugin;

    public class BoldReportsPlugin: ConnectionPluginBase
    {
        public BoldReportsPlugin()
        {
        }
        public override string CategoryName => "Database";

        public override int SortIndex => 2;

        public override bool IsRelational => true;

        public override Guid UID => new Guid("dc8b01ad-9970-4dab-a066-499ec13a6c26");

        public override string Name => "BoldReports Web Connection";

        public override string Author => "Syncfusion";

        public override string Description => "BoldReports Web Connection Sample";

        // TODO good candidate for IoC
        public override object DataProvider => new BoldReportsWebJsonDataProvider();

        public override ConnectionBase BuildNewConnection(DashboardContainer container)
        {
            return new BoldReportsWebConnection(container, UID, Guid.NewGuid().ToString("N"), "BoldReports Connection")
            {
                WebUrl = string.Empty
            };
        }
    }
}
