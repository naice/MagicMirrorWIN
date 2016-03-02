using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace MagicMirror.Controls
{
    [ContentProperty(Name = "Content")]
    public sealed class ContentSwitch : Control
    {
        public DataTemplate ContentTemplate
        {
            get { return (DataTemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }
        public static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(ContentSwitch), new PropertyMetadata(null));
        public object Content
        {
            get { return (object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
        public static DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(object), typeof(ContentSwitch), new PropertyMetadata(null));
        public bool Show
        {
            get { return (bool)GetValue(ShowProperty); }
            set { SetValue(ShowProperty, value); }
        }
        public static DependencyProperty ShowProperty = DependencyProperty.Register("Show", typeof(bool), typeof(ContentSwitch), new PropertyMetadata(false, new PropertyChangedCallback((A, B) => ((ContentSwitch)A).OnShowChanged())));

        private ContentPresenter _contentPresenter = null;

        public ContentSwitch()
        {
            this.DefaultStyleKey = typeof(ContentSwitch);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _contentPresenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;

            OnShowChanged();
        }



        private void OnShowChanged()
        {
            if (Show)
            {
                VisualStateManager.GoToState(this, "FadeIn", false);
            }
            else
            {
                VisualStateManager.GoToState(this, "FadeOut", false);
            }
        }
    }
}
