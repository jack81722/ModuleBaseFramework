using ModuleBased.Dialogue;
using System;

namespace ModuleBased.Example.Dialogue {
    public interface IGenericModuleCommand : IDialogueCommand, ICloneable {
        void SetParameters(object[] parameters);
    }
}