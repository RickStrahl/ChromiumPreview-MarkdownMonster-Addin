using System;
using System.Windows;
using System.Windows.Controls;
using CefSharp;
using FontAwesome.WPF;
using MarkdownMonster;
using MarkdownMonster.AddIns;
using MarkdownMonster.Windows;
using MarkdownMonster.Windows.PreviewBrowser;

namespace ChromiumPreviewerAddin
{
    public class ChromiumPreviewerAddin : MarkdownMonster.AddIns.MarkdownMonsterAddin
    {
        public override void OnApplicationStart()
        {
            base.OnApplicationStart();


            // Id - should match output folder name. REMOVE 'Addin' from the Id
            Id = "ChromiumPreviewerAddin";

            // a descriptive name - shows up on labels and tooltips for components
            // REMOVE 'Addin' from the Name
            Name = "Chromium Previewer";


            // by passing in the add in you automatically
            // hook up OnExecute/OnExecuteConfiguration/OnCanExecute
            var menuItem = new AddInMenuItem(this)
            {
                Caption = Name,

                // if an icon is specified it shows on the toolbar
                // if not the add-in only shows in the add-ins menu
                FontawesomeIcon = FontAwesomeIcon.Chrome
            };

            // if you don't want to display config or main menu item clear handler
            //menuItem.ExecuteConfiguration = null;

            // Must add the menu to the collection to display menu and toolbar items            
            this.MenuItems.Add(menuItem);

            InitializeCefSharp();

        }

        private static bool IsInitialized { get; set; } = false;

        public static void InitializeCefSharp()
        {
            if (!IsInitialized)
            {
                IsInitialized = true;

                CefSettings s = new CefSettings();
                s.DisableGpuAcceleration();
                s.WindowlessRenderingEnabled = false;
                s.SetOffScreenRenderingBestPerformanceArgs();
                CefSharpSettings.LegacyJavascriptBindingEnabled = true;                
                Cef.Initialize(s);
            }
        }

        public static void UnInitializeCefSharp()
        {
            Cef.Shutdown();
        }


        public override void OnApplicationShutdown()
        {
            base.OnApplicationShutdown();
            UnInitializeCefSharp();
        }

        public override void OnExecute(object sender)
        {

            
            // *** Some things you can do:

            // // modify selected editor text
            //var text = GetSelection();
            //text = "<small>" + text + "</small>";
            //SetSelection(text);
            //RefreshPreview();

            // // open a new tab with a file
            //OpenTab(Path.Combine(mmApp.Configuration.CommonFolder, "ChromiumPreviewerAddin.json"));

            // // run a process
            //var imageFile = GetSelection();  // assume image file is selected
            //if (!imageFile.Contains(":\\"))
            //    imageFile = Path.Combine(Path.GetDirectoryName(ActiveDocument.Filename), imageFile);
            //Process.Start("paint.exe", imageFile);
        }

        public override void OnExecuteConfiguration(object sender)
        {
          

            //MessageBox.Show("Configuration for our sample Addin", "Markdown Addin Sample",
            //                MessageBoxButton.OK, MessageBoxImage.Information);
        }

        public override bool OnCanExecute(object sender)
        {
            return true;
        }

        public override void OnWindowLoaded()
        {
          
        }

        public override IPreviewBrowser GetPreviewBrowserUserControl()
        {
            return new ChromiumPreviewControl();
        }
    }
}
