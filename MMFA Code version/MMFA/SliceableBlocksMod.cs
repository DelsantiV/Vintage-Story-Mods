using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.Server;

namespace MMFA
{
    public class SliceableBlocksMod : ModSystem
    {
        // Called on server and client
        public override void Start(ICoreAPI api)
        {
            api.Logger.Notification("My Modest food adjustments was loaded !");

            base.Start(api);
            api.RegisterBlockBehaviorClass("Sliceable", typeof(BlockBehaviorSliceable));
            api.RegisterBlockClass("SliceableBlock", typeof(SliceableBlock));
            api.Logger.Notification("My Modest food adjustments classes successfully registered !");
        }
    }
}