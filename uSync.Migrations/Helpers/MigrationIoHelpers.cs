using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using Org.BouncyCastle.Bcpg.Sig;

using Umbraco.Cms.Core;

namespace uSync.Migrations.Helpers;

/// <summary>
///  helpers for disk / folder io.
/// </summary>
internal static class MigrationIoHelpers
{
    public static Dictionary<int, string[]> WellKnownPaths = new Dictionary<int, string[]>()
    {
        { 7, new [] { "DataType", "DocumentType" } },
        { 8, new [] { "DataTypes", "ContentTypes"} }
    };


    public static Attempt<string> CheckForFolders(string folder, string[] expectedFolders)
    {
        foreach (var expectedFolder in expectedFolders)
        {
            if (!Directory.Exists(Path.Combine(folder, expectedFolder)))
                return Attempt<string>.Fail(new DirectoryNotFoundException($"Missing well known folders {expectedFolder}'"));

        }
        return Attempt<string>.Succeed("Pass");
    }

    /// <summary>
    ///  find the first uSync like folder in the path.
    /// </summary>
    public static Attempt<string> FinduSyncPath(string path)
    {
        foreach (var expectedFolders in MigrationIoHelpers.WellKnownPaths.Values)
        {
            if (MigrationIoHelpers.CheckForFolders(path, expectedFolders).Success)
                return Attempt<string>.Succeed(path);
        }

        foreach (var folder in Directory.GetDirectories(path))
        {
            var attempt = FinduSyncPath(folder);
            if (attempt.Success) return attempt;

        }

        return Attempt<string>.Fail("Cannot any uSync like folder");
    }

    /// <summary>
    ///  quick simple check for version...
    /// </summary>
    /// <param name="folder"></param>
    /// <returns></returns>
    public static int DetectVersion(string folder)
    {
        if (Directory.Exists(Path.Combine(folder, "DataTypes"))) return 8;
        if (Directory.Exists(Path.Combine(folder, "DataType"))) return 7;
        // default.
        return -1;
    }


}
