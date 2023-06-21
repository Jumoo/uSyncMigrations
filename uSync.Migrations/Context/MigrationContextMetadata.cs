using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace uSync.Migrations.Context;
public class MigrationContextMetadata
{
	public Guid MigrationId { get; }

	/// <summary>
	///  where the migration is coming from. 
	/// </summary>
	public string SourceFolder { get; }

	/// <summary>
	///  path to the 'site' files for this migration
	/// </summary>
	public string SiteFolder { get; }

    /// <summary>
    ///  Is the Site folder pointing to this site?
    /// </summary>
    /// <remarks>
    ///  If this site folder is this site then we can use Umbraco's calls to get 
    ///  things like config etc, and we don't have to load the files ourselves 
    ///  (better because Umbraco will munge configs for us)
    /// </remarks>
    public bool SiteFolderIsSite { get; }

	/// <summary>
	///  the version of the source files (7 for now)
	/// </summary>
	public int SourceVersion { get; }

	/// <summary>
	///  the current default language of the install. 
	/// </summary>
	public string DefaultLanguage { get; set; } = string.Empty;

	public MigrationContextMetadata(Guid migrationId, string sourceFolder, string siteFolder, bool siteIsSite, int sourceVersion)
	{
		MigrationId = migrationId;
		SourceFolder = sourceFolder;
		SourceVersion = sourceVersion;
		SiteFolder = siteFolder;
		SiteFolderIsSite = siteIsSite;
	}

}
