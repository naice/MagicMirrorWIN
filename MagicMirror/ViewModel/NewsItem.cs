using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.ViewModel
{
    public class NewsItem : BaseViewModel
    {
        /// <summary>
        /// WILL BE CHANGED ON UPDATE!!!!
        /// </summary>
        private string _ID;
        public string ID
        {
            get { return _ID; }
            private set
            {
                if (value != _ID)
                {
                    _ID = value;
                    RaisePropertyChanged("ID");
                }
            }
        }
        
        private string _Source;
        public string Source
        {
            get { return _Source; }
            set
            {
                if (value != _Source)
                {
                    _Source = value;
                    RaisePropertyChanged("Source");
                }
            }
        }
        private string _Title;
        public string Title
        {
            get { return _Title; }
            set
            {
                if (value != _Title)
                {
                    _Title = value;
                    RaisePropertyChanged("Title");
                }
            }
        }
        private string _Content;
        public string Content
        {
            get { return _Content; }
            set
            {
                if (value != _Content)
                {
                    _Content = value;
                    RaisePropertyChanged("Content");
                }
            }
        }
        private string _ContentRaw;
        public string ContentRaw
        {
            get { return _ContentRaw; }
            set
            {
                if (value != _ContentRaw)
                {
                    _ContentRaw = value;
                    RaisePropertyChanged("ContentRaw");
                }
            }
        }
        private string _Image;
        public string Image
        {
            get { return _Image; }
            set
            {
                if (value != _Image)
                {
                    _Image = value;
                    RaisePropertyChanged("Image");
                }
            }
        }
        private string _URIToSource;
        public string URIToSource
        {
            get { return _URIToSource; }
            set
            {
                if (value != _URIToSource)
                {
                    _URIToSource = value;
                    RaisePropertyChanged("URIToSource");
                }
            }
        }
        private DateTime _Created;
        public DateTime Created
        {
            get { return _Created; }
            set
            {
                if (value != _Created)
                {
                    _Created = value;
                    RaisePropertyChanged("Created");
                }
            }
        }
        private int _ImageWidth = 120;
        public int ImageWidth
        {
            get { return _ImageWidth; }
            set
            {
                if (value != _ImageWidth)
                {
                    _ImageWidth = value;
                    RaisePropertyChanged("ImageWidth");
                }
            }
        }

        public void Update(NewsItem ni)
        {
            this.Content = ni.Content;
            this.ContentRaw = ni.ContentRaw;
            this.Created = ni.Created;
            this.Image = ni.Image;
            this.ImageWidth = ni.ImageWidth;
            this.Source = ni.Source;
            this.Title = ni.Title;
            this.URIToSource = ni.URIToSource;

            this.GenerateID();
        }

        public void GenerateID()
        {
            string iddata = this.Title + this.Content + this.Created.ToString();
            ID = Tools.ComputeMD5(iddata);
        }
    }
}
