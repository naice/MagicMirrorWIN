using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using MagicMirror.Configuration;
using System.Net.Http;
using MagicMirror.Contracts;

namespace MagicMirror.ViewModel
{
    public class Radio : BaseViewModel, IMediaElementFeedback
    {
        private readonly IMediaElementControl _mediaControl;
        private string _RadioName;
        private bool _IsPlaying;

        public Radio(IMediaElementControl mediaControl)
        {
            _mediaControl = mediaControl;
        }

        public string RadioName
        {
            get { return _RadioName; }
            set
            {
                if (value != _RadioName)
                {
                    _RadioName = value;
                    RaisePropertyChanged("RadioName");
                }
            }
        }

        public bool IsPlaying
        {
            get { return _IsPlaying; }
            set
            {
                if (value != _IsPlaying)
                {
                    _IsPlaying = value;
                    RaisePropertyChanged("IsPlaying");
                }
            }
        }        

        public void Play()
        {
            _mediaControl.Play();
        }

        public void Pause()
        {
            _mediaControl.Pause();
        }

        public void Louder()
        {
            double vol = _mediaControl.Volume + 0.2;
            vol = vol > 1 ? 1 : vol;
            _mediaControl.Volume = vol;
        }

        public void Quieter()
        {
            double vol = _mediaControl.Volume - 0.2;
            vol = vol < 0.1 ? 0.1 : vol;
            _mediaControl.Volume = vol;
        }

        public void OnStop()
        {
            IsPlaying = false;
        }

        public void OnPause()
        {
            IsPlaying = false;
        }

        public void OnPlay()
        {
            IsPlaying = true;
        }

        public void OnBuffering()
        {
            //noop
        }

        public void Play(RadioConfig r)
        {
            RadioName = r.Name;
            _mediaControl.Play(new Uri(r.URL));
        }
        
    }
}
