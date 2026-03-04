using System;
using System.Text;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Datastructures;
using Vintagestory.API.MathTools;
using Vintagestory.GameContent;

namespace IceSkates
{
    /// <summary>
    /// Custom item class for ice skates. Handles two offhand interactions:
    /// 
    /// 1. LINING (offhand = linen/pelt/sturdyleather/woolcloth):
    ///    Permanent upgrade stored as tree attribute. Adds warmth + hunger reduction.
    /// 
    /// 2. WHETSTONE (offhand = item containing "whetstone"):
    ///    Field repair. Restores WhetstoneRepairPercent% of max durability.
    ///    Damages the whetstone by 1 durability.
    /// 
    /// Interaction priority: whetstone check first (since lined skates still
    /// need sharpening), then lining check.
    /// </summary>
    public class ItemIceSkates : ItemWearable
    {
        // ============================================
        // Lining definitions
        // ============================================

        public static float GetLiningWarmth(string lining)
        {
            return lining switch
            {
                "linen" => 1.0f,
                "pelt" => 2.5f,
                "sturdyleather" => 2.0f,
                "wool" => 3.0f,
                _ => 0f
            };
        }

        public static float GetLiningHungerReduction(string lining)
        {
            return lining switch
            {
                "linen" => -0.05f,
                "pelt" => -0.15f,
                "sturdyleather" => -0.20f,
                "wool" => -0.10f,
                _ => 0f
            };
        }

        public static string GetLiningDisplayName(string lining)
        {
            return lining switch
            {
                "linen" => "Linen",
                "pelt" => "Fur",
                "sturdyleather" => "Sturdy Leather",
                "wool" => "Wool",
                _ => "None"
            };
        }

        // ============================================
        // Right-click: whetstone repair OR lining
        // ============================================

        public override void OnHeldInteractStart(
            ItemSlot slot,
            EntityAgent byEntity,
            BlockSelection blockSel,
            EntitySelection entitySel,
            bool firstEvent,
            ref EnumHandHandling handling)
        {
            ItemSlot offhand = byEntity.LeftHandItemSlot;
            if (offhand == null || offhand.Empty)
            {
                base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
                return;
            }

            // --- Priority 1: Whetstone repair ---
            if (IsWhetstone(offhand.Itemstack))
            {
                TryWhetstoneRepair(slot, offhand, byEntity);
                handling = EnumHandHandling.PreventDefault;
                return;
            }

            // --- Priority 2: Lining upgrade ---
            string liningType = IdentifyLiningMaterial(offhand.Itemstack);
            if (liningType != null)
            {
                TryApplyLining(slot, offhand, byEntity, liningType);
                handling = EnumHandHandling.PreventDefault;
                return;
            }

            base.OnHeldInteractStart(slot, byEntity, blockSel, entitySel, firstEvent, ref handling);
        }

        // ============================================
        // Whetstone repair logic
        // ============================================

