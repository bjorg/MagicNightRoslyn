using System;

namespace StringFormatAttribute {
    [AttributeUsage(AttributeTargets.Method)]
    public class StringFormat : Attribute {

        //--- Fields ---
        public readonly string FormatParam;
        public readonly string ArgsParam;

        //--- Constructors ---
        public StringFormat(string formatParam = "format", string argsParam = "args") {
            this.FormatParam = formatParam;
            this.ArgsParam = argsParam;
        }
    }
}
