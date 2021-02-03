using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Panda;


public class AI : HasStats, IDamageable<float>
{

    // Placeholder for eventual Attack & Ability Equipables 
    [SerializeField] protected List<GameObject> AttackObjectList;   //Holds Attack Sequence Prefabs
    protected List<DefaultAttackSequence> AttackList;               //Instantiates Attack Sequence Prefabs as Child to this

    Transform player;                           // Player that AI will target for attacks and chasing
    public Transform[] players;                 // Positions of all players in game
    public GameObject healthBarUI;              // Healthbar Canvas Object
    public Slider healthBar;                    // Healthbar Slider Object

    public Vector3 destination;                 // The destination to move to
    public Vector3 target;                      // The position, or player position, to aim at for attack
    protected List<Vector3> currentPath;        // Stores the current path being used

    public float stoppingDistance = 0.5f;       // Distance from player AI stops at      
    public float visibleRange = 8.0f;           // Range AI needs to be in to see Player
    public float attackRange = 4.0f;            // Range AI needs to be in to attack
    public float expOnDeath = 10.0f;            // The amount of experience points AI gives to Scramblers on death
    public bool onlyAttackOnePlayer = false;    // AI will only target one player till they die
    protected float maxHealth;                  // Max AI health


    // Start is called before the first frame update
    void Start()
    {
        //set the max health
        maxHealth = stats[(int)Stats.health];

        //Sets the value of health bar based on percentage of current health and max health
        healthBar.value = CalculateHealth();
    }

    protected virtual void Awake()
    {
        // Instantiate attack sequences to reattach the instance to the player
        for (int i = 0; i < AttackObjectList.Count; i++)
        {
            AttackObjectList[i] = Instantiate(AttackObjectList[i], gameObject.transform);
        }


        AttackList = new List<DefaultAttackSequence>();
        for (int i = 0; i < AttackObjectList.Count; i++)
        {
            AttackList.Add(AttackObjectList[i].GetComponent<DefaultAttackSequence>());
        }
    }

    // Update is called once per frame
    void Update()
    {
        // Updates the position and value of the healthbar for this AI
        if (healthBar != null)
        {
            //Set the health bar active once damage is taken
            if (stats[(int)Stats.health] < maxHealth) healthBarUI.SetActive(true);

            //set the health bar value
            healthBar.value = CalculateHealth();
        }

        //If health is greater than max then set it to max (In case of healing)
        if (stats[(int)Stats.health] > maxHealth) stats[(int)Stats.health] = maxHealth;
    }

    //Calculates the health percentage for displaying on health bar
    protected float CalculateHealth()
    {
        return stats[(int)Stats.health] / maxHealth;
    }

    //When ability hits AI it takes damage
    public void Damage(float damageTaken)
    {
        stats[(int)Stats.health] -= damageTaken;
    }

    //Updates the AI health
    protected void UpdateHealth()
    {
        if (stats[(int)Stats.health] < 100)
        {
            stats[(int)Stats.health]++;
        }
    }

    //Gets the path given the start position and target position
    protected List<Vector3> GetPath(Vector3 startPos, Vector3 targetPos)
    {
        return Pathfinder.GetPath(startPos, targetPos, 90000);
    }

    //Determines if the AI is at the stopping distance from the player
    protected bool AtStoppingDistance()
    {
        Vector3 distance = player.transform.position - this.transform.position;
        if (distance.magnitude < stoppingDistance)
            return true;
        return false;
    }

    //Send EXP to Game Manager to send to all players
    protected void DisperseEXP()
    {
        //Get the game manager to disperse the exp to players
        GameManager.ManagerInstance.DistributeExperience(expOnDeath);
    }


    //Has AI move to given destination - currently the destination is the seen player
    [Task]
    protected void MoveToDestination()
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
            this.transform.position = Vector3.MoveTowards(transform.position, currentPath[i], stats[(int)Stats.movespeed] * Time.deltaTime);
        }
        Task.current.Succeed();
    }


    //Determines whether player is seen or not.
    //Will get the path from AI to player if the player is in sight.
    //Will also target the closest visible player
    [Task]
    protected bool SeePlayerAndSetTarget()
    {
        bool playerSeen = false;

        //If this AI targets only one player and they were already found then don't find a new target
        if (onlyAttackOnePlayer && player != null) return true;

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

                    //If true AI will only target this player seen
                    if (onlyAttackOnePlayer) break;
                }

                //Set player seen 
                playerSeen = true;
            }
        }

        return playerSeen;
    }

    //Determines if the Player as target is in range of attack
    [Task]
    protected bool AttackInRange()
    {
        target = player.transform.position;
        Vector3 distance = target - this.transform.position;

        if (distance.magnitude < attackRange)
            return true;

        return false;
    }

    //Spawns an attack on the attackSpawn towards player
    [Task]
    protected void AttackPlayer()
    {
        Vector3 direction = target - this.transform.position;
        AttackList[0].StartAIAttack(direction, this); //AI will attack in direction of player
        Task.current.Succeed();
    }

    //Checks if the health is less than the given value
    [Task]
    public bool IsHealthLessThan(float health)
    {
        return stats[(int)Stats.health] < health;
    }

    //Destroys AI object
    [Task]
    protected bool Death()
    {
        DisperseEXP(); //Send the experience for killing AI to players
        Destroy(this.gameObject);
        return true;
    }
}
