using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ImageRecognition
{
    public static class OptionExtension
    {
        public static Option<T> Find<T>(this IEnumerable<T> Enumerable)
        {
            return Find(Enumerable, t => true);
        }

        public static Option<T> Find<T>(this IEnumerable<T> Enumerable, Func<T, bool> pred)
        {
            try
            {
                T t = Enumerable.First();

                if (pred(t))
                {
                    return new Option<T>(t);
                }
                else
                {
                    return new Option<T>();
                }
            }
            catch (InvalidOperationException ignored)
            {
                return new Option<T>();
            }
        }
    }

    public class Option<T>
    {
        private readonly T data;
        private readonly bool hasData;

        public Option()
        {
            this.data = default(T);
            this.hasData = false;
        }

        public Option(T data)
        {
            if (data != null)
            {
                this.data = data;
                this.hasData = true;
            } else
            {
                this.data = default(T);
                this.hasData = false;
            }
        }

        public T OrDefault(T t)
        {
            return hasData ? data : t;
        }

        public bool IfPresent(Action<T> action)
        {
            if (hasData)
            {
                action(data);
            }

            return hasData;
        }

        public Option<V> Fmap<V>(Func<T, V> f)
        {
            if (hasData)
            {
                return new Option<V>(f(data));
            }
            else
            {
                return new Option<V>();
            }
        }
    }
}