        private void TryWhetstoneRepair(ItemSlot skateSlot, ItemSlot whetstoneSlot, EntityAgent byEntity)
        {
            var stack = skateSlot.Itemstack;
            if (stack == null) return;

            int maxDura = GetMaxDurability(stack);
            int curDura = maxDura - stack.Collectible.GetRemainingDurability(stack);
            int damage = maxDura - (stack.Attributes?.GetInt("durability", maxDura) ?? maxDura);

            // Check if skates actually need repair
            int remainingDura = stack.Attributes?.GetInt("durability", maxDura) ?? maxDura;
            if (remainingDura >= maxDura)
            {
                if (byEntity.World.Side == EnumAppSide.Client && byEntity is EntityPlayer)
                {
                    (api as Vintagestory.API.Client.ICoreClientAPI)?
                        .TriggerIngameError(this, "nodamage", "These skates don't need sharpening.");
                }
                return;
            }

            // Calculate repair amount from config
            int repairPct = IceSkatesMod.Config?.WhetstoneRepairPercent ?? 12;
            int repairAmount = Math.Max(1, (int)(maxDura * repairPct / 100f));
            int newDura = Math.Min(maxDura, remainingDura + repairAmount);

            // Apply repair
            stack.Attributes.SetInt("durability", newDura);
            skateSlot.MarkDirty();

            // Damage the whetstone
            if (byEntity.World.Side == EnumAppSide.Server)
            {
                whetstoneSlot.Itemstack.Collectible.DamageItem(
                    byEntity.World, byEntity, whetstoneSlot, 1
                );
                whetstoneSlot.MarkDirty();
            }

            // Feedback
            byEntity.World.PlaySoundAt(
                new AssetLocation("sounds/effect/stonecrush"),
                byEntity, null, true, 8f
            );

            if (byEntity.World.Side == EnumAppSide.Client && byEntity is EntityPlayer)
            {
                int pctNow = (int)(newDura * 100f / maxDura);
                (api as Vintagestory.API.Client.ICoreClientAPI)?
                    .ShowChatMessage($"Sharpened skates (+{repairAmount} durability, now {pctNow}%).");
            }
        }

        /// <summary>
        /// Matches offhand items that function as whetstones.
        /// Broad pattern matching for vanilla 1.22+, plus existing mods.
        /// </summary>
        private bool IsWhetstone(ItemStack stack)
        {
            if (stack?.Collectible == null) return false;
            string code = stack.Collectible.Code?.ToString() ?? "";
            // Match any item with "whetstone" in its code (any domain)
            return code.Contains("whetstone");
        }

        // ============================================
        // Lining logic
        // ============================================

        private void TryApplyLining(ItemSlot skateSlot, ItemSlot liningSlot, EntityAgent byEntity, string liningType)
        {
            string currentLining = GetLining(skateSlot.Itemstack);
            if (currentLining != "none")
            {
                if (byEntity.World.Side == EnumAppSide.Client && byEntity is EntityPlayer)
                {
                    (api as Vintagestory.API.Client.ICoreClientAPI)?
                        .TriggerIngameError(this, "alreadylined",
                            "Already lined with " + GetLiningDisplayName(currentLining) + ".");
                }
                return;
            }

            if (skateSlot.Itemstack.Attributes == null)
                skateSlot.Itemstack.Attributes = new TreeAttribute();

            skateSlot.Itemstack.Attributes.SetString("lining", liningType);
            liningSlot.TakeOut(1);
            liningSlot.MarkDirty();
            skateSlot.MarkDirty();

            byEntity.World.PlaySoundAt(
                new AssetLocation("sounds/player/clothrustles"),
                byEntity, null, true, 8f
            );

            if (byEntity.World.Side == EnumAppSide.Client && byEntity is EntityPlayer)
            {
                (api as Vintagestory.API.Client.ICoreClientAPI)?
                    .ShowChatMessage("Lined skates with " + GetLiningDisplayName(liningType) + ".");
            }
        }

        private string IdentifyLiningMaterial(ItemStack stack)
        {
            if (stack?.Collectible == null) return null;
            string code = stack.Collectible.Code?.Path ?? "";

            if (code.Contains("sturdyleather") || code.Contains("leather-sturdy"))
                return "sturdyleather";
            if (code.StartsWith("linen"))
                return "linen";
            if (code.StartsWith("pelt"))
                return "pelt";
            if (code.Contains("woolcloth") || code.Contains("wool-cloth") ||
                (code.Contains("wool") && code.Contains("cloth")))
                return "wool";

            return null;
        }

        // ============================================
        // Variant/stat accessors
        // ============================================

        public string GetBladeVariant()
        {
            string path = Code?.Path ?? "";
            string[] parts = path.Split('-');
            if (parts.Length >= 2) return parts[1];
            return "bone";
        }

        public string GetStrapVariant()
        {
            string path = Code?.Path ?? "";
            string[] parts = path.Split('-');
            if (parts.Length >= 3) return parts[parts.Length - 1];
            return "rawhide";
        }

