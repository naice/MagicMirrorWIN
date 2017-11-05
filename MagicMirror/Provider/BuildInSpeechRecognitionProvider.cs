using MagicMirror.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;

namespace MagicMirror.Provider
{
    public class BuildInSpeechRecognitionProvider
    {
        private readonly ISpeechRecognitionManager _manager;
        private readonly SpeechRecognizer _recognizer;

        public BuildInSpeechRecognitionProvider(ISpeechRecognitionManager manager)
        {
            _manager = manager;
            _recognizer = new SpeechRecognizer();
            InitializeSpeechRecognizer();
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
                    await _manager.OnRecognizedSpeech(text);
                }
            }

        }
        private async void RecognizerStateChanged(SpeechRecognizer sender, SpeechRecognizerStateChangedEventArgs args)
        {
            Log.i("SR State: " + args.State.ToString());

            await _manager.OnStateChanged(args.State);
        }
    }
}
