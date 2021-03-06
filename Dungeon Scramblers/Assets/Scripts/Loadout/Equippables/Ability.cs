﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;

public class Ability : Equippable
{
    protected ObjectPooler AbilityPooler;
    protected PhotonView pV;
    protected void Awake()
    {
        // Find object pooler child GameObject
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.CompareTag("Object Pooler"))
            {
                AbilityPooler = child.GetComponent<ObjectPooler>();
                //Debug.Log("Found pooler");
                break;
            }
        }
        pV = GetComponent<PhotonView>();
    }

    [Header("Ability Variables")]
    [SerializeField]
    protected int abilitySlot = 0; //the slot this fills when given to a player, i.e. 0 is basic attack, 1 is ability


    public virtual void StartAttack(Vector3 AttackDirection, AbstractPlayer Unit) { }

    public override void Equip(Player player)
    {
        base.Equip(player);
        player.AddAttack(this, abilitySlot);
    }

    public override void Unequip(Player player)
    {
        base.Unequip(player);
        player.RemoveAttack(this, abilitySlot);
    }

}
