﻿using Mystique.Core.DTOs;
using Mystique.Core.Helpers;
using Mystique.Core.ViewModels;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;

namespace Mystique.Core.Repositories
{
    public class PluginRepository : IPluginRepository
    {
        private readonly DbHelper _dbHelper = null;
        private readonly List<Command> _commands = null;

        public PluginRepository(DbHelper dbHelper, List<Command> commands)
        {
            _dbHelper = dbHelper;
            _commands = commands;
        }

        public void AddPlugin(AddPluginDTO dto)
        {
            Command command = new Command
            {
                Parameters = new List<SqlParameter>(),
                Sql = "INSERT INTO Plugins(PluginId, Name, UniqueKey, Version, DisplayName,Enable) values(@pluginId, @name, @uniqueKey, @version, @displayName, @enable)"
            };

            command.Parameters.Add(new SqlParameter { ParameterName = "@pluginId", SqlDbType = SqlDbType.UniqueIdentifier, Value = dto.PluginId });
            command.Parameters.Add(new SqlParameter { ParameterName = "@name", SqlDbType = SqlDbType.NVarChar, Value = dto.Name });

            command.Parameters.Add(new SqlParameter { ParameterName = "@uniqueKey", SqlDbType = SqlDbType.NVarChar, Value = dto.UniqueKey });

            command.Parameters.Add(new SqlParameter { ParameterName = "@version", SqlDbType = SqlDbType.NVarChar, Value = dto.Version });

            command.Parameters.Add(new SqlParameter { ParameterName = "@displayName", SqlDbType = SqlDbType.NVarChar, Value = dto.DisplayName });

            command.Parameters.Add(new SqlParameter { ParameterName = "@enable", SqlDbType = SqlDbType.Bit, Value = false });

            _commands.Add(command);
        }

        public void UpdatePluginVersion(Guid pluginId, string version)
        {
            Command command = new Command
            {
                Parameters = new List<SqlParameter>(),
                Sql = "UPDATE Plugins SET Version = @version WHERE PluginId = @pluginId"
            };


            command.Parameters.Add(new SqlParameter { ParameterName = "@pluginId", SqlDbType = SqlDbType.UniqueIdentifier, Value = pluginId });
            command.Parameters.Add(new SqlParameter { ParameterName = "@version", SqlDbType = SqlDbType.NVarChar, Value = version });

            _commands.Add(command);
        }

        public List<PluginListItemViewModel> GetAllPlugins()
        {
            List<PluginListItemViewModel> plugins = new List<PluginListItemViewModel>();
            string sql = "SELECT * from Plugins";

            DataTable table = _dbHelper.ExecuteDataTable(sql);

            foreach (DataRow row in table.Rows.Cast<DataRow>())
            {
                PluginListItemViewModel plugin = new PluginListItemViewModel
                {
                    PluginId = Guid.Parse(row["PluginId"].ToString()),
                    Name = row["Name"].ToString(),
                    UniqueKey = row["UniqueKey"].ToString(),
                    Version = row["Version"].ToString(),
                    DisplayName = row["DisplayName"].ToString(),
                    IsEnable = Convert.ToBoolean(row["Enable"])
                };

                plugins.Add(plugin);
            }

            return plugins;
        }

        public List<PluginListItemViewModel> GetAllEnabledPlugins()
        {
            return GetAllPlugins().Where(p => p.IsEnable).ToList();
        }

        public void SetPluginStatus(Guid pluginId, bool enable)
        {
            string sql = "UPDATE Plugins SET Enable=@enable WHERE PluginId = @pluginId";

            _dbHelper.ExecuteNonQuery(sql, new List<SqlParameter> {
                new SqlParameter{ParameterName = "@enable", SqlDbType = SqlDbType.Bit, Value= enable},
                new SqlParameter{ParameterName = "@pluginId", SqlDbType = SqlDbType.UniqueIdentifier, Value= pluginId}
             }.ToArray());
        }

        public PluginViewModel GetPlugin(string pluginName)
        {
            string sql = "SELECT * from Plugins where Name = @pluginName";

            DataTable table = _dbHelper.ExecuteDataTable(sql, new SqlParameter
            {
                ParameterName = "@pluginName",
                Value = pluginName,
                SqlDbType = SqlDbType.NVarChar
            });

            if (table.Rows.Cast<DataRow>().Count() == 0)
            {
                return null;
            }

            DataRow row = table.Rows.Cast<DataRow>().First();

            PluginViewModel plugin = new PluginViewModel
            {
                PluginId = Guid.Parse(row["PluginId"].ToString()),
                Name = row["Name"].ToString(),
                UniqueKey = row["UniqueKey"].ToString(),
                Version = row["Version"].ToString(),
                DisplayName = row["DisplayName"].ToString(),
                IsEnable = Convert.ToBoolean(row["Enable"])
            };

            return plugin;
        }

        public PluginViewModel GetPlugin(Guid pluginId)
        {
            string sql = "SELECT * from Plugins where PluginId = @pluginId";

            DataTable table = _dbHelper.ExecuteDataTable(sql, new SqlParameter
            {
                ParameterName = "@pluginId",
                Value = pluginId,
                SqlDbType = SqlDbType.UniqueIdentifier
            });

            if (table.Rows.Cast<DataRow>().Count() == 0)
            {
                throw new Exception("The plugin is missing in the system.");
            }

            DataRow row = table.Rows.Cast<DataRow>().First();

            PluginViewModel plugin = new PluginViewModel
            {
                PluginId = Guid.Parse(row["PluginId"].ToString()),
                Name = row["Name"].ToString(),
                UniqueKey = row["UniqueKey"].ToString(),
                Version = row["Version"].ToString(),
                DisplayName = row["DisplayName"].ToString(),
                IsEnable = Convert.ToBoolean(row["Enable"])
            };

            return plugin;

        }

        public void DeletePlugin(Guid pluginId)
        {
            string sqlPluginMigrations = "DELETE PluginMigrations where PluginId = @pluginId";

            _dbHelper.ExecuteNonQuery(sqlPluginMigrations, new List<SqlParameter>{new SqlParameter
            {
                ParameterName = "@pluginId",
                Value = pluginId,
                SqlDbType = SqlDbType.UniqueIdentifier
            } }.ToArray());

            string sqlPlugins = "DELETE Plugins where PluginId = @pluginId";

            _dbHelper.ExecuteNonQuery(sqlPlugins, new List<SqlParameter>{new SqlParameter
            {
                ParameterName = "@pluginId",
                Value = pluginId,
                SqlDbType = SqlDbType.UniqueIdentifier
            } }.ToArray());
        }

        public void RunDownMigrations(Guid pluginId)
        {
            string sql = "SELECT Down from PluginMigrations WHERE PluginId = @pluginId ORDER BY [Version] DESC";

            DataTable table = _dbHelper.ExecuteDataTable(sql, new SqlParameter
            {
                ParameterName = "@pluginId",
                Value = pluginId,
                SqlDbType = SqlDbType.UniqueIdentifier
            });

            foreach (DataRow item in table.Rows.Cast<DataRow>())
            {
                string script = item[0].ToString();

                _dbHelper.ExecuteNonQuery(script);
            }
        }
    }
}
