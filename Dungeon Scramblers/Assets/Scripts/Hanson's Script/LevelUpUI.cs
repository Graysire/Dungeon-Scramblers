using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class LevelUpUI : MonoBehaviour
{
    LevelUpSystem level;

    [SerializeField]
    private Image ExperienceBar;
    [SerializeField]
    private Text ExperienceText;
    [SerializeField]
    private Text LevelText;

    // Start is called before the first frame update
    void Start()
    {
        level = new LevelUpSystem();
        UpdateExperienceBar();
    }

    
    public void AddExperience(int experience)
    {
        level.AddExperience(experience);
        UpdateExperienceBar();
    }

    public void UpdateExperienceBar()
    {
        ExperienceBar.fillAmount = (float)level.getExperience() / level.getExperienceToNextLevel();
        ExperienceText.text = level.getExperience() + "/ " + level.getExperienceToNextLevel();
        LevelText.text = "Level: " + level.getLevel().ToString();
    }



}
