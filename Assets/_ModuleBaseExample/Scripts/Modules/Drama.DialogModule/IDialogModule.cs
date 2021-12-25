using System;

namespace ModuleBased.Example.Drama.Dialog
{
    public interface IDialogModule 
    {

        IDramaAction Say(string name, string content);
    }
}
