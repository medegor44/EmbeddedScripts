namespace HelperObjects
{
    public class HelperObject
    {
        public int x = 0;
        public int idx = 0;

        public int this[int i]
        {
            get => x;
            set
            {
                idx = i;
                x = value;
            }
        }
    }
}
