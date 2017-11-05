using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Contracts
{
    public class SpeechRecognitionResult
    {
        public string Text { get; set; }
        public string TextUpper { get { return _textUpper.Value; } }
        public bool IsHandled { get; set; }
        private readonly Lazy<string> _textUpper;

        public SpeechRecognitionResult()
        {
            _textUpper = new Lazy<string>(() => Text.ToUpper());
        }

        public bool IsCancel()
        {
            var text = TextUpper;
            return text == "HIDE" || text == "CLOSE" || text == "TIME" || text == "BACK" || text == "ESCAPE";
        }
    }
}
