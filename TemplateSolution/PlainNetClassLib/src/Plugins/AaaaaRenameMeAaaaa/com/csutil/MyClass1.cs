using System;

namespace com.csutil
{
    public class MyClass1
    {
        public bool isTrue()
        {
            object x = "'I am x'";
            if (x is String s)
            {
                Console.WriteLine("X is a string! s=" + s);
                return true;
            }
            return false;
        }
    }
}