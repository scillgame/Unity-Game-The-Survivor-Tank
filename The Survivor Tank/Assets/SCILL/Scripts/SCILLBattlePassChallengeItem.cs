﻿using System;
using System.Collections;
using System.Collections.Generic;
using SCILL.Model;
using UnityEngine;
using UnityEngine.UI;

public class SCILLBattlePassChallengeItem : MonoBehaviour
{
    [Tooltip("Text UI that is used to render the challenge name")]
    public Text challengeName;
    [Tooltip("Progress slider that is used to show the progress of the challenge")]
    public Slider challengeProgressSlider;
    [Tooltip("Text used to render the challenge goal as 34/500")]
    public Text challengeGoal;

    public BattlePassLevelChallenge challenge;
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateUI();
    }
    
    private void OnEnable()
    {
        SCILLBattlePassManager.OnBattlePassChallengeUpdate += OnBattlePassChallengeUpdate;
    }

    private void OnDestroy()
    {
        SCILLBattlePassManager.OnBattlePassChallengeUpdate -= OnBattlePassChallengeUpdate;
    }

    private void OnBattlePassChallengeUpdate(BattlePassChallengeChangedPayload challengeChangedPayload)
    {
        // Update local challenge object if it has the same id
        if (challengeChangedPayload.new_battle_pass_challenge.challenge_id == challenge.challenge_id)
        {
            challenge.type = challengeChangedPayload.new_battle_pass_challenge.type;
            challenge.user_challenge_current_score =
                challengeChangedPayload.new_battle_pass_challenge.user_challenge_current_score;
            UpdateUI();   
        }
    }

    public void UpdateUI()
    {
        if (challenge == null)
        {
            return;
        }
        
        if (challengeName) challengeName.text = challenge.challenge_name;
        if (challenge.challenge_goal > 0)
        {
            if (challengeProgressSlider)
            {
                challengeProgressSlider.value = (float) ((float)challenge.user_challenge_current_score / (float)challenge.challenge_goal);
            }    
        }
        if (challengeGoal) {
            challengeGoal.text = challenge.user_challenge_current_score.ToString() + "/" +
                                 challenge.challenge_goal.ToString();
        }
    }
}
