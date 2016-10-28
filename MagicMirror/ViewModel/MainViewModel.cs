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
    public class MainViewModel : BaseViewModel
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
                //if (this.Weather.ShowDetail)
                //    this.Weather.HideDetail();
                //else
                //    this.Weather.ViewDetail();

                var radioConfig = new Configuration.Configuration().Radios.FirstOrDefault();
                if (radioConfig != null)
                    Radio.Play(radioConfig);
            });

            Initzialize = new RelayCommand<object>(() =>
            {
                InitializeSpeechRecognizer();

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
                    () => { EnsureOnUI(() => ShowScreenSaver = true); },
                    config.ScreenSaverBegin, 
                    Manager.Schedule.RecurrenceDaily)
                );

            // Turnoff ScreenSaver
            Manager.ScheduleManager.Instance.Scheduler.StartSchedule(
                Manager.ScheduleManager.Instance.Scheduler.CreateRecurringScheduleFromToday(
                    () => { EnsureOnUI(() => ShowScreenSaver = false); },
                    config.ScreenSaverEnd,
                    Manager.Schedule.RecurrenceDaily)
                );

            await Task.Factory.StartNew(async() => {
                while (true)
                {
                    try
                    {
                        await Process();
                        await Task.Delay(60000);
                        await DateTimeFactory.Instance.UpdateTimeAsync();
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
                            await EnsureOnUIAsync(() => updateViewModel.UpdateUI(config, dat));
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
                await EnsureOnUIAsync(() => ShowSplashScreen = false);
        }
        #endregion

        private static async void EnsureOnUI(Windows.UI.Core.DispatchedHandler callback)
        {
            await EnsureOnUIAsync(callback);
        }
        private static async Task EnsureOnUIAsync(Windows.UI.Core.DispatchedHandler callback)
        {
            if (App.Dispatcher.HasThreadAccess)
            {
                callback.Invoke();
            }
            else
            {
                await App.Dispatcher.RunAsync(Windows.UI.Core.CoreDispatcherPriority.Normal, callback);
            }
        }

        #region SpeechRecognizer
        private SpeechRecognizer recognizer;
        bool isSpeechInizialized = false;

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

        private async void InitializeSpeechRecognizer()
        {
            if (isSpeechInizialized) return;

            // init recognizer
            recognizer = new SpeechRecognizer();
            var listConstraint = new SpeechRecognitionListConstraint(new string[]
            {
                "Show", "News", "Detail", "Weather",
                "Hide", "Close", "Time", "Back", "Escape",
                "Stop", "Pause", "Radio",
                "Louder", "Quieter",
            });

            foreach (var item in new Configuration.Configuration().Radios)
            {
                listConstraint.Commands.Add(item.PhoneticName);
            }

            recognizer.Constraints.Add(listConstraint);

            recognizer.StateChanged += RecognizerStateChanged;
            recognizer.ContinuousRecognitionSession.ResultGenerated += RecognizerResultGenerated;
            
            // compile constraints
            SpeechRecognitionCompilationResult compilationResult = await recognizer.CompileConstraintsAsync();
            
            // start recogition session if successful
            if (compilationResult.Status == SpeechRecognitionResultStatus.Success)
            {
                Log.i("SR Success");

                await recognizer.ContinuousRecognitionSession.StartAsync();
            }
            else
            {
                Log.w("SR Failed {0}", compilationResult.Status);
            }

            isSpeechInizialized = true;
        }
        private async void RecognizerResultGenerated(SpeechContinuousRecognitionSession session, SpeechContinuousRecognitionResultGeneratedEventArgs args)
        {
            Log.i(args.Result.Status.ToString());
            Log.i(args.Result.Confidence.ToString());
            Log.i(string.IsNullOrEmpty(args.Result.Text) ? "[NOTEXT]" : args.Result.Text);

            int confidence = (int)args.Result.Confidence;
            if (args.Result.Status == SpeechRecognitionResultStatus.Success && confidence < 2)
            {
                string text = args.Result.Text;

                if (!string.IsNullOrEmpty(text))
                {
                    text = text.ToUpper();

                    if (text == "SHOW" || text == "NEWS" || text == "DETAIL")
                    {
                        await EnsureOnUIAsync(() => this.News.ViewDetail());
                    }
                    else if (text == "HIDE" || text == "CLOSE" || text == "TIME" || text == "BACK" || text == "ESCAPE")
                    {
                        await EnsureOnUIAsync(() => { this.News.HideDetail(); this.Weather.HideDetail(); });
                    }
                    else if (text == "WEATHER")
                    {
                        await EnsureOnUIAsync(() => this.Weather.ViewDetail());
                    }
                    else if (text == "STOP" || text == "PAUSE")
                    {
                        await EnsureOnUIAsync(() => Radio.Pause());
                    }
                    else if (text == "LOUDER")
                    {
                        await EnsureOnUIAsync(() => Radio.Louder());
                    }
                    else if (text == "QUIETER")
                    {
                        await EnsureOnUIAsync(() => Radio.Quieter());
                    }
                    else if (text == "RADIO")
                    {
                        var radio = new Configuration.Configuration().Radios.FirstOrDefault();
                        if (radio != null)
                            await EnsureOnUIAsync(()=>Radio.Play(radio));
                    }
                    else
                    {
                        var radio = new Configuration.Configuration().Radios.Where(A => A.PhoneticName == text).FirstOrDefault();
                        if (radio != null)
                            await EnsureOnUIAsync(()=>Radio.Play(radio));
                    }
                }
            }

        }
        private async void RecognizerStateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            Log.i("SR State: " + args.State.ToString());

            await EnsureOnUIAsync(() => {
                ShowListeningInfo = args.State == SpeechRecognizerState.SpeechDetected;
            });
        }
        #endregion
    }
}
