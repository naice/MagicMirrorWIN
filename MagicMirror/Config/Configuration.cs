﻿using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace MagicMirror.Configuration
{
    public enum DayTime { NONE, Morning, Afternoon, Evening }

    public class CalendarAndColor
    {
        public string URL { get; set; }
        public string Foreground { get; set; }
    }

    public class RadioConfig
    {
        public string Name { get; set; }
        public string PhoneticName { get; set; }
        public string URL { get; set; }
    }

    public class Configuration
    {
        // IMAGE LIBRARY

        public bool SlideShowActivated { get; set; } = true;

        // COMPLIMENTS

        public Dictionary<DayTime, List<string>> DayTimeCompliments { get; set; } = new Dictionary<DayTime, List<string>>() {
            {
                DayTime.Morning,
                new List<string>() {
                    "Guten Morgen!",
                    "Frühes vögeln entspannt den Wurm!",
                    "Morgenstund' hat Gold im Mund.",
                }
            },
            {
                DayTime.Afternoon,
                new List<string>() {
                    "Nett dich zu sehen.",
                    "Hübsch, wie immer!",
                    "Sehr sehr geil.",
                }
            },
            {
                DayTime.Evening,
                new List<string>() {
                    "Heiß heiß heiß!",
                    "Du siehst so unfassbar geil aus!",
                    "Hey, sweety!",
                }
             },
        };

        // SCREEN SAVER

        public TimeSpan ScreenSaverBegin { get; set; } = TimeSpan.FromHours(1);
        public TimeSpan ScreenSaverEnd { get; set; } = TimeSpan.FromHours(6);

        // CALENDAR CONFIG

        public int MaxCalendarItems { get; set; } = 10;
        public List<CalendarAndColor> CalendarAndColor { get; set; } = new List<CalendarAndColor>() {
            new CalendarAndColor() {
                URL = "https://www.google.com/calendar/ical/mhkpje7sser0a1q2b99urco728@group.calendar.google.com/public/basic.ics",
                Foreground = "#ff3399ff",
            },
            new CalendarAndColor() {
                URL = "https://calendar.google.com/calendar/ical/im.naice%40gmail.com/private-4d01a1953bc6da5f195b42075004ac68/basic.ics",
                Foreground = "#ffffffff",
            },
            new CalendarAndColor() {
                URL = "https://calendar.google.com/calendar/ical/de.german%23holiday%40group.v.calendar.google.com/public/basic.ics",
                Foreground = "#ffffcc00",
            },
        };

        // WEATHER CONFIG

        public string WeatherAPIKey { get; set; } = "43de1514340bfa4096d38ec09fb74e79";
        public string WeatherAPIUrl { get; set; } = "http://api.openweathermap.org/data/2.5";
        public string WeatherCity { get; set; } = "Dortmund";
        public string WeatherCountry { get; set; } = "Germany";
        public string WeatherUnits { get; set; } = "metric";
        public string WeatherLanguage { get; set; } = "de";

        // NEWS CONFIG
        [JsonIgnore]
        public Factory.RSSFeed.IRSSItemCreator[] NewsFeeds { get; set; } = new Factory.RSSFeed.IRSSItemCreator[]
        {
            new Factory.RSSFeed.RSSCreatorGolem(),
            //new Factory.RSSFeed.RSSCreatorT3N(),
        };

        // RADIO CONFIG

        public List<RadioConfig> Radios { get; set; } = new List<RadioConfig>()
        {
            // EINS LIVE
            new RadioConfig() {
                Name = "1Live",
                URL = "http://1live.akacast.akamaistream.net/7/706/119434/v1/gnl.akacast.akamaistream.net/1live",
            },
        };
    }
}
