using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MarkdownMonster;
using MarkdownMonster.Windows;
using MarkdownMonster.Windows.PreviewBrowser;

namespace ChromiumPreviewerAddin
{
    /// <summary>
    /// Interaction logic for ChromiumPreviewControl.xaml
    /// </summary>
    public partial class ChromiumPreviewControl : UserControl, IPreviewBrowser
    {
        public ChromiumPreviewControl()
        {
            InitializeComponent();

            Model = mmApp.Model;
            Window = Model.Window;

            Loaded += ChromiumPreviewControl_Loaded;
            
            DataContext = Model;

            PreviewBrowser = new ChromiumPreviewHandler(ChromiumBrowser);
        }

        private void ChromiumPreviewControl_Loaded(object sender, RoutedEventArgs e)
        {
        }

        public AppModel Model { get; set; }

        public MainWindow Window { get; set; }

        IPreviewBrowser PreviewBrowser { get; set; }



        public void PreviewMarkdownAsync(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false,
            string renderedHtml = null)
        {
            PreviewBrowser.PreviewMarkdownAsync(editor, keepScrollPosition, renderedHtml);
        }

        public void PreviewMarkdown(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false, bool showInBrowser = false,
            string renderedHtml = null)
        {
            PreviewBrowser.PreviewMarkdown(editor, keepScrollPosition, showInBrowser, renderedHtml);
        }

        public void Navigate(string url)
        {
            PreviewBrowser.Navigate(url);
        }

        public void ExecuteCommand(string command, params dynamic[] args)
        {
            
        }

        private void WebBrowser_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            //if (e.NewSize.Width > 100)
            //{
            //    int width = Convert.ToInt32(Window.MainWindowPreviewColumn.Width.Value);
            //    if (width > 100)
            //        mmApp.Configuration.WindowPosition.SplitterPosition = width;
            //}
        }
    }
}
