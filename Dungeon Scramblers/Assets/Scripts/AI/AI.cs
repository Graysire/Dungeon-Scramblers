using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Panda;
using Photon.Pun;

public class AI : AbstractPlayer
{
    [Header("Abilities")]
    // Placeholder for eventual Attack & Ability Equipables 
    [SerializeField] protected List<GameObject> AttackObjectList;   //Holds Attack Sequence Prefabs
    protected List<DefaultAttackSequence> AttackList;               //Instantiates Attack Sequence Prefabs as Child to this

    Transform player;                           // Player that AI will target for attacks and chasing

    [HideInInspector]
    public Transform[] players;                 // Positions of all players in game

    [HideInInspector]
    public Vector3 destination;                 // The destination to move to
    [HideInInspector]
    public Vector3 target;                      // The position, or player position, to aim at for attack

    protected List<Vector3> currentPath;        // Stores the current path being used

    [Header("AI Variables")]
    [SerializeField]
    protected float stoppingDistance = 0.5f;       // Distance from player AI stops at      
    [SerializeField]
    protected float retreatDistance = 0.3f;       // Distance from player AI need to retreat at
    [SerializeField]
    protected float visibleRange = 8.0f;           // Range AI needs to be in to see Player
    [SerializeField]
    protected float attackRange = 4.0f;            // Range AI needs to be in to attack
    [SerializeField]
    protected int expOnDeath = 1000;               // The amount of experience points AI gives to Scramblers on death
    [SerializeField]
    protected bool onlyAttackOnePlayer = false;    // AI will only target one player till they die
    [SerializeField]
    protected float timeToWaitBeforeMoving = 1.0f; //Time for AI to wait before moving after making an attack

    private bool canMove = true;
    private Rigidbody2D rb;                     // This AI's rigidbody2D

    [Header("Spawner AI")]
    public GameObject enemyTypeToSpawn;         // This will instantiate the given enemy AI type into the game

    private bool alerted = false;
    private SpriteRenderer currentSprite;
    [Header("Mimic AI")]
    [SerializeField]
    private Sprite mimicSprite;



    /*
     * Note to self: If the player is dead how do we know?
     * Can fix by just having referene to players which allows for getting Transform and bool if they are dead. Discuss with Chloe before implementing
     */
    bool gotPlayers = false;

    // Start is called before the first frame update
    void Start()
    {

        //Get the rigidbody
        rb = GetComponent<Rigidbody2D>();

        //Get the list of players
        //players = GameManager.ManagerInstance.GetPlayerTransforms();

        //Sets the value of health bar based on percentage of current health and max health
        UpdateHealthbarUI();

        //set the current sprite renderer component to currentSprite
        currentSprite = GetComponent<SpriteRenderer>();
    }


