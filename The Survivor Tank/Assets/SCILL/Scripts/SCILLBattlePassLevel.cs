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

    public bool showLevelInfo = true;
    
    // Start is called before the first frame update
    void Start()
    {
        // Load a sprite with name challenge_icon from resources folder
        Sprite sprite = Resources.Load<Sprite>(battlePassLevel.reward_amount);
        rewardImage.sprite = sprite;

        levelName.text = battlePassLevel.level_priority.ToString();
        
        battlePassLevelInfo.SetActive(showLevelInfo);
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
