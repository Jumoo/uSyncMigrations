namespace uSync.Migrations.Configuration.Models
{
    public class TabOptions
    {
        /// <summary>
        /// The name of the tab you want to rename/delete.  This is the name NOT the alias.
        /// </summary>
        public string OriginalName { get; set; }
        /// <summary>
        /// The name of the tab you want to create or move properties too.  This is the name NOT the alias.
        /// </summary>
        public string NewName { get; set; }
        /// <summary>
        /// An alias for the tab.  If none is set, the alias will default to the new name in camel case.
        /// </summary>
        public string Alias { get; set; }
    }
}
