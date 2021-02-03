using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DumbAITest : MonoBehaviour
{
    [SerializeField]
    [Range(100,250)]
    float MovementSpeed = 500f;
    [SerializeField]
    [Range(2, 10)]
    float RotationSpeed = 5f;
    [SerializeField]
    [Range(5, 40)]
    private float visDist = 20;
    [SerializeField]
    [Range(30, 90)]
    float visAngle = 60f;

    private Rigidbody2D rb;

    //Get list of players from Initializer
    public GameObject[] Players;

    //Before countdown
    void Start()
    {
        //Get Players via nametag
        Invoke("LookforPlayer", 2);
        rb = GetComponent<Rigidbody2D>();
    }


    void LookforPlayer()
    {
        Players = GameObject.FindGameObjectsWithTag("Player");

    }
    void Update()
    {

            //Compare
            Transform player = GetClosestTarget();
            Debug.Log("Closest Player: " + GetClosestTarget().ToString());
            Vector2 direction = player.position - transform.position;

            float angle = Vector3.Angle(direction, transform.forward);

        rb.velocity = direction * Time.deltaTime * MovementSpeed;
        Debug.Log("velocity:" + rb.velocity);
     
        Debug.Log("direction:" + direction);
        //Debug.Log("direction Magnitude: " + direction.magnitude +  "\n" + "Angle: " + angle );
        if (direction.magnitude < visDist && angle < visAngle)
            {
                //This will make the skeleton rotate towards the player
                transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * RotationSpeed);

            //This will make the AI move towards the player
        


            }
            else
            {
                // Debug.Log("I don't see you");
            }
       
    }


    //This function will go through the list of players and find the closest one to him
    private Transform GetClosestTarget()
    {
        Transform closestPlayer = null;
        float minDist = Mathf.Infinity;
        for (int i = 0; i < Players.Length; i ++)
        {
            float distance = Vector3.Distance(transform.position, Players[i].transform.position);
            if(distance < minDist)
            {
                closestPlayer = Players[i].transform;
                minDist = distance;
            }
        }
        //Return the Player closest to us
        return closestPlayer;
    }
        

}

