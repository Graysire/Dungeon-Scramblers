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
    }

    //Sets the target to the player location
    [Task]
    public void TargetPlayer()
    {
        target = player.transform.position;
        Task.current.Succeed();
    }

    //Determines whether player is seen or not
    [Task]
    bool SeePlayer()
    {
        Vector3 distance = player.transform.position - this.transform.position;

        RaycastHit hit;
        bool seeWall = false;

        Debug.DrawRay(this.transform.position, distance, Color.red);

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

        if (distance.magnitude < visibleRange && !seeWall)
        {
            //Get path to player
            currentPath = GetPath(this.transform.position, player.transform.position);
            return true;
        }
        else
        {
            return false;
        }
    }
}
