using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using TMPro;

public class PhysicalVotingSystem : MonoBehaviour
{

    [SerializeField]
    private List<VoteButton> Buttons;

    private Boolean shown = false;

    public float TimeRemaining = 4.0f;

    VoteButton resultButton;

    public VoteButton getResultButton() { return resultButton; }

    public void IncrementButton(VoteButton button)
    {
        Buttons.Add(button);
    }

    public void HandleResult()
    {
        bool bEqual = false;

        resultButton = new VoteButton();

        foreach (VoteButton button in Buttons)
        {
            if (button && button.getVote() > resultButton.getVote())
            {
                resultButton = button;
                bEqual = false;

            }
            else if (button && resultButton.getVote() == button.getVote())
            {
                bEqual = true;

            }

        }

        if (bEqual)
        {
            int rand = UnityEngine.Random.Range(0, Buttons.Count);
            resultButton = Buttons[rand];
        }

        Debug.Log("Result: " + resultButton);
        if (resultButton != null)
        {
            GameManager.ManagerInstance.ApplyPerk(resultButton.getPerk());
        }


        foreach (VoteButton button in Buttons)
        {
            if (button)
            {
                Destroy(button.gameObject);
            }
        }
    }

}
