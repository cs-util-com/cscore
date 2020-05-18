using Newtonsoft.Json.Linq;
using System;

namespace com.csutil.model.mtvmtv {

    public static class ViewModelExtensions {

        public static JTokenType GetJTokenType(this ViewModel self) {
            return EnumUtil.Parse<JTokenType>(self.type);
        }

        public static JValue ParseToJValue(this ViewModel self, string newVal) {
            switch (self.GetJTokenType()) {
                case JTokenType.Integer: return new JValue(int.Parse(newVal));
                case JTokenType.Float: return new JValue(float.Parse(newVal));
                case JTokenType.String: return new JValue(newVal);
            }
            throw new NotImplementedException("Cant handle type " + self.type);
        }

    }

}