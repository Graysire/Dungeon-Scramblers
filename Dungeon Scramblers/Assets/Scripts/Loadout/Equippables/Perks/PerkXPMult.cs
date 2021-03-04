using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PerkXPMult : Perk
{
    [SerializeField]
    int experienceModifier = 50;

    public override void Equip(Player player)
    {
        GameManager.ManagerInstance.SetExperienceMultiplier(GameManager.ManagerInstance.GetExperienceMultiplier() + experienceModifier);
    }

    public override void Unequip(Player player)
    {
        GameManager.ManagerInstance.SetExperienceMultiplier(GameManager.ManagerInstance.GetExperienceMultiplier() - experienceModifier);
    }
}
