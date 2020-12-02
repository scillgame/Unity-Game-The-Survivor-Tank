using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Security.Permissions;
using SCILL.Model;
using UnityEngine;
using UnityEngine.UI;

public class SCILLBattlePass : SCILLThreadSafety
{
    [HideInInspector]
    public BattlePass battlePass;

    public Image image;
    
    public bool showLevelInfo = true;
    public GameObject levelPrefab;
    public Transform battlePassLevels;
    public SCILLRewardPreview rewardPreview;
    public GameObject unlockGroup;
    public GameObject prevButton;
    public GameObject nextButton;
    public Text pageText;
    public Text currentLevel;
    public SCILLBattlePassLevelChallenges activeChallenges;

    public int itemsPerPage = 5;
    public int currentPageIndex = 0;

    private List<BattlePassLevel> _levels;
    private SCILLBattlePassLevel _selectedBattlePassLevel;

    private Dictionary<int, GameObject> _levelObjects = new Dictionary<int, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (battlePass == null)
        {
            return;
        }
        
        if (battlePass.image != null && image)
        {
            Sprite sprite = Resources.Load<Sprite>(battlePass.image);
            image.sprite = sprite;
        }
        
        if (rewardPreview) rewardPreview.gameObject.SetActive(false);

        UpdateBattlePassLevels();
        UpdateBattlePass();
        
