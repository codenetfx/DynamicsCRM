using Microsoft.Win32;
using Microsoft.Xrm.Sdk;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Data.Common;
using System.Data.SqlClient;
using System.Linq;
using System.Text;

namespace Hsl.Xrm.Sdk.Plugin
{
    public static class SqlConnectionHelper
    {
        private static readonly string MSCRM_ConfigConnectionString;
        private static readonly Exception MSCRM_ConfigConnectionStringException;

        private static readonly ConcurrentDictionary<Guid, string> OrganizationConnectionStrings =
            new ConcurrentDictionary<Guid, string>();
        static SqlConnectionHelper()
        {
            try
            {
                using (var key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\MSCRM"))
                {
                    if (key == null)
                    {
                        MSCRM_ConfigConnectionStringException = new ApplicationException(@"Unable to find HKLM\Software\Microsoft\MSCRM");
                        return;
                    }
                    var configDbValue = key.GetValue("configdb");
                    if (configDbValue == null)
                    {
                        MSCRM_ConfigConnectionStringException =
                            new ApplicationException(@"Unable to find HKLM\Software\Microsoft\MSCRM\configdb");
                        return;
                    }
                    MSCRM_ConfigConnectionString = configDbValue.ToString();
                }
            }
            catch (Exception ex)
            {
                MSCRM_ConfigConnectionStringException = ex;
            }
        }

        public static SqlConnection GetSqlConnection(IPluginExecutionContext ctx)
        {
            string connStr = GetOrganizationDbConnectionString(ctx);
            try
            {
                return GetSqlConnectionFromCs(connStr);
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error obtaining SQL Connection", ex);
            }
        }

        public static string GetOrganizationDbConnectionString(IPluginExecutionContext ctx)
        {
            try
            {
                var connStr = OrganizationConnectionStrings.GetOrAdd(ctx.OrganizationId,
                    orgId => LoadOrganizationDbConnectionString(orgId));
                return connStr;
            }
            catch (Exception ex)
            {
                throw new InvalidPluginExecutionException("Error obtaining SQL Connection String", ex);
            }
        }

        private static string LoadOrganizationDbConnectionString(Guid organizationId)
        {
            if (MSCRM_ConfigConnectionStringException != null)
            {
                throw new InvalidPluginExecutionException(@"Error loading HKLM\Software\Microsoft\MSCRM\configdb setting. ",
                    MSCRM_ConfigConnectionStringException);
            }
            string connStr;
            using (var conn = GetSqlConnectionFromCs(MSCRM_ConfigConnectionString))
            {
                conn.Open();

                using (var cmd = new SqlCommand("SELECT ConnectionString FROM Organization WHERE Id = @id", conn))
                {
                    cmd.Parameters.Add(new SqlParameter("id", System.Data.SqlDbType.UniqueIdentifier) { Value = organizationId });
                    connStr = (string)cmd.ExecuteScalar();
                }
            }
            return connStr;
        }

        private static SqlConnection GetSqlConnectionFromCs(string connStr)
        {
            var b = new DbConnectionStringBuilder(false);
            b.ConnectionString = connStr;
            b.Remove("Provider");// We want to use SqlConnection rather than oledb since we know we'll be using SQL server.
            return new SqlConnection(b.ConnectionString);
        }
    }
}
