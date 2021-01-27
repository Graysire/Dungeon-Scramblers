﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultAttackSequence : MonoBehaviour
{
    [SerializeField]
    private Ability Ability;

    private Player Player;
    private Vector3 AttackDirection;

    private bool Attacked = false;

    public void StartAttack(Vector3 AttackDirection, Player Player)
    {
        this.AttackDirection = AttackDirection;
        this.Player = Player;
        if(!Attacked) StartCoroutine("AttackSequence");
    }


    protected virtual IEnumerator AttackSequence()
    {
        // Set Is currently attacking
        Player.SetAllowedToAttack(false);
        Attacked = true;
        // Wait for ability casting time before proceeding
        yield return new WaitForSeconds(Ability.GetCastingTime());
        // get mouse coordinate from camera when clicked and find the ending of the attack with the mouse clicked

        Vector3 AttackEnd = Player.transform.position + AttackDirection;
        // Get relative position of where the mouse was clicked to correctly calculate the angle for projectile
        Vector3 RelativeAttackEnd = AttackEnd - Player.transform.position;
        float dot = Vector3.Dot(Player.transform.up, RelativeAttackEnd);

        // Calculate the angle of the ability in radians with dot product formula A dot B = |A||B|cos(theta)
        float AbilityAngle = Mathf.Acos(dot / (Player.transform.up.magnitude * RelativeAttackEnd.magnitude)) * Mathf.Rad2Deg;

        // Normalize the direction of the attack for incrementing the attack movement
        Vector3 AttackNormal = (AttackEnd - Player.transform.position).normalized;
        // Transform vector with quick if statements for returning offset for attacks
        Vector3 AttackTransform = Player.transform.position + (RelativeAttackEnd.normalized * Ability.GetOffsetScale());
        Transform AbilityTransform = Instantiate(Ability.gameObject, AttackTransform,
            Quaternion.Euler(0, 0, AttackEnd.x >= Player.transform.position.x ? -AbilityAngle : AbilityAngle)).transform;
        AbilityTransform.GetComponent<Ability>().SetUp(AttackNormal);
        Player.SetAllowedToAttack(true);
        yield return new WaitForSeconds(Ability.GetCoolDownTime());
        Attacked = false;
        
    }


    //Attack caller for AI agents
    public void StartAIAttack(Vector3 AttackDirection, AI AI)
    {
        Vector3 AttackEnd = AI.transform.position + AttackDirection;
        // Get relative position of where the mouse was clicked to correctly calculate the angle for projectile
        Vector3 RelativeAttackEnd = AttackEnd - AI.transform.position;
        float dot = Vector3.Dot(AI.transform.up, RelativeAttackEnd);

        // Calculate the angle of the ability in radians with dot product formula A dot B = |A||B|cos(theta)
        float AbilityAngle = Mathf.Acos(dot / (AI.transform.up.magnitude * RelativeAttackEnd.magnitude)) * Mathf.Rad2Deg;

        // Normalize the direction of the attack for incrementing the attack movement
        Vector3 AttackNormal = (AttackEnd - AI.transform.position).normalized;
        // Transform vector with quick if statements for returning offset for attacks
        Vector3 AttackTransform = AI.transform.position + (RelativeAttackEnd.normalized * Ability.GetOffsetScale());
        Transform AbilityTransform = Instantiate(Ability.gameObject, AttackTransform,
            Quaternion.Euler(0, 0, AttackEnd.x >= AI.transform.position.x ? -AbilityAngle : AbilityAngle)).transform;
        AbilityTransform.GetComponent<Ability>().SetUp(AttackNormal);
    }
}
