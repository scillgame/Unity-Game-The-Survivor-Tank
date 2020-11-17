using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SCILL.Model;
using UnityEngine.UI;

public class SCILLCategoryItem : MonoBehaviour
{
    public GameObject challengePrefab;
    
    private ChallengeCategory _category;

    [HideInInspector]
    public ChallengeCategory Category
    {
        get => _category;
        set
        {
            _category = value;
            UpdateChallengeList();   
        }
    }
    
    public Text categoryName;
    
    // Start is called before the first frame update
    void Start()
    {
        
    }

    void UpdateChallengeList()
    {
        foreach (var challenge in _category.challenges)
        {
            var challengeGO = Instantiate(challengePrefab);
            var challengeItem = challengeGO.GetComponent<SCILLChallengeItem>();
            if (challengeItem)
            {
                challengeItem.challenge = challenge;
            }
            challengeGO.transform.SetParent(transform);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (_category == null)
        {
            return;
        } 
        
        categoryName.text = _category.category_name;
    }
}
