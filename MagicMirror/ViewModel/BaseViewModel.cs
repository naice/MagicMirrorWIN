using System;
using System.Reflection;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Text;

namespace MagicMirror.ViewModel
{
    public class BaseViewModel : INotifyPropertyChanged
    {
        public static bool DEEPVALUELOG = false;

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void RaisePropertyChanged(string propertyName)
        {
#if DEBUG
            if (DEEPVALUELOG)
            {
                object value = null;
                var property = this.GetType().GetRuntimeProperty(propertyName);
                if (property != null)
                    value = property.GetValue(this);
                Log.w("RAISE PROPERTY CHANGE {0}.{1} = {2}", this.GetType().FullName, propertyName, value);
            }
#endif


            if (PropertyChanged != null)
            {
                try
                {
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs(propertyName));
                }
                catch (UnauthorizedAccessException x)
                {
                    Log.e(x);
                }
            }
        }


    }
}
