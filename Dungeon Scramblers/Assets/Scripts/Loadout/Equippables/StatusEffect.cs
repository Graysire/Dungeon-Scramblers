using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect : HasStats
{
    /* This class is responsible for storing information on a status effect that will be applied to players/AI */
    /* The HasStats class is used to apply those values to the player it effects */
    /* Note: The effected GameObject (Player, AI, etc.) MUST have the HasStats class to apply the effects */



    public string uniqueStatusName;             //The name of this status effect

    public bool doesUpdate = false;             //If true then the value to apply to player will be applied again
    public float waitTimeToApplyAgain = 0.0f;   //If this effect updates then after this many seconds it will apply the values again
    private float waitTimeLeft;                 //Used to determine when to apply the status effect values again

    public int statToEffect;            //Uses this value to effect the stat is relates to
    public float timeTillEnd = 0.0f;    //The amount of time for this effect to last till worn off
    private float endTimeLeft;          //Used to determine when time ends

    private HasStats stats;             //gets the stats list of the affected unit

    private GameObject affectedUnit;    //The player or AI being affected
    private bool isActive;              //Determines if this effect is still active, when not it will be removed
    public bool resetStatOnEnd = false; //If true then when this stat ends it will reapply the original stat value it affected
    private float statValToReset;       //Stores the stat value player originally had to reapply it
    public int statNumToAffect;         //The HasStat enum value to effect on the Player/AI. Reference below:
                                                        /*  health = 0,
                                                            movespeed = 1,
                                                            attackdmg = 2,
                                                            attackspeed = 3,
                                                            abilitydmg = 4,
                                                            abilitycd = 5,
                                                            defense = 6
                                                         */


    // Start is called before the first frame update
    void Start()
    {
        isActive = true;

        //Get the time left till it will end
        ResetStatusTime();

        //Apply the status effect
        ApplyStatusEffectValue();
    }



    // Update is called once per frame
    void Update()
    {
        //Get the time left till status ends
        float timeLeft = endTimeLeft - Time.deltaTime;

        //If the time left is over then effect ends
        if (timeLeft <= 0.0f)
        {
            //reset the stat value
            if (resetStatOnEnd) { stats.GetAffectedStats()[statNumToAffect] = statValToReset; }

            //set this to end
            isActive = false;
        }

        //Get the time left to apply effect values again
        timeLeft = waitTimeLeft - Time.deltaTime;

        //If this effect updates then apply the vale when the timer ends
        if (doesUpdate && timeLeft <= 0.0f)
        {
            //Apply the status effect
            ApplyStatusEffectValue();

            //reset the wait time
            ResetWaitTime();
        }
    }

    //Sets the time for status to stay alive
        //If status is applied to same player again then time is reset -- subject to change
    public void ResetStatusTime()
    {
        endTimeLeft = timeTillEnd + Time.deltaTime; //Time till the status ends
        ResetWaitTime(); //Set the wait time
    }

    //Resets the wait time for applying effect values again
    private void ResetWaitTime()
    {
        waitTimeLeft = waitTimeToApplyAgain + Time.deltaTime; //Time till the status applies its values again
    }



    //Applies the status effect values to the player
    private void ApplyStatusEffectValue()
    {
        //If affected unit has stats to affect
        if (affectedUnit.GetType().IsSubclassOf(typeof(HasStats)))
        {
            stats = affectedUnit.GetComponent<HasStats>();

            //Save the original stat value to apply when status ends
            if (resetStatOnEnd) { statValToReset = stats.GetAffectedStats()[statNumToAffect]; }

            //Apply the stat from this status effect onto the affected units stat
            stats.GetAffectedStats()[statNumToAffect] += this.affectedStats[statNumToAffect];
        }
    }


    //This is called when player is hit
    public void SetAffectedPlayer(GameObject affectedUnit)
    {
        this.affectedUnit = affectedUnit;
    }
}
