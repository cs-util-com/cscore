using System;
using System.Collections;
using System.Collections.Generic;

namespace Xunit {

    public class Fact : Attribute {

        public string Skip { get; set; }
        
        public string DisplayName { get; set; }
        
    }

}
