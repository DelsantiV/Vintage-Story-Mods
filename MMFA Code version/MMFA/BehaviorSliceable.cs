using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.GameContent;
using Vintagestory.API.Datastructures;
using System.Text;
using Vintagestory.API.MathTools;
using Vintagestory.ServerMods;

namespace MMFA
{

    public class BlockBehaviorSliceable : BlockBehavior
    {
        int totalNumberOfSlices;
        int leftNumberOfSlices;
        AssetLocation sliceItemCode;
        public ItemStack sliceStack;

        public AssetLocation slicingSound;

        string baseSlicedBlockCode;
        AssetLocation slicedBlockCode;
        Block slicedBlock;
        int slicedBlockId;
        string interactionHelpCode;

        public BlockBehaviorSliceable(Block block) : base(block)
        {
        }

        public override void Initialize(JsonObject properties)
        {
            base.Initialize(properties);

            interactionHelpCode = properties["blockhelp"].AsString("mymodestfoodad:blockhelp-sliceable-cut");
            totalNumberOfSlices = properties["numberOfSlices"].AsInt(0);
            leftNumberOfSlices = totalNumberOfSlices;
            baseSlicedBlockCode = properties["slicedBlockCode"].AsString();


            string code = properties["sliceItemCode"].AsString();
            if (code != null)
            {
                sliceItemCode = AssetLocation.Create(code);
            }

            code = properties["slicingSound"].AsString("game:sounds/block/planks");
            if (code != null)
            {
                slicingSound = AssetLocation.Create(code);
            }
        }


        public override void OnLoaded(ICoreAPI api)
        {
            base.OnLoaded(api);
            sliceStack = new ItemStack(api.World.GetItem(sliceItemCode));
            if (sliceStack == null)
            {
                api.World.Logger.Warning("Unable to resolve slice item code '{0}' for block {1}. Will ignore.", sliceStack, block.Code);
            }

            slicedBlock = ActualizeSlicedBlock(leftNumberOfSlices, api.World);
        }

        public Block ActualizeSlicedBlock(int numberOfSlices, IWorldAccessor world)
        {
            if (leftNumberOfSlices <= 2) { return null; } // If 2 slices left, slicing will give 2 slices and destroy block.
            string code = baseSlicedBlockCode + "-" + (numberOfSlices - 1).ToString() + "slice"; 
            slicedBlockCode = AssetLocation.Create(code); 
            slicedBlock = world.GetBlock(slicedBlockCode);
            if (slicedBlock == null)
            {
                world.Logger.Warning("Unable to resolve sliced block code '{0}' for block {1}. Will ignore.", slicedBlockCode, block.Code);
            }
            return slicedBlock;
        }


        public override bool OnBlockInteractStart(IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handling)
        {
            handling = EnumHandling.PreventDefault;
            EnumTool? tool = byPlayer.InventoryManager.ActiveHotbarSlot?.Itemstack?.Collectible.Tool;

            if (!world.Claims.TryAccess(byPlayer, blockSel.Position, EnumBlockAccessFlags.Use))
            {
                return false;
            }

            if (tool == EnumTool.Knife || tool == EnumTool.Sword)
            {
                if (leftNumberOfSlices > 1 && sliceStack != null && world.Side == EnumAppSide.Client)
                {
                    world.PlaySoundAt(slicingSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);

                    if (leftNumberOfSlices == 2) { sliceStack.StackSize = 2; }

                    var origStack = sliceStack.Clone();

                    if (!byPlayer.InventoryManager.TryGiveItemstack(sliceStack))
                    {
                        world.SpawnItemEntity(sliceStack, blockSel.Position.ToVec3d().Add(0.5, 0.5, 0.5));
                    }

                    TreeAttribute tree = new TreeAttribute();
                    tree["itemstack"] = new ItemstackAttribute(origStack.Clone());
                    tree["byentityid"] = new LongAttribute(byPlayer.Entity.EntityId);
                    world.Api.Event.PushEvent("onitemcollected", tree);

                    if (slicedBlock != null)
                    {
                        world.BlockAccessor.SetBlock(slicedBlock.BlockId, blockSel.Position);
                    }
                    else
                    {
                        world.BlockAccessor.SetBlock(0, blockSel.Position);
                    }

                    leftNumberOfSlices -= 1;
                    slicedBlock = ActualizeSlicedBlock(leftNumberOfSlices, world);


                    world.PlaySoundAt(slicingSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
                    return true;
                }
                else
                {

                }
            }
            return false;
        }

        /*
        public override bool OnBlockInteractStep(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handled)
        {
            if (blockSel == null) return false;

            handled = EnumHandling.PreventDefault;

            (byPlayer as IClientPlayer)?.TriggerFpAnimation(EnumHandInteract.HeldItemAttack);

            if (world.Rand.NextDouble() < 0.05)
            {
                world.PlaySoundAt(slicingSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
            }

            if (world.Side == EnumAppSide.Client && world.Rand.NextDouble() < 0.25)
            {
                world.SpawnCubeParticles(blockSel.Position.ToVec3d().Add(blockSel.HitPosition), sliceStack, 0.25f, 1, 0.5f, byPlayer, new Vec3f(0, 1, 0));
            }

            return world.Side == EnumAppSide.Client || secondsUsed < slicingTime;
        }

        public override void OnBlockInteractStop(float secondsUsed, IWorldAccessor world, IPlayer byPlayer, BlockSelection blockSel, ref EnumHandling handled)
        {
            handled = EnumHandling.PreventDefault;


            if (secondsUsed > slicingTime - 0.05f && sliceStack != null && world.Side == EnumAppSide.Server)
            {
                if (sliceStack == null) return;
                var origStack = sliceStack.Clone();

                if (!byPlayer.InventoryManager.TryGiveItemstack(sliceStack))
                {
                    world.SpawnItemEntity(sliceStack, blockSel.Position.ToVec3d().Add(0.5, 0.5, 0.5));
                }

                TreeAttribute tree = new TreeAttribute();
                tree["itemstack"] = new ItemstackAttribute(origStack.Clone());
                tree["byentityid"] = new LongAttribute(byPlayer.Entity.EntityId);
                world.Api.Event.PushEvent("onitemcollected", tree);

                if (slicedBlock != null)
                {
                    world.BlockAccessor.SetBlock(slicedBlock.BlockId, blockSel.Position);
                }

                world.PlaySoundAt(slicingSound, blockSel.Position.X, blockSel.Position.Y, blockSel.Position.Z, byPlayer);
            }
        }
        */

        public override WorldInteraction[] GetPlacedBlockInteractionHelp(IWorldAccessor world, BlockSelection selection, IPlayer forPlayer, ref EnumHandling handled)
        {
            if (sliceStack != null)
            {
                bool notProtected = true;

                if (world.Claims != null && world is IClientWorldAccessor clientWorld && clientWorld.Player?.WorldData.CurrentGameMode == EnumGameMode.Survival)
                {
                    EnumWorldAccessResponse resp = world.Claims.TestAccess(clientWorld.Player, selection.Position, EnumBlockAccessFlags.Use);
                    if (resp != EnumWorldAccessResponse.Granted) notProtected = false;
                }

                if (notProtected) return new WorldInteraction[]
                {
                    new WorldInteraction()
                    {
                        ActionLangCode = interactionHelpCode,
                        MouseButton = EnumMouseButton.Right
                    }
                };
            }

            return base.GetPlacedBlockInteractionHelp(world, selection, forPlayer, ref handled);
        }
    }
}