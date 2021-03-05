using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//as PerkStatusEffect except it adds secondary status effects to other players
public class PerkStatusSplit : PerkStatusEffect
{
    [SerializeField]
    StatusEffect[] secondaryEffects;
    //applies the primary status effect to the main player multiple times;
    [SerializeField]
    bool applyToMainTargetAgain = false;


    public override void Equip(Player player)
    {
        //performs the basic equip on the target
        base.Equip(player);
        //for each other player apply the secondary effects
        foreach (Player p in GameManager.ManagerInstance.GetPlayers())
        {
            if (p != player && p != null)
            {
                if (applyToMainTargetAgain)
                {
                    base.Equip(player);
                }
                foreach (StatusEffect s in secondaryEffects)
                {
                    p.AddStatusEffect(s);
                }
            }
        }
    }

    public override void Unequip(Player player)
    {
        base.Unequip(player);
        foreach (Player p in GameManager.ManagerInstance.GetPlayers())
        {
            if (p != player && p != null)
            {
                if (applyToMainTargetAgain)
                {
                    base.Unequip(player);
                }
                foreach (StatusEffect s in secondaryEffects)
                {
                    p.RemoveStatusEffect(s);
                }
            }
        }
    }
}
