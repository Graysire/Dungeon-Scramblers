using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Panda;


public class AI : MonoBehaviour
{
    public Transform player;
    public Slider healthBar;
    public GameObject attackSpawn;
    public GameObject attackPrefab;
    
    public Vector3 destination; // The movement destination
    public Vector3 target;      // The position to aim to
    List<Vector3> currentPath;  // Stores the current path being used

    public float stoppingDistance = 30.0f;  //Distance from player AI stops at
    public float health = 100.0f;           //Ai health
    public float speed = 15.0f;             //Movement speed of AI
    public float visibleRange = 80.0f;      //Range AI needs to be in to see Player
    public float attackRange = 40.0f;       //Range AI needs to be in to attack
    public float attackForce = 2000.0f;     //Determines how fast projectile is



    // Start is called before the first frame update
    void Start()
    { 
        //InvokeRepeating("UpdateHealth", 5, 0.5f);
    }

    // Update is called once per frame
    void Update()
    {
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

    //Deals damage to AI if hit by a bullet
    void OnCollisionEnter(Collision col)
    {
        if (col.gameObject.tag == "bullet")
        {
            health -= 10;
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
        {
            return true;
        }
        return false;
    }






    //Has AI move to given destination
    [Task]
    public void MoveToDestination()
    {
        //Get path to player
        currentPath = GetPath(this.transform.position, player.transform.position);
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
    [Task]
    bool SeePlayer()
    {
        if (player != null)
        {
            //Get distance from AI and player
            Vector3 distance = player.transform.position - this.transform.position;

            RaycastHit hit;
            bool seeWall = false;

            Debug.DrawRay(this.transform.position, distance, Color.red);

            //Checks for wall w/ raycast
            if (Physics.Raycast(this.transform.position, distance, out hit))
            {
                if (hit.collider.gameObject.tag == "wall")
                {
                    seeWall = true;
                }
            }
            if (Task.isInspected)
            {
                Task.current.debugInfo = string.Format("wall={0}", seeWall);
            }

            //If player is in visible range of AI
            if (distance.magnitude < visibleRange && !seeWall)
            {
                return true;
            }
        }
        return false;
    }

    //Determines if the Player is in range of attack
    [Task]
    bool AttackInRange()
    {
        target = player.transform.position;
        Vector3 distance = target - this.transform.position;
        if (distance.magnitude < attackRange)
            return true;
        else
            return false;
    }

    //Spawns an attack on the attackSpawn towards player
    [Task]
    public bool Attack()
    {
        Vector3 direction = target - this.transform.position;
        attackSpawn.transform.up = direction;
        GameObject attack = GameObject.Instantiate(attackPrefab, attackSpawn.transform.position, attackSpawn.transform.rotation);
        attack.GetComponent<Rigidbody>().AddForce(direction * attackForce);
        return true;
    }

    //For AI death
    [Task]
    public bool Death()
    {
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }
        //Destroy(attackSpawn.gameObject);
        Destroy(this.gameObject);
        return true;
    }

}
