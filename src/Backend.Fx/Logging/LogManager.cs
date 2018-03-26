﻿namespace Backend.Fx.Logging
{
    using System;
    using System.Diagnostics;
    using System.Linq;
    using System.Threading;

    [DebuggerStepThrough]
    public abstract class LogManager
    {
        private static int activityIndex=1;
        private static ILoggerFactory factory = new DebugLoggerFactory();

        public static void Initialize(ILoggerFactory theFactory)
        {
            factory = theFactory;
        }

        public static ILogger Create<T>()
        {
            return Create(typeof(T));
        }

        public static ILogger Create(Type t)
        {
            string s = t.FullName;
            var indexOf = s.IndexOf('[');
            if (indexOf > 0)
            {
                s = s.Substring(0, indexOf);
            }
            return Create(s);
        }

        public static ILogger Create(string s)
        {
            return factory.Create(s);
        }

        public static void BeginActivity()
        {
            Interlocked.Increment(ref activityIndex);
            factory.BeginActivity(activityIndex);
        }

        public static void Shutdown()
        {
            factory.Shutdown();
        }
    }
}