        // Get notifications if battle pass changes
        SCILLManager.Instance.SCILLClient.StartBattlePassUpdateNotifications(this.battlePass.battle_pass_id, OnBattlePassChangedNotification);
    }

    private void OnBattlePassChangedNotification(BattlePassChallengeChangedPayload payload)
    {
        // Make sure we run this code on Unitys "main thread", i.e. in the Update function
        RunOnMainThread.Enqueue(() =>
        {
            Debug.Log("Received Battle Pass Update");
            Debug.Log(payload);

            if (payload.webhook_type == "battlepass-challenge-changed")
            {
                if (payload.new_battle_pass_challenge.type == "in-progress")
                {
                    // This challenge is still in-progress, so we just update the challenges counter.
                    var levelIndex = (int)payload.new_battle_pass_challenge.level_position_index;
                    GameObject levelGO = null;
                    if (_levelObjects.TryGetValue(levelIndex, out levelGO))
                    {
                        var levelItem = levelGO.GetComponent<SCILLBattlePassLevel>();
                        if (levelItem)
                        {
                            var challengeIndex = (int) payload.new_battle_pass_challenge.challenge_position_index;
                            if (challengeIndex < levelItem.battlePassLevel.challenges.Count)
                            {
                                var challenge = levelItem.battlePassLevel.challenges[challengeIndex];
                                challenge.user_challenge_current_score =
                                    payload.new_battle_pass_challenge.user_challenge_current_score;
                                levelItem.UpdateUI();
                            }
                            
                            // Update the challenges UI, too
                            if (activeChallenges)
                            {
                                activeChallenges.UpdateUI();
                            }
                        }
                        else
                        {
                            UpdateBattlePassLevels();
                        }
                    }
                    else
                    {
                        // Something is fishy, reload levels
                        UpdateBattlePassLevels();
                    }
                }
                else
                {
                    // The type of the challenge changed, i.e. it's possible that level state changed, reload the levels
                    UpdateBattlePassLevels();
                }
            }            
        });
    }

    private void OnDestroy()
    {
        if (battlePass != null)
        {
            SCILLManager.Instance.SCILLClient.StopBattlePassUpdateNotifications(this.battlePass.battle_pass_id, OnBattlePassChangedNotification);   
        }
    }

    async void UpdateBattlePassLevels()
    {
        if (battlePass != null)
        {
            _levels = await SCILLManager.Instance.SCILLClient.GetBattlePassLevelsAsync(battlePass.battle_pass_id);
            UpdateBattlePassLevelUI();
        }
    }

    void UpdateBattlePassLevelUI()
    {
        for (int i = 0; i < itemsPerPage; i++)
        {
            var levelIndex = (currentPageIndex * itemsPerPage) + i;
            Debug.Log(levelIndex);
            GameObject levelGO = null;
            if (_levelObjects.TryGetValue(i, out levelGO))
            {
                if (levelIndex >= _levels.Count)
                {
                    levelGO.SetActive(false);
                }
                else
                {
                    var levelItem = levelGO.GetComponent<SCILLBattlePassLevel>();
                    if (levelItem)
                    {
                        levelItem.battlePassLevel = _levels[levelIndex];
                        levelItem.showLevelInfo = showLevelInfo;
                    }
                    levelGO.SetActive(true);
                }
            }
            else
            {
                levelGO = Instantiate(levelPrefab, battlePassLevels, false);
                var levelItem = levelGO.GetComponent<SCILLBattlePassLevel>();
                if (levelItem)
                {
                    levelItem.battlePassLevel = _levels[levelIndex];
                    levelItem.showLevelInfo = levelItem;
                    levelItem.button.onClick.AddListener(delegate{OnBattlePassLevelClicked(levelItem);});
                }
                _levelObjects.Add(i, levelGO);
            }
        }

        // Find the current level and set the text
        if (currentLevel)
        {
            int currentLevelIndex = 0;
            for (int i = 0; i < _levels.Count; i++)
            {
                if (_levels[i].level_completed == true)
                {
                    currentLevelIndex = i;
                }
                else
                {
                    break;
                }
            }

            currentLevel.text = (currentLevelIndex+1).ToString();
        }
        
        if (activeChallenges)
        {
            int currentLevelIndex = 0;
            for (int i = 0; i < _levels.Count; i++)
            {
                if (_levels[i].level_completed == false)
                {
                    currentLevelIndex = i;
                    break;
                }
            }
            
            activeChallenges.battlePassLevel = _levels[currentLevelIndex];
            activeChallenges.UpdateChallengeList();
        }

        UpdateNavigationButtons();
    }
    
    void OnBattlePassLevelClicked(SCILLBattlePassLevel level)
    {
        if (_selectedBattlePassLevel)
        {
            _selectedBattlePassLevel.Deselect();
        }

        _selectedBattlePassLevel = level;
        _selectedBattlePassLevel.Select();
        
        var rewardAmount = level.battlePassLevel.reward_amount;
        if (!string.IsNullOrEmpty(rewardAmount))
        {
            rewardPreview.SelectedBattlePassLevel = level.battlePassLevel;
            rewardPreview.gameObject.SetActive(true);
        }
        else
        {
            rewardPreview.gameObject.SetActive(false);
        }
    }

    public async void OnBattlePassUnlockButtonPressed()
    {
        var purchaseInfo = new BattlePassUnlockPayload(0, "EUR");
        var unlockInfo = await SCILLManager.Instance.SCILLClient.UnlockBattlePassAsync(battlePass.battle_pass_id, purchaseInfo);
        if (unlockInfo != null)
        {
            battlePass.unlocked_at = unlockInfo.purchased_at;
            UpdateBattlePassLevels();
            UpdateBattlePass();
        }
    }

    void UpdateBattlePass()
    {
        if (battlePass.unlocked_at != null)
        {
            // This battle pass is unlocked
            unlockGroup.SetActive(false);
        }
        else
        {
            unlockGroup.SetActive(true);
        }
    }
    
    public void UpdateNavigationButtons()
    {
        if (_levels.Count <= 0)
        {
            return;
        }
        
        if (currentPageIndex <= 0)
        {
            prevButton.SetActive(false);
        }
        else
        {
            prevButton.SetActive(true);
        }
        
        Debug.Log(_levels.Count + ":" + itemsPerPage);

        if (currentPageIndex >= Decimal.Ceiling((decimal) _levels.Count / (decimal) itemsPerPage) - 1)
        {
            nextButton.SetActive(false);
        }
        else
        {
            nextButton.SetActive(true);
        }

        pageText.text = "Page " + (currentPageIndex + 1) + "/" + Decimal.Ceiling((decimal) _levels.Count / (decimal) itemsPerPage);
    }

    public void OnNextPage()
    {
        currentPageIndex += 1;
        UpdateBattlePassLevelUI();
    }

    public void OnPrevPage()
    {
        currentPageIndex -= 1;
        UpdateBattlePassLevelUI();
    }

    public void OnClaimRewardItem()
    {
        
    }
}
