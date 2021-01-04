﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HansonsOverlord : Player
{
    [SerializeField]
    private LineRenderer line;
    [SerializeField]
    private GameObject ability; 

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
        line = GetComponent<LineRenderer>();
    }

    protected override void Attack(float f)
    { // MOUSE ATTACK INPUT
        Vector3 MouseWorldCoord = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        Vector3 AttackEnd = new Vector3(MouseWorldCoord.x, MouseWorldCoord.y, 0);
        // set the origin position of the attack indicator
        line.SetPosition(0, transform.position);
        // set the ending position of the attack indicator
        line.SetPosition(1, AttackEnd);

        Debug.DrawLine(transform.position, AttackEnd, Color.red, 10f);

        Vector3 AttackDirection = (AttackEnd - transform.position).normalized;
        Transform AbilityTransform = Instantiate(ability, transform.position, Quaternion.identity).transform;
        AbilityTransform.GetComponent<Ability>().SetUp(AttackDirection);
    }
    protected override void Attack(Vector2 d)
    { // TOUCHSCREEN ATTACK INPUT
        Debug.Log("Overlord attack on phone");
        
    }
}
