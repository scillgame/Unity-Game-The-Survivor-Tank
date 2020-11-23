using System.Collections;
using System.Collections.Generic;
using SCILL.Model;
using UnityEngine;

public class SCILLBattlePasses : MonoBehaviour
{
    private List<BattlePass> _battlePasses;

    public GameObject battlePassPrefab;
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateBattlePasses();
    }

    void UpdateBattlePasses()
    {
        _battlePasses = SCILLManager.Instance.SCILLClient.GetBattlePasses();
        Debug.Log("Loaded Battle Passes" + _battlePasses.Count);
        
        foreach (var battlePass in _battlePasses)
        {
            var battlePassGO = Instantiate(battlePassPrefab);
            var battlePassScript = battlePassGO.GetComponent<SCILLBattlePass>();
            if (battlePassScript)
            {
                battlePassScript.battlePass = battlePass;
                battlePassScript.showLevelInfo = false;
            }
            battlePassGO.transform.SetParent(this.transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
