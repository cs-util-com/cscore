using com.csutil;
using System;

namespace com.AaaaaRenameMeAaaaa {

    public class AaaaaRenameMeAaaaa_ExampleClass1 {

        public string name;
        public AaaaaRenameMeAaaaa_ExampleClass1(string name) { this.name = name; }

        public bool isTrue() {
            object x = "'I am x'";
            if (x is String s) {
                Console.WriteLine("X is a string! s=" + s);
                return true;
            }
            Log.d("Test");
            return false;
        }

    }

}