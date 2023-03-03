using System.ComponentModel;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader.Config;

namespace ReviveReaverShark
{
    [Label("World Configs")]
    public class ReaverConfig_Server : ModConfig
    {
        public override ConfigScope Mode => ConfigScope.ServerSide;

        [Header("Server/World Settings")]

        [Label("Use Dynamic Scaling")]
        [Tooltip("When enabled, the Reaver Shark will dynamically scale in power according to progression.\n" +
            "When disabled, the mod will simply un-nerf the Reaver Shark back to 100% flat power.")]
        [DefaultValue(true)]
        public bool dynamicScaling;

        public override bool AcceptClientChanges(ModConfig pendingConfig, int whoAmI, ref string message)
        {
            if (pendingConfig is ReaverConfig_Server server)
            {
                if (server.dynamicScaling)
                    message = "Dynamic Scaling enabled!";
                else
                    message = "Dynamic Scaling disabled!";
                
                return true;
            }
            return false;
        }

        public override void OnChanged()
        {
            if (Main.netMode is NetmodeID.Server || Main.gameMenu || WorldGen.gen || Main.PlayerLoaded is false)
                return;

            ReviveReaverShark.RecalculatePowers(Main.player[Main.myPlayer]);
        }
    }
}