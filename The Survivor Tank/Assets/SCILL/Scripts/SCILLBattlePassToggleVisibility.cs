using System.Collections;
using System.Collections.Generic;
using SCILL.Model;
using UnityEngine;
using UnityEngine.UI;

public enum SCILLBattlePassVisibility
{
    Visible,
    Hidden,
    DoNothing
}

public class SCILLBattlePassToggleVisibility : MonoBehaviour
{
    public SCILLBattlePassVisibility ifLocked;
    public SCILLBattlePassVisibility ifUnlocked;
    
    [HideInInspector]
    public BattlePass battlePass;
    
    private Image _image;
    
    // Start is called before the first frame update
    void Start()
    {
        var battlePassUI = GetComponentInParent<SCILLBattlePass>();
        if (battlePassUI)
        {
            battlePass = battlePassUI.battlePass;
        }

        _image = GetComponent<Image>();
    }

    // Update is called once per frame
    void Update()
    {
        if (!_image || battlePass == null)
        {
            return;
        }

        if (ifLocked != SCILLBattlePassVisibility.DoNothing)
        {
            if (battlePass.unlocked_at == null)
            {
                _image.enabled = (ifLocked == SCILLBattlePassVisibility.Visible);
            }
        }
        
        if (ifUnlocked != SCILLBattlePassVisibility.DoNothing)
        {
            if (battlePass.unlocked_at != null)
            {
                _image.enabled = (ifUnlocked == SCILLBattlePassVisibility.Visible);
            }
        }
    }
}
