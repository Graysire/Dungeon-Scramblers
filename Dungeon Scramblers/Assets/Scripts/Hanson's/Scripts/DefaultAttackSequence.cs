using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DefaultAttackSequence : MonoBehaviour
{
    [SerializeField]
    protected ProjectileStats Projectile;
    protected ObjectPooler AbilityPooler;
    //protected bool needsMoreInstances;

    protected Player Player;
    protected Vector3 AttackDirection;

    protected bool Attacked = false;

    private void Start()
    {
        // Find object pooler child GameObject
        for (int i = 0; i < transform.childCount; i++)
        {
            Transform child = transform.GetChild(i);
            if (child.CompareTag("Object Pooler"))
            {
                AbilityPooler = child.GetComponent<ObjectPooler>();
                Debug.Log("Found pooler");
                break;
            }
        }
    }

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
        yield return new WaitForSeconds(Projectile.GetCastingTime());
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
        Vector3 AttackTransform = Player.transform.position + (RelativeAttackEnd.normalized * Projectile.GetOffsetScale());

        // Get instance of ability from object pooler
        Transform AbilityTransform = AbilityPooler.GetPooledObject(AttackTransform, AttackEnd, Player.gameObject, AbilityAngle).transform;
        AbilityTransform.GetComponent<ProjectileStats>().SetUp(AttackNormal);
        Player.SetAllowedToAttack(true);
        yield return new WaitForSeconds(Projectile.GetCoolDownTime());
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
        Vector3 AttackTransform = AI.transform.position + (RelativeAttackEnd.normalized * Projectile.GetOffsetScale());
        
        //Creates the attack through the object pooler of AI attack
        Transform AbilityTransform = AbilityPooler.GetPooledObject(AttackTransform, AttackEnd, AI.gameObject, AbilityAngle).transform;
        AbilityTransform.GetComponent<ProjectileStats>().SetUp(AttackNormal);
    }
}
