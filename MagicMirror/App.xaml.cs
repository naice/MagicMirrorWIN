using System;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;

namespace MagicMirror
{
    sealed partial class App : Application
    {
        public static CoreDispatcher Dispatcher { get; private set; }

        #region Default implementations for NcodedUniversal
        private class JsonConvert : NcodedUniversal.Converter.IJsonConvert
        {
            public T DeserializeObject<T>(string jsonString) where T : class
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonString);
            }

            public string SerializeObject(object obj)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            }
        }

        private class StorageIO : NcodedUniversal.Storage.IStorageIO
        {
            private readonly StorageFolder _folder;

            public StorageIO(StorageFolder folder)
            {
                _folder = folder ?? ApplicationData.Current.LocalFolder;
            }

            public async Task<string> ReadAllTextAsync(string name)
            {
                var file = await _folder.GetFileAsync(name);

                return await FileIO.ReadTextAsync(file);
            }

            public async Task WriteAllTextAsync(string name, string text)
            {
                var file = await _folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteTextAsync(file, text);
            }
        }
        #endregion

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <inheritdoc/>
        protected async override void OnLaunched(LaunchActivatedEventArgs e)
        {
            // defaults
            NcodedUniversal.Configuration.Begin()
                .Set(new JsonConvert())
                .Set(new StorageIO(null));

            await ConfigServer.ConfigServer.Instance.Run();

            return;

            // disable cursor. 
            CoreWindow.GetForCurrentThread().PointerCursor = null;
            Window.Current.CoreWindow.PointerCursor = null;

            // get ui dispatcher
            Dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            Frame rootFrame = Window.Current.Content as Frame;
            if (rootFrame == null)
            {
                rootFrame = new Frame();
                rootFrame.NavigationFailed += OnNavigationFailed;
                Window.Current.Content = rootFrame;
            }

            if (rootFrame.Content == null)
            {
                rootFrame.Navigate(typeof(MainPage), e.Arguments);
            }
            Window.Current.Activate();
        }
        
        void OnNavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            throw new Exception("Failed to load Page " + e.SourcePageType.FullName);
        }
        
        private void OnSuspending(object sender, SuspendingEventArgs e)
        {
            var deferral = e.SuspendingOperation.GetDeferral();

            deferral.Complete();
        }
    }
}
