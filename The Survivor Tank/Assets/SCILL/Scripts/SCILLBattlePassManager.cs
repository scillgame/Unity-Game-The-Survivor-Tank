using System.Collections;
using System.Collections.Generic;
using SCILL.Model;
using UnityEngine;

public class SCILLBattlePassManager : MonoBehaviour
{
    [Tooltip("The Battle Pass UI to set and render the selected battle pass")]
    public SCILLBattlePass battlePassUI;

    private List<BattlePass> _battlePasses;
    
    // Start is called before the first frame update
    void Start()
    {
        if (!battlePassUI)
        {
            return;
        }
        
        _battlePasses = SCILLManager.Instance.SCILLClient.GetBattlePasses();
        Debug.Log("Loaded Battle Passes" + _battlePasses.Count);

        BattlePass selectedBattlePass = null;
        for (var i = 0; i < _battlePasses.Count; i++)
        {
            var battlePass = _battlePasses[i];
            if (battlePass.unlocked_at != null)
            {
                selectedBattlePass = battlePass;
                break;
            }
        }

        if (selectedBattlePass == null)
        {
            if (_battlePasses.Count > 0)
            {
                selectedBattlePass = _battlePasses[0];
            }
        }

        if (selectedBattlePass != null)
        {
            battlePassUI.battlePass = selectedBattlePass;
            battlePassUI.UpdateUI();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
