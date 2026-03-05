using System;
using Vintagestory.API.Common;
using Vintagestory.API.Common.Entities;
using Vintagestory.API.Config;
using Vintagestory.API.MathTools;

namespace IceSkates
{
    public enum SkateInputState
    {
        None,
        Thrust,
        Brake,
        CarvingLeft,
        CarvingRight
    }

    public class EntityBehaviorIceSkating : EntityBehavior
    {
        private const string SPEED_STAT_KEY = "iceskatesspeed";
        private const string HUNGER_STAT_KEY = "iceskateshunger";

        // Two-tier tick accumulators
        private float physicsAccumulator = 0f;
        private float heavyAccumulator = 0f;
        private const float HEAVY_TICK_INTERVAL = 0.2f;

        private float durabilityAccumulator = 0f;
        private const float DURABILITY_INTERVAL = 4.0f;
        private float particleAccumulator = 0f;
        private const float PARTICLE_INTERVAL = 0.08f;

        // Skating state
        private bool isSkating = false;
        private double skateVelX = 0;
        private double skateVelZ = 0;
        private bool wasWearingSkates = false;
        private bool wasOnIce = false;

        // Debug HUD state
        public bool DebugIsOnIce { get; private set; }
        public bool DebugIsWearingSkates { get; private set; }
        public bool DebugIsSkating => isSkating;
        public SkateInputState DebugInputState { get; private set; }
        public double DebugSpeed => entity.World.Side == EnumAppSide.Client
            ? clientDebugSpeed
            : Math.Sqrt(skateVelX * skateVelX + skateVelZ * skateVelZ);
        public float DebugMaxSpeed { get; private set; }
        public float DebugCurrentHungerMod { get; private set; }
        public string DebugSkateVariant { get; private set; } = "";

        private double clientDebugSpeed;
        private static SimpleParticleProperties snowParticles;

        public EntityBehaviorIceSkating(Entity entity) : base(entity)
        {
            InitParticles();
        }

        public override string PropertyName() => "iceskating";

        private static void InitParticles()
        {
            if (snowParticles != null) return;
            snowParticles = new SimpleParticleProperties(
                2, 6, ColorUtil.ColorFromRgba(230, 240, 255, 180),
                new Vec3d(), new Vec3d(),
                new Vec3f(-0.3f, 0.15f, -0.3f), new Vec3f(0.3f, 0.4f, 0.3f),
                0.6f, 0.4f, 0.05f, 0.15f, EnumParticleModel.Cube
            );
            snowParticles.SizeEvolve = EvolvingNatFloat.create(EnumTransformFunction.LINEARREDUCE, 0.1f);
            snowParticles.OpacityEvolve = EvolvingNatFloat.create(EnumTransformFunction.LINEARREDUCE, 180);
            snowParticles.SelfPropelled = false;
        }

        public override void OnGameTick(float deltaTime)
        {
            if (entity is not EntityPlayer playerEntity) return;

            var config = IceSkatesMod.Config;
            float physicsInterval = config?.PhysicsTickInterval ?? 0.05f;

            bool isServer = entity.World.Side == EnumAppSide.Server;
            bool isClient = entity.World.Side == EnumAppSide.Client;

            // --- Physics tick (20Hz) ---
            physicsAccumulator += deltaTime;
            while (physicsAccumulator >= physicsInterval)
            {
                physicsAccumulator -= physicsInterval;
                if (isSkating && isServer)
                    UpdateSkatingPhysics(playerEntity, config);
            }

            // --- Client-side debug state (mirrors server debug values for HUD) ---
            if (isClient && isSkating)
                UpdateClientDebugState(playerEntity, config);

            // --- Heavy tick (5Hz) ---
            heavyAccumulator += deltaTime;
            if (heavyAccumulator >= HEAVY_TICK_INTERVAL)
            {
                heavyAccumulator = 0f;
                UpdateHeavyTick(playerEntity, isServer, isClient, config);
            }

            // --- Particles (client-side, ~12Hz) ---
            if (isClient && isSkating)
            {
                particleAccumulator += deltaTime;
                if (particleAccumulator >= PARTICLE_INTERVAL)
                {
                    particleAccumulator = 0f;
                    SpawnSkatingParticles(playerEntity);
                }
            }
        }

