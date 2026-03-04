using Vintagestory.API.Common;
using Vintagestory.API.Datastructures;

namespace IceSkates
{
    /// <summary>
    /// Block entity for the sharpening jig. Tracks remaining uses.
    /// Uses are saved/loaded via tree attributes so they persist
    /// across world saves and chunk loads.
    /// </summary>
    public class BlockEntitySharpenerJig : BlockEntity
    {
        public int UsesRemaining { get; set; }

        public override void Initialize(ICoreAPI api)
        {
            base.Initialize(api);

            // Initialize uses on first placement
            if (UsesRemaining <= 0)
            {
                UsesRemaining = IceSkatesMod.Config?.SharpenerJigMaxUses ?? 15;
            }
        }

        public override void ToTreeAttributes(ITreeAttribute tree)
        {
            base.ToTreeAttributes(tree);
            tree.SetInt("usesRemaining", UsesRemaining);
        }

        public override void FromTreeAttributes(ITreeAttribute tree, IWorldAccessor worldForResolve)
        {
            base.FromTreeAttributes(tree, worldForResolve);
            UsesRemaining = tree.GetInt("usesRemaining",
                IceSkatesMod.Config?.SharpenerJigMaxUses ?? 15);
        }
    }
}
