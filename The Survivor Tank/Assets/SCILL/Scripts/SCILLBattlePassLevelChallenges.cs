using System;
using System.Collections;
using System.Collections.Generic;
using SCILL.Model;
using UnityEngine;

public class SCILLBattlePassLevelChallenges : MonoBehaviour
{
    public GameObject challengePrefab;
    public BattlePassLevel battlePassLevel;

    private void Awake()
    {
        ClearChallenges();     
    }
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateChallengeList();
    }

    private void OnEnable()
    {
        SCILLBattlePassManager.OnBattlePassLevelsUpdatedFromServer += OnBattlePassLevelsUpdatedFromServer;
    }

    private void OnDestroy()
    {
        SCILLBattlePassManager.OnBattlePassLevelsUpdatedFromServer -= OnBattlePassLevelsUpdatedFromServer;
    }

    private void OnBattlePassLevelsUpdatedFromServer(List<BattlePassLevel> battlePassLevels)
    {
        // Find current level and update the challenges list
        int currentLevelIndex = 0;
        for (int i = 0; i < battlePassLevels.Count; i++)
        {
            if (battlePassLevels[i].level_completed == false)
            {
                currentLevelIndex = i;
                break;
            }
        }
            
        battlePassLevel = battlePassLevels[currentLevelIndex];
        UpdateChallengeList();
    }

    void ClearChallenges()
    {
        // Make sure we delete all items from the battle pass levels container
        // This way we can leave some dummy level items in Unity Editor which makes it easier to design UI
        foreach (SCILLBattlePassChallengeItem child in GetComponentsInChildren<SCILLBattlePassChallengeItem>()) {
            Destroy(child.gameObject);
        }        
    }

    public void UpdateChallengeList()
    {
        if (battlePassLevel == null)
        {
            return;
        }
        
        // Make sure we remove old challenges from the list
        ClearChallenges();
        
        Debug.Log("UPDATE CHALLENGE LIST");
        foreach (var challenge in battlePassLevel.challenges)
        {
            // Only add active challenges to the list
            if (challenge.type == "in-progress")
            {
                var challengeGO = Instantiate(challengePrefab, transform);
                var challengeItem = challengeGO.GetComponent<SCILLBattlePassChallengeItem>();
                if (challengeItem)
                {
                    challengeItem.challenge = challenge;
                    challengeItem.UpdateUI();
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
