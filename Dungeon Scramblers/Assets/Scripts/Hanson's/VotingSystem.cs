using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Doozy.Engine.UI;
using System;
using System.Linq;
using UnityEngine.UI;

public class VotingSystem : MonoBehaviour
{
    [SerializeField]
    private UIButton[] Buttons;
    [SerializeField]
    private GameObject[] Images;
    [SerializeField]
    private UIButton Timer;
    [SerializeField]
    private UIPopup Popup;

    [SerializeField]
    private Image PopupImage;
    [SerializeField]
    private Text PopupText;

    int[] count = {0,0,0};
    public double TimeRemaining = 10;
    private Boolean shown =false;
    // Start is called before the first frame update
    void Start()
    {
        // Set default time for the timer, this can be changed within the editor
        TimeRemaining = 10f;
    }

    // subcribe to updateOccured delegate if the script is enabled
    private void OnEnable()
    {
        UpdateHandler.UpdateOccurred += TimerCountDown;
    }

    // unsubcribe to updateOccurred delegate if the script is disabled
    private void OnDisable()
    {
        UpdateHandler.UpdateOccurred -= TimerCountDown;
    }

    // Update is called once per frame with update handler
    void TimerCountDown()
    {
        //TimeRemaining -= Time.deltaTime;
        //TimeRemaining = Math.Round(TimeRemaining, 2);
        //Timer.SetLabelText("Time: " + TimeRemaining);
    }

    // Temporary usage for update
    // Calculate timer coundown and update the display
    private void Update()
    {
        if(TimeRemaining >= 0)
        {
            TimeRemaining -= 2 * Time.deltaTime;
            double TempTime = Math.Round(TimeRemaining, 2);
            Timer.SetLabelText("Time: " + TempTime);
        }
        else if (!shown)
        {
            shown = true;
            Timer.SetLabelText("Time: 0");
            // Call handle result here, possibly pop-up tab with the item that the player get
            HandleResult();
        }

        
    }

    // Call onclick
    public void IncrementCount(int index)
    {
        count[index]++;
        Buttons[index].SetLabelText(count[index].ToString());
    }

    // Call on click into another item, the event are not currently working
    public void DecrementCount(int index)
    {
        count[index]--;
        Buttons[index].SetLabelText(count[index].ToString());
    }

    public void HidePopup()
    {
        Popup.Hide();
    }

    private void HandleResult()
    {
        Debug.Log("result");
        int maxVote = count.Max();
        int maxIndex = count.ToList().IndexOf(maxVote);

        Debug.Log("Item #" + (maxIndex + 1) + " was picked");
        PopupText.text = "Item #" + (maxIndex + 1) + " was picked";
        PopupImage.sprite = Images[maxIndex].GetComponent<Image>().sprite;
        // additional condition if any scrambler is dead
        if (maxIndex == 2)
        {
            HandleRevive();
        }
        Popup.Show();
        GameManager.ManagerInstance.ApplyPerk(Buttons[maxIndex].GetComponent<Perk>());
    }

    private void HandleRevive()
    {
        Debug.Log("All players strength are restored for the next level");
    }


}
