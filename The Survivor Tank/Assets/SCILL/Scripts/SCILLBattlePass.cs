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
    public SCILLRewardPreview rewardPreview;

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
                levelGO = Instantiate(levelPrefab);
                var levelItem = levelGO.GetComponent<SCILLBattlePassLevel>();
                if (levelItem)
                {
                    levelItem.battlePassLevel = _levels[i];
                    levelItem.showLevelInfo = levelItem;
                    levelItem.button.onClick.AddListener(delegate{OnBattlePassLevelClicked(levelItem);});
                }
                levelGO.transform.SetParent(this.transform);
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
    
    // Update is called once per frame
    void Update()
    {
        
    }
}
