using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect : MonoBehaviour
{
    /* This class is responsible for storing information on a status effect that will be applied to players/AI */
    /* The HasStats class is used to apply those values to the player it effects */
    /* Note: The effected GameObject (Player, AI, etc.) MUST have the HasStats class to apply the effects */

    public string statusName;                   //Unique name for this status effect

    [SerializeField]
    bool doesReapply = false;                   //If true then the value to apply to unit will be applied again
    [SerializeField]
    bool resetEffectOnHit = false;              //If true, then when a unit is hit with the status effect already applied, then it will reset the timer on that instance.
    [SerializeField]
    [Min(2)]
    int waitTimeToApplyAgain = 0;               //If this effect updates then after this many hundredths of SECONDS it will apply the values again
    private int waitTimeLeft = 0;                   //Used to determine when to apply the status effect values again

    [SerializeField]
    bool isPermanent;                           //Whether this status effect lasts forever
    [SerializeField]
    int timeTillEnd = 0;                         //The amount of time in hundredths of SECONDS for this effect to last till worn off
    private int endTimeLeft;                     //Used to determine when the effect ends
    private int timesApplied = 0;                //Number of times this effect was applied
    private int maxTimesApplied;

    [SerializeField]
    int valueOfEffect;                 //The value of this status effect to apply
    [SerializeField]
    HasStats.Stats statValueToAffect;    //The HasStat enum value to effect on the Player/AI. Reference below:
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
    private bool reverseEffectOnEnd = false;        //If true then when this stat ends it will reapply the original stat value it affected
    //private float statValToReset;               //Stores the stat value player originally had to reapply it once status effect ends
    private bool updateReady = false;           //Used to let the updater know when it can perform the necessary operations after Start has finished

    /*
     * BUG: When player dies then if hit by status effect, instead of reapplying it creates new instances of it.
     */
    private void OnEnable()
    {
        StartCoroutine(StatusStart());
        
    }
    private void OnDisable()
    {
        UpdateHandler.UpdateOccurred -= Updater;
    }


    private IEnumerator StatusStart()
    {
        Debug.Log(statusName + " created...");
        
        if(unit == null) { yield return new WaitForSeconds(0f); }
        //Debug.Log(unit.name);
        //Get the time left till it will end
        ResetStatusTime();

        //Apply the status effect if this doesn't reapply
        if (!doesReapply)
        {
            ApplyStatusEffectValue();
        }

        //Set the update to run
        UpdateHandler.UpdateOccurred += Updater;
    }

    //Update for update handler 
    private void Updater()
    {
        StartCoroutine(UpdateEffect());
    }

    //Performs the update on the status effect
    IEnumerator UpdateEffect()
    {
        //If the unit is null or its not ready for the update then wait 
        while (unit == null) { yield return new WaitForSeconds(0.00f); }

        //Debug.Log("In Status Effect Update...");

        //Only update if player information is given
        if (unit != null)
        {
            //Used for indepth testing of time
            //if (Time.fixedTime <= 50)
            //{
            //    Debug.Log("WTF " + Time.fixedTime + " " + (waitTimeLeft - Mathf.CeilToInt(Time.fixedTime * 100))); //+ " " + Mathf.FloorToInt(Time.time * 100) + " " + Mathf.CeilToInt(Time.time * 100f));
            //}

            //Get the time left to apply effect values again [Deprecated]
            //int timeLeft = waitTimeLeft - Mathf.CeilToInt(Time.fixedTime * 100);


            //subtracts the fixed time since the last frame from the duration and time until reapplied
            waitTimeLeft -= Mathf.CeilToInt(Time.deltaTime * 100);
            endTimeLeft -= Mathf.CeilToInt(Time.deltaTime * 100);

            //Debug.Log("Wait: " + waitTimeLeft + " End: " + endTimeLeft + " Applied: " + timesApplied);

            //Debug.Log("Time " + Time.time + " Wait Left " + waitTimeLeft + " Time Left "  + timeLeft);

            //Debug.Log("Time left for status effect to reapply affect: " + timeLeft);

            //If this effect reapplies, then apply the value when the wait timer ends
            while (doesReapply && waitTimeLeft <= 0 && timesApplied < maxTimesApplied)
            {
                //Debug.Log("Applying Status Effect Again");

                //Apply the status effect
                ApplyStatusEffectValue();

                //reset the wait time
                ResetWaitTime();
            }

            //Debug.Log("Wait: " + waitTimeLeft + " End: " + endTimeLeft + " Applied: " + timesApplied + " Max: " + maxTimesApplied);

            //Get the time left till status ends [Deprecated]
            //timeLeft = endTimeLeft - Mathf.CeilToInt(Time.fixedTime * 100);


            //Debug.Log("Time left for status effect to end: " + timeLeft);

            //If the time left is over, then the effect ends
            if (endTimeLeft <= 0 && !isPermanent)
            {
                EndEffect();
            }

            
        }
        //yield return new WaitForSecondsRealtime(0.00f);
    }

    //Sets the time for status to stay alive
        //If status is applied to same player again then time is reset -- subject to change
    public void ResetStatusTime()
    {
        //resets how long the status will last
        endTimeLeft = timeTillEnd;// + Mathf.CeilToInt(Time.fixedTime * 100); //Time till the status ends
        maxTimesApplied += timeTillEnd / waitTimeToApplyAgain;
       
        //Debug.Log("End Time TIME: " + Time.time);
        //Debug.Log("End Time Set to: " + endTimeLeft);

        ResetWaitTime(); //Set the wait time as well
    }

    //Resets the wait time for applying effect values again
    private void ResetWaitTime()
    {
        //reset the wait time
        waitTimeLeft += waitTimeToApplyAgain;// + Mathf.CeilToInt(Time.fixedTime * 100); //Time till the status applies its values again

        //Debug.Log("Wait Time TIME: " + Time.fixedTime + ", " + Time.time );
        //Debug.Log("Wait Time Set to: " + waitTimeLeft);
    }

    public bool GetResetOnHit()
    {
        return resetEffectOnHit;
    }

    //ends this status effect
    public void EndEffect()
    {
        //Debug.Log("Status Effect Ending...");
        //reset the stat value to its original value
        if (reverseEffectOnEnd)
        {
            //unit.GetAffectedStats()[(int)statValueToAffect] = statValToReset;
            unit.GetAffectedStats()[(int)statValueToAffect] += timesApplied * valueOfEffect * -1;
        }

        //Lets the update handler know this is done 
        gameObject.SetActive(false);

        //Destroy this object
        Destroy(gameObject);
    }


    //Applies the status effect values to the player
    //IEnumerator ApplyStatusEffectValue()
    void ApplyStatusEffectValue()
    {
        //Save the original stat value to apply when status ends
        //if (reverseEffectOnEnd) { statValToReset = unit.GetAffectedStats()[(int)statValueToAffect]; }

        //wait until the unit is assigned to apply the damage
        //while (unit == null) { yield return new WaitForSecondsRealtime(0.00f); }

        //Debug.Log("Applying effect value: " + valueOfEffect); 
        
        //Apply the stat from this status effect onto the affected units stat
        unit.GetAffectedStats()[(int)statValueToAffect] += valueOfEffect;
        timesApplied++;
    }


    //This is called when player is hit
    public void SetAffectedPlayer(AbstractPlayer unit)
    {
        this.unit = unit;
    }
}