using Cheap2.Plugin.Pool;

namespace ModuleBased.Example.Drama.Portrait
{
    public class PortraitAgent : UIAgent
    {
        public IPool<PortraitAgent> pool { get; set; }

        public override void Dispose(bool destroyed)
        {
            base.Dispose(destroyed);
            if (destroyed)
                return;
            pool.Push(this);
        }
    }
}
