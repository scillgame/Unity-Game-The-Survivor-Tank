using System.Collections;
using System.Collections.Generic;
using SCILL.Model;
using UnityEngine;
using UnityEngine.UI;

public enum SCILLBattlePassLevelVisibility
{
    Visible,
    Hidden
}

public class SCILLBattlePassLevelToggleVisibility : MonoBehaviour
{
    public SCILLBattlePassLevelVisibility ifLocked;
    public SCILLBattlePassLevelVisibility ifUnlocked;

    [HideInInspector]
    public BattlePassLevel battlePassLevel;

    private Image _image;
    
    // Start is called before the first frame update
    void Start()
    {
        var battlePassLevelUI = GetComponentInParent<SCILLBattlePassLevel>();
        if (battlePassLevelUI)
        {
            battlePassLevel = battlePassLevelUI.battlePassLevel;
        }

        _image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_image || battlePassLevel == null)
        {
            return;
        }
        
        if (battlePassLevel.level_completed == false)
        {
            _image.enabled = (ifLocked == SCILLBattlePassLevelVisibility.Visible);
        }
        else
        {
            _image.enabled = (ifUnlocked == SCILLBattlePassLevelVisibility.Visible);
        }
    }
}
