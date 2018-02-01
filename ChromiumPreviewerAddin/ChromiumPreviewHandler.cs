using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;
using System.Windows.Threading;
using CefSharp;
using CefSharp.Wpf;
using FontAwesome.WPF;
using MarkdownMonster;
using MarkdownMonster.Windows;
using MarkdownMonster.Windows.PreviewBrowser;
using Westwind.Utilities;

namespace ChromiumPreviewerAddin
{
    public class ChromiumPreviewHandler : IPreviewBrowser
    {

        public static bool IsInitialized = false;

        /// <summary>
        /// Instance of the Web Browser control that hosts ACE Editor
        /// </summary>
        public ChromiumWebBrowser WebBrowser { get; set; }

        public dynamic BrowserPreview { get; set; }

        

        /// <summary>
        /// Reference back to the main Markdown Monster window that 
        /// </summary>
        public MainWindow Window { get; set; }

        public AppModel Model { get; set; }

        public bool IsVisible
        {
            get { return this.WebBrowser.Visibility == Visibility.Visible; }
            set { _isVisible = value; }
        }


        public DotnetProxy DotnetProxy { get; set; }
        private bool _isVisible;

        public ChromiumPreviewHandler(ChromiumWebBrowser browser)
        {
            
            WebBrowser = browser;
            Model = mmApp.Model;
            Window = Model.Window;

            DotnetProxy = new DotnetProxy(mmApp.Model,null,WebBrowser);
            WebBrowser.RegisterJsObject("dotnetProxy",DotnetProxy); //Standard object rego
            WebBrowser.FrameLoadEnd += WebBrowser_FrameLoadEnd;

            InitializePreviewBrowser();            
        }

        private async void WebBrowser_FrameLoadEnd(object sender, FrameLoadEndEventArgs e)
        {
            DotnetProxy.Browser = e.Browser;
            DotnetProxy.ChromeProxy.InitializeInterop();
        }


        /// <summary>
        /// Renders the current document or passed in HTML in the Web Browser preview
        /// or external preview
        /// </summary>
        /// <param name="editor">An instance of the active document editor</param>
        /// <param name="keepScrollPosition">True if scroll position should be maintained if possible</param>
        /// <param name="showInBrowser">If true renders in an external browser.</param>
        /// <param name="renderedHtml">Optional - pass in an HTML string. If null active document is rendered to HTML</param>
        public void PreviewMarkdown(MarkdownDocumentEditor editor = null,
            bool keepScrollPosition = false,
            bool showInBrowser = false,
            string renderedHtml = null)
        {
            try
            {
                // only render if the preview is actually visible and rendering in Preview Browser
                if (!Model.IsPreviewBrowserVisible && !showInBrowser)
                    return;

                if (editor == null)
                    editor = Window.GetActiveMarkdownEditor();

                if (editor == null)
                    return;

                var doc = editor.MarkdownDocument;
                var ext = Path.GetExtension(doc.Filename).ToLower().Replace(".", "");

                string mappedTo = "markdown";
                if (!string.IsNullOrEmpty(renderedHtml))
                {
                    mappedTo = "html";
                    ext = null;
                }
                else
                {
                    // only show preview for Markdown and HTML documents
                    Model.Configuration.EditorExtensionMappings.TryGetValue(ext, out mappedTo);
                    mappedTo = mappedTo ?? string.Empty;
                }

                if (string.IsNullOrEmpty(ext) || mappedTo == "markdown" || mappedTo == "html")
                {
                    // TODO: Get DOM and 
                    //dynamic dom = null;
                    //if (!showInBrowser)
                    //{
                    //    if (keepScrollPosition)
                    //    {
                    //        dom = WebBrowser..Document;
                    //        editor.MarkdownDocument.LastEditorLineNumber = dom.documentElement.scrollTop;
                    //    }
                    //    else
                    //    {
                    //        Window.ShowPreviewBrowser(false, false);
                    //        editor.MarkdownDocument.LastEditorLineNumber = 0;
                    //    }
                    //}

                    if (mappedTo == "html")
                    {
                        if (string.IsNullOrEmpty(renderedHtml))
                            renderedHtml = editor.MarkdownDocument.CurrentText;

                        if (!editor.MarkdownDocument.WriteFile(editor.MarkdownDocument.HtmlRenderFilename, renderedHtml))
                            // need a way to clear browser window
                            return;
                    }
                    else
                    {
                        bool usePragma = !showInBrowser && mmApp.Configuration.PreviewSyncMode != PreviewSyncMode.None;
                        if (string.IsNullOrEmpty(renderedHtml))
                            renderedHtml = editor.MarkdownDocument.RenderHtmlToFile(usePragmaLines: usePragma,
                                renderLinksExternal: mmApp.Configuration.MarkdownOptions.RenderLinksAsExternal);

                        if (renderedHtml == null)
                        {
                            Window.SetStatusIcon(FontAwesomeIcon.Warning, Colors.Red, false);
                            Window.ShowStatus($"Access denied: {Path.GetFileName(editor.MarkdownDocument.Filename)}",
                                5000);
                            // need a way to clear browser window

                            return;
                        }

                        renderedHtml = StringUtils.ExtractString(renderedHtml,
                            "<!-- Markdown Monster Content -->",
                            "<!-- End Markdown Monster Content -->");
                    }

                    if (showInBrowser)
                    {
                        var url = editor.MarkdownDocument.HtmlRenderFilename;
                        mmFileUtils.ShowExternalBrowser(url);
                        return;
                    }

                    WebBrowser.Cursor = Cursors.None;
                    WebBrowser.ForceCursor = true;

                    // if content contains <script> tags we must do a full page refresh
                    bool forceRefresh = renderedHtml != null && renderedHtml.Contains("<script ");


                    if (keepScrollPosition && !mmApp.Configuration.AlwaysUsePreviewRefresh && !forceRefresh)
                    {
                        string browserUrl = WebBrowser.Address.ToLower();
                        string documentFile = "file:///" +
                                              editor.MarkdownDocument.HtmlRenderFilename.Replace('\\', '/')
                                                  .ToLower();
                        if (browserUrl == documentFile)
                        {
                            

                            if (string.IsNullOrEmpty(renderedHtml))
                                PreviewMarkdown(editor, false, false); // fully reload document
                            else
                            {
                                try
                                {
                                    DotnetProxy.ChromeProxy.UpdateDocumentContent(renderedHtml);

                                    try
                                    {
                                        // scroll preview to selected line
                                        if (mmApp.Configuration.PreviewSyncMode ==
                                            PreviewSyncMode.EditorAndPreview ||
                                            mmApp.Configuration.PreviewSyncMode == PreviewSyncMode.EditorToPreview)
                                        {
                                            int lineno = editor.GetLineNumber();
                                            if (lineno > -1)
                                                DotnetProxy.ChromeProxy.ScrollToPragmaLine(lineno);
                                        }
                                    }
                                    catch
                                    {
                                        /* ignore scroll error */
                                    }
                                }
                                catch
                                {
                                    // Refresh doesn't fire Navigate event again so
                                    // the page is not getting initiallized properly
                                    //PreviewBrowser.Refresh(true);
                                    WebBrowser.Tag = "EDITORSCROLL";


                                    WebBrowser.Load(editor.MarkdownDocument.HtmlRenderFilename);
                                }
                            }

                            return;
                        }
                    }

                    WebBrowser.Tag = "EDITORSCROLL";
                    WebBrowser.Load(editor.MarkdownDocument.HtmlRenderFilename);
                    return;
                }

                // not a markdown or HTML document to preview
                Window.ShowPreviewBrowser(true, keepScrollPosition);
            }
            catch (Exception ex)
            {
                //mmApp.Log("PreviewMarkdown failed (Exception captured - continuing)", ex);
                Debug.WriteLine("PreviewMarkdown failed (Exception captured - continuing)", ex);
            }
        }

