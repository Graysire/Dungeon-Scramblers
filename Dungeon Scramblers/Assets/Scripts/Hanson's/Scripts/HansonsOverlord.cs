﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HansonsOverlord : Player
{
    [SerializeField]
    private LineRenderer line;
    [SerializeField]
    private Ability ability;

    bool bBasicAttack = false;

    protected override void OnEnable()
    {
        controls.Enable();
        UpdateHandler.UpdateOccurred += Die;
        UpdateHandler.FixedUpdateOccurred += ApplyMove;

    }
    protected override void OnDisable()
    {
        controls.Disable();
        UpdateHandler.UpdateOccurred -= Die;
        UpdateHandler.FixedUpdateOccurred -= ApplyMove;
    }

    protected virtual void Awake()
    {
        base.Awake();
        //line = GetComponent<LineRenderer>();
        
    }

    protected override void Attack(float f)
    { // MOUSE ATTACK INPUT
        if(!bBasicAttack) StartCoroutine("AttackSequence");
    }
    protected override void Attack(Vector2 d)
    { // TOUCHSCREEN ATTACK INPUT
        Debug.Log("Overlord attack on phone");
        
    }

    IEnumerator AttackSequence()
    {
        bBasicAttack = true;
        yield return new WaitForSeconds(ability.GetCastingTime());
        Vector3 MouseWorldCoord = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 AttackEnd = new Vector3(MouseWorldCoord.x, MouseWorldCoord.y, 0);
        // set the origin position of the attack indicator
        //line.SetPosition(0, transform.position);
        // set the ending position of the attack indicator
        //line.SetPosition(1, AttackEnd);

        Debug.DrawLine(transform.position, AttackEnd, Color.red, 10f);
        Debug.Log(transform.up);
        // Get relative position of where the mouse was clicked to correctly calculate the angle for projectile
        Vector3 RelativeAttackEnd = AttackEnd - transform.position;
        float dot = Vector3.Dot(transform.up, RelativeAttackEnd);
        Debug.Log("Dot: " + dot);
        // Calculate the angle of the ability in radians with dot product formula A dot B = |A||B|cos(theta)
        float AbilityAngle = Mathf.Acos(dot / (transform.up.magnitude * RelativeAttackEnd.magnitude)) * Mathf.Rad2Deg;
        Debug.Log("Angle: " + AbilityAngle);
        
        Vector3 AttackDirection = (AttackEnd - transform.position).normalized;
        Transform AbilityTransform = Instantiate(ability.gameObject, transform.position, 
            Quaternion.Euler(0,0, AttackEnd.x >= transform.position.x ? -AbilityAngle : AbilityAngle)).transform;
        AbilityTransform.GetComponent<Ability>().SetUp(AttackDirection);
        yield return new WaitForSeconds(ability.GetCoolDownTime());
        bBasicAttack = false;
    }
}
