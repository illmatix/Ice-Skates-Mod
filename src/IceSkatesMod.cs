using System.Collections.Generic;
using Vintagestory.API.Common;
using Vintagestory.API.Server;
using Vintagestory.API.Client;
using Vintagestory.GameContent;

namespace IceSkates
{
    public class IceSkatesMod : ModSystem
    {
        public static IceSkatesConfig Config { get; private set; }

        public override void Start(ICoreAPI api)
        {
            base.Start(api);

            Config = api.LoadModConfig<IceSkatesConfig>("IceSkatesConfig.json");
            if (Config == null)
            {
                Config = new IceSkatesConfig();
                api.StoreModConfig(Config, "IceSkatesConfig.json");
            }

            api.RegisterItemClass("ItemIceSkates", typeof(ItemIceSkates));
            api.RegisterEntityBehaviorClass("iceskating", typeof(EntityBehaviorIceSkating));
            api.RegisterItemClass("ItemIceRinkCreator", typeof(ItemIceRinkCreator));
            api.RegisterBlockClass("BlockSharpenerJig", typeof(BlockSharpenerJig));
            api.RegisterBlockEntityClass("BESharpenerJig", typeof(BlockEntitySharpenerJig));
        }

        public override void StartServerSide(ICoreServerAPI sapi)
        {
            sapi.Event.PlayerNowPlaying += (serverPlayer) =>
            {
                var entity = serverPlayer.Entity;
                if (entity != null && !entity.HasBehavior("iceskating"))
                    entity.AddBehavior(new EntityBehaviorIceSkating(entity));
            };
        }

        public override void StartClientSide(ICoreClientAPI capi)
        {
            capi.Event.PlayerEntitySpawn += (byPlayer) =>
            {
                var entity = byPlayer.Entity;
                if (entity != null && !entity.HasBehavior("iceskating"))
                    entity.AddBehavior(new EntityBehaviorIceSkating(entity));
            };

            var debugHud = new HudIceSkatesDebug(capi);
            capi.ChatCommands.Create("icedebug")
                .WithDescription("Toggle ice skates debug HUD")
                .HandleWith((args) =>
                {
                    debugHud.Toggle();
                    return TextCommandResult.Success();
                });
        }

        public override void AssetsLoaded(ICoreAPI api)
        {
            base.AssetsLoaded(api);
            TextureGenerator.GenerateAndInject(api);
        }

        /// <summary>
        /// After assets load, prune disabled metals: remove smithing recipes
        /// and clear creative inventory for any configurable metal that's off.
        /// Always-on metals (bone, iron, meteoriciron, steel) are never pruned.
        /// </summary>
        public override void AssetsFinalize(ICoreAPI api)
        {
            base.AssetsFinalize(api);

            List<string> disabled = new List<string>();
            foreach (string metal in IceSkatesConfig.ConfigurableMetals)
            {
                if (!Config.IsMetalEnabled(metal))
                    disabled.Add(metal);
            }

            if (disabled.Count == 0) return;

            // Prune smithing recipes (server side)
            if (api.Side == EnumAppSide.Server)
            {
                var registry = api.ModLoader.GetModSystem<RecipeRegistrySystem>();
                if (registry != null)
                {
                    var recipes = registry.SmithingRecipes;
                    for (int i = recipes.Count - 1; i >= 0; i--)
                    {
                        string outputCode = recipes[i].Output?.Code?.Path ?? "";
                        foreach (string metal in disabled)
                        {
                            if (outputCode.Contains("skateblade-" + metal))
                            {
                                recipes.RemoveAt(i);
                                break;
                            }
                        }
                    }
                }
            }

            // Hide disabled items from creative inventory
            foreach (string metal in disabled)
            {
                // Hide blade item
                Item blade = api.World.GetItem(new AssetLocation("iceskates", "skateblade-" + metal));
                if (blade != null)
                    blade.CreativeInventoryTabs = new string[0];

                // Hide all strap variants of this blade
                foreach (string strap in new[] { "rawhide", "leather", "fur" })
                {
                    Item skate = api.World.GetItem(
                        new AssetLocation("iceskates", "iceskates-" + metal + "-" + strap));
                    if (skate != null)
                        skate.CreativeInventoryTabs = new string[0];
                }
            }
        }
    }
}
