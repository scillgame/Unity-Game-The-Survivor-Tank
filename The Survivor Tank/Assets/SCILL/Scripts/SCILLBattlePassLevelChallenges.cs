using System.Collections;
using System.Collections.Generic;
using SCILL.Model;
using UnityEngine;

public class SCILLBattlePassLevelChallenges : MonoBehaviour
{
    public GameObject challengePrefab;
    public BattlePassLevel battlePassLevel;
    
    private Dictionary<string, GameObject> _challengeObjects = new Dictionary<string, GameObject>();
    
    private void Awake()
    {
        // Make sure we delete all items from the battle pass levels container
        // This way we can leave some dummy level items in Unity Editor which makes it easier to design UI
        foreach (SCILLBattlePassChallengeItem child in GetComponentsInChildren<SCILLBattlePassChallengeItem>()) {
            Destroy(child.gameObject);
        }            
    }
    
    // Start is called before the first frame update
    void Start()
    {
        UpdateChallengeList();
    }

    public void UpdateChallengeList()
    {
        if (battlePassLevel == null)
        {
            return;
        }
        
        Debug.Log("UPDATE CHALLENGE LIST");
        foreach (var challenge in battlePassLevel.challenges)
        {
            GameObject challengeGO = null;
            if (_challengeObjects.TryGetValue(challenge.challenge_id, out challengeGO))
            {
                if (challenge.type != "in-progress")
                {
                    // This challenge has been completed. Remove from the list
                    Destroy(challengeGO);
                }
                else
                {
                    var challengeItem = challengeGO.GetComponent<SCILLBattlePassChallengeItem>();
                    if (challengeItem)
                    {
                        challengeItem.challenge = challenge;
                        challengeItem.UpdateUI();
                    }                    
                }
            }
            else
            {
                // Only add active challenges to the list
                if (challenge.type == "in-progress")
                {
                    challengeGO = Instantiate(challengePrefab, transform);
                    var challengeItem = challengeGO.GetComponent<SCILLBattlePassChallengeItem>();
                    if (challengeItem)
                    {
                        challengeItem.challenge = challenge;
                        challengeItem.UpdateUI();
                    }

                    _challengeObjects.Add(challenge.challenge_id, challengeGO);                    
                }
            }
        }
    }

    public void UpdateUI()
    {
        foreach (var challengeGO in _challengeObjects.Values)
        {
            var challengeItem = challengeGO.GetComponent<SCILLBattlePassChallengeItem>();
            challengeItem.UpdateUI();
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
