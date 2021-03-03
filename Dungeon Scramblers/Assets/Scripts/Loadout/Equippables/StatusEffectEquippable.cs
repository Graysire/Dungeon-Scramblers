using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Equippable that applies a status effect to the equipper
public class StatusEffectEquippable : Equippable
{
    [SerializeField]
    StatusEffect[] effects;

    public override void Equip(Player player)
    {
        base.Equip(player);
        foreach (StatusEffect effect in effects)
        {
            player.AddStatusEffect(effect);
        }
    }

    public override void Unequip(Player player)
    {
        base.Unequip(player);
        foreach (StatusEffect effect in effects)
        {
            player.RemoveStatusEffect(effect);
        }
    }
}
