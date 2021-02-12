using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LevelUpSystem
{
    private int Level = 1;
    private int Experience = 0;
    private int ExperienceToNextLevel = 100;
    // Start is called before the first frame update
   

    public void AddExperience(int amount)
    {
        Experience += amount;
        if(Experience >= ExperienceToNextLevel)
        {
            // Enough experience to level up
            Level++;
            Experience -= ExperienceToNextLevel;
        }
    }
    
    public int getLevel()
    {
        return Level; 
    }

    public int getExperience()
    {
        return Experience;
    }
    
    public int getExperienceToNextLevel()
    {
        return ExperienceToNextLevel;
    }

}
