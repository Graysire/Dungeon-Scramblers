using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerkRevive : Perk
{
    public override void Equip(Player player)
    {
        ((Scrambler) player).Revive(75, true);
    }
}
