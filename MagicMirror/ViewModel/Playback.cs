using System;
using System.Collections.Generic;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;
using MagicMirror.Configuration;
using System.Net.Http;

namespace MagicMirror.ViewModel
{
    public class PlaylistTrack
    {
        public string Album { get; set; }
        public string AlbumArt { get; set; }
        public string Artist { get; set; }
        public string Source { get; set; }
        public string Title { get; set; }
        public PlaylistTrack()
        {

        }
        public PlaylistTrack(string source, string title, string artist, string album, string albumart)
        {
            this.Album = album;
            this.AlbumArt = albumart;
            this.Artist = artist;
            this.Source = source;
            this.Title = title;
        }

        public string GetHash()
        {
            string data = "";
            data += string.IsNullOrEmpty(Album) ? "N" : Album;
            data += string.IsNullOrEmpty(AlbumArt) ? "N" : AlbumArt;
            data += string.IsNullOrEmpty(Artist) ? "N" : Artist;
            data += string.IsNullOrEmpty(Source) ? "N" : Source;
            data += string.IsNullOrEmpty(Title) ? "N" : Title;

            return Tools.ComputeMD5(data);
        }
    }
    public class Playlist : List<PlaylistTrack>
    {
        public DateTime Timestamp { get; set; }

        public string GetHash()
        {
            string data = "playlist";
            if (this.Count > 0)
            {
                foreach (var item in this)
                {
                    data += item.GetHash();
                }
            }

            return Tools.ComputeMD5(data);
        }
    }

    public class Playback : BaseViewModel
    {
        private abstract class PlaylistParsingBase
        {
            public abstract string[] FExt { get; }
            public abstract Playlist Parse(string content);

            public virtual bool IsExtensionMatch(string path)
            {
                return FExt.FirstOrDefault((A) => path.EndsWith(A)) != null;
            }
        }
        private class M3UParsing : PlaylistParsingBase
        {
            public override string[] FExt
            {
                get { return new string[] { ".m3u" }; }
            }

            public override Playlist Parse(string content)
            {
                Playlist l = new Playlist();

                foreach (var line in content.Split('\r', '\n'))
                {
                    string ln = line.Trim();
                    if (!ln.StartsWith("#") && ln.StartsWith("http") && Uri.IsWellFormedUriString(ln, UriKind.Absolute))
                        l.Add(Playback.CreateTrack(ln));
                }

                return l;
            }

        }
        private class PLSParsing : PlaylistParsingBase
        {
            public override string[] FExt
            {
                get { return new string[] { ".pls" }; }
            }

            public override Playlist Parse(string content)
            {
                Playlist l = new Playlist();

                foreach (var line in content.Split('\r', '\n'))
                {
                    string ln = line.Trim();
                    if (ln.Contains("http"))
                    {
                        string url = ln.Substring(ln.IndexOf("http"));
                        if (Uri.IsWellFormedUriString(url, UriKind.Absolute))
                            l.Add(Playback.CreateTrack(url));
                    }
                }

                return l;
            }
        }

        private static List<PlaylistParsingBase> PlaylistParsingProvider = new List<PlaylistParsingBase>(){
            new M3UParsing(),
            new PLSParsing(),
        };
        public static Playback Instance { get; private set; }

        public static Playlist Playlist { get; private set; }
        public string StreamURL { get; private set; }

        private Windows.UI.Xaml.Controls.MediaElement _mediaElement;
        public Windows.UI.Xaml.Controls.MediaElement MediaElement
        {
            get { return _mediaElement; }
            set
            {
                if (_mediaElement != null)
                {
                    _mediaElement.CurrentStateChanged -= MediaElement_CurrentStateChanged;
                }

                _mediaElement = value;

                if (_mediaElement != null)
                {
                    _mediaElement.CurrentStateChanged += MediaElement_CurrentStateChanged;
                }
            }
        }


        private string _RadioName;
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

        private PlaylistTrack _CurrentTrack;
        public PlaylistTrack CurrentTrack
        {
            get { return _CurrentTrack; }
            set
            {
                if (value != _CurrentTrack)
                {
                    _CurrentTrack = value;
                    RaisePropertyChanged("CurrentTrack");
                }
            }
        }

        public Playback()
        {
            Instance = this;
            //MediaElement = new Windows.UI.Xaml.Controls.MediaElement();
            //MediaElement.CurrentStateChanged += MediaElement_CurrentStateChanged;
        }

