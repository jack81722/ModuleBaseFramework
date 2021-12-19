using System;

namespace ModuleBased.Example.Drama.Dialog
{
    public interface IDialogModule
    {
        void Load(string chapterName);

        void RegisterAction(string name, Func<string[], IDramaAction> action);

        void Play();

        void Pause();

        void Resume();

        void Stop();
    }
}
