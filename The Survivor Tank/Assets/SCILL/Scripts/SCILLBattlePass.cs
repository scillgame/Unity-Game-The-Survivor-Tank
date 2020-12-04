using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Management.Instrumentation;
using System.Runtime.CompilerServices;
using System.Security.Permissions;
using SCILL.Model;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class SCILLBattlePass : SCILLThreadSafety
{
    [HideInInspector]
    public BattlePass battlePass;

    [Header("Required connections")]
    [Tooltip("Connect a game object in your hierarchy that shows a UI with a button to unlock the Battle Pass. It will be hidden if the battle pass is already unlocked")]
    public GameObject unlockGroup;
    
    [Tooltip("Connect a button that will be used to trigger the previous page of the battle pass levels. It will be hidden if the first page is displayed")]
    public Button prevButton;
    [Tooltip("Connect a button that will be used to trigger the next page of the battle pass levels. It will be hidden if there are no more pages left")]
    public Button nextButton;
    [Tooltip("A text field which is used to set show the user the current navigation state, i.e. Page 1/10")]
    public Text pageText;
    [Tooltip("A text field that will be set with the current active level. Just a number like 2 or 99")]
    public Text currentLevel;
    [Tooltip("All Battle Pass Level prefabs will be instantiated into this transform. Make sure to apply some sort of automatic layout system")]
    public Transform battlePassLevels;
    
    [Header("Optional connections")]
    [Tooltip("Connect the active challenges UI which will be updated with the challenges of the current active level automatically")]
    public SCILLBattlePassLevelChallenges activeChallenges;
    [Tooltip("Connect a reward preview. It will be hidden if no level is selected and will be shown with the current levels reward if a level is selected")]
    public SCILLRewardPreview rewardPreview;
    [Tooltip("An image that will be set with the image set for the battle pass. It will be loaded as a Sprite with the name you set in Admin Panel. Make sure this Sprite is in a Resources folder - otherwise it will not be loaded at runtime")]
    public Image image;
    
    [Header("Prefabs")]
    [Tooltip("Choose one of the Battle Pass Level prefabs. This prefab will be instantiated for each level available in the battle pass and will be added to the battlePassLevels transform")]
    public GameObject levelPrefab;

    [Header("Settings")]
    [Tooltip("Number of battle pass levels shown per page.")]
    public int itemsPerPage = 5;
    [Tooltip("Indicate if you want to show the level number for each reward. This value will be set for each Battle Pass Level Prefab.")]
    public bool showLevelInfo = true;

    private List<BattlePassLevel> _levels;
    private SCILLBattlePassLevel _selectedBattlePassLevel;
    private int currentPageIndex = 0;
    private Dictionary<int, GameObject> _levelObjects = new Dictionary<int, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        UpdateUI();
    }

    private void Awake()
    {
        // Make sure we delete all items from the battle pass levels container
        // This way we can leave some dummy level items in Unity Editor which makes it easier to design UI
        if (battlePassLevels)
        {
            foreach (SCILLBattlePassLevel child in GetComponentsInChildren<SCILLBattlePassLevel>()) {
                Destroy(child.gameObject);
            }            
        }
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
            if (prevButton) prevButton.gameObject.SetActive(false);
        }
        else
        {
            if (prevButton) prevButton.gameObject.SetActive(true);
        }
        
        Debug.Log(_levels.Count + ":" + itemsPerPage);

        if (currentPageIndex >= Decimal.Ceiling((decimal) _levels.Count / (decimal) itemsPerPage) - 1)
        {
            if (nextButton) nextButton.gameObject.SetActive(false);
        }
        else
        {
            if (nextButton) nextButton.gameObject.SetActive(true);
        }

        if (pageText) pageText.text = "Page " + (currentPageIndex + 1) + "/" + Decimal.Ceiling((decimal) _levels.Count / (decimal) itemsPerPage);
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