        private void MediaElement_CurrentStateChanged(object sender, Windows.UI.Xaml.RoutedEventArgs e)
        {
            switch (MediaElement.CurrentState)
            {
                case Windows.UI.Xaml.Media.MediaElementState.Closed:
                    break;
                case Windows.UI.Xaml.Media.MediaElementState.Opening:
                    break;
                case Windows.UI.Xaml.Media.MediaElementState.Buffering:
                    break;
                case Windows.UI.Xaml.Media.MediaElementState.Playing:
                    IsPlaying = true;
                    break;
                case Windows.UI.Xaml.Media.MediaElementState.Paused:
                    IsPlaying = false;
                    break;
                case Windows.UI.Xaml.Media.MediaElementState.Stopped:
                    IsPlaying = false;
                    break;
                default:
                    break;
            }
        }

        public void Play()
        {
            MediaElement.Play();
        }
        public void Pause()
        {
            MediaElement.Pause();
        }

        public void Louder()
        {
            double vol = MediaElement.Volume + 0.2;
            vol = vol > 1 ? 1 : vol;
            MediaElement.Volume = vol;
        }

        public void Quieter()
        {
            double vol = MediaElement.Volume - 0.2;
            vol = vol < 0.1 ? 0.1 : vol;
            MediaElement.Volume = vol;
        }
                
        private async Task<bool> UpdateStreamURL(string newStreamURL)
        {
            if (StreamURL == newStreamURL)
                return false;

            if (string.IsNullOrEmpty(newStreamURL))
            {
                return false;
            }

            StreamURL = newStreamURL;

            Playlist = await ParsePlaylist(newStreamURL);
            if (Playlist != null)
            {
                return true;
            }

            // keine playlist erzeugt
            StreamURL = null;

            return false;
        }

        private void SavePlaylist(Playlist Playlist, string filename)
        {
            Playlist.Timestamp = DateTime.Now;
            using (IsolatedStorageFile storage = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (IsolatedStorageFileStream stream = storage.CreateFile(filename))
                {
                    XmlSerializer xmlSerializer = new XmlSerializer(typeof(Playlist));
                    xmlSerializer.Serialize(stream, Playlist);
                }
            }
        }

        private async Task<Playlist> DownloadAndParsePlaylist(string url, PlaylistParsingBase parser)
        {
            using (var client = new HttpClient())
            {
                client.Timeout = TimeSpan.FromSeconds(5);
                var content = await client.GetStringAsync(url);

                return parser.Parse(content);
            }
        }

        string currentlyParsingURL = null;
        private async Task<Playlist> ParsePlaylist(string url)
        {
            Playlist playlist = null;
            if (currentlyParsingURL != url)
            {
                currentlyParsingURL = url;
                playlist = new Playlist();

                var matchingProvider = PlaylistParsingProvider.Where(A => A.IsExtensionMatch(url)).FirstOrDefault();

                if (matchingProvider != null)
                {
                    try
                    {
                        playlist = await DownloadAndParsePlaylist(url, matchingProvider);
                    }
                    catch (Exception ex)
                    {
                        Log.e(ex);
                        return null;
                    }
                }
                else
                {
                    // alle probieren..
                    foreach (var item in PlaylistParsingProvider)
                    {
                        try
                        {
                            playlist = await DownloadAndParsePlaylist(url, matchingProvider);
                        }
                        catch (Exception ex)
                        {
                            Log.e(ex);
                            return null;
                        }
                    }
                }

                // wenn immer noch null einfach die url als track erzeugen.
                if (playlist.Count == 0)
                {
                    playlist.Add(CreateTrack(url));
                }

                currentlyParsingURL = null;
            }

            return playlist;
        }

        public async void LoadAndPlay(RadioConfig radio)
        {
            RadioName = radio.Name;
            Playlist = await ParsePlaylist(radio.URL);

            if (Playlist != null && Playlist.Count > 0)
            {
                var firstTrack = Playlist.First();

                CurrentTrack = firstTrack;

                MediaElement.Source = new Uri(CurrentTrack.Source);

                Play();
            }
        }

        private static PlaylistTrack CreateTrack(string url, string trackName = null)
        {
            string trackname = string.IsNullOrEmpty(trackName) ? "Kein track name" : trackName;
            return new PlaylistTrack(url, trackname, null, null, "Assets/ApplicationIcon.png");
        }

    }
}
