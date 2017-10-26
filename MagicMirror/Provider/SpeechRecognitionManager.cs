using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;

namespace MagicMirror.Provider
{
    // Distribution logic for speech interactions.
    public class SpeechRecognitionManager : ISpeechRecognitionManager
    {
        #region SINGLETON
        private static SpeechRecognitionManager _Instance;
        public static SpeechRecognitionManager Instance
        {
            get
            {
                if (_Instance == null)
                    _Instance = new SpeechRecognitionManager();
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
        private SpeechRecognitionManager()
        {
            _speechRecognitionInterfacesLock = new SemaphoreSlim(1, 1);
            _speechRecognitionInterfaces = new HashSet<object>();

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

        public async Task OnRecognizedSpeech(string text)
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

        public async Task OnStateChanged(SpeechRecognizerState state)
        {
            foreach (var stateChangeInterface in await GetSpeechRecognitionInterfaces<ISpeechRecognitionStateChange>())
            {
                stateChangeInterface.SpeechRecognitionStateChanged(state);
            }
        }
    }
}
