using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Threading;

namespace IHCSClassLibrary
{
    class Clock
    {
        DispatcherTimer timer;
        public string TIME;

        public void WindowClock ()
        {
            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromSeconds(1.0);
            timer.Start();
            timer.Tick += new EventHandler(delegate(object s, EventArgs a)
            {

              TIME  = "" + DateTime.Now.Hour + ":"
              + DateTime.Now.Minute + ":"
              + DateTime.Now.Second;


            });
        }

        
    }
    
    
}

