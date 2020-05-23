﻿using System;
using System.Threading.Tasks;
using com.csutil.model.mtvmtv;
using UnityEngine.UI;

namespace com.csutil.ui.mtvmtv {

    public class ListFieldView : FieldView {

        public Button selectAll;
        public Button add;
        public Button up;
        public Button down;
        public Button delete;
        public Button search;

        protected override Task Setup(string fieldName, string fullPath) {
            return Task.FromResult(true);
        }

    }

}