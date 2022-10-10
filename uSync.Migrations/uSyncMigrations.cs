using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Microsoft.Identity.Client;

namespace uSync.Migrations;
internal static class uSyncMigrations
{
    public const string PluginFolder = "/App_Plugins/uSyncMigrations";
    public const string AppName = "Migrations";
    public static string AppVersion =>
        typeof(uSyncMigrations).Assembly.GetName().Version?.ToString(3) ?? "10.0";

    internal static class Priorities
    {

        public const int Templates = 5;
        public const int Languages = 6;
        public const int Dictionary = 7;

        public const int DataTypes = 10;
        public const int ContentTypes = 20;
        public const int MediaTypes = 30;

        public const int Macros = 40;

        public const int Content = 100;
        public const int Media = 110;

    }
}
