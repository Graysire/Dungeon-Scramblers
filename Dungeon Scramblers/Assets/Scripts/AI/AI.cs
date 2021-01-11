using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Panda;


public class AI : MonoBehaviour
{
    public Transform player;
    public Transform bulletSpawn;
    public Slider healthBar;
    public GameObject bulletPrefab;
    
    public Vector3 destination; // The movement destination
    public Vector3 target;      // The position to aim to
    List<Vector3> currentPath;  // Stores the current path being used

    float health = 100.0f;
    public float speed = 15.0f;
    float visibleRange = 80.0f;
    float shotRange = 40.0f;


    
    // Start is called before the first frame update
    void Start()
    { 
        InvokeRepeating("UpdateHealth", 5, 0.5f);
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



    //Has AI move to given destination
    [Task]
    public void MoveToDestination()
    {
        for (int i = 0; i < currentPath.Count; i++)
        {
            this.transform.position = Vector3.MoveTowards(transform.position, currentPath[i], speed * Time.deltaTime);
        }
        Task.current.Succeed();
    }

    //Sets the target to the player location, can be used for AI attack
    [Task]
    public void TargetPlayer()
    {
        target = player.transform.position;
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
                //Get path to player
                currentPath = GetPath(this.transform.position, player.transform.position);
                return true;
            }
        }
        return false;
    }

    //Spawns a bullet using a bullet prefab (which is not currently made)
    [Task]
    public bool Fire()
    {
        GameObject bullet = GameObject.Instantiate(bulletPrefab, bulletSpawn.transform.position, bulletSpawn.transform.rotation);
        bullet.GetComponent<Rigidbody>().AddForce(bullet.transform.forward * 2000);
        return true;
    }

    //For AI death
    [Task]
    public bool Explode()
    {
        if (healthBar != null)
        {
            Destroy(healthBar.gameObject);
        }

        Destroy(this.gameObject);
        return true;
    }
    
    //Lines up the shot to player
    [Task]
    bool ShotLinedUp()
    {
        Vector3 distance = target - this.transform.position;
        if (distance.magnitude < shotRange && Vector3.Angle(this.transform.forward, distance) < 1.0f)
            return true;
        else
            return false;
    }
}
