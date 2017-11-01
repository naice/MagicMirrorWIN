using System;
using System.Threading.Tasks;
using ConfigServer.Converter;
using Windows.ApplicationModel;
using Windows.ApplicationModel.Activation;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Reflection;
using System.IO;
using NcodedUniversal.Storage;

namespace MagicMirror
{
    sealed partial class App : Application
    {
        public static CoreDispatcher Dispatcher { get; private set; }

        #region Default implementations
        private class JsonConvert : NcodedUniversal.Converter.IJsonConvert, ConfigServer.Converter.IJsonConvert
        {
            public T DeserializeObject<T>(string jsonString) where T : class
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject<T>(jsonString);
            }

            public string SerializeObject(object obj)
            {
                return Newtonsoft.Json.JsonConvert.SerializeObject(obj);
            }

            object IJsonConvert.DeserializeObject(string jsonString, Type type)
            {
                return Newtonsoft.Json.JsonConvert.DeserializeObject(jsonString, type);
            }

            string IJsonConvert.SerializeObject(object obj)
            {
                return SerializeObject(obj);
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

                try
                {
                    var file = await _folder.GetFileAsync(name);
                    return await FileIO.ReadTextAsync(file);
                }
                catch (IOException ex)
                {
                    Log.e(ex);
                }

                return null;
            }

            public async Task WriteAllTextAsync(string name, string text)
            {
                var file = await _folder.CreateFileAsync(name, CreationCollisionOption.ReplaceExisting);

                await FileIO.WriteTextAsync(file, text);
            }
        }

        private class ConfigurationContract : ConfigServer.IConfigurationContract
        {
            public Storage<Configuration.Configuration> ConfigurationStorage = new Storage<Configuration.Configuration>("config.json");
            public Type ConfigurationType => typeof(Configuration.Configuration);

            public async Task<object> ConfigurationRequest()
            {
                return await ConfigurationStorage.Get();
            }
            public async Task ConfigurationUpdated(object newConfigurationObject)
            {
                await ConfigurationStorage.Replace(newConfigurationObject as Configuration.Configuration);
            }
            public string ConfigurationValidation(object newConfigurationObject)
            {
                // TODO: Implement sanity checks
                return null;
            }

            #region Schema
            static string FromRessource(string path)
            {
                var assembly = typeof(ConfigurationContract).GetTypeInfo().Assembly;
                using (var sr = new StreamReader(assembly.GetManifestResourceStream(path)))
                {
                    return sr.ReadToEnd();
                }
            }
            static Lazy<string> SCHEMA = new Lazy<string>(() => FromRessource("MagicMirror.Config.ConfigurationSchema.json"));
            static Lazy<string> UI_SCHEMA = new Lazy<string>(() => FromRessource("MagicMirror.Config.ConfigurationUiSchema.json"));
            public string GetConfigurationSchema()
            {
                return SCHEMA.Value;
            }
            public string GetConfigurationUiSchema()
            {
                return UI_SCHEMA.Value;
            }
            #endregion
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

            ConfigServer.DependencyConfiguration.Begin()
                .Set(new JsonConvert())
                .Set(new ConfigurationContract());

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
