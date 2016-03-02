using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MagicMirror.ControlExtensions
{
    public static class WebViewExtensions
    {
        #region UriSource
        public static string GetUriSource(WebView view)
        {
            return (string)view.GetValue(UriSourceProperty);
        }

        public static void SetUriSource(WebView view, string value)
        {
            view.SetValue(UriSourceProperty, value);
        }

        public static readonly DependencyProperty UriSourceProperty =
            DependencyProperty.RegisterAttached(
            "UriSource", typeof(string), typeof(WebViewExtensions),
            new PropertyMetadata(null, OnUriSourcePropertyChanged));

        private static void OnUriSourcePropertyChanged(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            var webView = sender as WebView;
            if (webView == null)
                throw new NotSupportedException();

            if (e.NewValue != null)
            {
                var uri = new Uri(e.NewValue.ToString());
                webView.Navigate(uri);
            }
        }
        #endregion

        #region HTMLSource

        public static string GetHtmlSource(WebView view)
        {
            return (string)view.GetValue(HtmlSourceProperty);
        }

        public static void SetHtmlSource(WebView view, string value)
        {
            view.SetValue(HtmlSourceProperty, value);
        }

        public static readonly DependencyProperty HtmlSourceProperty =
            DependencyProperty.RegisterAttached(
            "HtmlSource", typeof(string), typeof(WebViewExtensions),
            new PropertyMetadata(null, OnHtmlSourcePropertyChanged));

        private static void OnHtmlSourcePropertyChanged(DependencyObject sender,
            DependencyPropertyChangedEventArgs e)
        {
            var webView = sender as WebView;
            if (webView == null)
                throw new NotSupportedException();

            if (e.NewValue is string)
            {
                if (e.NewValue != null)
                {
                    webView.NavigateToString(e.NewValue as string);
                }
            }
        }

        #endregion
    }
}
