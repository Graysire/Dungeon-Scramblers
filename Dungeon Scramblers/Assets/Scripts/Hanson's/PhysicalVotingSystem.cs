using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class PhysicalVotingSystem : MonoBehaviour
{

    [SerializeField]
    private VoteButton[] Buttons;

    [SerializeField]
    private TextMeshPro Timer;

    private Boolean shown = false;

    public float TimeRemaining = 4.0f;

    VoteButton resultButton;

    public VoteButton getResultButton() { return resultButton; }

    private void Update()
    {
        if (TimeRemaining >= 0)
        {
            TimeRemaining -= Time.deltaTime;
            double TempTime = Math.Round(TimeRemaining, 2);
            Timer.text = ("Time: " + TempTime);
        }
        else if (!shown)
        {
            shown = true;
            Timer.text = ("Time: 0");
            // Call handle result here, possibly pop-up tab with the item that the player get
            HandleResult();
        }

    }

    private void HandleResult()
    {
        bool bEqual = false;
       
        resultButton = new VoteButton();

        foreach (VoteButton button in Buttons)
        {
            if(button.getVote() > resultButton.getVote())
            {
                resultButton = button;
                bEqual = false;

            }
            else if(resultButton.getVote() == button.getVote())
            {
                bEqual = true;
                
            }
            
        }

        if(bEqual)
        {
            int rand = UnityEngine.Random.Range(0, Buttons.Length);
            resultButton = Buttons[rand];
        }
        
        Debug.Log("Result: " + resultButton);
       
    }

}
