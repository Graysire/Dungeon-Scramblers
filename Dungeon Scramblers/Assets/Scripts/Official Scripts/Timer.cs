using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Timer : MonoBehaviour
{

    private float timeRemaining;        //Remaining time left
    private bool timerActive = false;   //Determines if timer is counting down
    public Text timeText;               //Text display to show time left to players
    bool isMatchTimer;                  //Determines if timer is for match or voting

    //link to reference : https://gamedevbeginner.com/how-to-make-countdown-timer-in-unity-minutes-seconds/

    //Update handler stuff
    protected virtual void OnEnable()
    {
        UpdateHandler.UpdateOccurred += RunTimerCountdown;
    }
    protected virtual void OnDisable()
    {
        UpdateHandler.UpdateOccurred -= RunTimerCountdown;
    }


    public void Awake()
    {
        //Makes this game object and text object disabled on awake
        gameObject.SetActive(false);
        timeText.gameObject.SetActive(false);
    }

    //Initializes the timer values and enables timer countdown
        //time - time to run till end
        //isMatchTimer - whether the timer is for a match countdown
            //if not then its for voting timer
    public void InitializeAndStartTimer(float time, bool isMatchTimer)
    {
        this.isMatchTimer = isMatchTimer;
        timeRemaining = time;

        DisplayTime(timeRemaining); //update the timer-on-screen info

        //Start timer by activating it and enable timer
        timeText.gameObject.SetActive(true);
        gameObject.SetActive(true);
        timerActive = true;
    }

    //Disables the timer from updating
        //matchTimeOver -- true if game over sequence wanted after
        //voteTimeOver -- true if round start sequence wanted after
        //NOTE: DO NOT HAVE BOTH PARAMETERS TRUE
    public void DisableTimer(bool matchTimeOver, bool voteTimeOver)
    {
        //sets time to 0 and disable timer
        timerActive = false;
        timeRemaining = 0;
        timeText.gameObject.SetActive(false);
        gameObject.SetActive(false);

        //if out of time then send ping to GameManager for game over
        if (matchTimeOver)
        { 
            //Send ping to GameManager that timer ran out so it can handle game over state
            GameManager.ManagerInstance.TimerOver();
        }
        //if out of time then send ping to game manager to start round
        if (voteTimeOver)
        {
            GameManager.ManagerInstance.BeginMatchTimer();
        }
    }


    //Will run by update handler
    private void RunTimerCountdown()
    {
        if (timerActive)
        {
            //Timer countdown
            if (timeRemaining > 0)
            {
                //update time remaining
                timeRemaining -= Time.deltaTime;
            }
            //Timer done counting down
            else
            {
                //If this is a match timer
                if (isMatchTimer)
                    DisableTimer(true, false);
                //if this is a vote/overlord setup timer
                else
                    DisableTimer(false, true);
            }

            //Display the timer information onto player screens
            DisplayTime(timeRemaining);
        }
    }

    //Updates the text information with current time left
    private void DisplayTime(float timeToDisplay)
    {
        //Get the minutes and seconds left to display onto text object
        //float minutes = Mathf.FloorToInt(timeToDisplay / 60);
        float seconds = Mathf.FloorToInt(timeToDisplay);

        //Set text object
        timeText.text = string.Format("{0:00}", seconds);
    }

}
