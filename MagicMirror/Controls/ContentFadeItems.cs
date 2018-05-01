using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Documents;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The Templated Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234235

namespace MagicMirror.Controls
{
    public sealed class ContentFadeItems : Control
    {
        public DataTemplate ContentTemplate
        {
            get { return (DataTemplate)GetValue(ContentTemplateProperty); }
            set { SetValue(ContentTemplateProperty, value); }
        }
        public static readonly DependencyProperty ContentTemplateProperty =
            DependencyProperty.Register("ContentTemplate", typeof(DataTemplate), typeof(ContentFadeItems), new PropertyMetadata(null));
        public Object SourceItems
        {
            get { return (Object)GetValue(SourceItemsProperty); }
            set { SetValue(SourceItemsProperty, value); }
        }
        public static DependencyProperty SourceItemsProperty = DependencyProperty.Register("SourceItems", typeof(Object), typeof(ContentFadeItems), new PropertyMetadata(null, new PropertyChangedCallback((A, B) => ((ContentFadeItems)A).OnSourceItemsChanged())));
        public Object CurrentItem
        {
            get { return (Object)GetValue(CurrentItemProperty); }
            set { SetValue(CurrentItemProperty, value); }
        }
        public static readonly DependencyProperty CurrentItemProperty =
            DependencyProperty.Register("CurrentItem", typeof(Object), typeof(ContentFadeItems), new PropertyMetadata(null));
        public double Duration
        {
            get { return (double)GetValue(DurationProperty); }
            set { SetValue(DurationProperty, value); }
        }
        public static readonly DependencyProperty DurationProperty =
            DependencyProperty.Register("Duration", typeof(double), typeof(ContentFadeItems), new PropertyMetadata(180));


        private double _duration = 0;
        private DispatcherTimer _timer = null;
        private object[] _items = null;
        private int _currentIndex = 0;

        public ContentFadeItems()
        {
            this.DefaultStyleKey = typeof(ContentFadeItems);
        }

        protected override void OnApplyTemplate()
        {
            base.OnApplyTemplate(); 
            SetContent();
            ResetTimer();
        }

        private void ResetTimer()
        {
            if (_timer == null || _duration != Duration)
            {
                if (_timer == null)
                {
                    _timer = new DispatcherTimer();
                    this._timer.Tick += TimerTick;
                }
                else
                    _timer.Stop();

                _duration = Duration;
                _timer.Interval = TimeSpan.FromSeconds(_duration);
            }

            if (_items != null && _items.Length > 0 && _timer.IsEnabled == false)
                _timer.Start();
        }

        private void SetContent()
        {
            if (_items != null && _items.Length > 0)
            {
                _currentIndex = _currentIndex % _items.Length;
                CurrentItem = _items[_currentIndex++];
            }
        }

        private void TimerTick(object sender, object e)
        {
            SetContent();
            ResetTimer();
        }

        private void OnSourceItemsChanged()
        {
            if (SourceItems != null && SourceItems is System.Collections.IList)
            {
                var list = SourceItems as System.Collections.IList;
                _items = new object[list.Count];
                for (int i = 0; i < list.Count; i++)
                {
                    _items[i] = list[i];
                }
            }
            else
                _items = null;
            //_currentIndex = 0;
            
            SetContent();
            ResetTimer();
        }
    }
}
