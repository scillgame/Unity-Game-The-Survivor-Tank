using System;
using System.Collections;
using System.Collections.Generic;
using SCILL.Model;
using UnityEngine;
using UnityEngine.UI;

public class SCILLBattlePassLevel : MonoBehaviour
{
    [HideInInspector]
    public BattlePassLevel battlePassLevel;

    public GameObject battlePassLevelInfo;
    public Text levelName;
    public Image rewardImage;
    public Button button;
    public GameObject reward;

    public bool showLevelInfo = true;

    private SCILLReward _reward;
    

    private void Awake()
    {
        button = GetComponent<Button>();
    }

    // Start is called before the first frame update
    void Start()
    {
        _reward = Resources.Load<SCILLReward>(battlePassLevel.reward_amount);
        if (_reward)
        {
            rewardImage.sprite = _reward.image;
            reward.SetActive(true);
        }
        else
        {
            reward.SetActive(false);
        }

        levelName.text = battlePassLevel.level_priority.ToString();
        battlePassLevelInfo.SetActive(showLevelInfo);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
