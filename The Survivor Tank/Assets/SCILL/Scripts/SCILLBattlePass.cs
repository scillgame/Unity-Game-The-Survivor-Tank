using System;
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
    public GameObject prevButton;
    public GameObject nextButton;
    public Text pageText;

    public int itemsPerPage = 5;
    public int currentPageIndex = 0;

    private List<BattlePassLevel> _levels;
    private SCILLBattlePassLevel _selectedBattlePassLevel;

    private Dictionary<int, GameObject> _levelObjects = new Dictionary<int, GameObject>();
    
    // Start is called before the first frame update
    void Start()
    {
        if (battlePass.image != null && image)
        {
            Sprite sprite = Resources.Load<Sprite>(battlePass.image);
            image.sprite = sprite;
        }
        
        if (rewardPreview) rewardPreview.gameObject.SetActive(false);

        UpdateBattlePassLevels();
        UpdateBattlePass();
    }

    async void UpdateBattlePassLevels()
    {
        _levels = await SCILLManager.Instance.SCILLClient.GetBattlePassLevelsAsync(battlePass.battle_pass_id);
        UpdateBattlePassLevelUI();
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

    // Update is called once per frame
    void Update()
    {

    }
}
