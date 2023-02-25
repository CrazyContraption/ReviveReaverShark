using Microsoft.Xna.Framework;
using Terraria;
using Terraria.ID;
using Terraria.ModLoader;

namespace ReviveReaverShark
{
    public class ReviveReaverShark : Mod
    {
        // Recalculate the powers of any of the specified player's items
        internal static void RecalculatePowers(Player player)
        {
            if (player.active)
                foreach (var item in player.inventory)
                    if (item.netID is ItemID.ReaverShark)
                        RecalculatePowers(item);
        }

        // Recalculate the powers of a specific item
        internal static bool RecalculatePowers(Item anItem)
        {
            if (anItem.active)
            {
                if (anItem is null)
                    return false; // No item found

                if (NPC.downedEmpressOfLight)
                    anItem.pick = 300;
                else if (NPC.downedMoonlord)
                    anItem.pick = 250;
                else if (NPC.downedTowerNebula || NPC.downedTowerSolar || NPC.downedTowerStardust || NPC.downedTowerVortex) // Any tower
                    anItem.pick = 225;
                else if (NPC.downedGolemBoss)
                    anItem.pick = 210;
                else if (NPC.downedMechBoss1 && NPC.downedMechBoss2 && NPC.downedMechBoss3) // All mechs
                    anItem.pick = 200;
                else if (NPC.downedMechBoss1 || NPC.downedMechBoss2 || NPC.downedMechBoss3) // Any mech
                    anItem.pick = 170;
                else if (NPC.downedQueenSlime)
                    anItem.pick = 150;
                else if (Main.hardMode) // WoF
                    anItem.pick = 110;
                else if (NPC.downedBoss2) // Either corruption boss
                    anItem.pick = 70;
                else if (NPC.downedBoss1) // EoC
                    anItem.pick = 65;
                // Item normally has 59% power

                Main.NewText($"Your {anItem.Name}, tembles with the power of your fallen foes!\nIt has reached {anItem.pick}% of its potential!", Color.Orange);

                return true; // Power update attempted
            }
            return false; // No player found
        }
    }

    public class ReaverItem : GlobalItem
    {
        public override void SetDefaults(Item item)
        {
            base.SetDefaults(item);
            if (item.netID is ItemID.ReaverShark)
                ReviveReaverShark.RecalculatePowers(item);
        }
    }

    public class ReaverPlayer : ModPlayer
    {
        public override void OnEnterWorld(Player player)
        {
            base.OnEnterWorld(player);
            ReviveReaverShark.RecalculatePowers(player);
        }
    }

    public class ReaverNPC : GlobalNPC
    {
        public override void HitEffect(NPC npc, int hitDirection, double damage)
        {
            base.HitEffect(npc, hitDirection, damage);
            if (npc.boss)
                if (npc.active)
                    return;
                else
                    ReviveReaverShark.RecalculatePowers(Main.player[Main.myPlayer]);
        }
    }
}