using System;
using System.Collections;
using System.Collections.Generic;
using SCILL.Model;
using UnityEngine;
using UnityEngine.UI;

public class SCILLBattlePassLevel : MonoBehaviour
{
    [Header("Optional connections")]
    [Tooltip("Set a game object that will be hidden if showLevelInfo of the SCILLBattlePass script will be false. It typical indicates the level number")]
    public GameObject battlePassLevelInfo;
    [Tooltip("A textfield that will be used to render the level number")]
    public Text levelName;
    [Tooltip("An image field that will be used to set the image of the reward that is set in the Admin Panel")]
    public Image rewardImage;
    
    [Tooltip("A game object that will be hidden if not reward is set in the Admin Panel for this level. Otherwise it will be shown.")]
    public GameObject reward;
    [Tooltip("An array of objects that will be shown if the level is locked and hidden if it is not locked. It's used to render an overlay above the level to dim it.")]
    public GameObject[] locked;
    [Tooltip("An array of objects that will be shown if the level is claimed and hidden if it is not claimed. It's used to render a checkbox.")]
    public GameObject[] claimed;
    [Tooltip("A slider that will be used to render the current progress in this level")]
    public Slider progressSlider;
    [Tooltip("A game object that will be shown if this is the current level and hidden if not")]
    public GameObject progress;

    [HideInInspector]
    public bool showLevelInfo = true;
    [HideInInspector]
    public Button button;
    
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
    
    private void OnEnable()
    {
        SCILLBattlePassManager.OnBattlePassChallengeUpdate += OnBattlePassChallengeUpdate;
        UpdateUI();
    }

    private void OnDestroy()
    {
        SCILLBattlePassManager.OnBattlePassChallengeUpdate -= OnBattlePassChallengeUpdate;
    }

    private void OnBattlePassChallengeUpdate(BattlePassChallengeChangedPayload challengeChangedPayload)
    {
        UpdateUI();
    }

    public void UpdateUI()
    {
        if (battlePassLevel == null)
        {
            return;
        }
        
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
        
        // Update slider
        if (progressSlider)
        {
            float totalProgress = 0;
            foreach (var challenge in battlePassLevel.challenges)
            {
                float challengeProgress = 0;
                if (challenge.challenge_goal > 0)
                {
                    challengeProgress = (float) challenge.user_challenge_current_score /
                                        (float) challenge.challenge_goal;
                }

                totalProgress += challengeProgress * (1.0f / (float)battlePassLevel.challenges.Count);
            }
            
            if (totalProgress <= 0)
            {
                progressSlider.gameObject.SetActive(false);
            }
            else
            {
                progressSlider.value = totalProgress;
                progressSlider.gameObject.SetActive(true);
            }
        }

        if (progress)
        {
            if (battlePassLevel.activated_at != null && battlePassLevel.level_completed == false)
            {
                // Current level
                progress.SetActive(true);
            }
            else
            {
                progress.SetActive(false);
            }
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
        if (battlePassLevel == null)
        {
            return;
        }
        
        // Show all game objects representing locked state
        foreach (var go in locked)
        {
            go.SetActive(battlePassLevel.activated_at == null);
        }

        // Show all game objects representing locked state
        foreach (var go in claimed)
        {
            go.SetActive(battlePassLevel.reward_claimed == true);
        }
    }
}
