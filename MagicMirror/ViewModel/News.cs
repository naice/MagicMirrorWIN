using MagicMirror.Factory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;

namespace MagicMirror.ViewModel
{
    public class News : BaseViewModel, IUpdateViewModel
    {
        private NewsItem _CurrentNews;
        public NewsItem CurrentNews
        {
            get { return _CurrentNews; }
            set
            {
                if (value != _CurrentNews)
                {
                    _CurrentNews = value;
                    RaisePropertyChanged("CurrentNews");
                }
            }
        }

        private bool _ShowDetail;
        public bool ShowDetail
        {
            get { return _ShowDetail; }
            private set
            {
                if (value != _ShowDetail)
                {
                    _ShowDetail = value;

                    if (_ShowDetail == false)
                    {
                        DetailNews = null;
                    }
                    RaisePropertyChanged("ShowDetail");
                }
            }
        }
        private NewsItem _DetailNews;
        public NewsItem DetailNews
        {
            get { return _DetailNews; }
            set
            {
                if (value != _DetailNews)
                {
                    _DetailNews = value;
                    RaisePropertyChanged("DetailNews");
                }
            }
        }
                
        private ObservableCollection<NewsItem> _Items = new ObservableCollection<NewsItem>();
        public ObservableCollection<NewsItem> Items
        {
            get { return _Items; }
            set
            {
                if (value != _Items)
                {
                    _Items = value;
                    RaisePropertyChanged("Items");
                }
            }
        }

        private DispatcherTimer DetailNewsTimer;
        public async void HideDetail()
        {
            if (_ShowDetail)
            {
                DetailNewsTimer.Stop();
                ShowDetail = false;
                await Task.Delay(900);
                DetailNews = null;
            }
        }
        public async void ViewDetail()
        {
            if (DetailNewsTimer == null)
            {
                DetailNewsTimer = new DispatcherTimer();
                DetailNewsTimer.Interval = TimeSpan.FromMinutes(5);
                DetailNewsTimer.Tick += DetailNewsTimer_Tick;
            }
            else
                DetailNewsTimer.Stop();

            if (!_ShowDetail)
            {
                if (CurrentNews != null)
                {
                    if (!string.IsNullOrEmpty(CurrentNews.ContentRaw))
                    {
                        DetailNews = CurrentNews;
                        ShowDetail = true;
                        await Task.Delay(900);

                        DetailNewsTimer.Start();
                    }
                    else
                    {
                        // TODO: Inform user that there are no contents to display..
                    }
                }
            }
        }
        private void DetailNewsTimer_Tick(object sender, object e)
        {
            HideDetail();
        }

        #region UPDATE MECHANISM
        private NewsFactory _newsFactory = new NewsFactory();
        public TimeSpan UpdateTimeout { get; set; } = TimeSpan.FromHours(1);
        public DateTime LastUpdate { get; set; } = DateTime.MinValue;
        public SemaphoreSlim UILock { get; set; } = new SemaphoreSlim(1, 1);

        public async Task<object> ProcessData(Configuration.Configuration config)
        {
            return await _newsFactory.GetNewsAsync(config);
        }

        public void UpdateUI(Configuration.Configuration config, object data)
        {
            var newsItems = data as List<NewsItem>;

            if (newsItems != null && newsItems.Count > 0)
            {
                if (this.Items.Count == newsItems.Count)
                {
                    for (int i = 0; i < newsItems.Count; i++)
                    {
                        this.Items[i].Update(newsItems[i]);
                    }
                }
                else
                {
                    this.Items = new ObservableCollection<NewsItem>(newsItems);
                }
            }
        }
        #endregion
    }
}
