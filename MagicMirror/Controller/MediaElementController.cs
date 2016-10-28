using MagicMirror.Contracts;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace MagicMirror.Controller
{
    public class MediaElementController : IMediaElementControl
    {
        private readonly MediaElement _mediaElement;

        public IMediaElementFeedback FeedbackReciever { get; set; }

        public MediaElementController(MediaElement mediaElement)
        {
            Debug.Assert(mediaElement != null);

            _mediaElement = mediaElement;
            _mediaElement.CurrentStateChanged += UpdateFeedback;
        }

        public double Volume
        {
            get
            {
                return _mediaElement.Volume;
            }

            set
            {
                _mediaElement.Volume = value;
            }
        }

        private void UpdateFeedback(object _, RoutedEventArgs __)
        {
            if (FeedbackReciever == null)
            {
                return;
            }

            switch (_mediaElement.CurrentState)
            {
                case Windows.UI.Xaml.Media.MediaElementState.Closed:
                    break;
                case Windows.UI.Xaml.Media.MediaElementState.Opening:
                    break;
                case Windows.UI.Xaml.Media.MediaElementState.Buffering:
                    FeedbackReciever.OnBuffering();
                    break;
                case Windows.UI.Xaml.Media.MediaElementState.Playing:
                    FeedbackReciever.OnPlay();
                    break;
                case Windows.UI.Xaml.Media.MediaElementState.Paused:
                    FeedbackReciever.OnPause();
                    break;
                case Windows.UI.Xaml.Media.MediaElementState.Stopped:
                    FeedbackReciever.OnStop();
                    break;
                default:
                    break;
            }
        }

        public void Pause()
        {
            _mediaElement.Pause();
        }

        public void Play()
        {
            _mediaElement.Play();
        }

        public void SetSource(Uri source)
        {
            // todo: handle m3u, etc. again..

            if (_mediaElement.CurrentState == Windows.UI.Xaml.Media.MediaElementState.Playing)
            {
                _mediaElement.Stop();
            }

            _mediaElement.Source = source;
        }

        public void Stop()
        {
            _mediaElement.Stop();
        }

        public void Play(Uri source)
        {
            SetSource(source);
            Play();
        }
    }
}