        private DateTime invoked = DateTime.MinValue;

        public void PreviewMarkdownAsync(MarkdownDocumentEditor editor = null, bool keepScrollPosition = false, string renderedHtml = null)
        {
            if (!mmApp.Configuration.IsPreviewVisible)
                return;

            var current = DateTime.UtcNow;

            // prevent multiple stacked refreshes
            if (invoked == DateTime.MinValue) // || current.Subtract(invoked).TotalMilliseconds > 4000)
            {
                invoked = current;
                Application.Current.Dispatcher.InvokeAsync(
                    () => {
                        try
                        {
                            PreviewMarkdown(editor, keepScrollPosition, renderedHtml: renderedHtml);
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine("Preview Markdown Async Exception: " + ex.Message);
                        }
                        finally
                        {
                            invoked = DateTime.MinValue;
                        }
                    }, DispatcherPriority.ApplicationIdle);
            }
        }

        public void Navigate(string url)
        {
            WebBrowser.Load(url);
        }

        public void ExecuteCommand(string command, params dynamic[] args)
        {
            MessageBox.Show("PreviewBrowser Command not implemented: " + command);
        }


        private void InitializePreviewBrowser()
        {
            // wbhandle has additional browser initialization code
            // using the WebBrowserHostUIHandler
            //WebBrowser.LoadCompleted += PreviewBrowserOnLoadCompleted;
        }


        //private void PreviewBrowserOnLoadCompleted(object sender, NavigationEventArgs e)
        //{
        //    string url = e.Uri.ToString();
        //    if (!url.Contains("_MarkdownMonster_Preview") && !url.Contains("__untitled.htm"))
        //        return;

        //    bool shouldScrollToEditor = WebBrowser.Tag != null && WebBrowser.Tag.ToString() == "EDITORSCROLL";
        //    WebBrowser.Tag = null;

        //    dynamic window = null;
        //    MarkdownDocumentEditor editor = null;
        //    try
        //    {
        //        editor = Window.GetActiveMarkdownEditor();
        //        dynamic dom = WebBrowser.Document;
        //        window = dom.parentWindow;
        //        dom.documentElement.scrollTop = editor.MarkdownDocument.LastEditorLineNumber;

        //        window.initializeinterop(editor);

        //        if (shouldScrollToEditor)
        //        {
        //            try
        //            {
        //                // scroll preview to selected line
        //                if (mmApp.Configuration.PreviewSyncMode == PreviewSyncMode.EditorAndPreview ||
        //                    mmApp.Configuration.PreviewSyncMode == PreviewSyncMode.EditorToPreview)
        //                {
        //                    int lineno = editor.GetLineNumber();
        //                    if (lineno > -1)
        //                        window.scrollToPragmaLine(lineno);
        //                }
        //            }
        //            catch
        //            {
        //                /* ignore scroll error */
        //            }
        //        }
        //    }
        //    catch
        //    {
        //        // try again
        //        Task.Delay(500).ContinueWith(t =>
        //        {
        //            try
        //            {
        //                window.initializeinterop(editor);
        //            }
        //            catch
        //            {
        //                //mmApp.Log("Preview InitializeInterop failed: " + url, ex);
        //            }
        //        });
        //    }
        //}


    }
}
