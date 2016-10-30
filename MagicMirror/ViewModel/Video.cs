using MagicMirror.Contracts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.ViewModel
{
    public class Video : BaseViewModel, IMediaElementFeedback
    {
        private readonly IMediaElementControl _videoController;

        public Video(IMediaElementControl videoController)
        {
            _videoController = videoController;
        }


        private bool _ShowVideo;
        public bool ShowVideo
        {
            get { return _ShowVideo; }
            set
            {
                if (value != _ShowVideo)
                {
                    _ShowVideo = value;
                    RaisePropertyChanged("ShowVideo");
                }
            }
        }
        private bool _IsPlaying;
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
        private bool _IsBuffering;
        public bool IsBuffering
        {
            get { return _IsBuffering; }
            set
            {
                if (value != _IsBuffering)
                {
                    _IsBuffering = value;
                    RaisePropertyChanged("IsBuffering");
                }
            }
        }
        private string _Thumbnail;
        public string Thumbnail
        {
            get { return _Thumbnail; }
            set
            {
                if (value != _Thumbnail)
                {
                    _Thumbnail = value;
                    RaisePropertyChanged("Thumbnail");
                }
            }
        }

        public void OnPause()
        {
            IsPlaying = false;
            IsBuffering = false;
        }

        public void OnPlay()
        {
            IsPlaying = true;
            IsBuffering = false;
        }

        public void OnStop()
        {
            IsPlaying = false;
            IsBuffering = false;
        }

        public void OnBuffering()
        {
            IsBuffering = true;
        }
    }
}
