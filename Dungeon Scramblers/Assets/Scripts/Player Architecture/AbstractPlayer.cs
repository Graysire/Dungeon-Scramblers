using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//Contains the resources shared between the AI and players such as their ability to be damaged
public abstract class AbstractPlayer : HasStats, IDamageable<float>
{

    // Temporary stats - The stats that the player currently has through the game i.e. 130/200 health
    [SerializeField] protected float[] affectedStats = new float[] { 0f, 0f, 0f, 0f, 0f, 0f, 0f };
    [SerializeField] protected List<StatusEffect> statusEffects;

    //Adds the given status effect into the list of status effects
    public void AddStatusEffect(StatusEffect se)
    {
        statusEffects.Add(se);
    }

    //Removes inactive status effects
    public void SearchForInactveStatusEffects()
    {
        for (int i = 0; i < statusEffects.Count; ++i)
        {
            if (!statusEffects[i].IsActive())
            {
                statusEffects.RemoveAt(i);
            }
        }
    }

    protected virtual void Awake()
    {
        //set player affectedStats equal to their stats
        for (int i = 0; i < stats.Length; i++)
        {
            affectedStats[i] = stats[i];
        }
    }


    //Getter for affected stats
    public float[] GetAffectedStats()
    {
        return affectedStats;
    }

    //Reduces the health of the AbstractPlayer by damageTaken
    public virtual void Damage(float damageTaken)
    {
        affectedStats[(int)Stats.health] -= damageTaken;
    }

    public virtual void Die()
    {
        Destroy(this.gameObject);
    }

}
