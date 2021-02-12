using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect : MonoBehaviour
{
    /* This class is responsible for storing information on a status effect that will be applied to players/AI */
    /* The HasStats class is used to apply those values to the player it effects */
    /* Note: The effected GameObject (Player, AI, etc.) MUST have the HasStats class to apply the effects */



    //public string uniqueStatusName;             //The name of this status effect

    public bool doesReapply = false;            //If true then the value to apply to unit will be applied again
    public float waitTimeToApplyAgain = 0.0f;   //If this effect updates then after this many SECONDS it will apply the values again
    private float waitTimeLeft;                 //Used to determine when to apply the status effect values again

    public float timeTillEnd = 0.0f;            //The amount of time in SECONDS for this effect to last till worn off
    private float endTimeLeft;                  //Used to determine when time ends


    public float valueOfEffect;                 //The value of this status effect to apply
    public int statNumToAffect;                 //The HasStat enum value to effect on the Player/AI. Reference below:
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

    //Returns if this effect is active
    public bool IsActive()
    {
        return isActive;
    }

    protected void OnEnable(){
        isActive = true;
        UpdateHandler.UpdateOccurred += StatusUpdate;
        UpdateHandler.StartOccurred += StatusStart;
    }
    private void OnDisable(){
        isActive = false;
        UpdateHandler.UpdateOccurred -= StatusUpdate;
        UpdateHandler.StartOccurred -= StatusStart;
    }


    /*
     * Add the status effects to a player through the OnTriggerEnter2D function inside ProjectileStats.
     * ProjectileStats will contain a list of status effects that we add. 
     * Player will store their StatusEffects applied to them until they are inactive.
     */

    //Starts the Status Effect to update
    protected void StatusStart() 
    {
        //Get the time left till it will end
        ResetStatusTime();

        //Apply the status effect
        ApplyStatusEffectValue();

        //Sets this gameobject as inactive
        gameObject.SetActive(true);
    }


    //Update Function for checking if time has ended for this status
        //or to update the value applied
    protected void StatusUpdate() 
    {
        //Only update if player information is given
        if (unit != null)
        {
            //Get the time left till status ends
            float timeLeft = endTimeLeft - Time.deltaTime;

            //If the time left is over, then the effect ends
            if (timeLeft <= 0.0f)
            {
                //reset the stat value to its original value
                if (resetStatOnEnd) { unit.GetAffectedStats()[statNumToAffect] = statValToReset; }

                gameObject.SetActive(false);
            }

            //Get the time left to apply effect values again
            timeLeft = waitTimeLeft - Time.deltaTime;

            //If this effect reapplies, then apply the value when the wait timer ends
            if (doesReapply && timeLeft <= 0.0f)
            {
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