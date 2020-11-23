using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using SCILL.Model;
using UnityEngine.UI;

public class SCILLCategoryItem : MonoBehaviour
{
    public GameObject challengePrefab;
    
    private Dictionary<string, GameObject> _challengeObjects = new Dictionary<string, GameObject>();
    private ChallengeCategory _category;

    private void Awake()
    {

    }

    [HideInInspector]
    public ChallengeCategory Category
    {
        get => _category;
        set
        {
            _category = value;
        }
    }

    public bool expanded = true;
    public Text categoryName;
    public Transform challengesContainer;
    
    // Start is called before the first frame update
    void Start()
    {
        if (challengePrefab == null)
        {
            PersonalChallenges personalChallenges = GetComponentInParent<PersonalChallenges>();
            if (personalChallenges)
            {
                challengePrefab = personalChallenges.challengePrefab;
            }            
        }
        
        UpdateChallengeList();           
    }

    public void OnToggleExpanded()
    {
        expanded = !expanded;

        var challengeItems = GetComponentsInChildren<SCILLChallengeItem>(true);
        foreach (var challengeItem in challengeItems)
        {
            challengeItem.gameObject.SetActive(expanded);
        }
    }

    public void UpdateChallenge(Challenge challenge)
    {
        GameObject challengeGO = null;
        if (_challengeObjects.TryGetValue(challenge.challenge_id, out challengeGO))
        {
            var challengeItem = challengeGO.GetComponent<SCILLChallengeItem>();
            if (challengeItem)
            {
                challengeItem.challenge = challenge;
            }
        }
    }

    public void UpdateChallengeList()
    {
        
        Debug.Log("UPDATE CHALLENGE LIST");
        foreach (var challenge in _category.challenges)
        {
            GameObject challengeGO = null;
            if (_challengeObjects.TryGetValue(challenge.challenge_id, out challengeGO))
            {
                var challengeItem = challengeGO.GetComponent<SCILLChallengeItem>();
                if (challengeItem)
                {
                    challengeItem.challenge = challenge;
                }
            }
            else
            {
                challengeGO = Instantiate(challengePrefab);
                var challengeItem = challengeGO.GetComponent<SCILLChallengeItem>();
                if (challengeItem)
                {
                    challengeItem.challenge = challenge;
                }

                // Group all challenges in a container if provided
                if (challengesContainer)
                {
                    challengeGO.transform.SetParent(challengesContainer);    
                }
                else
                {
                    challengeGO.transform.SetParent(transform);
                }
                

                _challengeObjects.Add(challenge.challenge_id, challengeGO);
            }
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
