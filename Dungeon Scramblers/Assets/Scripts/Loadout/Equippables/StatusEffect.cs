using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect : MonoBehaviour
{
    /* This class is responsible for storing information on a status effect that will be applied to players/AI */
    /* The HasStats class is used to apply those values to the player it effects */
    /* Note: The effected GameObject (Player, AI, etc.) MUST have the HasStats class to apply the effects */



    //public string uniqueStatusName;             //The name of this status effect

    public bool doesUpdate = false;             //If true then the value to apply to player will be applied again
    public float waitTimeToApplyAgain = 0.0f;   //If this effect updates then after this many seconds it will apply the values again
    private float waitTimeLeft;                 //Used to determine when to apply the status effect values again

    public int statToEffect;            //Uses this value to effect the stat is relates to
    public float timeTillEnd = 0.0f;    //The amount of time for this effect to last till worn off
    private float endTimeLeft;          //Used to determine when time ends

    //private float[] stats;             //gets the stats list of the affected unit

    public float valueOfEffect;         //The value of this status effect to apply
    public int statNumToAffect;         //The HasStat enum value to effect on the Player/AI. Reference below:
    /*  health = 0,
        movespeed = 1,
        attackdmg = 2,
        attackspeed = 3,
        abilitydmg = 4,
        abilitycd = 5,
        defense = 6
     */

    private AbstractPlayer unit;        //The player or AI to effect
    

    [SerializeField]
    private bool resetStatOnEnd = false; //If true then when this stat ends it will reapply the original stat value it affected
    private float statValToReset;       //Stores the stat value player originally had to reapply it
    private bool isActive;              //Determines if this effect is still active, when not it will be removed



    protected void OnEnable(){
        UpdateHandler.UpdateOccurred += StatusUpdate;
        UpdateHandler.StartOccurred += StatusStart;
    }
    private void OnDisable(){
        UpdateHandler.UpdateOccurred -= StatusUpdate;
        UpdateHandler.StartOccurred -= StatusStart;
    }

    protected void StatusStart() {
        isActive = true;

        //Get the time left till it will end
        ResetStatusTime();

        //Apply the status effect
        ApplyStatusEffectValue();
    }

    protected void StatusUpdate() {
        //Get the time left till status ends
        float timeLeft = endTimeLeft - Time.deltaTime;

        //If the time left is over then effect ends
        if (timeLeft <= 0.0f)
        {
            //reset the stat value
            if (resetStatOnEnd) { unit.GetAffectedStats()[statNumToAffect] = statValToReset; }

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

        //Save the original stat value to apply when status ends
        if (resetStatOnEnd) { statValToReset = unit.GetAffectedStats()[statNumToAffect]; }

        //Apply the stat from this status effect onto the affected units stat
        unit.GetAffectedStats()[statNumToAffect] += valueOfEffect;
    }


    //This is called when player is hit
    public void SetAffectedPlayer(AbstractPlayer unit)
    {
        this.unit = unit;
    }
}




/*  Graysons Recomnedation using a OnTick implementation
statsToAffect[] = (HasStats.Stats.Health, -10), (moveSpeed, -20000)
bool reversedOnEnd

(stat, effectValue)

Player calls OnTick(Player.affectedStats)


Tick(Stats targetStats)
    for each statAffected
        target.Stats += statEffect
    duration--;
    if(duration == 0 && reversedOnEnd)
        Reverse(targetStats)

Reverse(targetStats)
    for each statAffected
        targetStats -= statEffect * originalDuration




struct statTuples
{
    public hasStats.Stats
}


Player.OnTick()
{
    for each Effect e in Effects
        effect.apply(
}
*/