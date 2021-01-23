using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Panda;


public class AI : MonoBehaviour
{
    Transform player;
    public Transform[] players;
    public Slider healthBar;
    public GameObject attackSpawn;
    public GameObject attackPrefab;

    public DefaultAttackSequence attack;    // The attack to call for attacking
    public Vector3 destination;             // The destination to move to
    public Vector3 target;                  // The position, or player position, to aim at for attack
    List<Vector3> currentPath;              // Stores the current path being used

    public float stoppingDistance = 30.0f;  //Distance from player AI stops at
    public float health = 100.0f;           //Ai health
    public float speed = 15.0f;             //Movement speed of AI
    public float visibleRange = 80.0f;      //Range AI needs to be in to see Player
    public float attackRange = 40.0f;       //Range AI needs to be in to attack
    public float damageTaken = 10.0f;       //The amount of damage that will be applied to the AI when hit



    // Start is called before the first frame update
    void Start()
    {
        // Updates health over time - commented out till feature is requested
        //InvokeRepeating("UpdateHealth", 5, 0.5f);

        //Get the attack ability to use for attacking
        attack = gameObject.GetComponent<DefaultAttackSequence>();
    }

    // Update is called once per frame
    void Update()
    {
        // Updates the position and value of the healthbar for this AI
        if (healthBar != null)
        {
            Vector3 healthBarPos = Camera.main.WorldToScreenPoint(this.transform.position);
            healthBar.value = (int)health;
            healthBar.transform.position = healthBarPos + new Vector3(0, 60, 0);
        }
    }

    //Updates the AI health
    void UpdateHealth()
    {
        if (health < 100)
        {
            health++;
        }
    }

    //Deals damage to AI if hit by a bullet -- Subject to change
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "bullet")
        {
            health -= damageTaken;
        }
    }

    //Gets the path given the start position and target position
    public List<Vector3> GetPath(Vector3 startPos, Vector3 targetPos)
    {
        return Pathfinder.GetPath(startPos, targetPos, 90000);
    }

    //Determines if the AI is at the stopping distance from the player
    public bool AtStoppingDistance()
    {
        Vector3 distance = player.transform.position - this.transform.position;
        if (distance.magnitude < stoppingDistance)
            return true;
        return false;
    }


    //Has AI move to given destination - currently destination is the player
    [Task]
    public void MoveToDestination()
    {
        //Get path to player
        currentPath = GetPath(this.transform.position, player.transform.position);

        //Move to each path node until reaching stopping distance
        for (int i = 0; i < currentPath.Count; i++)
        {
            //Stop moving if at the stopping distance
            if (AtStoppingDistance())
            {
                break;
            }
            this.transform.position = Vector3.MoveTowards(transform.position, currentPath[i], speed * Time.deltaTime);
        }
        Task.current.Succeed();
    }

    //Determines whether player is seen or not.
    //Will get the path from AI to player if the player is in sight.
    //Will also target the closest visible player
    [Task]
    bool SeePlayerAndSetTarget()
    {
        bool playerSeen = false;

        //Set the closest visible player
        foreach (Transform p in players)
        {
            //Get distance from AI and player
            Vector3 distance = p.transform.position - this.transform.position;


            //Use raycast to determine if player is in sight
            RaycastHit hit;
            bool seeWall = false;

            //Create visual debug of raycast
            Debug.DrawRay(this.transform.position, distance, Color.red);

            //Checks for walls with raycast
            if (Physics.Raycast(this.transform.position, distance, out hit))
            {
                if (hit.collider.gameObject.tag == "wall")
                {
                    seeWall = true;
                }
            }

            //Debug message for determining if wall is in path of raycast
            if (Task.isInspected)
            {
                Task.current.debugInfo = string.Format("wall={0}", seeWall);
            }



            //If player is in visible range of AI and there is no wall blocking sight
            if (distance.magnitude < visibleRange && !seeWall)
            {
                //Set this player to interact with (move to and attack)
                if (player != null)
                {
                    //If this player is closer than the previously targeted player then make this player the new target
                    if (distance.magnitude < (player.transform.position - this.transform.position).magnitude)
                    {
                        player = p;
                    }
                }
                else
                {
                    player = p;
                }

                //Set player seen 
                playerSeen = true;
            }
        }

        return playerSeen;
    }

    //Determines if the Player as target is in range of attack
    [Task]
    bool AttackInRange()
    {
        target = player.transform.position;
        Vector3 distance = target - this.transform.position;

        if (distance.magnitude < attackRange)
            return true;

        return false;
    }

    //Spawns an attack on the attackSpawn towards player
    [Task]
    public void Attack()
    {
        Vector3 direction = target - this.transform.position;
        attack.StartAIAttack(direction, this);
        Task.current.Succeed();
    }

    //For AI death
    [Task]
    public bool Death()
    {
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }
        Destroy(this.gameObject);
        return true;
    }

}
