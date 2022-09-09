namespace Dashboard.Connection.BoldReportsWebConnection.Json
{
    using Newtonsoft.Json;
    using Newtonsoft.Json.Serialization;
    using Syncfusion.Dashboard.Core.JsonRepository;
    public class JsonBoldReportsConnection : JsonConnectionBase
    {
        [JsonProperty("url")]
        public string WebUrl
        {
            get;
            set;
        }

        [JsonProperty("username")]
        public string Username
        {
            get;
            set;
        }

        [JsonProperty("password")]
        public string Password
        {
            get;
            set;
        }

        [JsonProperty("isBoldReportsconnection")]
        public bool IsBoldReportsConnection
        {
            get;
            set;
        }

        [JsonProperty("methodtype")]
        public string MethodType
        {
            get;
            set;
        }

        [JsonProperty("jsondata")]
        public object JsonData
        {
            get;
            set;
        }

        [JsonProperty("oauthproviderinfo")]
        public object OAuthProviderInfo
        {
            get;
            set;
        }

        [JsonProperty("targetProviderType")]
        public string TargetProviderType
        {
            get;
            set;
        }

        [JsonProperty("targetConnectionString")]
        public string TargetConnectionString
        {
            get;
            set;
        }
    }
}
