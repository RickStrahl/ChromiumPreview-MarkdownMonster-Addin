using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Threading;
using CefSharp;
using CefSharp.Wpf;
using MahApps.Metro.Controls;
using MarkdownMonster;

namespace ChromiumPreviewerAddin
{
    public class DotnetProxy
    {
        internal ChromiumWebBrowser WebBrowser;
        internal IBrowser Browser;
        internal AppModel Model;
        internal ChromiumPreviewerAddin Addin;
        internal ChromeProxy ChromeProxy;

        public string htmlToUpdate { get; set; }

        public string Parm2 { get; set; }
        
        public DotnetProxy(AppModel model, ChromiumPreviewerAddin addin, ChromiumWebBrowser browser)
        {
            WebBrowser = browser;
            Model = model;
            Addin = addin;
            ChromeProxy = new ChromeProxy(this);
        }

        public void GotoLine(int editorLine)
        {
            Dispatcher.CurrentDispatcher.Invoke(()=>Model.ActiveEditor?.GotoLine(editorLine));
        }

        public bool IsPreviewToEditorSync()
        {
            return Model.Window.Invoke(()=> Model.ActiveEditor.IsPreviewToEditorSync());
        }

        public void PreviewContextMenu(int top, int left)
        {
            Dispatcher.CurrentDispatcher.Invoke(()=>Model.ActiveEditor.PreviewContextMenu(position: new {top = top, left = left}));
        }
    }

    public class ChromeProxy
    {
        private DotnetProxy DotnetProxy;

        public ChromeProxy(DotnetProxy proxy)
        {
            DotnetProxy = proxy;
        }
        
        public async void InitializeInterop()
        {
            var jsResult = await DotnetProxy.Browser.MainFrame.EvaluateScriptAsync("initializeinterop()");

        }


        public async void UpdateDocumentContent(string html)
        {
            DotnetProxy.htmlToUpdate = html;
            var jsResult = await DotnetProxy.Browser.MainFrame.EvaluateScriptAsync("updateDocumentContent(dotnetProxy.htmlToUpdate)");
        }

        public async void ScrollToPragmaLine(int lineNo)
        {
            var jsResult = await DotnetProxy.Browser.MainFrame.EvaluateScriptAsync($"scrollToPragmaLine({lineNo})");
        }
        
    }
}
