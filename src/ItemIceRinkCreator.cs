using Vintagestory.API.Common;
using Vintagestory.API.MathTools;
using Vintagestory.API.Server;

namespace IceSkates
{
    public class ItemIceRinkCreator : Item
    {
        private const int RinkLength = 30; // Z axis
        private const int RinkWidth = 15;  // X axis
        private const int BoardHeight = 2;

        public override void OnHeldInteractStart(ItemSlot slot, EntityAgent byEntity, BlockSelection blockSel,
            EntitySelection entitySel, bool firstEvent, ref EnumHandHandling handling)
        {
            if (blockSel == null) return;
            if (byEntity.World.Side != EnumAppSide.Server)
            {
                handling = EnumHandHandling.PreventDefault;
                return;
            }

            handling = EnumHandHandling.PreventDefault;

            var world = byEntity.World;
            var center = blockSel.Position;
            int halfX = RinkWidth / 2;  // 7
            int halfZ = RinkLength / 2; // 15

            int y = center.Y;
            int minX = center.X - halfX;
            int maxX = center.X + halfX;
            int minZ = center.Z - halfZ;
            int maxZ = center.Z + halfZ;

            Block iceBlock = world.GetBlock(new AssetLocation("game:lakeice"));
            Block planksNS = world.GetBlock(new AssetLocation("game:planks-oak-ns"));
            Block planksWE = world.GetBlock(new AssetLocation("game:planks-oak-we"));
            Block planksFallback = world.GetBlock(new AssetLocation("game:planks-oak-ud"));

            if (iceBlock == null)
            {
                SendMessage(byEntity, "Could not find lake ice block.");
                return;
            }

            // Use fallback plank orientation if specific ones don't exist
            Block boardNS = planksNS ?? planksFallback;
            Block boardWE = planksWE ?? planksFallback;

            if (boardNS == null || boardWE == null)
            {
                SendMessage(byEntity, "Could not find plank blocks.");
                return;
            }

            var pos = new BlockPos(0, 0, 0, center.dimension);

            // Surface pass: lay ice across entire rink area
            for (int x = minX; x <= maxX; x++)
            {
                for (int z = minZ; z <= maxZ; z++)
                {
                    pos.Set(x, y, z);
                    world.BlockAccessor.SetBlock(iceBlock.Id, pos);
                }
            }

            // Board pass: 2-high walls around perimeter with chamfered corners
            for (int layer = 1; layer <= BoardHeight; layer++)
            {
                int py = y + layer;

                // North wall (z = minZ) and South wall (z = maxZ) — run east-west
                for (int x = minX; x <= maxX; x++)
                {
                    bool isCorner = (x == minX || x == maxX);

                    // North wall
                    if (!isCorner)
                    {
                        pos.Set(x, py, minZ);
                        world.BlockAccessor.SetBlock(boardWE.Id, pos);
                    }

                    // South wall
                    if (!isCorner)
                    {
                        pos.Set(x, py, maxZ);
                        world.BlockAccessor.SetBlock(boardWE.Id, pos);
                    }
                }

                // East wall (x = maxX) and West wall (x = minX) — run north-south
                for (int z = minZ; z <= maxZ; z++)
                {
                    bool isCorner = (z == minZ || z == maxZ);

                    // West wall
                    if (!isCorner)
                    {
                        pos.Set(minX, py, z);
                        world.BlockAccessor.SetBlock(boardNS.Id, pos);
                    }

                    // East wall
                    if (!isCorner)
                    {
                        pos.Set(maxX, py, z);
                        world.BlockAccessor.SetBlock(boardNS.Id, pos);
                    }
                }
            }

            world.BlockAccessor.Commit();

            // Sound + message feedback
            world.PlaySoundAt(new AssetLocation("game:sounds/environment/largesplash1"),
                center.X, center.Y, center.Z);
            SendMessage(byEntity, "Ice rink created! (30x15)");
        }

        private void SendMessage(EntityAgent byEntity, string message)
        {
            var player = (byEntity as EntityPlayer)?.Player;
            if (player == null)
                player = byEntity.World.PlayerByUid((byEntity as EntityPlayer)?.PlayerUID);

            if (player is IServerPlayer sp)
            {
                sp.SendMessage(0, message, EnumChatType.Notification);
            }
        }
    }
}
