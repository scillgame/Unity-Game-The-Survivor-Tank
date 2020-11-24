using System;
using System.Collections;
using System.Collections.Generic;
using SCILL.Model;
using UnityEngine;
using UnityEngine.UI;

public class SCILLBattlePassLevel : MonoBehaviour
{
    [HideInInspector]
    private BattlePassLevel _battlePassLevel;

    public BattlePassLevel battlePassLevel
    {
        get => _battlePassLevel;
        set
        {
            _battlePassLevel = value;
            UpdateUI();
        }
    }

    public GameObject battlePassLevelInfo;
    public Text levelName;
    public Image rewardImage;
    public Button button;
    public GameObject reward;
    public GameObject[] locked;
    public GameObject[] claimed;

    public bool showLevelInfo = true;

    private SCILLReward _reward;
    

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    // Start is called before the first frame update
    void Start()
    {
        UpdateUI();
    }

    void UpdateUI()
    {
        _reward = Resources.Load<SCILLReward>(battlePassLevel.reward_amount);
        if (_reward)
        {
            rewardImage.sprite = _reward.image;
            if (reward) reward.SetActive(true);
        }
        else
        {
            if (reward) reward.SetActive(false);
        }

        levelName.text = battlePassLevel.level_priority.ToString();
        battlePassLevelInfo.SetActive(showLevelInfo);

        // Show all game objects representing locked state
        foreach (var go in locked)
        {
            go.SetActive(battlePassLevel.level_completed == false);
        }

        // Show all game objects representing locked state
        foreach (var go in claimed)
        {
            go.SetActive(battlePassLevel.reward_claimed == true);
        }
    }

    public void Select()
    {
        
    }

    public void Deselect()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
