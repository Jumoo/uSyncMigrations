namespace uSync.Migrations.Configuration.Models
{
    public class TabOptions
    {
        /// <summary>
        /// The name of the tab you want to change.  This is the name NOT the alias.
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

        /// <summary>
        ///  remove the tab. 
        /// </summary>
        /// <remarks>
        ///  tabs will also be removed if the newName is blank, but this is more explicit.
        /// </remarks>
        public bool DeleteTab { get; set; }
    }
}
