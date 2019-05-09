using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Contracts
{
    public interface IMediaElementControl
    {
        double Volume { get; set; }

        void SetSource(Uri source);
        void Play();
        void Play(Uri source);
        void Pause();
        void Stop();
    }
}
