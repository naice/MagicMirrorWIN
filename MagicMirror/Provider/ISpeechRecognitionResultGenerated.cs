using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Provider
{
    public interface ISpeechRecognitionResultGenerated
    {
        void SpeechRecognitionResultGenerated(SpeechRecognitionResult result);
    }
}
