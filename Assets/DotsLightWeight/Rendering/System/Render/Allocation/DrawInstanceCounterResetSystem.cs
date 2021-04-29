using DotsLite.Misc;
using Unity.Entities;

namespace DotsLite.Draw
{

    ////[UpdateBefore( typeof( DrawCullingSystem ) )]
    ////[UpdateInGroup(typeof( SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup))]
    [UpdateInGroup(typeof(SystemGroup.Presentation.DrawModel.DrawPrevSystemGroup.ResetCounter))]
    public class DrawInstanceCounterResetSystem : ComponentSystem
    {

        protected override void OnStartRunning()
        {
            this.Entities
                .ForEach(
                    ( ref DrawModel.InstanceCounterData counter ) =>
                    {
                        counter.InstanceCounter = new ThreadSafeCounter<Persistent>(0);
                    }
                );
        }

        protected override void OnUpdate()
        {

            this.Entities
                .ForEach(
                    ( ref DrawModel.InstanceCounterData counter ) =>
                    {
                        counter.InstanceCounter.Reset();
                    }
                );

        }

        protected override void OnDestroy()
        {
            this.Entities
                .ForEach(
                    ( ref DrawModel.InstanceCounterData counter ) =>
                    {
                        counter.InstanceCounter.Dispose();
                    }
                );
        }

    }

}
