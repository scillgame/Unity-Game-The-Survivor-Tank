using System.Collections;
using System.Collections.Generic;
using SCILL.Model;
using UnityEngine;
using UnityEngine.UI;

public class SCILLBattlePass : MonoBehaviour
{
    [HideInInspector]
    public BattlePass battlePass;

    public Image image;
    
    public bool showLevelInfo = true;
    public GameObject levelPrefab;
    public Transform battlePassLevels;
    public SCILLRewardPreview rewardPreview;
    public GameObject unlockGroup;

    private List<BattlePassLevel> _levels;
    
    private Dictionary<int, GameObject> _levelObjects = new Dictionary<int, GameObject>();
    
    // Start is called before the first frame update
    void Start()
    {
        if (battlePass.image != null)
        {
            Sprite sprite = Resources.Load<Sprite>(battlePass.image);
            image.sprite = sprite;
        }
        
        UpdateBattlePassLevels();
    }

    async void UpdateBattlePassLevels()
    {
        _levels = await SCILLManager.Instance.SCILLClient.GetBattlePassLevelsAsync(battlePass.battle_pass_id);

        for (int i = 0; i < _levels.Count; i++)
        {
            GameObject levelGO = null;
            if (_levelObjects.TryGetValue(i, out levelGO))
            {
                var levelItem = levelGO.GetComponent<SCILLBattlePassLevel>();
                if (levelItem)
                {
                    levelItem.battlePassLevel = _levels[i];
                    levelItem.showLevelInfo = showLevelInfo;
                }
            }
            else
            {
                levelGO = Instantiate(levelPrefab, battlePassLevels, false);
                var levelItem = levelGO.GetComponent<SCILLBattlePassLevel>();
                if (levelItem)
                {
                    levelItem.battlePassLevel = _levels[i];
                    levelItem.showLevelInfo = levelItem;
                    levelItem.button.onClick.AddListener(delegate{OnBattlePassLevelClicked(levelItem);});
                }
                _levelObjects.Add(i, levelGO);
            }
        }
    }
    
    void OnBattlePassLevelClicked(SCILLBattlePassLevel level)
    {
        var rewardAmount = level.battlePassLevel.reward_amount;
        SCILLReward reward = Resources.Load<SCILLReward>(rewardAmount);
        if (reward)
        {
            rewardPreview.reward = reward;
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
            UpdateBattlePassLevels();
        }
    }
    
    // Update is called once per frame
    void Update()
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
}
