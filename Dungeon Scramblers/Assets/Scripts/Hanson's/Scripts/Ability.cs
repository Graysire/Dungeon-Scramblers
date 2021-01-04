using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Ability : MonoBehaviour
{
    [SerializeField]
    private float Range;
    [SerializeField]
    private float MoveSpeed;

    private Vector3 AttackDir;
    private Vector3 PositionTraveled = Vector3.zero;
    Collider2D Collider;

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
        if(AttackDir != null)
        {
            transform.position += AttackDir * Time.deltaTime * MoveSpeed;
            PositionTraveled += AttackDir * Time.deltaTime * MoveSpeed;
            if(PositionTraveled.magnitude >= Range)
            {
                Destroy(this.gameObject);
            }
        }
        
    }
}
