using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;

namespace MagicMirror.Provider
{
    public interface ISpeechRecognitionManager
    {
        Task OnRecognizedSpeech(string text);
        Task OnStateChanged(SpeechRecognizerState state);
    }
}