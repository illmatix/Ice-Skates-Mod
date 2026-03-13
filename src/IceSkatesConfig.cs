namespace IceSkates
{
    /// <summary>
    /// Mod configuration, saved to ModConfig/IceSkatesConfig.json.
    /// Compatible with ConfigLib for in-game GUI editing.
    /// 
    /// Metals are grouped by progression tier, each with a master toggle
    /// and individual alloy switches underneath.
    /// 
    /// ALWAYS-ON metals (not configurable):
    ///   Bone, Iron, Meteoric Iron, Steel
    /// 
    /// CONFIGURABLE metals by tier group:
    ///   Early-game:  Copper
    ///   Mid-game:    Tin Bronze, Bismuth Bronze, Black Bronze
    ///   Late-game:   Blister Steel
    ///   Novelty:     Silver, Gold
    /// </summary>
    public class IceSkatesConfig
    {
        // ===========================================
        // Early-Game Bridges (bone → bronze gap)
        // ===========================================

        /// <summary>Master toggle for early-game bridging metals.</summary>
        public bool EnableEarlyGameBridges { get; set; } = false;

        /// <summary>Copper blades (+25% speed, 180 dura). Earliest metal tier.</summary>
        public bool EnableCopper { get; set; } = true;

        // ===========================================
        // Mid-Game Bridges (bronze → iron gap)
        // ===========================================

        /// <summary>Master toggle for mid-game bridging metals (bronzes).</summary>
        public bool EnableMidGameBridges { get; set; } = false;

        /// <summary>Tin bronze blades (+30% speed, 250 dura). Most common alloy.</summary>
        public bool EnableTinBronze { get; set; } = true;

        /// <summary>Bismuth bronze blades (+30% speed, 250 dura).</summary>
        public bool EnableBismuthBronze { get; set; } = false;

        /// <summary>Black bronze blades (+32% speed, 280 dura). Expensive alloy.</summary>
        public bool EnableBlackBronze { get; set; } = false;

        // ===========================================
        // Late-Game Bridges (iron → steel gap)
        // ===========================================

        /// <summary>Master toggle for late-game bridging metals.</summary>
        public bool EnableLateGameBridges { get; set; } = false;

        /// <summary>Blister steel blades (+50% speed, 480 dura). Bridges iron→meteoric iron.</summary>
        public bool EnableBlisterSteel { get; set; } = true;

        // ===========================================
        // Novelty Metals (vanity / roleplay)
        // ===========================================

        /// <summary>Master toggle for novelty metals. These are mechanically weak but flashy.</summary>
        public bool EnableNoveltyMetals { get; set; } = false;

        /// <summary>Silver blades (+18% speed, 80 dura). Soft metal, looks great.</summary>
        public bool EnableSilver { get; set; } = true;

        /// <summary>Gold blades (+15% speed, 60 dura). Softest metal, ultimate vanity.</summary>
        public bool EnableGold { get; set; } = true;

        // ===========================================
        // Skating Physics
        // ===========================================

        /// <summary>Forward acceleration per physics tick (scaled by blade bonus). Default 0.006.</summary>
        public float ThrustForce { get; set; } = 0.006f;

        /// <summary>Hard speed cap (scaled by blade bonus). Default 0.35.</summary>
        public float MaxSkatingSpeed { get; set; } = 0.35f;

        /// <summary>Deceleration per tick when Backward is held. Default 0.02.</summary>
        public float BrakeForce { get; set; } = 0.02f;

        /// <summary>Radians per tick turn rate at low speed. Default 0.06.</summary>
        public float TurnRateBase { get; set; } = 0.06f;

        /// <summary>Higher speed = wider turns. Default 3.0.</summary>
        public float TurnRateSpeedFalloff { get; set; } = 3.0f;

        /// <summary>Per-tick velocity multiplier when coasting on ice (1.0 = no friction). Default 0.998.</summary>
        public float CoastFriction { get; set; } = 0.998f;

        /// <summary>Thrust and max speed multiplier when sprinting. Default 1.4.</summary>
        public float SprintMultiplier { get; set; } = 1.4f;

        /// <summary>Physics tick interval in seconds (20Hz = 0.05). Default 0.05.</summary>
        public float PhysicsTickInterval { get; set; } = 0.05f;

        /// <summary>Immediate velocity multiplier when player leaves ice while skating. Default 0.3.</summary>
        public float OffIceBrakeFactor { get; set; } = 0.3f;

        /// <summary>Minimum speed before skating stops completely. Default 0.001.</summary>
        public float SlideStopThreshold { get; set; } = 0.001f;

        // ===========================================
        // Sharpener Settings
        // ===========================================

        /// <summary>% of max durability restored by whetstone (portable). Default 12.</summary>
        public int WhetstoneRepairPercent { get; set; } = 12;

        /// <summary>% of max durability restored by sharpening jig. Default 45.</summary>
        public int SharpenerJigRepairPercent { get; set; } = 45;

        /// <summary>Total uses before jig breaks. Default 15.</summary>
        public int SharpenerJigMaxUses { get; set; } = 15;

        // ===========================================
        // Helpers
        // ===========================================

        /// <summary>
        /// Checks whether a specific metal is enabled, considering
        /// both the group master toggle and the individual switch.
        /// Returns true for always-on metals (bone, iron, meteoriciron, steel).
        /// </summary>
        public bool IsMetalEnabled(string metal)
        {
            return metal switch
            {
                // Always-on
                "bone" => true,
                "iron" => true,
                "meteoriciron" => true,
                "steel" => true,

                // Early-game
                "copper" => EnableEarlyGameBridges && EnableCopper,

                // Mid-game
                "tinbronze" => EnableMidGameBridges && EnableTinBronze,
                "bismuthbronze" => EnableMidGameBridges && EnableBismuthBronze,
                "blackbronze" => EnableMidGameBridges && EnableBlackBronze,

                // Late-game
                "blistersteel" => EnableLateGameBridges && EnableBlisterSteel,

                // Novelty
                "silver" => EnableNoveltyMetals && EnableSilver,
                "gold" => EnableNoveltyMetals && EnableGold,

                _ => false
            };
        }

        /// <summary>All configurable (non-always-on) metal codes.</summary>
        public static readonly string[] ConfigurableMetals = {
            "copper",
            "tinbronze", "bismuthbronze", "blackbronze",
            "blistersteel",
            "silver", "gold"
        };
    }
}
