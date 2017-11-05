using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Media.SpeechRecognition;

namespace MagicMirror.Contracts
{
    public interface ISpeechRecognitionStateChange
    {
        void SpeechRecognitionStateChanged(SpeechRecognizerState state);
    }
}
