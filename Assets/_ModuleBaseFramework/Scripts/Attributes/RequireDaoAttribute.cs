using System;

namespace ModuleBased {
    /// <summary>
    /// RequireModuleAttribute will be declare on the module field/property
    /// </summary>
    /// <remarks>
    /// GameCore system will catch this attribute and auto assign initialized module 
    /// </remarks>
    [AttributeUsage(AttributeTargets.Field | AttributeTargets.Property)]
    public class RequireDaoAttribute : Attribute { }
}