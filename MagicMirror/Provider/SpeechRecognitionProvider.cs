using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;

namespace MagicMirror.Provider
{
    public class SpeechRecognitionProvider
    {
        #region SINGLETON
        private static SpeechRecognitionProvider _Instance;
        public static SpeechRecognitionProvider Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new SpeechRecognitionProvider();
                return _Instance;
            }
        }
        public void Touch()
        { /* no op, just for initialization */ }
        #endregion

        public async void Register<T>(T speechRecognitionInterface)
        {
            if (!(speechRecognitionInterface is ISpeechRecognitionResultGenerated
                || speechRecognitionInterface is ISpeechRecognitionStateChange))
            {
                throw new ArgumentException("Argument must be a valid SpeechRecognition interface.", nameof(speechRecognitionInterface));
            }

            await _speechRecognitionInterfacesLock.WaitAsync().ConfigureAwait(true);
            _speechRecognitionInterfaces.Add(speechRecognitionInterface);
            _speechRecognitionInterfacesLock.Release();
        }
        public async void Unregister<T>(T speechRecognitionInterface)
        {
            if (!(speechRecognitionInterface is ISpeechRecognitionResultGenerated
                || speechRecognitionInterface is ISpeechRecognitionStateChange))
            {
                throw new ArgumentException("Argument must be a valid SpeechRecognition interface.", nameof(speechRecognitionInterface));
            }

            await _speechRecognitionInterfacesLock.WaitAsync();
            if (_speechRecognitionInterfaces.Contains(speechRecognitionInterface))
            {
                _speechRecognitionInterfaces.Remove(speechRecognitionInterface);
            }
            _speechRecognitionInterfacesLock.Release();
        }

        private readonly SemaphoreSlim _speechRecognitionInterfacesLock;
        private readonly HashSet<object> _speechRecognitionInterfaces;
        private readonly SpeechRecognizer _recognizer;
        private SpeechRecognitionProvider()
        {
            _recognizer = new SpeechRecognizer();
            _speechRecognitionInterfacesLock = new SemaphoreSlim(1, 1);
            _speechRecognitionInterfaces = new HashSet<object>();
            InitializeSpeechRecognizer();
        }

        private async Task<T[]> GetSpeechRecognitionInterfaces<T>()
        {
            await _speechRecognitionInterfacesLock.WaitAsync();

            var result = _speechRecognitionInterfaces
                .Where(A => A is T)
                .Cast<T>()
                .ToArray();

            _speechRecognitionInterfacesLock.Release();

            return result;
        }
        private async void InitializeSpeechRecognizer()
        {
            // init recognizer
            // Todo: remove that stupid list constraint and make a grammar!
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

            _recognizer.Constraints.Add(listConstraint);

            _recognizer.StateChanged += RecognizerStateChanged;
            _recognizer.ContinuousRecognitionSession.ResultGenerated += RecognizerResultGenerated;

            // compile constraints
            SpeechRecognitionCompilationResult compilationResult = await _recognizer.CompileConstraintsAsync();

            // start recogition session if successful
            if (compilationResult.Status == SpeechRecognitionResultStatus.Success)
            {
                Log.i("SR Success");

                await _recognizer.ContinuousRecognitionSession.StartAsync();
            }
            else
            {
                Log.w("SR Failed {0}", compilationResult.Status);
            }
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
                    foreach (var resultGeneratedInterface in await GetSpeechRecognitionInterfaces<ISpeechRecognitionResultGenerated>())
                    {
                        var result = new SpeechRecognitionResult()
                        {
                            Text = text
                        };

                        resultGeneratedInterface.SpeechRecognitionResultGenerated(result);

                        if (result.IsHandled)
                        {
                            break;
                        }
                    }
                }
            }

        }
        private async void RecognizerStateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            Log.i("SR State: " + args.State.ToString());

            foreach (var stateChangeInterface in await GetSpeechRecognitionInterfaces<ISpeechRecognitionStateChange>())
            {
                stateChangeInterface.SpeechRecognitionStateChanged(args.State);
            }
        }
    }
}
