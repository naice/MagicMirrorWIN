using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace MagicMirror.Factory
{
    public static class WebHelper
    {
        public static async Task<string> DownloadString(string url)
        {
            string result = null;
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(url);
            request.Method = "GET";
            try
            {
                //using (StreamWriter w = new StreamWriter(await request.GetRequestStreamAsync()))
                //{
                //    await w.WriteAsync(parameter.ToString());
                //}

                var response = await request.GetResponseAsync();
                using (StreamReader sr = new StreamReader(response.GetResponseStream()))
                {
                    result = sr.ReadToEnd();
                }
            }
            catch (WebException ex)
            {
                Log.e(ex);
            }
            catch (Exception ex)
            {
                Log.e(ex);
            }

            return result;
        }
    }
}
