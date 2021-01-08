using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour, IAbilityInterface
{
    [SerializeField]
    private float Range;
    [SerializeField]
    private float DecayTime;
    [SerializeField]
    private float MoveSpeed;
    [SerializeField]
    private float CastingTime;
    [SerializeField]
    private float CoolDown;



    private Vector3 AttackDir;
    private float totalTime = 0;
    private bool canCast = true;
    private Vector3 PositionTraveled = Vector3.zero;
    Collider2D Collider;

    public float GetCastingTime()
    {
        return CastingTime;
    }

    public float GetCoolDownTime()
    {
        return CoolDown;
    }

    public void SetUp(Vector3 AttackDir)
    {
        this.AttackDir = AttackDir;
        
    }

    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {
        if (AttackDir != null)
        {
            totalTime += Time.deltaTime;
            transform.position += AttackDir * Time.deltaTime * MoveSpeed;
            PositionTraveled += AttackDir * Time.deltaTime * MoveSpeed;
            if (PositionTraveled.magnitude >= Range || totalTime >= DecayTime)
            {
                Destroy(this.gameObject);
            }
        }

    }

}
