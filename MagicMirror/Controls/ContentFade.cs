using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace MagicMirror.Controls
{
    public sealed class ContentFade : Control
    {
        public ContentFade()
        {
            this.DefaultStyleKey = typeof(ContentFade);
        }

        public DataTemplate ContentTemplate
        {
            get { return (DataTemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }
        public static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(ContentFade), new PropertyMetadata(null));


        public Object Content
        {
            get { return (Object)GetValue(ContentProperty); }
            set { SetValue(ContentProperty, value); }
        }
        public static DependencyProperty ContentProperty = DependencyProperty.Register("Content", typeof(Object), typeof(ContentFade), new PropertyMetadata(null, new PropertyChangedCallback((A, B) => ((ContentFade)A).OnContentChanged())));

        private const int TRANSITION_TIMEOUT = 900;
        
        private object _content = null;
        private ContentPresenter _contentPresenter = null;
        
        private void SetContent()
        {
            if (_contentPresenter != null)
            {
                _contentPresenter.Content = _content;
            }

            FadeIn();
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _contentPresenter = GetTemplateChild("PART_ContentPresenter") as ContentPresenter;
        }

        private void OnContentChanged()
        {
            FadeOut(() => {
                // update content
                _content = Content;

                SetContent();
            });
        }


        private async void FadeOut(Action a = null)
        {
            VisualStateManager.GoToState(this, "FadeOut", false);
            if (a != null)
            {
                await Task.Delay(TRANSITION_TIMEOUT);
                a();
            }
        }
        private async void FadeIn(Action a = null)
        {
            VisualStateManager.GoToState(this, "FadeIn", false);
            if (a != null)
            {
                await Task.Delay(TRANSITION_TIMEOUT);
                a();
            }
        }
    }
}
