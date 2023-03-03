using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveReaverShark
{
    public class ReviveReaverShark : Mod
    {
        internal static ReaverConfig_Server ServerConfig = ModContent.GetInstance<ReaverConfig_Server>();

        /// <summary>
        /// Forces a recalculation of a specific player's inventory of items, and updates the powers of them.
        /// Also is responsible for informing the player of the changes to the item.
        /// Server, menu, and worldgen calls are ignored.
        /// </summary>
        /// <param name="player">Player to search within and issue the updates to.</param>
        internal static void RecalculatePowers(Player player)
        {
            if (Main.netMode is NetmodeID.Server || Main.gameMenu || WorldGen.gen || Main.PlayerLoaded is false)
                return;
            
            if (player.active)
            {
                int oldMinPower = int.MaxValue;
                int? newPower = ServerConfig.dynamicScaling ? GetPower() : 100;

                foreach (var item in player.inventory)
                    if (item.netID is ItemID.ReaverShark && item.active)
                    {
                        int oldpower = item.pick;
                        int currPower = newPower ?? oldpower;
                        item.pick = currPower;

                        if (oldMinPower > oldpower)
                            oldMinPower = oldpower;
                    }
                if (oldMinPower is int.MaxValue)
                    return;
                if (player.whoAmI == Main.myPlayer && ServerConfig.dynamicScaling is false && newPower != oldMinPower)
                    Main.NewText($"Your Reaver Shark feels a powerful worldly force preventing its true power. It's limited to {newPower}% of its potential!", Color.Orange);
                else if (player.whoAmI == Main.myPlayer && newPower < oldMinPower)
                    Main.NewText($"Your Reaver Shark shrivels with the lack of power!\nIt has been reduced to {newPower}% of its potential!", Color.Orange);
                else if (player.whoAmI == Main.myPlayer && newPower > oldMinPower)
                    Main.NewText($"Your Reaver Shark tembles with the power of your fallen foes!\nIt has reached {newPower}% of its potential!", Color.Orange);
                
            }
        }

        /// <summary>
        /// Gets the calculated power of the current world.
        /// Server, menu, and worldgen calls are ignored.
        /// </summary>
        /// <returns>The pickaxe power of the world's closest progression.</returns>
        internal static int? GetPower()
        {
            if (Main.netMode is NetmodeID.Server || Main.gameMenu || WorldGen.gen || Main.PlayerLoaded is false)
                return null;

            if (ServerConfig.dynamicScaling is false)
                return 100;

            if (NPC.downedMoonlord && NPC.downedEmpressOfLight && NPC.downedFishron)
                return 300;
            if (NPC.downedMoonlord && NPC.downedFishron)
                return 285;
            if (NPC.downedMoonlord || NPC.downedFishron)
                return 275;
            if (NPC.downedEmpressOfLight)
                return 255;
            if (NPC.downedTowerNebula && NPC.downedTowerSolar && NPC.downedTowerStardust && NPC.downedTowerVortex) // Any tower
                return 235;
            if (NPC.downedTowerNebula || NPC.downedTowerSolar || NPC.downedTowerStardust || NPC.downedTowerVortex) // Any tower
                return 225;
            if (NPC.downedAncientCultist)
                return 220;
            if (NPC.downedGolemBoss)
                return 210;
            if (NPC.downedPlantBoss)
                return 205;
            if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3) // All mechs
                return 200;
            if (NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3) // Any mech
                return 180;
            if (NPC.downedQueenSlime)
                return 150;
            if (Main.hardMode) // WoF
                return 110;
            if (NPC.downedDeerclops) // Deerclopse
                return 100;
            if (NPC.downedBoss3) // Skeletron
                return 90;
            if (NPC.downedQueenBee)
                return 80;
            if (NPC.downedBoss2) // Either corruption boss
                return 70;
            if (NPC.downedBoss1) // EoC
                return 64;
            if (NPC.downedSlimeKing)
                return 60;
            return 59;
        }
    }

    public class ReaverItem : GlobalItem
    {
        /// <summary>
        /// Called whenever an item is created, copied, moved, or otherwise updated.
        /// </summary>
        /// <param name="item">The item being being set(up)</param>
        public override void SetDefaults(Item item)
        {
            if (item.netID is ItemID.ReaverShark)
                if (item.active)
                    if (ReviveReaverShark.ServerConfig.dynamicScaling)
                        item.pick = ReviveReaverShark.GetPower() ?? item.pick;
                    else
                        item.pick = 100;
        }
    }

    public class ReaverPlayer : ModPlayer
    {
        /// <summary>
        /// Runs when a player joins a world. Only called on the client that joined, and nobody else.
        /// </summary>
        /// <param name="player">The player that joined.</param>
        public override void OnEnterWorld(Player player)
        {
            new Task(delegate
            {
                Thread.Sleep(1500);
                ReviveReaverShark.RecalculatePowers(Main.player[Main.myPlayer]);
            }).Start();
        }
    }

    public class ReaverNPC : GlobalNPC
    {
        /// <summary>
        /// Called when an NPC is hit, just before the hit takes place.
        /// </summary>
        /// <param name="npc">The NPC being hit</param>
        /// <param name="hitDirection">Direction of the hit</param>
        /// <param name="damage">Damage of the hit</param>
        public override void HitEffect(NPC npc, int hitDirection, double damage)
        {
            if (npc.boss || npc.type is NPCID.LunarTowerVortex or NPCID.LunarTowerStardust or NPCID.LunarTowerSolar or NPCID.LunarTowerNebula) // Is the NPC tied to progression?
                if (npc.life <= 0) // Is the boss dead/dying?
                    new Task(delegate
                    {
                        Thread.Sleep(1000);
                        ReviveReaverShark.RecalculatePowers(Main.player[Main.myPlayer]);
                    }).Start();
        }
    }
}