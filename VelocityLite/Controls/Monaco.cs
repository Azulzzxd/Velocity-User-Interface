using Microsoft.Web.WebView2.Core;
using Microsoft.Web.WebView2.Wpf;
using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Threading.Tasks;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;

#nullable disable
namespace VelocityLite.Controls
{
    public enum Types
    {
        None,
        Function,
        Variable,
        Keyword
    }

   
    public class WebViewAPI : WebView2CompositionControl
    {
        public string PendingText { get; set; } = null;

        private string LatestRecievedText;
        private TaskCompletionSource<string> _getTextTcs;
        public bool isDOMLoaded { get; set; } = false;

        public event EventHandler EditorReady;

        [DllImport("user32.dll")]
        private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool SetWindowPos(
          IntPtr hWnd,
          IntPtr hWndInsertAfter,
          int X,
          int Y,
          int cx,
          int cy,
          uint uFlags);

        public WebViewAPI(string text = "")
        {
            this.CoreWebView2InitializationCompleted += WebViewAPI_CoreWebView2InitializationCompleted;

            InitializeAsync();
        }

        private async void InitializeAsync()
        {
            try
            {
                await this.EnsureCoreWebView2Async(null);
            }
            catch (Exception)
            {
            }
        }

        protected virtual void OnEditorReady()
        {
            if (!string.IsNullOrEmpty(PendingText))
            {
                _ = SetText(PendingText);
            }
            EditorReady?.Invoke(this, EventArgs.Empty);
        }

        private void WebViewAPI_CoreWebView2InitializationCompleted(object sender, CoreWebView2InitializationCompletedEventArgs e)
        {
            if (!e.IsSuccess)
            {
                return;
            }

            this.CoreWebView2.DOMContentLoaded += CoreWebView2_DOMContentLoaded;
            this.CoreWebView2.WebMessageReceived += CoreWebView2_WebMessageReceived;

            this.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;
            this.CoreWebView2.Settings.AreDevToolsEnabled = false;
        }

        private async void CoreWebView2_DOMContentLoaded(object sender, CoreWebView2DOMContentLoadedEventArgs e)
        {
            await Task.Delay(500);
            this.isDOMLoaded = true;
            this.OnEditorReady();
        }

        private void CoreWebView2_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            string message = e.TryGetWebMessageAsString();

            if (_getTextTcs != null && !_getTextTcs.Task.IsCompleted)
            {
                _getTextTcs.SetResult(message);
            }

            this.LatestRecievedText = message;
        }

        public async Task<string> GetText()
        {
            if (!this.isDOMLoaded)
            {
                return string.Empty;
            }

            _getTextTcs = new TaskCompletionSource<string>();

            await this.ExecuteScriptAsync("window.chrome.webview.postMessage(editor.getValue());");

            return await _getTextTcs.Task;
        }

        public async Task SetText(string text)
        {
            if (!this.isDOMLoaded)
            {
                PendingText = text;
                return;
            }

            try
            {
                string escapedText = HttpUtility.JavaScriptStringEncode(text);
                string script = $"editor.setValue('{escapedText}');";
                await this.CoreWebView2.ExecuteScriptAsync(script);
                PendingText = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Exception in SetText: {ex.Message}");
            }
        }

        public void AddIntellisense(string label, Types type, string description, string insert)
        {
            if (!this.isDOMLoaded)
            {
                return;
            }

            string typeString = type == Types.None ? "" : type.ToString();
            label = HttpUtility.JavaScriptStringEncode(label);
            description = HttpUtility.JavaScriptStringEncode(description);
            insert = HttpUtility.JavaScriptStringEncode(insert);

            string script = $"AddIntellisense('{label}', '{typeString}', '{description}', '{insert}');";
            this.ExecuteScriptAsync(script);
        }

        public void Refresh()
        {
            this.CoreWebView2?.Reload();
        }


        public async Task EnableMiniMap()
        {
            if (!isDOMLoaded) return;
            await ExecuteScriptAsync("window.EnableMiniMap();");
        }

        public async Task DisableMiniMap()
        {
            if (!isDOMLoaded) return;
            await ExecuteScriptAsync("window.DisableMiniMap();");
        }

    }

    public static class MonacoEditorPool
    {
        private static readonly Queue<WebViewAPI> _pool = new();
        private static readonly object _lock = new();
        private const int PreloadCount = 1;

        public static async Task InitializePool()
        {
            for (int i = 0; i < PreloadCount; i++)
                await AddNewInstance();
        }

        public static WebViewAPI GetEditor()
        {
            lock (_lock)
            {
                if (_pool.Count > 0)
                    return _pool.Dequeue();
            }

            var editor = CreateEditorInstance();
            _ = PreloadOneAsync();
            return editor;
        }

        public static async Task PreloadOneAsync()
        {
            await AddNewInstance();
        }

        private static async Task AddNewInstance()
        {
            var editor = CreateEditorInstance();
            await WaitUntilEditorReady(editor);
            lock (_lock)
            {
                _pool.Enqueue(editor);
            }
        }

        private static WebViewAPI CreateEditorInstance()
        {
            return new WebViewAPI
            {
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
                Source = new Uri("http://localhost:3000/"),
                Visibility = Visibility.Hidden
            };
        }

        public static void ReturnToPool(WebViewAPI editor)
        {
            if (editor == null) return;
            lock (_lock)
            {
                _pool.Enqueue(editor);
            }
        }

        private static Task WaitUntilEditorReady(WebViewAPI editor)
        {
            var tcs = new TaskCompletionSource<bool>();
            EventHandler handler = null;
            handler = (sender, e) =>
            {
                editor.EditorReady -= handler;
                tcs.TrySetResult(true);
            };
            editor.EditorReady += handler;
            return tcs.Task;
        }
    }
}