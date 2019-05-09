using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Storage;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media.Imaging;

namespace MagicMirror.ViewModel
{
    public class SlideShowItem
    {
        public string Url { get; set; }
        public DateTimeOffset DateCreated { get; set; }
        public StorageFile File { get; set; }
    }

    public class SlideShow : BaseViewModel
    {
        public BitmapImage ImageSource { get; set; }

        private readonly DispatcherTimer _refreshItemsTimer;
        private readonly DispatcherTimer _slideTimer;
        private readonly SemaphoreSlim _uiLock;

        private List<SlideShowItem> _items = new List<SlideShowItem>();
        private int _currentIndex = 0;
        private bool _firstRun = true;

        public SlideShow()
        {
            _uiLock = new SemaphoreSlim(1, 1);

            _refreshItemsTimer = new DispatcherTimer();
            _refreshItemsTimer.Tick += (_, __) => UpdateItemsTick();
            _refreshItemsTimer.Interval = TimeSpan.FromSeconds(5);

            _slideTimer = new DispatcherTimer();
            _slideTimer.Tick += (_, __) => SlideTick();
            _slideTimer.Interval = TimeSpan.FromSeconds(8);
        }

        public void Enable()
        {
            _firstRun = true;
            _refreshItemsTimer.Start();
        }
        public void Disable()
        {
            _slideTimer.Stop();
            _refreshItemsTimer.Stop();
            _items.Clear();
        }

        private async void UpdateItemsTick()
        {
            await UpdateItems();

            if (_firstRun)
            {
                _firstRun = false;
                _slideTimer.Start();
            }
        }

        private async void SlideTick()
        {
            await SlideImage();
        }

        private async Task SlideImage()
        {
            await _uiLock.WaitAsync();
            try
            {
                var items = _items;
                if (items == null || items.Count < 0)
                    return;

                if (_slideTimer.IsEnabled)
                {
                    _slideTimer.Stop();
                }

                _currentIndex = _currentIndex % items.Count;
                var item = items[_currentIndex++];

                var image = new BitmapImage();
                ExceptionRoutedEventHandler imageFailedHandler = null;
                RoutedEventHandler imageOpenedHandler = null;
                imageFailedHandler = (_, __) =>
                {
                    image.ImageFailed -= imageFailedHandler;
                    _ = SlideImage();
                };
                image.ImageFailed += imageFailedHandler;
                imageOpenedHandler = (_, __) =>
                {
                    image.ImageFailed -= imageFailedHandler;
                    OnImageSourceLoaded(item);
                };
                image.ImageOpened += imageOpenedHandler;

                ImageSource = image;

                await image.SetSourceAsync(await item.File.OpenReadAsync());
            }
            catch (Exception ex)
            {
                Log.e(ex);
                if (!_slideTimer.IsEnabled)
                {
                    _slideTimer.Start();
                }
            }
            finally
            {
                _uiLock.Release();
            }
        }

        private void OnImageSourceLoaded(SlideShowItem item)
        {
            RaisePropertyChanged(nameof(ImageSource));
            if (!_slideTimer.IsEnabled)
            {
                _slideTimer.Start();
            }
        }

        private async Task UpdateItems()
        {
            await _uiLock.WaitAsync();

            try
            {
                var files = await Windows.Storage.KnownFolders.PicturesLibrary.GetFilesAsync(Windows.Storage.Search.CommonFileQuery.OrderByDate);

                if (files.Count != _items.Count)
                {
                    _items = new List<SlideShowItem>(files.Select(file => CreateSlideShowItem(file)).OrderByDescending((item)=>item.DateCreated));
                }
            }
            catch (Exception ex)
            {
                Log.e(ex);
            }
            finally
            {
                _uiLock.Release();
            }
        }

        private SlideShowItem CreateSlideShowItem(Windows.Storage.StorageFile file)
        {
            return new SlideShowItem() { Url = file.Path, DateCreated = file.DateCreated, File = file };
        }
    }
}
