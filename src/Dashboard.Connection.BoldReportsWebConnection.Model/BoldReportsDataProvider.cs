//using Syncfusion.Dashboard.Connection.LiveWebConnection.Model;
namespace Dashboard.Connection.BoldReportsWebConnection.Model
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data;
    using System.Linq;
    using System.Threading;
    using System.Threading.Tasks;
    using Dashboard.Connection.BoldReports.Model;
    using Newtonsoft.Json;
    using Newtonsoft.Json.Linq;
    using Syncfusion.Dashboard.Connection.BoldReportsWebConnection.Model;
    using Syncfusion.Dashboard.Core;
    using Syncfusion.Dashboard.Core.Common;
    using Syncfusion.Dashboard.Core.Connection;
    using Syncfusion.Dashboard.Core.Connection.Schema;
    using Syncfusion.Dashboard.Core.DataSources;
    using Syncfusion.Dashboard.Core.Helpers;
    using Syncfusion.Dashboard.Core.Helpers.Security;
    using Syncfusion.Dashboard.Designer.Logger;
    using Syncfusion.Dashboard.Web.Data.Handler;
    using Syncfusion.Dashboard.Web.Data.Handler.DataProviders;

    public class BoldReportsDataProvider
    {
        public BoldReportsWebConnection Connection { get; set; }

        public ConnectionBase requiredConnection { get; set; }
        public DashboardContainer dashboardObject { get; set; }
        public BoldReportsDataProvider(BoldReportsWebConnection connection, DashboardContainer container)
        {
            this.Connection = connection;
            this.dashboardObject = container;
        }

        public async Task<QueryResult> GetResultDataAsync(QueryObject query, CancellationToken cancellationToken)
        {
            var result = new QueryResult() { Limit = query.MaximumRows, StartIndex = query.StartIndex };
            result = await GetDataTableForLiveModeAsync(query);
            return result;
        }

        public ObservableCollection<DataTable> ResultTables { get; set; }


        public async Task GetSchemaHierachyAsync(string id, string tableName)
        {
            try
            {
                List<SelectedTableSchema> selectedTableSchemas = new List<SelectedTableSchema>();
                Syncfusion.Dashboard.Web.Serialization.Model.DataSources.DataSource dataSource = new Syncfusion.Dashboard.Web.Serialization.Model.DataSources.DataSource();
                BoldReportsWebConnection requiredConnection = Connection;
                string inputString = Connection.JsonData.ToString();
                if (!string.IsNullOrEmpty(id) && IsLiveConnectionPlugin(id))
                {
                    requiredConnection = this.dashboardObject.Connections?.FirstOrDefault(i => i.Id == id) as BoldReportsWebConnection;
                    
                }
                string selectedSchemas = (await new DBServiceHelper(null)?.GetSelectedSchemaForLiveWebFromDatasourceAsync(this.dashboardObject.DataSources.Get(requiredConnection.Id)?.OriginalDataSource))?.ToString();
                selectedTableSchemas = !string.IsNullOrEmpty(selectedSchemas) ? await this.GetSelectedSchemasFromDatasourceAsync(selectedSchemas) : await this.GetSelectedSchemasFromJsonStringAsync(inputString);
                dataSource = JsonConvert.DeserializeObject<Syncfusion.Dashboard.Web.Serialization.Model.DataSources.DataSource>(SetCommandTimeOut(requiredConnection.JsonData.ToString()));
                object ProviderInfo = null;
				if(dataSource.OAuthConnection != null)
				{
					//ProviderInfo = new DashboardDesignerHelper().FillOAuthDetailsForLiveWebConnection(Connection.WebDataSourceObject);
				}
                var dataTable =(await new DBServiceHelper(null).GetDataForLiveWebConnectionAsync(dataSource, "datatable", ProviderInfo, this.dashboardObject, selectedTableSchemas)) as DataTable;
                ResultTables = ResultTables != null ? ResultTables : new ObservableCollection<DataTable>();
                if (dataSource != null && dataSource.ProviderType.ToUpperInvariant() == "MARKUPTRACE")
                {
                    dataTable.TableName = "Result";
                }
                else
                {
                    dataTable.TableName = tableName;

                }
                ResultTables.Add(dataTable);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// method to get selected table schema for live if the selected schema is null
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        private async Task<List<SelectedTableSchema>> GetSelectedSchemasFromJsonStringAsync(string inputString)
        {
            List<SelectedTableSchema> selectedTableSchemas = new List<SelectedTableSchema>();
            JObject obj = JObject.Parse(inputString);
            if (obj["SelectedSchemasForLiveWeb"] != null)
            {
                selectedTableSchemas = JsonConvert.DeserializeObject<List<SelectedTableSchema>>(obj["SelectedSchemasForLiveWeb"].ToString());
            }
            return selectedTableSchemas;
        }

        /// <summary>
        /// Method to get selected table schema for live if the selected schema is not null
        /// </summary>
        /// <param name="inputString"></param>
        /// <returns></returns>
        private async Task<List<SelectedTableSchema>> GetSelectedSchemasFromDatasourceAsync(string inputString)
        {
            List<SelectedTableSchema> selectedTableSchemas = null;
            if (!string.IsNullOrEmpty(inputString))
            {
                selectedTableSchemas = JsonConvert.DeserializeObject<List<SelectedTableSchema>>(inputString);
            }
            return selectedTableSchemas;
        }

        private bool IsLiveConnectionPlugin(string id)
        {
            return (this.dashboardObject.Connections?.FirstOrDefault(i => i.Id == id)?.PluginUID != null) &&
                    this.dashboardObject.Connections?.FirstOrDefault(i => i.Id == id)?.PluginUID == new Guid("dc8b01ad-9970-4dab-a066-499ec13a6c26");
        }

        private async Task<QueryResult> GetDataTableForLiveModeAsync(QueryObject queryObj)
        {
            QueryObject clonedQueryObject = queryObj;
            try
            {
                requiredConnection = !string.IsNullOrEmpty(Connection.TargetProviderType) ? this.GetConnection(Connection.TargetProviderType) : this.GetConnection(TargetServerEnum.SQLite);
                if (!string.IsNullOrEmpty(Connection.TargetProviderType))
                {
                    GettingSchemaNameForQueryObject(queryObj);
                    GettingDefaultColumnnameForFilterFields(queryObj);
                }

                QueryResult queryResult = requiredConnection.GetDataTableAsync(queryObj).Result.Result;
                LogHandler.LogInfo(string.Format("\nRows:{1} Columns:{2}\nSchema:{0} Query Executed:{3}\n", queryObj?.Tables.FirstOrDefault()?.SchemaName, queryResult?.Data?.Rows?.Count, queryResult?.Data?.Columns?.Count, !string.IsNullOrEmpty(queryResult?.queryString) ? "True": "False"));
                return queryResult;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }

        /// <summary>
        /// Assigning shcmea name for Query object tables.
        /// </summary>
        private void GettingSchemaNameForQueryObject(QueryObject queryObj)
        {
            if (queryObj != null && queryObj.Tables.Count > 0)
            {
                foreach (var table in queryObj.Tables)
                {
                    switch(Connection.TargetProviderType)
                    {
                        case TargetServerEnum.PostgresSQL:
                            {
                                table.SchemaName = Connection.IsViewMode && !string.IsNullOrEmpty(Connection.DatasourceCallerId) ? (Connection.Id + "_" + Connection.DatasourceCallerId).Substring(0, 63) : Connection.Id;
                                break;
                            }

                        default:
                            {
                                table.SchemaName = Connection.Id;
                                break;
                            }
                    }
                }
            }
        }

        /// <summary>
        /// Assigning shcmea name for Dataset tables.
        /// </summary>
        internal void GettingSchemaNameForDataSetObject(DataSource dataset)
        {
            if (dataset != null && dataset.Tables.Count > 0)
            {
                foreach (var table in dataset.Tables)
                {
                    switch (Connection.TargetProviderType)
                    {
                        case TargetServerEnum.PostgresSQL:
                            {
                                table.Schema = Connection.IsViewMode && !string.IsNullOrEmpty(Connection.DatasourceCallerId) ? (Connection.Id + "_" + Connection.DatasourceCallerId).Substring(0, 63) : Connection.Id;
                                break;
                            }
                        default:
                            {
                                table.Schema = Connection.Id;
                                break;
                            }
                    }
                }
            }
        }

        /// <summary>
        /// Assigning default columnname for filter fields.
        /// </summary>
        private void GettingDefaultColumnnameForFilterFields(QueryObject queryObject)
        {
            if (queryObject?.UrlFilterFields != null && queryObject?.UrlFilterFields.Count > 0)
            {
                foreach (var UrlFilterFields in queryObject.UrlFilterFields)
                {
                    UrlFilterFields.DefaultColumnName = dashboardObject?.DataSources?.Get(Connection.Id)?.Tables[0]?.Fields?.FirstOrDefault(i => i.Name == UrlFilterFields.ColumnName)?.DefaultColumnName;
                }
            }
            if (queryObject?.AggregateFields != null && queryObject?.AggregateFields.Count > 0)
            {
                foreach (var AggregateFields in queryObject.AggregateFields)
                {
                    AggregateFields.DefaultColumnName = dashboardObject?.DataSources?.Get(Connection.Id)?.Tables[0]?.Fields?.FirstOrDefault(i => i.Name == AggregateFields.ColumnName)?.DefaultColumnName;
                }
            }
            if (queryObject?.IsolationFilterFields != null && queryObject?.IsolationFilterFields.Count > 0)
            {
                foreach (var IsolationFilterFields in queryObject.IsolationFilterFields)
                {
                    IsolationFilterFields.DefaultColumnName = dashboardObject?.DataSources?.Get(Connection.Id)?.Tables[0]?.Fields?.FirstOrDefault(i => i.Name == IsolationFilterFields.ColumnName)?.DefaultColumnName;
                }
            }
            if (queryObject?.MainFilterFields != null && queryObject?.MainFilterFields.Count > 0)
            {
                foreach (var MainFilterFields in queryObject.MainFilterFields)
                {
                    MainFilterFields.DefaultColumnName = dashboardObject?.DataSources?.Get(Connection.Id)?.Tables[0]?.Fields?.FirstOrDefault(i => i.Name == MainFilterFields.ColumnName)?.DefaultColumnName;
                }
            }
            if (queryObject?.UserBasedFilterFields != null && queryObject?.UserBasedFilterFields.Count > 0)
            {
                foreach (var UserBasedFilterFields in queryObject.UserBasedFilterFields)
                {
                    UserBasedFilterFields.DefaultColumnName = dashboardObject?.DataSources?.Get(Connection.Id)?.Tables[0]?.Fields?.FirstOrDefault(i => i.Name == UserBasedFilterFields.ColumnName)?.DefaultColumnName;
                }
            }
        }

        public async Task<ConnectionSchema> GetConnectionSchemaAsync(LazySchemaCategory category, SchemaDetails schemaDetails)
        {
            string parentSchemaName = string.Empty;
            if (schemaDetails != null && !string.IsNullOrEmpty(schemaDetails.schemaName))
            {
                parentSchemaName = schemaDetails.schemaName;
            }
            await GetSchemaHierachyAsync(parentSchemaName, schemaDetails != null && schemaDetails.tableName != null ? schemaDetails.tableName : "Result");
            ConnectionSchema cs = new ConnectionSchema(Connection.Id);
            SchemaCategory sc = new SchemaCategory(cs) { Id = "tables" };
            List<TreeViewInfo> tableList = new List<TreeViewInfo>();
            foreach (var schema in ResultTables)
            {
                SchemaTable spt = new SchemaTable(sc)
                {
                    Id = schema.TableName,
                    Schema = parentSchemaName
                };


                BuildFields(schema.Columns.Cast<DataColumn>()?.ToList(), spt, parentSchemaName);
                if (category == LazySchemaCategory.Tables)
                {
                    foreach (DataColumn dataColumn in schema.Columns)
                    {
                        tableList.Add(new TreeViewInfo() { Name = dataColumn.ColumnName, DataType = dataColumn.DataType.Name.Equals("Object") ? "String" : dataColumn.DataType.Name });
                    }
                }
                else
                {
                    tableList.Add(new TreeViewInfo() { Name = schema.TableName });
                }
            }
            cs.LazyResults = tableList;
            return cs;
        }

        List<SchemaField> fields = new List<SchemaField>();
        public List<SchemaField> BuildFields(List<DataColumn> columns, SchemaTable spt, string parentSchemaName)
        {
            foreach (var schema in columns)
            {
                string dsId = string.IsNullOrEmpty(parentSchemaName) ? Connection.Id : parentSchemaName;
                DataSource dataSource = dashboardObject.DataSources.Get(dsId);
                SchemaField field = new SchemaField(spt)
                {
                    Id = schema.ColumnName,
                    Type = new DataTypeInfo(schema.DataType.ToSimplified(), schema.DataType.Name.ToString().ToLower()),
                    DefaultColumnName = dataSource != null && dataSource.Tables.Count > 0 ? dataSource.Tables.FirstOrDefault(i => i.ConnectionId == dsId)?.Fields.FirstOrDefault(i => i.Name == schema.ColumnName)?.DefaultColumnName : string.Empty
                };
                fields.Add(field);
            }
            
            return fields;
        }

        public ConnectionBase GetConnection(string target)
        {
            ConnectionBase requiredConnection = null;
            Dictionary<string, string> con = new Dictionary<string, string>();
            target = !string.IsNullOrEmpty(Connection.TargetProviderType) ? target : TargetServerEnum.SQLite.ToString();
            switch (target)
            {
                case TargetServerEnum.SqlServer:
                    {
                        requiredConnection = CoreConfiguration.Instance.ConnectionFactory.GetPlugin(ConnectionInformation.GetConnectionId(TargetServerEnum.SqlServer)).BuildNewConnection(this.dashboardObject);
                        requiredConnection.TargetConnectionString = TokenCryptoHelper.DoDecryption(Connection.TargetConnectionString);
                        requiredConnection.TargetProviderType = Connection.TargetProviderType;
                        con.Add("ConnectionString", requiredConnection.TargetConnectionString);
                        con.Add("ParentSchemaName", Connection.Id);
                        requiredConnection.IsExtract = true;
                        break;
                    }

                case TargetServerEnum.PostgresSQL:
                    {
                        requiredConnection = CoreConfiguration.Instance.ConnectionFactory.GetPlugin(ConnectionInformation.GetConnectionId(TargetServerEnum.PostgresSQL)).BuildNewConnection(this.dashboardObject);
                        requiredConnection.TargetConnectionString = TokenCryptoHelper.DoDecryption(Connection.TargetConnectionString);
                        requiredConnection.TargetProviderType = Connection.TargetProviderType;
                        con.Add("ConnectionString", requiredConnection.TargetConnectionString);
                        con.Add("ParentSchemaName", Connection.IsViewMode && !Connection.IsCustomQuery && !string.IsNullOrEmpty(Connection.DatasourceCallerId) ? (Connection.Id + "_" + Connection.DatasourceCallerId).Substring(0, 63) : Connection.Id);
                        requiredConnection.IsExtract = true;
                        break;
                    }

                case TargetServerEnum.MySQL:
                    {
                        requiredConnection = CoreConfiguration.Instance.ConnectionFactory.GetPlugin(ConnectionInformation.GetConnectionId(TargetServerEnum.MySQL)).BuildNewConnection(this.dashboardObject);
                        requiredConnection.TargetConnectionString = TokenCryptoHelper.DoDecryption(Connection.TargetConnectionString);
                        requiredConnection.TargetProviderType = Connection.TargetProviderType;
                        con.Add("ConnectionString", requiredConnection.TargetConnectionString);
                        con.Add("ParentSchemaName", Connection.Id);
                        requiredConnection.IsExtract = true;
                        break;
                    }

                default:
                    {
                        requiredConnection = CoreConfiguration.Instance.ConnectionFactory.GetPlugin(ConnectionInformation.GetConnectionId(TargetServerEnum.SQLite)).BuildNewConnection(this.dashboardObject);
                        con.Add("Datasource", GetDataSource());
                        con.Add("IsLiveConnection", "true");
                        break;
                    }
            }

            requiredConnection.PortConnection(con);
            return requiredConnection;
        }
        //For build version Above 3.3.88  the "commandtimeout" value was came as null.To overcome that we handle the code changes here
        public string SetCommandTimeOut(string jsonData)
        {
#if onpremiseboldbi
            var webDataSourceObject = JObject.Parse(jsonData);
            if(!string.IsNullOrEmpty(webDataSourceObject["ConnectionProperties"].ToString()) && string.IsNullOrEmpty(webDataSourceObject["ConnectionProperties"]["CommandTimeout"].ToString()))
            {
                webDataSourceObject["ConnectionProperties"]["CommandTimeout"] = 0;
            }
            return webDataSourceObject?.ToString();
#else
            return jsonData;
#endif
        }

        public string GetDataSource()
        {
            string jsonString = SetCommandTimeOut(Connection.JsonData?.ToString());
            var webDataSourceObject = JsonConvert.DeserializeObject<Syncfusion.Dashboard.Web.Serialization.Model.DataSources.DataSource>(jsonString);
            return !string.IsNullOrEmpty(webDataSourceObject.FilePath) ? webDataSourceObject.FilePath : ":memory:";
        }
    }
}
