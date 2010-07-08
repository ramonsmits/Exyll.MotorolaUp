namespace Exyll.MotorolaUp
{
    using System;
    using System.Net;
    using System.Timers;

    public class UptimeController
    {
        public class UptimeEventArgs : EventArgs
        {
            public DateTime At { get; set; }
        }

        UptimeResolver _resolver = new UptimeResolver();
        Timer _t;
        TimeSpan _current;

        public event EventHandler<UptimeEventArgs> OnReboot;
        public event EventHandler<UptimeEventArgs> OnError;
        public event EventHandler<UptimeEventArgs> OnUpdate;

        public TimeSpan Uptime
        {
            get
            {
                return _current;
            }
        }

        public DateTime Boottime
        {
            get
            {
                return DateTime.Now - _current;
            }
        }

        public UptimeController()
        {
            _t = new Timer(60 * 1000);
            _t.Elapsed += new ElapsedEventHandler(t_Elapsed);
            _t.Start();
        }

        void t_Elapsed(object sender, ElapsedEventArgs e)
        {
            try
            {
                lock (_t)
                {
                    var uptime = _resolver.GetUptime();

                    if (_current > uptime)
                    {
                        Reboot();
                    }

                    _current = uptime;

                    Update();
                }
            }
            catch (WebException)
            {
            }
            catch (Exception ex)
            {
                Error();
                throw;
            }
            finally
            {
                GC.Collect();
            }
        }

        void Error()
        {
            FireEvent<UptimeEventArgs>(OnError, new UptimeEventArgs{ At = DateTime.Now });
        }

        void Update()
        {
            FireEvent<UptimeEventArgs>(OnUpdate, new UptimeEventArgs { At = this.Boottime });
        }

        void Reboot()
        {
            FireEvent<UptimeEventArgs>(OnReboot, new UptimeEventArgs { At = this.Boottime });
        }

        void FireEvent<T>(EventHandler<T> e, T d) where T : EventArgs
        {
            if (e != null)
            {
                foreach (EventHandler<T> i in e.GetInvocationList())
                {
                    try
                    {
                        i.DynamicInvoke(new object[] { this, d });
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
    }
}
