using MagicMirror.Contracts;
using MagicMirror.Factory;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.ApplicationModel;
using Windows.Media.SpeechRecognition;
using Windows.Storage;
using Windows.UI.Xaml;

namespace MagicMirror.ViewModel
{
    public class MainViewModel : BaseViewModel, Provider.ISpeechRecognitionStateChange
    {
        // compliments
        public Compliments Compliments { get; private set; } = new Compliments();

        // calendar / time
        public Calendar Calendar { get; private set; } = new Calendar();

        // weather
        public Weather Weather { get; private set; } = new Weather();

        // news
        public News News { get; private set; } = new News();

        // radio
        public Radio Radio { get; private set; }

        // video
        public Video Video { get; private set; }
        
        // splash
        private bool _ShowSplashScreen = true;
        public bool ShowSplashScreen
        {
            get { return _ShowSplashScreen; }
            set
            {
                if (value != _ShowSplashScreen)
                {
                    _ShowSplashScreen = value;
                    RaisePropertyChanged("ShowSplashScreen");
                }
            }
        }

        // screen saver
        private bool _ShowScreenSaver = false;
        public bool ShowScreenSaver
        {
            get { return _ShowScreenSaver; }
            set
            {
                if (value != _ShowScreenSaver)
                {
                    _ShowScreenSaver = value;
                    RaisePropertyChanged("ShowScreenSaver");
                }
            }
        }

        // the speechrecognizer is listening.
        private bool _ShowListeningInfo;
        public bool ShowListeningInfo
        {
            get { return _ShowListeningInfo; }
            set
            {
                if (value != _ShowListeningInfo)
                {
                    _ShowListeningInfo = value;
                    RaisePropertyChanged("ShowListeningInfo");
                }
            }
        }

        // this commadn is for testing purpose if no microphone or whatsoever
        public RelayCommand<object> Clicked { get; set; }
        // this command will start our update mechanism and init some basics e.g. speech recognition
        public RelayCommand<object> Initzialize { get; set; }
        
        private readonly IUpdateViewModel[] _updateViewModels;

        public MainViewModel()
        {
            //todo: design instance only
        }

        public MainViewModel(Video video, Radio radio)
        {
            Video = video;
            Radio = radio;

            _updateViewModels = new IUpdateViewModel[] {
                Compliments, Calendar, Weather, News
            };
            // this commadn is for testing purpose if no microphone or whatsoever
            Clicked = new RelayCommand<object>(() => {
                var radioConfig = new Configuration.Configuration().Radios.FirstOrDefault();
                if (radioConfig != null)
                    Radio.Play(radioConfig);
            });

            Initzialize = new RelayCommand<object>(() =>
            {
                Provider.SpeechRecognitionProvider.Instance.Touch();
                Provider.SpeechRecognitionProvider.Instance.Register<Provider.ISpeechRecognitionStateChange>(this);
                Provider.SpeechRecognitionProvider.Instance.Register<Provider.ISpeechRecognitionResultGenerated>(Weather);
                Provider.SpeechRecognitionProvider.Instance.Register<Provider.ISpeechRecognitionResultGenerated>(News);
                Provider.SpeechRecognitionProvider.Instance.Register<Provider.ISpeechRecognitionResultGenerated>(Radio);

                StartUpdateTask();
            });
        }

        async void StartUpdateTask()
        {
            await DateTimeFactory.Instance.UpdateTimeAsync();
            var config = new Configuration.Configuration();

            // Turnon ScreenSaver
            Manager.ScheduleManager.Instance.Scheduler.StartSchedule(
                Manager.ScheduleManager.Instance.Scheduler.CreateRecurringScheduleFromToday(
                    () => { UI.EnsureOn(() => ShowScreenSaver = true); },
                    config.ScreenSaverBegin, 
                    Manager.Schedule.RecurrenceDaily)
                );

            // Turnoff ScreenSaver
            Manager.ScheduleManager.Instance.Scheduler.StartSchedule(
                Manager.ScheduleManager.Instance.Scheduler.CreateRecurringScheduleFromToday(
                    () => { UI.EnsureOn(() => ShowScreenSaver = false); },
                    config.ScreenSaverEnd,
                    Manager.Schedule.RecurrenceDaily)
                );

            await Task.Factory.StartNew(async() => {
                while (true)
                {
                    try
                    {
                        // todo: remove
                        await UI.EnsureOnAsync(() => this.ShowSplashScreen = false);
                        
                        //await Process();
                        await Task.Delay(60000);
                        //await DateTimeFactory.Instance.UpdateTimeAsync();

                    }
                    catch (Exception ex)
                    {
                        Log.e(ex);
                    }
                }
            });
        }

        #region DATA Processing
        private async Task Process()
        {
            var config = new Configuration.Configuration();

            foreach (var updateViewModel in _updateViewModels)
            {
                var now = DateTimeFactory.Instance.Now;
                if ((now - updateViewModel.LastUpdate) > updateViewModel.UpdateTimeout)
                {
                    updateViewModel.LastUpdate = now;

                    object dat = null;

                    try
                    {
                        dat = await updateViewModel.ProcessData(config);
                    }
                    catch (Exception ex)
                    {
                        Log.e(ex);
                    }                    

                    if (dat != null)
                    {
                        await updateViewModel.UILock.WaitAsync();
                        try
                        {
                            await UI.EnsureOnAsync(() => updateViewModel.UpdateUI(config, dat));
                        }
                        catch (Exception ex)
                        {
                            Log.e(ex);
                        }
                        updateViewModel.UILock.Release();
                    }
                }
            }
            

            if (ShowSplashScreen)
                await UI.EnsureOnAsync(() => ShowSplashScreen = false);
        }
        #endregion
        
        public void SpeechRecognitionStateChanged(SpeechRecognizerState state)
        {
            UI.EnsureOn(() => { ShowListeningInfo = state == SpeechRecognizerState.SpeechDetected; });
        }
    }
}
