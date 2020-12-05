﻿using System;
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
    [Tooltip("A text UI element that is used to render the name of the battle pass")]
    public Text battlePassNameText;

    [Header("Optional connections")]
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

    public delegate void CurrentPageChangedAction(int currentPageIndex);
    public event CurrentPageChangedAction OnCurrentPageChanged;

    // Start is called before the first frame update
    void Start()
    {
        UpdateUI();
    }

    private void OnEnable()
    {
        SCILLBattlePassManager.OnBattlePassUpdatedFromServer += OnBattlePassUpdatedFromServer;
        SCILLBattlePassManager.OnBattlePassLevelsUpdatedFromServer += OnBattlePassLevelsUpdatedFromServer;
        UpdateUI();
    }
    
    private void OnDestroy()
    {
        SCILLBattlePassManager.OnBattlePassUpdatedFromServer -= OnBattlePassUpdatedFromServer;
        SCILLBattlePassManager.OnBattlePassLevelsUpdatedFromServer -= OnBattlePassLevelsUpdatedFromServer;
    }

    private void OnBattlePassUpdatedFromServer(BattlePass battlePass)
    {
        this.battlePass = battlePass;
        UpdateUI();
    }
    
    private void OnBattlePassLevelsUpdatedFromServer(List<BattlePassLevel> battlePassLevels)
    {
        _levels = battlePassLevels;
        
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
        
        UpdateNavigationButtons();
    }

    private void UpdateUI()
    {
        if (battlePass == null)
        {
            return;
        }

        if (battlePassNameText)
        {
            battlePassNameText.text = battlePass.battle_pass_name;
        }
        
        if (battlePass.image != null && image)
        {
            Sprite sprite = Resources.Load<Sprite>(battlePass.image);
            image.sprite = sprite;
        }

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

    public async void OnBattlePassUnlockButtonPressed()
    {
        var purchaseInfo = new BattlePassUnlockPayload(0, "EUR");
        var unlockInfo = await SCILLManager.Instance.SCILLClient.UnlockBattlePassAsync(battlePass.battle_pass_id, purchaseInfo);
        if (unlockInfo != null)
        {
            battlePass.unlocked_at = unlockInfo.purchased_at;
            SendMessageUpwards("UpdateBattlePassLevelsFromServer");
            UpdateUI();
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
        
        UpdateNavigationButtons();
        
        OnCurrentPageChanged?.Invoke(currentPageIndex);
    }

    public void OnPrevPage()
    {
        currentPageIndex -= 1;

        UpdateNavigationButtons();
        
        OnCurrentPageChanged?.Invoke(currentPageIndex);
    }
}