    protected override void Awake()
    {
        base.Awake();
        
        // Instantiate attack sequences to reattach the instance to the player
        for (int i = 0; i < AttackObjectList.Count; i++)
        {
            AttackObjectList[i] = Instantiate(AttackObjectList[i], gameObject.transform);
            AttackObjectList[i].transform.localScale = transform.localScale;
        }


        AttackList = new List<DefaultAttackSequence>();
        for (int i = 0; i < AttackObjectList.Count; i++)
        {
            //Set the layer of the attack sequecne to Overlord
            AttackObjectList[i].layer = this.gameObject.layer;
            AttackList.Add(AttackObjectList[i].GetComponent<DefaultAttackSequence>());
            AttackObjectList[i].GetComponent<DefaultAttackSequence>().Equip(this);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (!gotPlayers && MapMaker.mapFinished)
        {
            //Get the list of players
            players = GameManager.ManagerInstance.GetPlayerTransforms();
            gotPlayers = true;

            if (players == null)
            {
                gotPlayers = false;
            }
        }

        //If health is greater than max then set it to max (In case of healing)
        if (affectedStats[(int)Stats.health] > stats[(int)Stats.health]) affectedStats[(int)Stats.health] = stats[(int)Stats.health];
    }

    //Takes an array of all player transforms so AI can track players in the game
    public void CopyPlayersTransformList(Transform[] players)
    {
        this.players = players;
    }

    //Updates AI UI healthbar
    public override void UpdateHealthbarUI()
    {
        if (HealthBar != null)
        {
            HealthBar.SetValue(affectedStats[0] / (float)stats[0]);

            //Set the health bar active once damage is taken
            if (affectedStats[0] < stats[0]) HealthBar.gameObject.SetActive(true);
        }
    }

    //Gets the path given the start position and target position
    protected List<Vector3> GetPath(Vector3 startPos, Vector3 targetPos)
    {
        return Pathfinder.GetPath(startPos, targetPos, 90000);
    }

    //Send EXP to Game Manager to send to all players
    protected void DisperseEXP()
    {
        //Get the game manager to disperse the exp to players
        GameManager.ManagerInstance.DistributeExperience(expOnDeath);
    }

    //Determines if the AI is at the stopping distance from the player
    [Task]
    protected bool AtStoppingDistance()
    {
        if (player == null) return false;
        Vector3 distance = player.transform.position - transform.position;
        if (distance.magnitude < stoppingDistance)
        {
            return true;
        }
        return false;
    }

    //Determines if the AI is at the rereat distance from the player
    [Task]
    protected bool AtRetreatDistance()
    {
        if (player == null) return false;
        Vector3 distance = player.transform.position - transform.position;
        if (distance.magnitude < retreatDistance)
        {
            return true;
        }
        return false;
    }



    //Has AI move to given destination - currently the destination is the seen player
    [Task]
    protected void MoveToPlayer()
    {
        //Get shortest path to player
        currentPath = GetPath(this.transform.position, player.transform.position);

        //Move to each path node until reaching stopping distance
        for (int i = 0; i < currentPath.Count; i++)
        {
            if (canMove)
            {
                //Get vector to node
                Vector2 movementDir = (currentPath[i] - transform.position).normalized;

                //Make vector scaled to movement speed
                Vector2 nextFramePosition = movementDir * (affectedStats[(int)Stats.movespeed] /100f);

                //Set the AI's velocity toward that position
                rb.velocity = nextFramePosition;
            }
            else
            {
                StopMoving();
            }
        }
        Task.current.Succeed();
    }

    //Has AI move to given destination - currently the destination is the seen player
    [Task]
    protected void MoveAwayFromPlayer()
    {
        //Get shortest path to player
        currentPath = GetPath(this.transform.position, player.transform.position);

        //Move to each path node until reaching stopping distance
        for (int i = 0; i < currentPath.Count; i++)
        {
            if (canMove)
            {
                Debug.Log("Can Move");
                //Get vector to node
                Vector2 movementDir = -(currentPath[i] - transform.position).normalized;

                //Make vector scaled to movement speed
                Vector2 nextFramePosition = movementDir * (affectedStats[(int)Stats.movespeed] / 100f);

                //Set the AI's velocity toward that position
                rb.velocity = nextFramePosition;
            }
            else
            {
                Debug.Log("Cannot Move");
                StopMoving();
            }
        }
        Task.current.Succeed();
    }

    //Stops the AI from moving
    [Task]
    protected void StopMoving()
    {
        //Set velocity to zero to stop AI movement
        rb.velocity = Vector2.zero;
        Task.current.Succeed();
    }


    //Determines whether player is seen or not.
    //Will get the path from AI to player if the player is in sight.
    //Will also target the closest visible player
    [Task]
    protected bool SeePlayerAndSetTarget()
    {
        //temp
        if (!gotPlayers || players == null) { return false; }


        //This removes error log whith no players assigned
        bool playersIsNull = true;
        //If there are no assigned players to attack then stop looking
        for (int i = 0; i < players.Length; i++)
        {
            if (players[i] != null) { playersIsNull = false; break; }
        }
        if (players.Length <= 0 || playersIsNull)
        {
            return false;
        }

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
            if (distance.magnitude < visibleRange && !seeWall && !p.gameObject.GetComponent<Scrambler>().GetUntargetable())
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

        if (distance.magnitude <= attackRange)
            return true;

        return false;
    }

    //Spawns an attack on the attackSpawn towards player
    [Task]
    protected void AttackPlayer()
    {
        if (allowedToAttack && !disarmed)
        {
            StartCoroutine(WaitToMove());

            //Get vector towards player to hit
            Vector3 direction = new Vector3(target.x - this.transform.position.x,
                                            target.y - this.transform.position.y,
                                            0); 
            AttackList[0].StartAttack(direction, this); //AI will attack in direction of player

            Task.current.Succeed();
        }
        else
        {
            Task.current.Fail();
        }
    }

    private IEnumerator WaitToMove()
    {
        Debug.Log("Waiting");
        canMove = false;
        yield return new WaitForSeconds(timeToWaitBeforeMoving);
        Debug.Log("Done waiting");
        canMove = true;
    }

    //Checks if the health is less than the given value
    [Task]
    public bool IsHealthLessThan(int health)
    {
        return affectedStats[(int)Stats.health] <= health;
    }

    //Destroys AI object
    //NOTE: If AI.Die() is called while AI is being referenced as AbstractPlayer, AbstractPlayer.Die() will be used instead
    [Task] [PunRPC]//Make Photon RPC
    public new bool Die()
    {
        if(PhotonNetwork.CurrentRoom != null)
        {
            PhotonView OPview = gameObject.GetPhotonView();
            OPview.RPC("Die", RpcTarget.OthersBuffered);
            DisperseEXP();
        }
        Debug.Log("AI Dying");
        DisperseEXP(); //Send the experience for killing AI to players
        //Destroy(this.gameObject);
        if(PhotonNetwork.CurrentRoom != null)
        {
            PhotonNetwork.Destroy(GetComponent<PhotonView>());
        }
        else
        {
            Destroy(this.gameObject);
        }

        return true;
    }


    //Spawns the selected AI type into the game scene
    [Task]
    protected bool SpawnMinion()
    {
        //If the enemy type is not set return a fail
        if (enemyTypeToSpawn == null)
        {
            Debug.Log("Enemy type not set for spawning!");
            return false;
        }
        //Spawn the enemy
        else
        {
            GameObject minion = Instantiate(enemyTypeToSpawn, this.transform.position, Quaternion.identity);
            minion.GetComponent<AI>().expOnDeath = 0;
            return true;
        }
    }
    


    //NOTE: This current implementation seems "Hacky", so think of alternative solution...
    //Method used to ensure the triggering of an AI's death so that it will not move after attacking
    //This directly affects SuicideAI so that it will die after attack
    [Task]
    protected void EnsureDeath()
    {
        gameObject.GetComponent<SpriteRenderer>().enabled = !enabled; //disables the sprite renderer
        gameObject.GetComponent<BoxCollider2D>().enabled = !enabled; //disables the box collider
        attackRange = 100;      //Ensures AI will not move after it triggers death
        stoppingDistance = 10;  //Ensures AI to stop moving
        Task.current.Succeed();
    }

    [Task]
    // Condition statement for check if the AI has been alerted
    protected bool Alerted()
    {
        return alerted;
    }

    [Task]
    // Alert the mimic first time the player is in it range to change the sprite and visible range
    protected void AlertMimic()
    {
        if (!alerted)
        {
            alerted = true;
            visibleRange = 8.0f;
            currentSprite.sprite = mimicSprite;

        }
    }

    public override void Damage(int damageTaken)
    {
        Debug.Log("Call HitIndicator");
        StartCoroutine("HitIndicator");
        base.Damage(damageTaken);
    }

    public IEnumerator HitIndicator()
    {
        currentSprite.color = new Color(currentSprite.color.r, currentSprite.color.g, currentSprite.color.b, .5f);
        Debug.Log("Changed opacity");

        yield return new WaitForSeconds(0.2f);

        currentSprite.color = new Color(currentSprite.color.r, currentSprite.color.g, currentSprite.color.b, 1f);
        Debug.Log("Changed back to normal");
    }
}