        private void UpdateSkatingPhysics(EntityPlayer playerEntity, IceSkatesConfig config)
        {
            float thrust = config?.ThrustForce ?? 0.006f;
            float maxSpeed = config?.MaxSkatingSpeed ?? 0.35f;
            float brakeForce = config?.BrakeForce ?? 0.02f;
            float turnBase = config?.TurnRateBase ?? 0.06f;
            float turnFalloff = config?.TurnRateSpeedFalloff ?? 3.0f;
            float coastFriction = config?.CoastFriction ?? 0.993f;
            float sprintMult = config?.SprintMultiplier ?? 1.4f;

            // Scale max speed and thrust by blade bonus
            ItemSlot footSlot = GetFootSlot(playerEntity);
            float bladeBonus = 0f;
            if (footSlot?.Itemstack?.Item is ItemIceSkates skates)
                bladeBonus = skates.GetSpeedBonus();

            float effectiveMaxSpeed = maxSpeed * (1f + bladeBonus);
            float effectiveThrust = thrust * (1f + bladeBonus);

            // Read controls
            var controls = playerEntity.ServerControls;
            bool forward = controls.Forward;
            bool backward = controls.Backward;
            bool left = controls.Left;
            bool right = controls.Right;
            bool sprint = controls.Sprint;
            bool onGround = playerEntity.OnGround;

            // Sprint multiplier
            if (sprint)
            {
                effectiveThrust *= sprintMult;
                effectiveMaxSpeed *= sprintMult;
            }

            double speed = Math.Sqrt(skateVelX * skateVelX + skateVelZ * skateVelZ);

            // Determine input state for debug/particles
            if (backward && speed > 0.001)
                DebugInputState = SkateInputState.Brake;
            else if (forward)
                DebugInputState = SkateInputState.Thrust;
            else if (left && speed > 0.001)
                DebugInputState = SkateInputState.CarvingLeft;
            else if (right && speed > 0.001)
                DebugInputState = SkateInputState.CarvingRight;
            else
                DebugInputState = SkateInputState.None;

            DebugMaxSpeed = effectiveMaxSpeed;

            // --- Thrust (forward) ---
            if (forward && onGround)
            {
                float yaw = playerEntity.ServerPos.Yaw;
                // VS yaw: 0=south, increases clockwise. Convert to motion direction.
                double dirX = -Math.Sin(yaw);
                double dirZ = -Math.Cos(yaw);

                skateVelX += dirX * effectiveThrust;
                skateVelZ += dirZ * effectiveThrust;

                // Cap speed
                speed = Math.Sqrt(skateVelX * skateVelX + skateVelZ * skateVelZ);
                if (speed > effectiveMaxSpeed)
                {
                    double scale = effectiveMaxSpeed / speed;
                    skateVelX *= scale;
                    skateVelZ *= scale;
                }
            }

            // --- Brake (backward) ---
            if (backward && onGround && speed > 0.001)
            {
                double decel = Math.Min(brakeForce, speed);
                double brakeDirX = skateVelX / speed;
                double brakeDirZ = skateVelZ / speed;
                skateVelX -= brakeDirX * decel;
                skateVelZ -= brakeDirZ * decel;
            }

            // --- Carving (left/right) ---
            if ((left || right) && speed > 0.001 && onGround)
            {
                float turnRate = turnBase / (1f + (float)speed * turnFalloff);
                if (left) turnRate = -turnRate; // left = counter-clockwise

                double cosT = Math.Cos(turnRate);
                double sinT = Math.Sin(turnRate);
                double newVelX = skateVelX * cosT - skateVelZ * sinT;
                double newVelZ = skateVelX * sinT + skateVelZ * cosT;
                skateVelX = newVelX;
                skateVelZ = newVelZ;
            }

            // --- Friction (always applies on ground) ---
            if (onGround)
            {
                skateVelX *= coastFriction;
                skateVelZ *= coastFriction;
            }

            // --- Stop threshold ---
            speed = Math.Sqrt(skateVelX * skateVelX + skateVelZ * skateVelZ);
            float stopThreshold = config?.SlideStopThreshold ?? 0.001f;
            if (speed < stopThreshold && !forward)
            {
                skateVelX = 0;
                skateVelZ = 0;
            }

            // --- Apply motion (overwrite horizontal, preserve vertical) ---
            playerEntity.ServerPos.Motion.X = skateVelX;
            playerEntity.ServerPos.Motion.Z = skateVelZ;
        }

