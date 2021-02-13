using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect : MonoBehaviour
{
    /* This class is responsible for storing information on a status effect that will be applied to players/AI */
    /* The HasStats class is used to apply those values to the player it effects */
    /* Note: The effected GameObject (Player, AI, etc.) MUST have the HasStats class to apply the effects */

    public string statusName;                   //Unique name for this status effect

    public bool doesReapply = false;            //If true then the value to apply to unit will be applied again
    public bool resetEffectOnHit = false;       //If true, then when a unit is hit with the status effect already applied, then it will reset the timer on that instance.
    public float waitTimeToApplyAgain = 0.0f;   //If this effect updates then after this many SECONDS it will apply the values again
    private float waitTimeLeft;                 //Used to determine when to apply the status effect values again

    public float timeTillEnd = 0.0f;            //The amount of time in SECONDS for this effect to last till worn off
    private float endTimeLeft;                  //Used to determine when time ends

    public float valueOfEffect;                 //The value of this status effect to apply
    public int statValueToAffect;               //The HasStat enum value to effect on the Player/AI. Reference below:
    /*  health = 0,
        movespeed = 1,
        attackdmg = 2,
        attackspeed = 3,
        abilitydmg = 4,
        abilitycd = 5,
        defense = 6
     */

    private AbstractPlayer unit;                //The player or AI to effect

    [SerializeField]
    private bool resetStatOnEnd = false;        //If true then when this stat ends it will reapply the original stat value it affected
    private float statValToReset;               //Stores the stat value player originally had to reapply it once status effect ends


    /*
     * BUG: When player dies then if hit by status effect, instead of reapplying it creates new instances of it.
     */
    private void OnEnable()
    {
        StatusStart();
        UpdateHandler.UpdateOccurred += UpdateEffect;
    }
    private void OnDisable()
    {
        UpdateHandler.UpdateOccurred -= UpdateEffect;
    }


    private void StatusStart()
    {
        Debug.Log(statusName + " created...");

        //Get the time left till it will end
        ResetStatusTime();

        //Apply the status effect
        ApplyStatusEffectValue();
    }

    private void UpdateEffect()
    {
        Debug.Log("In Status Effect Update...");

        //Only update if player information is given
        if (unit != null)
        {
            //Get the time left till status ends
            float timeLeft = endTimeLeft - Time.time;
            Debug.Log("Time left for status effect to end: " + timeLeft);

            //If the time left is over, then the effect ends
            if (timeLeft <= 0.0f)
            {
                Debug.Log("Status Effect Ending...");
                //reset the stat value to its original value
                if (resetStatOnEnd) { unit.GetAffectedStats()[statValueToAffect] = statValToReset; }

                gameObject.SetActive(false);
                Destroy(gameObject);
            }

            //Get the time left to apply effect values again
            timeLeft = waitTimeLeft - Time.time;
            Debug.Log("Time left for status effect to reapply affect: " + timeLeft);

            //If this effect reapplies, then apply the value when the wait timer ends
            if (doesReapply && timeLeft <= 0.0f)
            {
                Debug.Log("Applying Status Effect Again");
                //Apply the status effect
                ApplyStatusEffectValue();

                //reset the wait time
                ResetWaitTime();
            }
        }
    }

    //Sets the time for status to stay alive
        //If status is applied to same player again then time is reset -- subject to change
    public void ResetStatusTime()
    {
        endTimeLeft = timeTillEnd + Time.time; //Time till the status ends
       // Debug.Log("End Time TIME: " + Time.time);
       // Debug.Log("End Time Set to: " + endTimeLeft);
        ResetWaitTime(); //Set the wait time as well
    }

    //Resets the wait time for applying effect values again
    private void ResetWaitTime()
    {
        waitTimeLeft = waitTimeToApplyAgain + (Time.time); //Time till the status applies its values again
        //Debug.Log("Wait Time TIME: " + Time.time);
        //Debug.Log("Wait Time Set to: " + waitTimeLeft);
    }



    //Applies the status effect values to the player
    private void ApplyStatusEffectValue()
    {
        //Debug.Log("Applying Damage: " + valueOfEffect); 

        //Save the original stat value to apply when status ends
        if (resetStatOnEnd) { statValToReset = unit.GetAffectedStats()[statValueToAffect]; }

        //Apply the stat from this status effect onto the affected units stat
        unit.GetAffectedStats()[statValueToAffect] += valueOfEffect;
    }


    //This is called when player is hit
    public void SetAffectedPlayer(AbstractPlayer unit)
    {
        this.unit = unit;
    }
}