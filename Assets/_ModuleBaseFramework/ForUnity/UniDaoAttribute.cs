using System;
using System.Collections;
using System.Collections.Generic;

namespace ModuleBased.ForUnity {
    [AttributeUsage(AttributeTargets.Class)]
    public class UniDaoAttribute : Attribute {
        public Type ItfType { get; }

        public UniDaoAttribute(Type itfType) {
            ItfType = itfType;
        }
    }
}