        private void UpdateClientDebugState(EntityPlayer playerEntity, IceSkatesConfig config)
        {
            float maxSpeed = config?.MaxSkatingSpeed ?? 0.35f;
            float sprintMult = config?.SprintMultiplier ?? 1.4f;

            ItemSlot footSlot = GetFootSlot(playerEntity);
            float bladeBonus = 0f;
            if (footSlot?.Itemstack?.Item is ItemIceSkates skates)
                bladeBonus = skates.GetSpeedBonus();

            float effectiveMaxSpeed = maxSpeed * (1f + bladeBonus);

            var controls = playerEntity.Controls;
            if (controls.Sprint)
                effectiveMaxSpeed *= sprintMult;

            double motionX = playerEntity.Pos.Motion.X;
            double motionZ = playerEntity.Pos.Motion.Z;
            double speed = Math.Sqrt(motionX * motionX + motionZ * motionZ);

            if (controls.Backward && speed > 0.001)
                DebugInputState = SkateInputState.Brake;
            else if (controls.Forward)
                DebugInputState = SkateInputState.Thrust;
            else if (controls.Left && speed > 0.001)
                DebugInputState = SkateInputState.CarvingLeft;
            else if (controls.Right && speed > 0.001)
                DebugInputState = SkateInputState.CarvingRight;
            else
                DebugInputState = SkateInputState.None;

            DebugMaxSpeed = effectiveMaxSpeed;
            clientDebugSpeed = speed;
        }

        private void UpdateHeavyTick(EntityPlayer playerEntity, bool isServer, bool isClient, IceSkatesConfig config)
        {
            ItemSlot footSlot = GetFootSlot(playerEntity);
            bool wearingSkates = footSlot != null && !footSlot.Empty
                && footSlot.Itemstack?.Item is ItemIceSkates;

            if (!wearingSkates)
            {
                DebugIsWearingSkates = false;
                DebugIsOnIce = false;
                DebugCurrentHungerMod = 0f;
                DebugSkateVariant = "";
                DebugInputState = SkateInputState.None;

                if (wasWearingSkates)
                {
                    if (isServer) RemoveAllEffects(playerEntity);
                    StopSkating(playerEntity, isServer);
                    wasWearingSkates = false;
                }
                return;
            }

            wasWearingSkates = true;
            ItemIceSkates skates = footSlot.Itemstack.Item as ItemIceSkates;
            float offIcePenalty = skates.GetOffIcePenalty();
            float baseHungerMod = skates.GetBaseHungerMultiplier() - 1.0f;
            string lining = ItemIceSkates.GetLining(footSlot.Itemstack);
            float liningHungerRed = ItemIceSkates.GetLiningHungerReduction(lining);
            float netHungerMod = baseHungerMod + liningHungerRed;

            bool onIce = IsOnIce(playerEntity);

            // Update debug state
            DebugIsWearingSkates = true;
            DebugIsOnIce = onIce;
            DebugSkateVariant = skates.GetBladeVariant() + "-" + skates.GetStrapVariant();

            if (onIce)
            {
                // --- Transition onto ice ---
                if (!wasOnIce && !isSkating)
                {
                    // Seed velocity from current motion for smooth handoff
                    skateVelX = playerEntity.Pos.Motion.X;
                    skateVelZ = playerEntity.Pos.Motion.Z;
                    isSkating = true;
                }

                if (isServer)
                {
                    // Remove walkspeed stat on ice — physics tick controls motion directly
                    playerEntity.Stats.Remove(SPEED_STAT_KEY, "walkspeed");

                    // Hunger modifier when moving
                    double speed = Math.Sqrt(skateVelX * skateVelX + skateVelZ * skateVelZ);
                    if (speed > 0.01 && Math.Abs(netHungerMod) > 0.01f)
                        playerEntity.Stats.Set(HUNGER_STAT_KEY, "hungerrate", netHungerMod, false);
                    else
                        playerEntity.Stats.Remove(HUNGER_STAT_KEY, "hungerrate");
                }

                DebugCurrentHungerMod = netHungerMod;

                // --- Durability ---
                double curSpeed = Math.Sqrt(skateVelX * skateVelX + skateVelZ * skateVelZ);
                if (curSpeed > 0.01)
                {
                    durabilityAccumulator += HEAVY_TICK_INTERVAL;
                    if (durabilityAccumulator >= DURABILITY_INTERVAL)
                    {
                        durabilityAccumulator = 0f;
                        DamageSkates(footSlot, playerEntity);
                    }
                }
            }
            else
            {
                // Off ice
                if (isServer)
                {
                    playerEntity.Stats.Set(SPEED_STAT_KEY, "walkspeed", offIcePenalty, false);
                    playerEntity.Stats.Remove(HUNGER_STAT_KEY, "hungerrate");
                }

                DebugCurrentHungerMod = 0f;

                // Transition off ice while skating
                if (isSkating)
                {
                    float offIceBrake = config?.OffIceBrakeFactor ?? 0.3f;
                    if (isServer)
                    {
                        // One frame of braked momentum
                        playerEntity.ServerPos.Motion.X = skateVelX * offIceBrake;
                        playerEntity.ServerPos.Motion.Z = skateVelZ * offIceBrake;
                    }
                    StopSkating(playerEntity, isServer);
                }

                durabilityAccumulator = 0f;
                particleAccumulator = 0f;
            }

            wasOnIce = onIce;
        }

