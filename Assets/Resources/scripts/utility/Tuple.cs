using System.Collections;

namespace Tuples
{
    public class Tuple<T1, T2>
    {
        private T1 i1;
        private T2 i2;

        public Tuple(T1 item1, T2 item2)
        {
            i1 = item1;
            i2 = item2;
        }

        public T1 Item1
        {
            get { return i1; }
            set { i1 = value; }
        }
        public T2 Item2
        {
            get { return i2; }
            set { i2 = value; }
        }
    }

    public class Tuple<T1, T2, T3>
    {
        private T1 i1;
        private T2 i2;
        private T3 i3;

        public Tuple(T1 item1, T2 item2, T3 item3)
        {
            i1 = item1;
            i2 = item2;
            i3 = item3;
        }

        public T1 Item1
        {
            get { return i1; }
            set { i1 = value; }
        }
        public T2 Item2
        {
            get { return i2; }
            set { i2 = value; }
        }
        public T3 Item3
        {
            get { return i3; }
            set { i3 = value; }
        }
    }

}

