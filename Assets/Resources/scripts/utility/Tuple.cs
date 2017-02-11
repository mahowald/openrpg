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

        public override int GetHashCode()
        {
            int result = 37;
            result *= 397;
            if (Item1 != null)
                result += Item1.GetHashCode();
            result *= 397;
            if (Item2 != null)
                result += Item2.GetHashCode();

            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Tuple<T1, T2> tuple = (Tuple<T1, T2>)obj;

            return Item1.Equals(tuple.Item1) && Item2.Equals(tuple.Item2);
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

        public override int GetHashCode()
        {
            int result = 37;
            result *= 397;
            if (Item1 != null)
                result += Item1.GetHashCode();
            result *= 397;
            if (Item2 != null)
                result += Item2.GetHashCode();
            result *= 397;
            if (Item3 != null)
                result += Item3.GetHashCode();

            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Tuple<T1, T2, T3> tuple = (Tuple<T1, T2, T3>)obj;

            return Item1.Equals(tuple.Item1) && Item2.Equals(tuple.Item2) && Item3.Equals(tuple.Item3);
        }
    }

    public class Tuple<T1, T2, T3, T4>
    {
        private T1 i1;
        private T2 i2;
        private T3 i3;
        private T4 i4;

        
        public Tuple(T1 item1, T2 item2, T3 item3, T4 item4)
        {
            i1 = item1;
            i2 = item2;
            i3 = item3;
            i4 = item4;
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
        public T4 Item4
        {
            get { return i4; }
            set { i4 = value; }
        }

        public override int GetHashCode()
        {
            int result = 37;
            result *= 397;
            if (Item1 != null)
                result += Item1.GetHashCode();
            result *= 397;
            if (Item2 != null)
                result += Item2.GetHashCode();
            result *= 397;
            if (Item3 != null)
                result += Item3.GetHashCode();
            result *= 397;
            if (Item4 != null)
                result += Item4.GetHashCode();

            return result;
        }

        public override bool Equals(object obj)
        {
            if (obj == null || GetType() != obj.GetType())
                return false;

            Tuple<T1, T2, T3, T4> tuple = (Tuple<T1, T2, T3, T4>)obj;

            return Item1.Equals(tuple.Item1) && Item2.Equals(tuple.Item2) && Item3.Equals(tuple.Item3) && Item4.Equals(tuple.Item4);
        }
    }

}

