using MarkdownMonster.AddIns;

namespace ChromiumPreviewerAddin
{
    public class ChromiumPreviewerAddinConfiguration : BaseAddinConfiguration<ChromiumPreviewerAddinConfiguration>
    {
        public ChromiumPreviewerAddinConfiguration()
        {
            // uses this file for storing settings in `%appdata%\Markdown Monster`
            // to persist settings call `ChromiumPreviewerAddinConfiguration.Current.Write()`
            // at any time or when the addin is shut down
            ConfigurationFilename = "ChromiumPreviewerAddin.json";
        }

        // Add properties for any configuration setting you want to persist and reload
        // you can access this object as 
        //     ChromiumPreviewerAddinConfiguration.Current.PropertyName
    }
}