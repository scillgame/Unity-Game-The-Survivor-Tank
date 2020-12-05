using System;
using System.Collections;
using System.Collections.Generic;
using SCILL.Model;
using UnityEngine;

public class SCILLBattlePassManager : SCILLThreadSafety
{
    [Tooltip("The Battle Pass UI to set and render the selected battle pass")]
    public SCILLBattlePass battlePassUI;
    public SCILLBattlePassLevelChallenges activeBattlePassLevelChallenges;

    private List<BattlePass> _battlePasses;
    private BattlePass _selectedBattlePass;

    public delegate void BattlePassUpdatedFromServerAction(BattlePass battlePass);
    public static event BattlePassUpdatedFromServerAction OnBattlePassUpdatedFromServer;
    
    public delegate void BattlePassLevelsUpdatedFromServerAction(List<BattlePassLevel> battlePassLevels);
    public static event BattlePassLevelsUpdatedFromServerAction OnBattlePassLevelsUpdatedFromServer;

    public delegate void BattlePassChallengeUpdateAction(BattlePassChallengeChangedPayload challengeChangedPayload);
    public static event BattlePassChallengeUpdateAction OnBattlePassChallengeUpdate;
    
    // Start is called before the first frame update
    void Start()
    {
        if (!battlePassUI)
        {
            return;
        }
        
        _battlePasses = SCILLManager.Instance.SCILLClient.GetBattlePasses();
        Debug.Log("Loaded Battle Passes" + _battlePasses.Count);

        BattlePass selectedBattlePass = null;
        for (var i = 0; i < _battlePasses.Count; i++)
        {
            var battlePass = _battlePasses[i];
            if (battlePass.unlocked_at != null)
            {
                selectedBattlePass = battlePass;
                break;
            }
        }

        if (selectedBattlePass == null)
        {
            if (_battlePasses.Count > 0)
            {
                selectedBattlePass = _battlePasses[0];
            }
        }

        if (selectedBattlePass != null)
        {
            _selectedBattlePass = selectedBattlePass;

            Debug.Log("HUHU");

            // Inform delegates that a new battle pass has been selected
            OnBattlePassUpdatedFromServer?.Invoke(selectedBattlePass);

            // Get notifications from SCILL backend whenever battle pass changes
            SCILLManager.Instance.SCILLClient.StartBattlePassUpdateNotifications(selectedBattlePass.battle_pass_id, OnBattlePassChangedNotification);
            
            // Load battle pass levels from SCILL backend
            UpdateBattlePassLevelsFromServer();
        }
    }

    public async void UpdateBattlePassLevelsFromServer()
    {
        var levels = await SCILLManager.Instance.SCILLClient.GetBattlePassLevelsAsync(_selectedBattlePass.battle_pass_id);
        OnBattlePassLevelsUpdatedFromServer?.Invoke(levels);
    }
    
    private void OnBattlePassChangedNotification(BattlePassChallengeChangedPayload payload)
    {
        // Make sure we run this code on Unitys "main thread", i.e. in the Update function
        RunOnMainThread.Enqueue(() =>
        {
            Debug.Log("Received Battle Pass Update");
            Debug.Log(payload);
            
            // The battle pass challenge changed
            if (payload.webhook_type == "battlepass-challenge-changed")
            {
                // Check if the challenge is still in-progress. If not, we need to reload the levels to update
                // current state - as change is not isolated to one challenge
                if (payload.new_battle_pass_challenge.type == "in-progress")
                {
                    // Inform all delegates of the challenge update
                    OnBattlePassChallengeUpdate?.Invoke(payload);
                }
                else
                {
                    // Reload the levels from the server and update UI
                    UpdateBattlePassLevelsFromServer();
                }
            }
            else
            {
                // Reload the levels from the server and update UI
                UpdateBattlePassLevelsFromServer();
            }        
        });
    }

    private void OnDestroy()
    {
        SCILLManager.Instance.SCILLClient.StopBattlePassUpdateNotifications(_selectedBattlePass.battle_pass_id, OnBattlePassChangedNotification);   
    }
}
