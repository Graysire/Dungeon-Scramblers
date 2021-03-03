using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Contains the resources shared between the AI and players such as their ability to be damaged
public abstract class AbstractPlayer : HasStats, IDamageable<int>
{

    // Temporary stats - The stats that the player currently has through the game i.e. 130/200 health
    [SerializeField] protected int[] affectedStats = new int[] { 0, 0, 0, 0, 0, 0, 0 };

    protected bool allowedToAttack = true;

    //The list of status effects applied to the player
    public List<StatusEffect> statusEffects = new List<StatusEffect>();

    //Adds the given status effect into the list of status effects
        //instantiates the objects, preps it for use, and sets it to active
    public void AddStatusEffect(StatusEffect statusEffectPrefab)
    {
        //If the status effect is null then fire debug message
        if (statusEffectPrefab == null)
        {
            Debug.Log("Given Status Effect is null...");
        }

        //Find instance of this object already created
        StatusEffect existingInstance = FoundInstanceOfStatusEffect(statusEffectPrefab);
        
        //If this object doesnt exist
        if (existingInstance == null)
        {
            //Debug.Log("CREATING NEW INSTANCE OF STATUS EFFECT");

            //Create instance of this object
            StatusEffect statusEffect = Instantiate(statusEffectPrefab, gameObject.GetComponentInParent<Transform>());

            //Add item into empty slot or append at end
            bool added = false;
            for (int i = 0; i < statusEffects.Count; ++i)
            {
                if (statusEffects[i] == null)
                {
                    statusEffects[i] = statusEffect;
                    added = true;
                }
            }
            if (!added)
                statusEffects.Add(statusEffect);

            //Debug.Log("SETTING PLAYER INFORMATION INTO INSTANCE");

            statusEffect.SetAffectedPlayer(this);
        }
        else if (existingInstance.GetResetOnHit())
        {
            //Debug.Log("RESETTING INSTANCE OF STATUS EFFECT");

            //if this object is already made then reset its timer apply for full duration again
            existingInstance.ResetStatusTime();
        }
    }

    //Checks if the given status effect is already applied to a player
    //if exists - returns the object
    //else - null
    private StatusEffect FoundInstanceOfStatusEffect(StatusEffect statusEffect)
    {
        //If there is not instantiated status effects or the given status effect is null
        if (statusEffect == null || statusEffects == null) return null;

        for (int i = 0; i < statusEffects.Count; ++i)
        {
            //skip if null
            if (statusEffects[i] == null) { continue; }

            //compare names
            if (statusEffects[i].statusName == statusEffect.statusName)
            {
                return statusEffects[i];
            }
        }
        return null;
    }


    protected virtual void Awake()
    {
        //set player affectedStats equal to their stats
        for (int i = 0; i < stats.Length; i++)
        {
            affectedStats[i] = stats[i];
        }
    }

    public void SetAllowedToAttack(bool atk)
    {
        allowedToAttack = atk;
    }

    //Getter for affected stats
    public int[] GetAffectedStats()
    {
        return affectedStats;
    }

    //Reduces the health of the AbstractPlayer by damageTaken
    public virtual void Damage(int damageTaken)
    {
        if (damageTaken - affectedStats[(int)Stats.defense] < 5 && damageTaken != 0)
        {
            damageTaken = 5;
        }
        else
        {
            damageTaken -= affectedStats[(int)Stats.defense];
        }

        Debug.Log("Damage: " + damageTaken);
        affectedStats[(int)Stats.health] -= damageTaken;
    }

    public virtual void Die()
    {
        Destroy(this.gameObject);
    }

}
