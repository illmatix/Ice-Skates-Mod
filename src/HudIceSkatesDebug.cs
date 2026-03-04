using System;
using Vintagestory.API.Client;
using Vintagestory.API.Common;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace IceSkates
{
    public class HudIceSkatesDebug : HudElement
    {
        private long tickListenerId;
        private bool hudEnabled = false;

        public HudIceSkatesDebug(ICoreClientAPI capi) : base(capi)
        {
            SetupDialog();
        }

        private void SetupDialog()
        {
            ElementBounds textBounds = ElementBounds.Fixed(10, 10, 280, 160);
            ElementBounds bgBounds = textBounds.ForkBoundingParent(5, 5, 5, 5);

            SingleComposer = capi.Gui
                .CreateCompo("iceskates-debug-hud", bgBounds)
                .AddDialogBG(ElementBounds.Fill, false)
                .AddDynamicText("", CairoFont.WhiteSmallText(), textBounds, "debugtext")
                .Compose();
        }

        public new void Toggle()
        {
            hudEnabled = !hudEnabled;

            if (hudEnabled)
            {
                TryOpen();
                tickListenerId = capi.Event.RegisterGameTickListener(OnTick, 250);
            }
            else
            {
                if (tickListenerId != 0)
                {
                    capi.Event.UnregisterGameTickListener(tickListenerId);
                    tickListenerId = 0;
                }
                TryClose();
            }
        }

        private void OnTick(float dt)
        {
            if (!IsOpened()) return;

            var player = capi.World.Player;
            if (player?.Entity == null) return;

            var inv = player.InventoryManager?.GetOwnInventory(GlobalConstants.characterInvClassName);
            int footIndex = (int)EnumCharacterDressType.Foot;
            ItemSlot footSlot = (inv != null && inv.Count > footIndex) ? inv[footIndex] : null;
            bool wearingSkates = footSlot != null && !footSlot.Empty
                && footSlot.Itemstack?.Item is ItemIceSkates;

            if (!wearingSkates)
            {
                SingleComposer.GetDynamicText("debugtext")
                    .SetNewText("[IceSkates Debug]\nSkates: Not equipped");
                return;
            }

            var skates = footSlot.Itemstack.Item as ItemIceSkates;
            string variant = skates.GetBladeVariant() + "-" + skates.GetStrapVariant();
            string lining = ItemIceSkates.GetLining(footSlot.Itemstack);
            string liningStr = lining != "none" ? $" [{ItemIceSkates.GetLiningDisplayName(lining)}]" : "";

            bool onIce = IsOnIce(player.Entity);
            string surface = onIce ? "Ice" : "Not Ice";

            // Read physics state from the behavior
            var behavior = player.Entity.GetBehavior<EntityBehaviorIceSkating>();
            string inputStr = "N/A";
            string speedStr = "0.000 / 0.000 max";
            string hungerStr = "0%";

            if (behavior != null)
            {
                inputStr = behavior.DebugInputState.ToString();
                double speed = behavior.DebugSpeed;
                float maxSpeed = behavior.DebugMaxSpeed;
                speedStr = $"{speed:0.000} / {maxSpeed:0.000} max";

                float hungerMod = behavior.DebugCurrentHungerMod;
                string hungerSign = hungerMod >= 0 ? "+" : "";
                hungerStr = $"{hungerSign}{hungerMod * 100:0}%";
            }

            string text =
                $"[IceSkates Debug]\n" +
                $"Surface: {surface}\n" +
                $"Skates: Equipped ({variant}{liningStr})\n" +
                $"Input: {inputStr}\n" +
                $"Speed: {speedStr}\n" +
                $"Hunger mod: {hungerStr}";

            SingleComposer.GetDynamicText("debugtext").SetNewText(text);
        }

        private bool IsOnIce(EntityPlayer p)
        {
            BlockPos bp = p.Pos.AsBlockPos.DownCopy();
            Block b = p.World.BlockAccessor.GetBlock(bp);
            if (IsIceBlock(b)) return true;
            return IsIceBlock(p.World.BlockAccessor.GetBlock(bp.DownCopy()));
        }

        private bool IsIceBlock(Block b)
        {
            if (b == null) return false;
            string c = b.Code?.ToString() ?? "";
            return c.Contains("lakeice") || c.Contains("glacierice") || b.BlockMaterial == EnumBlockMaterial.Ice;
        }

        public override string ToggleKeyCombinationCode => null;

        public override void Dispose()
        {
            if (tickListenerId != 0)
            {
                capi.Event.UnregisterGameTickListener(tickListenerId);
                tickListenerId = 0;
            }
            base.Dispose();
        }
    }
}