        private void StopSkating(EntityPlayer playerEntity, bool isServer)
        {
            isSkating = false;
            skateVelX = 0;
            skateVelZ = 0;
            DebugInputState = SkateInputState.None;
            if (isServer)
            {
                playerEntity.ServerPos.Motion.X = 0;
                playerEntity.ServerPos.Motion.Z = 0;
            }
        }

        private void SpawnSkatingParticles(EntityPlayer player)
        {
            if (snowParticles == null) return;

            double speed = Math.Sqrt(skateVelX * skateVelX + skateVelZ * skateVelZ);
            if (speed < 0.005) return;

            // Determine input state from client controls
            var controls = player.Controls;
            SkateInputState clientInput;
            if (controls.Backward && speed > 0.001)
                clientInput = SkateInputState.Brake;
            else if (controls.Forward)
                clientInput = SkateInputState.Thrust;
            else if (controls.Left && speed > 0.001)
                clientInput = SkateInputState.CarvingLeft;
            else if (controls.Right && speed > 0.001)
                clientInput = SkateInputState.CarvingRight;
            else
                clientInput = SkateInputState.None;

            double dirX = speed > 0.001 ? skateVelX / speed : 0;
            double dirZ = speed > 0.001 ? skateVelZ / speed : 0;

            Vec3d feetPos = player.Pos.XYZ.Clone();
            feetPos.Y += 0.05;

            Random rand = new Random();

            switch (clientInput)
            {
                case SkateInputState.Brake:
                    // Heavy spray in velocity direction (ice shavings)
                    snowParticles.MinQuantity = 6; snowParticles.AddQuantity = 10;
                    snowParticles.MinSize = 0.08f; snowParticles.MaxSize = 0.25f;
                    snowParticles.LifeLength = 0.8f;
                    snowParticles.MinVelocity = new Vec3f(
                        (float)(dirX * 1.5) - 0.3f, 0.2f, (float)(dirZ * 1.5) - 0.3f);
                    snowParticles.AddVelocity = new Vec3f(0.6f, 0.5f, 0.6f);
                    feetPos.X += dirX * 0.3; feetPos.Z += dirZ * 0.3;
                    // Double burst for heavy brake effect
                    snowParticles.MinPos = feetPos;
                    snowParticles.AddPos = new Vec3d(0.3, 0.1, 0.3);
                    snowParticles.Color = ColorUtil.ColorFromRgba(
                        210 + rand.Next(45), 225 + rand.Next(30), 240 + rand.Next(15), 160 + rand.Next(80));
                    player.World.SpawnParticles(snowParticles);
                    player.World.SpawnParticles(snowParticles);
                    return;

                case SkateInputState.CarvingLeft:
                case SkateInputState.CarvingRight:
                    // Perpendicular spray (outside of turn)
                    snowParticles.MinQuantity = 3; snowParticles.AddQuantity = 5;
                    snowParticles.MinSize = 0.05f; snowParticles.MaxSize = 0.18f;
                    snowParticles.LifeLength = 0.6f;
                    // Perpendicular to velocity: rotate 90 degrees
                    double perpX = clientInput == SkateInputState.CarvingLeft ? dirZ : -dirZ;
                    double perpZ = clientInput == SkateInputState.CarvingLeft ? -dirX : dirX;
                    snowParticles.MinVelocity = new Vec3f(
                        (float)(perpX * 0.8) - 0.15f, 0.15f, (float)(perpZ * 0.8) - 0.15f);
                    snowParticles.AddVelocity = new Vec3f(0.3f, 0.3f, 0.3f);
                    feetPos.X += perpX * 0.2; feetPos.Z += perpZ * 0.2;
                    break;

                case SkateInputState.Thrust:
                    // Kick spray behind skater
                    snowParticles.MinQuantity = 2; snowParticles.AddQuantity = 4;
                    snowParticles.MinSize = 0.04f; snowParticles.MaxSize = 0.14f;
                    snowParticles.LifeLength = 0.5f;
                    snowParticles.MinVelocity = new Vec3f(
                        (float)(-dirX * 0.6) - 0.1f, 0.1f, (float)(-dirZ * 0.6) - 0.1f);
                    snowParticles.AddVelocity = new Vec3f(0.2f, 0.25f, 0.2f);
                    feetPos.X -= dirX * 0.4; feetPos.Z -= dirZ * 0.4;
                    break;

                default:
                    // Coast — light trail under blades
                    snowParticles.MinQuantity = 1; snowParticles.AddQuantity = 2;
                    snowParticles.MinSize = 0.03f; snowParticles.MaxSize = 0.08f;
                    snowParticles.LifeLength = 0.4f;
                    snowParticles.MinVelocity = new Vec3f(
                        (float)(-dirX * 0.3) - 0.05f, 0.05f, (float)(-dirZ * 0.3) - 0.05f);
                    snowParticles.AddVelocity = new Vec3f(0.1f, 0.15f, 0.1f);
                    break;
            }

            snowParticles.MinPos = feetPos;
            snowParticles.AddPos = new Vec3d(0.3, 0.1, 0.3);
            snowParticles.Color = ColorUtil.ColorFromRgba(
                210 + rand.Next(45), 225 + rand.Next(30), 240 + rand.Next(15), 160 + rand.Next(80));
            player.World.SpawnParticles(snowParticles);
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

        private bool footSlotWarned = false;

        private ItemSlot GetFootSlot(EntityPlayer p)
        {
            IInventory inv = p.Player?.InventoryManager?.GetOwnInventory(GlobalConstants.characterInvClassName);

            if (inv == null)
            {
                IPlayer playerByUid = entity.World.PlayerByUid(p.PlayerUID);
                inv = playerByUid?.InventoryManager?.GetOwnInventory(GlobalConstants.characterInvClassName);
            }

            if (inv == null)
            {
                if (!footSlotWarned)
                {
                    entity.World.Logger.Warning("[IceSkates] Could not resolve character inventory for {0} (Player null: {1})",
                        p.GetName(), p.Player == null);
                    footSlotWarned = true;
                }
                return null;
            }

            footSlotWarned = false;
            int footIndex = (int)EnumCharacterDressType.Foot;
            return inv.Count > footIndex ? inv[footIndex] : null;
        }

        private void DamageSkates(ItemSlot s, EntityPlayer p)
        {
            if (s?.Itemstack == null || p.World.Side != EnumAppSide.Server) return;
            s.Itemstack.Collectible.DamageItem(p.World, p, s, 1);
            s.MarkDirty();
        }

        private void RemoveAllEffects(EntityPlayer p)
        {
            p.Stats.Remove(SPEED_STAT_KEY, "walkspeed");
            p.Stats.Remove(HUNGER_STAT_KEY, "hungerrate");
        }

        public override void OnEntityDespawn(EntityDespawnData d)
        {
            if (entity is EntityPlayer p)
            {
                RemoveAllEffects(p);
                skateVelX = 0;
                skateVelZ = 0;
                isSkating = false;
            }
            base.OnEntityDespawn(d);
        }
    }
}
