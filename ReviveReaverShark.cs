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
        /// <summary>
        /// Forces a recalculation of a specific player's inventory of items, and updates the powers of them.
        /// Also is responsible for informing the player of the changes to the item.
        /// Server, menu, and worldgen calls are ignored.
        /// </summary>
        /// <param name="player">Player to search within and issue the updates to.</param>
        internal static void RecalculatePowers(Player player)
        {
            if (Main.netMode is NetmodeID.Server || Main.gameMenu || WorldGen.gen)
                return;

            if (player.active)
            {
                int oldMinPower = int.MaxValue;
                int? newPower = GetPower();

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

                if (player.whoAmI == Main.myPlayer && newPower < oldMinPower)
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
            if (Main.netMode is NetmodeID.Server || Main.gameMenu || WorldGen.gen)
                return null;
            if (NPC.downedEmpressOfLight)
                return 300;
            else if (NPC.downedMoonlord)
                return 250;
            else if (NPC.downedTowerNebula && NPC.downedTowerSolar && NPC.downedTowerStardust && NPC.downedTowerVortex) // Any tower
                return 225;
            else if (NPC.downedTowerNebula || NPC.downedTowerSolar || NPC.downedTowerStardust || NPC.downedTowerVortex) // Any tower
                return 225;
            else if (NPC.downedAncientCultist)
                return 220;
            else if (NPC.downedGolemBoss)
                return 210;
            else if (NPC.downedPlantBoss)
                return 205;
            else if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3) // All mechs
                return 200;
            else if (NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3) // Any mech
                return 170;
            else if (NPC.downedQueenSlime)
                return 150;
            else if (Main.hardMode) // WoF
                return 110;
            else if (NPC.downedDeerclops) // Skeletron
                return 100;
            else if (NPC.downedBoss3) // Skeletron
                return 90;
            else if (NPC.downedQueenBee)
                return 80;
            else if (NPC.downedBoss2) // Either corruption boss
                return 70;
            else if (NPC.downedBoss1) // EoC
                return 65;
            else
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
            base.SetDefaults(item);
            if (item.netID is ItemID.ReaverShark)
                if (item.active)
                    item.pick = ReviveReaverShark.GetPower() ?? item.pick;
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
            base.OnEnterWorld(player);
            new Task(delegate
            {
                Thread.Sleep(1000);
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
            base.HitEffect(npc, hitDirection, damage);

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