using MagicMirror.ViewModel;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

namespace MagicMirror
{
    public sealed partial class MainPage : Page
    {
        public MainPage()
        {
            InitializeComponent();

            InitializeViewModel();
        }

        private void InitializeViewModel()
        {
            var radioController = new Controller.MediaElementController(mediaElementRadio);
            var videoController = new Controller.MediaElementController(mediaElementVideo);
            var radio = new Radio(radioController);
            radioController.FeedbackReciever = radio;
            var video = new Video(videoController);
            videoController.FeedbackReciever = video;
            var main = new MainViewModel(video, radio);

            DataContext = main;
        }
    }
}
