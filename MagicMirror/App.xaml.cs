﻿using System;
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
using System.Linq;
using MagicMirror.Contracts;
using System.Net;
using System.Collections.Generic;
using Windows.Networking.Connectivity;
using Sentry;

namespace MagicMirror
{
    sealed partial class App : Application
    {
        public static CoreDispatcher Dispatcher { get; private set; }
        public static Storage<Configuration.Configuration> ConfigStorage => ConfigurationContract.ConfigurationStorage;
        private static ConfigurationContract ConfigurationContract = new ConfigurationContract();
        private IDisposable _sentryDeferral;

        #region Default implementations
        private class JsonConvert : NcodedUniversal.Converter.IJsonConvert, IJsonConvert
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
        private class StorageIO : IStorageIO
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
                    //Sentry.SentrySdk.CaptureException(ex);
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
        #endregion

        public App()
        {
            this.InitializeComponent();
            this.Suspending += OnSuspending;
        }

        /// <inheritdoc/>
        protected override async void OnLaunched(LaunchActivatedEventArgs e)
        {
            _sentryDeferral = SentrySdk.Init("https://66aa7ee7818e41cbb53f4fe8c7c75c6f@sentry.io/1452668");

            // defaults
            NcodedUniversal.Configuration.Begin()
                .Set(new JsonConvert())
                .Set(new StorageIO(null));

            await ConfigStorage.Get();

            ConfigServer.DependencyConfiguration.Begin()
                .SetPort(8887)
                .Set(new JsonConvert())
                .Set(ConfigurationContract);
            
            ConfigServer.ConfigServer.Instance.Run();

            // disable cursor. 
            CoreWindow.GetForCurrentThread().PointerCursor = null;
            Window.Current.CoreWindow.PointerCursor = null;

            // get ui dispatcher
            Dispatcher = CoreWindow.GetForCurrentThread().Dispatcher;

            if (!(Window.Current.Content is Frame rootFrame))
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

            if (_sentryDeferral != null)
            {
                _sentryDeferral.Dispose();
                _sentryDeferral = null;
            }

            deferral.Complete();
        }

        private static IPEndPoint GetDefaultEndPoint(int port)
        {
            List<IPAddress> ipAddresses = new List<IPAddress>();
            var hostnames = NetworkInformation.GetHostNames();
            foreach (var hn in hostnames)
            {
                if (hn.IPInformation != null &&
                     (hn.IPInformation.NetworkAdapter.IanaInterfaceType == 71 ||
                      hn.IPInformation.NetworkAdapter.IanaInterfaceType == 6))
                {
                    string strIPAddress = hn.DisplayName;

                    if (IPAddress.TryParse(strIPAddress, out IPAddress address))
                        ipAddresses.Add(address);
                }
            }

            if (ipAddresses.Count < 1)
            {
                return new IPEndPoint(IPAddress.Loopback, port);
            }
            else
            {
                return new IPEndPoint(ipAddresses[ipAddresses.Count - 1], port);
            }
        }
    }
}
