using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StatusEffect : Ability
{
    /* This class is responsible for storing information on a status effect that will be applied to players/AI */
    /* The HasStats class is used to apply those values to the player it effects */
    /* Note: The effected GameObject (Player, AI, etc.) MUST have the HasStats class to apply the effects */

    public float cooldownTime = 0.0f;           //The time to wait after activating this ability. Use this when Status Effect is an activatable ability.

    [Header("Status Variables")]
    [SerializeField]
    bool doesReapply = false;                   //If true then the value to apply to unit will be applied again
    [SerializeField]
    [Min(2)]
    int waitTimeToReapply = 2;               //If this effect updates then after this many hundredths of SECONDS it will apply the values again
    [SerializeField]
    bool canStack = false;                      //If true, this effect can stack dealing its valueofEffect*stack ever application
    [SerializeField]
    bool resetEffectOnHit = false;              //If true, then when a unit is hit with the status effect already applied, then it will reset the timer on that instance.
    [SerializeField]
    bool appliesToBaseStats = false;            //If true, affects the stats and affected stats of a unit
    private int waitTimeLeft = 0;                   //Used to determine when to apply the status effect values again

    [SerializeField]
    bool isPermanent;                           //Whether this status effect lasts forever
    [SerializeField]
    int timeTillEnd = 0;                         //The amount of time in hundredths of SECONDS for this effect to last till worn off
    private int endTimeLeft;                     //Used to determine when the effect ends
    private int timesApplied = 0;                //Number of times this effect was applied
    private int maxTimesApplied;                 //the maximum number of times this effect can be applied based on its duration and wait time 
    private int totalValueApplied = 0;          //the total value of effect applied
    private int numStacks = 0;                   //the number of times this effect gas been stacked
    private bool Activated = false;             //Bool for keeping player from calling ability too often

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

    [Header("Disarm")]
    [SerializeField] private bool disarm = false; //Disarms the unit till the end of the effect

    private void OnDisable()
    {
        UpdateHandler.UpdateOccurred -= Updater;
    }

    public void BeginStatus()
    {
        StartCoroutine(StatusStart());
    }

    //Adds this status effect onto the given player
    public override void StartAttack(Vector3 AttackDirection, AbstractPlayer Unit)
    {
        //If this wasn't already activated then add status
        if (!Activated)
        {
            Activated = true;
            Unit.AddStatusEffect(this);
            StartCoroutine(Cooldown());
        }
    }

    //Cooldown time to use status again
    private IEnumerator Cooldown()
    {
        yield return new WaitForSeconds(cooldownTime);
        Activated = false;
    }


    private IEnumerator StatusStart()
    {
        Debug.Log("Status Started");

        //For disarming players
        if (disarm) unit.SetDisarmed(true);

        numStacks++;

        if(unit == null) { yield return new WaitForSeconds(0f); }

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


        //Only update if player information is given
        if (unit != null)
        {
            //subtracts the fixed time since the last frame from the duration and time until reapplied
            waitTimeLeft -= Mathf.CeilToInt(Time.deltaTime * 100);
            endTimeLeft -= Mathf.CeilToInt(Time.deltaTime * 100);

            //If this effect reapplies, then apply the value when the wait timer ends
            while (doesReapply && waitTimeLeft <= 0 && timesApplied < maxTimesApplied)
            {
                //Apply the status effect
                ApplyStatusEffectValue();

                //reset the wait time
                ResetWaitTime();
            }

            //If the time left is over, then the effect ends
            if (endTimeLeft <= 0 && !isPermanent)
            {
                EndEffect();
            }
        }
    }

    //Sets the time for status to stay alive
        //If status is applied to same player again then time is reset -- subject to change
    public void ResetStatusTime()
    {
        //resets how long the status will last
        endTimeLeft = timeTillEnd; //Time till the status ends
        maxTimesApplied += timeTillEnd / waitTimeToReapply;

        ResetWaitTime(); //Set the wait time as well
    }

    //Resets the wait time for applying effect values again
    private void ResetWaitTime()
    {
        //reset the wait time
        waitTimeLeft += waitTimeToReapply; //Time till the status applies its values again
    }

    //returns whether this statuseffect should reset on a hit
    public bool GetResetOnHit()
    {
        return resetEffectOnHit;
    }

    //ends this status effect
    public void EndEffect()
    {
        //For rearming players
        if (disarm) unit.SetDisarmed(false);

        //reset the stat value to its original value
        if (reverseEffectOnEnd)
        {
            unit.GetAffectedStats()[(int)statValueToAffect] -= totalValueApplied;
            if (appliesToBaseStats)
            {
                unit.GetStats()[(int)statValueToAffect] -= totalValueApplied;
            }
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
        //Apply the stat from this status effect onto the affected units stat
        Debug.Log(unit);
        unit.GetAffectedStats()[(int)statValueToAffect] += valueOfEffect * numStacks;
        if (appliesToBaseStats)
        {
            unit.GetStats()[(int)statValueToAffect] += valueOfEffect * numStacks;
        }
        timesApplied++;
        totalValueApplied += valueOfEffect * numStacks;

        //if this status effect updates health then update the Health UI
        if (statValueToAffect == HasStats.Stats.health)
        {
            unit.UpdateHealthbarUI();
        }
    }


    //This is called when player is hit
    public void SetAffectedPlayer(AbstractPlayer unit)
    {
        this.unit = unit;
    }

    //Reapplies this effect, adding stacks, resetting wait time and other functions as applicable
    public void ReapplyEffect()
    {
        //if the duration of this effect resets on hit, reset it
        if (resetEffectOnHit)
        {
            ResetStatusTime();
        }

        //if this effect can stack, add a stack
        if (canStack)
        {
            numStacks++;
            //if this effect doesn't naturally reapply, add one stack of value of effect directly
            if (!doesReapply)
            {
                unit.GetAffectedStats()[(int)statValueToAffect] += valueOfEffect;
                totalValueApplied += valueOfEffect;
                if (appliesToBaseStats)
                {
                    unit.GetStats()[(int)statValueToAffect] += valueOfEffect;
                }
            }
        }
    }
}