        public float GetSpeedBonus()
        {
            string blade = GetBladeVariant();
            return Attributes?["skateStats"]?["bladeSpeedBonus"]?[blade]?.AsFloat(0.2f) ?? 0.2f;
        }

        public float GetOffIcePenalty()
        {
            string strap = GetStrapVariant();
            return Attributes?["skateStats"]?["strapControl"]?[strap]?.AsFloat(-0.15f) ?? -0.15f;
        }

        public float GetBaseHungerMultiplier()
        {
            string strap = GetStrapVariant();
            return Attributes?["skateStats"]?["strapHungerRate"]?[strap]?.AsFloat(1.0f) ?? 1.0f;
        }

        public static string GetLining(ItemStack stack)
        {
            return stack?.Attributes?.GetString("lining", "none") ?? "none";
        }

        // ============================================
        // Tooltip
        // ============================================

        public override void GetHeldItemInfo(ItemSlot inSlot, StringBuilder dsc, IWorldAccessor world, bool withDebugInfo)
        {
            base.GetHeldItemInfo(inSlot, dsc, world, withDebugInfo);

            string blade = GetBladeVariant();
            string strap = GetStrapVariant();
            string lining = GetLining(inSlot.Itemstack);

            float speedBonus = GetSpeedBonus();
            float offIcePenalty = GetOffIcePenalty();
            float baseHunger = GetBaseHungerMultiplier();
            float liningHungerRed = GetLiningHungerReduction(lining);
            float liningWarmth = GetLiningWarmth(lining);
            float netHungerMod = (baseHunger - 1.0f) + liningHungerRed;

            int speedPct = (int)(speedBonus * 100);
            int penaltyPct = (int)(offIcePenalty * 100);
            int hungerPct = (int)(netHungerMod * 100);

            dsc.AppendLine();
            dsc.AppendLine($"Blade: {CapFirst(blade)} | Strap: {CapFirst(strap)}");

            if (lining != "none")
                dsc.AppendLine($"Lining: {GetLiningDisplayName(lining)} (+{liningWarmth:0.#}\u00b0C warmth)");
            else
                dsc.AppendLine("<font color=\"#AAAAAA\">Unlined \u2014 offhand lining material + right-click to upgrade</font>");

            dsc.AppendLine();
            dsc.AppendLine("<font color=\"#84DCFF\">On ice:</font>");
            dsc.AppendLine($"  +{speedPct}% movement speed");

            if (hungerPct > 0)
                dsc.AppendLine($"  +{hungerPct}% hunger drain while skating");
            else if (hungerPct < 0)
                dsc.AppendLine($"  <font color=\"#84FF84\">{hungerPct}% hunger drain while skating</font>");
            else
                dsc.AppendLine("  Normal hunger drain");

            if (liningWarmth > 0)
                dsc.AppendLine($"  <font color=\"#84FF84\">+{liningWarmth:0.#}\u00b0C warmth</font>");

            dsc.AppendLine("<font color=\"#FF8484\">Off ice:</font>");
            dsc.AppendLine($"  {penaltyPct}% movement speed");

            dsc.AppendLine();
            dsc.AppendLine("<font color=\"#AAAAAA\">Sharpen with whetstone (offhand) or sharpening jig</font>");
        }

        private string CapFirst(string s)
        {
            if (string.IsNullOrEmpty(s)) return s;
            return s switch
            {
                "meteoriciron" => "Meteoric Iron",
                "blistersteel" => "Blister Steel",
                "rawhide" => "Rawhide",
                "sturdyleather" => "Sturdy Leather",
                "tinbronze" => "Tin Bronze",
                "bismuthbronze" => "Bismuth Bronze",
                "blackbronze" => "Black Bronze",
                _ => char.ToUpper(s[0]) + s.Substring(1)
            };
        }
    }
}
