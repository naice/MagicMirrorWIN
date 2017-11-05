using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;

namespace MagicMirror.Contracts
{
    public interface ISpeechRecognitionManager
    {
        Task OnRecognizedSpeech(string text);
        Task OnStateChanged(SpeechRecognizerState state);
    }
}