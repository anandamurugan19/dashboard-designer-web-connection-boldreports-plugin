namespace Dashboard.Connection.BoldReports.Model
{
    using Dashboard.Connection.BoldReportsWebConnection.Model;
    using Newtonsoft.Json;
    using Syncfusion.Dashboard.Connection.BoldReportsWebConnection.Model;
    using Syncfusion.Dashboard.Core;
    using Syncfusion.Dashboard.Core.Connection;
    using Syncfusion.Dashboard.Core.Connection.Schema;
    using Syncfusion.Dashboard.Core.DataSources;
    using Syncfusion.Dashboard.Core.Exceptions;
    using Syncfusion.Dashboard.Core.Helpers;
    using Syncfusion.Dashboard.Core.Properties;
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Data.Common;
    using System.Linq;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using System.Web;
    public class BoldReportsWebConnection : ConnectionBase
    {
        public string WebUrl { get; set; }
        internal  BoldReportsDataProvider DataProvider { get; set; }

        public object OAuthProviderInfo { get; set; }

        public bool IsBoldReportsConnection { get; set; }

        public bool IsCustomQuery { get; set; }

        public bool IsViewMode { get; set; }

        public bool IsDesignMode { get; set; }

        public string clientId { get; set; }

        public bool IsDataAlertMode { get; set; }

        public string DatasourceCallerId { get; set; }

        public BoldReportsWebConnection(DashboardContainer dashboard, Guid pluginId, string id, string name) : base(dashboard, pluginId, id, name)
        {
            DataProvider = new BoldReportsDataProvider(this, dashboard);
        }
        public override bool IsRelational => true;

        public override bool AreParametersOk()
        {
            return true;
        }

        public override async Task<List<string>> BuildFormattedExpressionTextAsync(string displayText, DataSource dataSet, bool isVirtualExpression = false)
        {
            ConnectionBase connection = DataProvider.GetConnection(this.TargetProviderType);
            DataProvider.GettingSchemaNameForDataSetObject(dataSet);
            return await connection.BuildFormattedExpressionTextAsync(displayText, dataSet, isVirtualExpression);
        }


        public object JsonData { get; set; }
        public override IItem Clone(bool newId = false)
        {
            return new BoldReportsWebConnection(this.Dashboard, new Guid(), newId ? Guid.NewGuid().ToString("N") : this.Id, this.Name)
            {
                JsonData = this.JsonData,
                IsBoldReportsConnection = this.IsBoldReportsConnection,
                TargetConnectionString = this.TargetConnectionString,
                TargetProviderType = this.TargetProviderType,
                clientId = this.clientId,
                IsViewMode = this.IsViewMode
            };
        }

        public override async Task<SimplifiedDataType> GetExpressionDataTypeAsync(string query, DataSource dataSet)
        {
            ConnectionBase connection = DataProvider.GetConnection(this.TargetProviderType);
            DataProvider.GettingSchemaNameForDataSetObject(dataSet);
            return await connection.GetExpressionDataTypeAsync(query, dataSet);
        }

        public override string GetFieldFullName(QueryField f)
        {
            return DataProvider.GetConnection(this.TargetProviderType).GetFieldFullName(f);
        }

        public override FiscalYearFormat GetFiscalYearStartFromField(string displayText, DataSource dataSet)
        {
            return DataProvider.GetConnection(this.TargetProviderType).GetFiscalYearStartFromField(displayText, dataSet);
        }

        public override Dictionary<string, string> GetIdentityParameters()
        {
            Dictionary<string, string> parameters = new Dictionary<string, string>();
            parameters.Add("JsonData", this.JsonData != null ? this.JsonData.ToString() : string.Empty);
            parameters.Add("IsBoldReport", this.IsBoldReportsConnection.ToString());
            parameters.Add("ProviderInfo", this.OAuthProviderInfo != null ?this.OAuthProviderInfo.ToString() : string.Empty);
            return parameters;
        }

        public override string GetOriginalConnectionString()
        {
            throw new NotImplementedException();
        }

        public override bool IsExpressionContainsFieldName(string displayText, DataSource dataSet)
        {
            return DataProvider.GetConnection(this.TargetProviderType).IsExpressionContainsFieldName(displayText, dataSet);
        }


        public override async Task<bool> IsValidQueryAsync(string query)
        {
            return true;
        }

        public override ConnectionBase PortConnection(Dictionary<string, string> connectionInfo)
        {
            this.IsBoldReportsConnection = connectionInfo.ContainsKey("IsBoldReport") ? bool.Parse(connectionInfo["IsBoldReport"].ToString()) : true;
            this.JsonData = connectionInfo.ContainsKey("JsonData") ? DataProvider.SetCommandTimeOut(connectionInfo["JsonData"]?.ToString()) : null;
            this.OAuthProviderInfo = connectionInfo.ContainsKey("ProviderInfo") ? connectionInfo["ProviderInfo"] : null;
            this.IsViewMode = connectionInfo.ContainsKey("IsViewMode") ? bool.Parse(connectionInfo["IsViewMode"].ToString()) : false;
            this.IsDesignMode = connectionInfo.ContainsKey("IsViewMode") && bool.Parse(connectionInfo["IsViewMode"].ToString()) == false ? true : false;
            this.clientId = connectionInfo.ContainsKey("ClientId") ? connectionInfo["ClientId"].ToString() : string.Empty;
            this.IsCustomQuery = connectionInfo.ContainsKey("IsCustomQuery") ? bool.Parse(connectionInfo["IsCustomQuery"].ToString()) : false;
            this.IsDataAlertMode = connectionInfo.ContainsKey("IsDataAlertMode") ? bool.Parse(connectionInfo["IsDataAlertMode"].ToString()) : false;
            this.DatasourceCallerId = connectionInfo.ContainsKey("DatasourceCallerId") ? connectionInfo["DatasourceCallerId"] : string.Empty;
            return this;
        }

        public override SimplifiedDataType ToSimplified(string connectionFieldType)
        {
            return SimplifiedDataType.String;
        }

        public override DateTime UpdateEndDate(DateTime date)
        {
            return DataProvider.GetConnection(this.TargetProviderType).UpdateEndDate(date);
        }

        protected override async Task<bool> DoesConnectionSucceedAsync()
        {
            return true;
        }

        protected async override Task<bool> DoesFieldExistAsync(QueryField field, QueryTable table, CancellationToken cancellationToken)
        {
            return true;
        }

        protected override async Task<bool> DoesTableExistAsync(QueryTable table)
        {
            return true;
        }

        protected async override Task<QueryResult> GetAggregatedDataFromSummarizedTableInternalAsync(QueryObject query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await DataProvider.GetResultDataAsync(query, cancellationToken);
        }

        protected override async Task<IEnumerable<SchemaField>> GetCustomQuerySchemaFieldsInternalAsync(CustomQueryTable queryTable)
        {
            //cancellationToken.ThrowIfCancellationRequested();
            ConnectionBase connection = DataProvider.GetConnection(this.TargetProviderType);
            return await connection.GetCustomQuerySchemaFieldsInternalForLiveAsync(queryTable);
            //throw new NotImplementedException();
        }

        protected async override Task<QueryResult> GetDataTableInternalAsync(QueryObject query, CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return await DataProvider.GetResultDataAsync(query, cancellationToken);
        }

        /// <summary>
        /// Returns the query object using a QueryStringBuilder.
        /// </summary>
        /// <param name="query">The query object.</param>
        /// <returns>The query object using a QueryStringBuilder.</returns>
        public async override Task<string> GetQueryString(QueryObject query)
        {
           var requiredConnection = !string.IsNullOrEmpty(this.TargetProviderType) ? DataProvider.GetConnection(this.TargetProviderType) : DataProvider.GetConnection(TargetServerEnum.SQLite);
            return await requiredConnection.GetQueryString(query);
        }

        protected override Task<DbConnection> GetProgrammabilityConnectionFromCache(string uniqueId)
        {
            //return DataProvider.GetConnection(this.targetServer).GetProgrammabilityConnectionFromCache(uniqueId);
            throw new NotImplementedException();
        }

        protected override Task<IEnumerable<SchemaField>> GetProgrammibilityFieldsInternalAsync(QueryProgrammabilityTable programmabilityTable)
        {
            throw new NotImplementedException();
        }

        protected override Task<QueryResult> GetQueryStringInternalAsync(QueryObject query, CancellationToken cancellationToken)
        {
            return DataProvider.GetConnection(this.TargetProviderType).GetQueryStringForLive(query, cancellationToken);
        }

        protected async Task<ConnectionSchema> GetSchemaInternalAsync(LazySchemaCategory category, SchemaDetails schemaDetails, bool shouldLazyLoad)
        {
            //return null;
            return await DataProvider.GetConnectionSchemaAsync(category, schemaDetails);
        }

        protected override Task<SchemaTable> GetSchemaTableInternalAsync(QueryTable table, CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        public override async Task<DateTime> GetCurrentDateTimeAsync()
        {
            return await DataProvider.GetConnection(this.TargetProviderType).GetCurrentDateTimeAsync();
        }

        public override string PopDateFormat(string dateIdentifier, string startRange, string endRange, string field)
        {
            return DataProvider.GetConnection(this.TargetProviderType).PopDateFormat(dateIdentifier, startRange, endRange, field);
        }
        public override async Task<OperationResult<bool>> CreateTableAsync(DataSourceTable table, List<string> primaryKey = null)
        {
            ConnectionBase connection = DataProvider.GetConnection(this.TargetProviderType);
            return await connection.CreateTableAsync(table, primaryKey);
        }

        public override async Task<OperationResult<bool>> MoveDataToConnectionAsync(string input, DataInputType inputType, DataSourceTable tableSchema)
        {
            ConnectionBase connection = DataProvider.GetConnection(this.TargetProviderType);
            return await connection.MoveDataToConnectionAsync(input, inputType, tableSchema);
        }

        public override async Task<OperationResult<bool>> MoveDataTableToConnectionAsync(DataTable table, DataSourceTable tableSchema, bool isAlterTableOnSchemaChangeNeeded = false)
        {
            ConnectionBase connection = DataProvider.GetConnection(this.TargetProviderType);
            return await connection.MoveDataTableToConnectionAsync(table, tableSchema, isAlterTableOnSchemaChangeNeeded);
        }

        public override async Task<OperationResult<bool>> RemoveDataTableAsync(DataSourceTable table)
        {
            ConnectionBase connection = DataProvider.GetConnection(this.TargetProviderType);
            DataSourceTable dsTable = new DataSourceTable();
            dsTable.Name = table.Name;
            dsTable.Schema = table.Schema;
            return await connection.RemoveDataTableAsync(dsTable);
        }

        public override Task<OperationResult<bool>> RenameTableInTargetServerAsync(string tablename, DataSourceTable dataSourceTable)
        {
            throw new NotImplementedException();
        }

        public override Task<string> GetLastModifiedDatetimeAsync(string destinationTableName, string lastModifiedColumn)
        {
            throw new NotImplementedException();
        }

        public override Task<OperationResult<bool>> IsInsertDataToDestinationAsync(string tableName, Dictionary<string, string> primaryKey)
        {
            throw new NotImplementedException();
        }

        public override Task<OperationResult<bool>> UpdateDataToDestinationAsync(string destinationTableName, Dictionary<string, string> primaryvalues, Dictionary<string, string> updaterows)
        {
            throw new NotImplementedException();
        }

        public override async Task<OperationResult<bool>> CreateSchemaInTargerServerAsync(bool isRefresh = false)
        {
            ConnectionBase connection = DataProvider.GetConnection(this.TargetProviderType);
            if (connection.PluginUID == Syncfusion.Dashboard.Core.Common.ConnectionInformation.GetConnectionId(TargetServerEnum.SQLite))
            {
                return new OperationResult<bool> { Result = true };
            }

            return await connection.CreateSchemaInTargerServerAsync(isRefresh);
        }

        public override Task<DataTable> GetExecuteForStoredProcedureAsync(string schemaName, string procedureName, ExtractParameters parameters, bool isSchema = false)
        {
            throw new NotImplementedException();
        }

        public override Task GetSelectQueryForSourceConnectionAsync(DataSourceTable dataSourceTable, string tableName, ConnectionBase destinationConnection, bool isRefresh = false)
        {
            throw new NotImplementedException();
        }

        public override Task<DataTable> GetTableSchemaInformationAsync(string schemaName, string tableName)
        {
            throw new NotImplementedException();
        }

        public override Task<DataTable> GetUpdatedTableInformationAsync(string schemaName, string tableName, string lastModifiedColumn, string lastModifiedTime)
        {
            throw new NotImplementedException();
        }

        public override Task<List<string>> GetPrimaryKeyAsync(string schemaName, string tableName, bool isProcedure = false)
        {
            throw new NotImplementedException();
        }

        public override Task<DataSourceTable> PrePareDataSourceTableAsync(DataTable dataTable, string tableName, string schemaName, string sourceProvider = null, bool isProcedure = false)
        {
            throw new NotImplementedException();
        }

        public override async Task<OperationResult<bool>> RemoveSchemaFromTargetServerAsync()
        {
            ConnectionBase connection = DataProvider.GetConnection(this.TargetProviderType);
            if (connection.PluginUID == Syncfusion.Dashboard.Core.Common.ConnectionInformation.GetConnectionId(TargetServerEnum.SQLite))
            {
                return new OperationResult<bool> { Result = true };
            }

            return await connection.RemoveSchemaFromTargetServerAsync();
        }

        protected override async Task<ConnectionSchema> GetSchemaInternalAsync(LazySchemaCategory category, SchemaDetails schemaDetails, bool shouldLazyLoad, bool isTableView = false, string searchText = null)
        {
            return await DataProvider.GetConnectionSchemaAsync(category, schemaDetails);
        }

        public override Task<Dictionary<string, List<string>>> IsTableExistAsync(List<DataSourceTable> tableNames)
        {
            ConnectionBase connection = DataProvider.GetConnection(this.TargetProviderType);
            return connection.IsTableExistAsync(tableNames);
        }

        protected override Task GetTempTableForProgrammibilityFieldAsync(QueryProgrammabilityTable programmabilityTable)
        {
            throw new NotImplementedException();
        }

        public override async Task<bool> IsValidExpressionAsync(string expression, string expressionName, string customQuery, List<string> tableId)
        {
            ConnectionBase connection = DataProvider.GetConnection(this.TargetProviderType);
            return await connection.IsValidExpressionAsync(expression, expressionName, customQuery, tableId);
        }
    }
}
