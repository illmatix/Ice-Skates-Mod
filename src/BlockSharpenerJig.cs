using System;
using Vintagestory.API.Common;
using Vintagestory.API.MathTools;

namespace IceSkates
{
    /// <summary>
    /// Sharpening jig — a placed block for workshop-level skate repair.
    /// 
    /// Historically, skate sharpeners were wooden cradles that held
    /// the blade at a fixed angle against a honing stone. This block
    /// works similarly: place it on any surface, right-click while
    /// holding skates to sharpen them.
    /// 
    /// Crafted from: boards + stone block + nails.
    /// Has limited uses before breaking (configurable).
    /// Restores ~45% durability per use (configurable).
    /// </summary>
    public class BlockSharpenerJig : Block
    {
        public override bool OnBlockInteractStart(
            IWorldAccessor world,
            IPlayer byPlayer,
            BlockSelection blockSel)
        {
            // Check if player is holding ice skates
            ItemSlot activeSlot = byPlayer.InventoryManager.ActiveHotbarSlot;
            if (activeSlot == null || activeSlot.Empty) return base.OnBlockInteractStart(world, byPlayer, blockSel);

            if (activeSlot.Itemstack?.Item is not ItemIceSkates) return base.OnBlockInteractStart(world, byPlayer, blockSel);

            var stack = activeSlot.Itemstack;
            int maxDura = stack.Collectible.GetMaxDurability(stack);
            int remainingDura = stack.Attributes?.GetInt("durability", maxDura) ?? maxDura;

            // Check if skates need repair
            if (remainingDura >= maxDura)
            {
                if (world.Side == EnumAppSide.Client)
                {
                    (world.Api as Vintagestory.API.Client.ICoreClientAPI)?
                        .TriggerIngameError(this, "nodamage", "These skates are already sharp.");
                }
                return true;
            }

            // Check jig uses remaining
            BlockEntitySharpenerJig be = world.BlockAccessor.GetBlockEntity(blockSel.Position)
                as BlockEntitySharpenerJig;

            if (be == null) return base.OnBlockInteractStart(world, byPlayer, blockSel);

            if (be.UsesRemaining <= 0)
            {
                if (world.Side == EnumAppSide.Client)
                {
                    (world.Api as Vintagestory.API.Client.ICoreClientAPI)?
                        .TriggerIngameError(this, "jigeworn", "This sharpening jig is worn out.");
                }
                return true;
            }

            // --- Perform sharpening (server side) ---
            if (world.Side == EnumAppSide.Server)
            {
                int repairPct = IceSkatesMod.Config?.SharpenerJigRepairPercent ?? 45;
                int repairAmount = Math.Max(1, (int)(maxDura * repairPct / 100f));
                int newDura = Math.Min(maxDura, remainingDura + repairAmount);

                stack.Attributes.SetInt("durability", newDura);
                activeSlot.MarkDirty();

                // Consume a jig use
                be.UsesRemaining--;
                be.MarkDirty(true);

                // If jig is fully worn, break it
                if (be.UsesRemaining <= 0)
                {
                    world.BlockAccessor.BreakBlock(blockSel.Position, byPlayer);
                }
            }

            // Feedback (both sides)
            world.PlaySoundAt(
                new AssetLocation("sounds/effect/stonecrush"),
                blockSel.Position.X + 0.5, blockSel.Position.Y + 0.5, blockSel.Position.Z + 0.5,
                byPlayer, true, 12f
            );

            if (world.Side == EnumAppSide.Client)
            {
                int repairPct = IceSkatesMod.Config?.SharpenerJigRepairPercent ?? 45;
                int repairAmount = Math.Max(1, (int)(maxDura * repairPct / 100f));
                int newDura = Math.Min(maxDura, remainingDura + repairAmount);
                int pctNow = (int)(newDura * 100f / maxDura);

                (world.Api as Vintagestory.API.Client.ICoreClientAPI)?
                    .ShowChatMessage($"Sharpened skates on jig (+{repairAmount} durability, now {pctNow}%). Jig uses: {(be?.UsesRemaining ?? 0)}");
            }

            // Spawn some stone dust particles
            if (world.Side == EnumAppSide.Client)
            {
                Vec3d pos = blockSel.Position.ToVec3d().Add(0.5, 0.75, 0.5);
                for (int i = 0; i < 8; i++)
                {
                    world.SpawnParticles(1,
                        ColorUtil.ToRgba(200, 180, 170, 160),
                        pos.AddCopy(-0.1, 0, -0.1),
                        pos.AddCopy(0.1, 0.2, 0.1),
                        new Vec3f(-0.3f, 0.2f, -0.3f),
                        new Vec3f(0.3f, 0.5f, 0.3f),
                        0.5f, 0.5f, 0.1f, EnumParticleModel.Cube);
                }
            }

            return true;
        }

        public override string GetPlacedBlockInfo(IWorldAccessor world, BlockPos pos, IPlayer forPlayer)
        {
            BlockEntitySharpenerJig be = world.BlockAccessor.GetBlockEntity(pos)
                as BlockEntitySharpenerJig;

            if (be != null)
            {
                int maxUses = IceSkatesMod.Config?.SharpenerJigMaxUses ?? 15;
                return $"Sharpening Jig\nUses remaining: {be.UsesRemaining}/{maxUses}\n" +
                       "Right-click with ice skates to sharpen.";
            }

            return base.GetPlacedBlockInfo(world, pos, forPlayer);
        }
    }
}
