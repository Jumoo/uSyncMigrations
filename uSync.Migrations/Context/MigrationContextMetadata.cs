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
	///  the version of the source files (7 for now)
	/// </summary>
	public int SourceVersion { get; }

	/// <summary>
	///  the current default language of the install. 
	/// </summary>
	public string DefaultLanguage { get; set; } = string.Empty;

	public MigrationContextMetadata(Guid migrationId, string sourceFolder, int sourceVersion)
	{
		MigrationId = migrationId;
		SourceFolder = sourceFolder;
		SourceVersion = sourceVersion;
	}

}
