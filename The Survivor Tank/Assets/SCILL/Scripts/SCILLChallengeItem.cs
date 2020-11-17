using System;
using System.Collections;
using System.Collections.Generic;
using SCILL.Model;
using UnityEngine;
using UnityEngine.UI;

public class SCILLChallengeItem : MonoBehaviour
{
    public Text challengeName;
    public Image challengeImage;
    public Slider challengeProgressSlider;
    public Text challengeGoal;
    public RectTransform challengeProgress;
    public Text timeRemaining;

    public Challenge challenge;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }
    
    public string StrikeThrough(string s)
    {
        string strikethrough = "";
        foreach (char c in s)
        {
            strikethrough = strikethrough + c + '\u0336';
        }
        return strikethrough;
    }

    // Update is called once per frame
    void Update()
    {
        challengeName.text = challenge.challenge_name;
        if (challenge.challenge_goal > 0)
        {
            challengeProgressSlider.value = (float) ((float)challenge.user_challenge_current_score / (float)challenge.challenge_goal);    
        }
        challengeGoal.text = challenge.user_challenge_current_score.ToString() + "/" +
                             challenge.challenge_goal.ToString();

        if (challenge.type == "in-progress")
        {
            challengeProgress.gameObject.SetActive(true);

            var timeText = "";
            var date = DateTime.Parse(challenge.user_challenge_activated_at);
            date = date.AddMinutes((double)challenge.challenge_duration_time);

            var now = DateTime.Now;
            var diff = date.Subtract(now);

            if (diff.Days > 0)
            {
                timeText = "+24 hours";
            }
            else
            {
                timeText = String.Format("{0:00}:{1:00}:{2:00}", diff.Hours, diff.Minutes, diff.Seconds);
            }

            timeRemaining.text = timeText;
        }
        else if (challenge.type == "unclaimed")
        {
            challengeName.text = StrikeThrough(challenge.challenge_name);
            challengeProgress.gameObject.SetActive(false);
        }
        else
        {
            challengeProgress.gameObject.SetActive(false);
        }
    }
}
