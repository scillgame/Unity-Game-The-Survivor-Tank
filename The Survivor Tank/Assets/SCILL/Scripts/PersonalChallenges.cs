using System.Collections;
using System.Collections.Generic;
using SCILL;
using SCILL.Model;
using UnityEngine;

public class PersonalChallenges : MonoBehaviour
{
    public GameObject categoryPrefab;
    public GameObject challengePrefab;
    public bool AutoActivateChallenges = false;
    private List<ChallengeCategory> _categories;

    private Dictionary<string, GameObject> _categoryObjects = new Dictionary<string, GameObject>();

    // Start is called before the first frame update
    void Start()
    {
        UpdatePersonalChallengesList();

        /*
        foreach (var category in categories)
        {
            foreach (var challenge in category.challenges)
            {
                if (challenge.type == "unlock")
                {
                    await SCILLManager.Instance.SCILLClient.UnlockPersonalChallengeAsync(challenge.challenge_id);
                    await SCILLManager.Instance.SCILLClient.ActivatePersonalChallengeAsync(challenge.challenge_id);
                } else if (challenge.type == "unlocked")
                {
                    await SCILLManager.Instance.SCILLClient.ActivatePersonalChallengeAsync(challenge.challenge_id);
                }
            }
        }*/

        SCILLManager.Instance.OnChallengeWebhookMessage += OnChallengeWebhookMessage;
    }

    private Challenge FindChallengeById(string id)
    {
        foreach (var category in _categories)
        {
            foreach (var challenge in category.challenges)
            {
                if (challenge.challenge_id == id)
                {
                    return challenge;
                }
            }
        }

        return null;
    }

    public async void UpdatePersonalChallengesList()
    {
        var categories = await SCILLManager.Instance.SCILLClient.GetPersonalChallengesAsync();
        _categories = categories;
        UpdateCategories(categories);
    }

    void UpdateCategories(List<ChallengeCategory> categories)
    {
        foreach (var category in categories)
        {
            GameObject categoryGO = null;
            if (_categoryObjects.TryGetValue(category.category_id, out categoryGO))
            {
                var categoryItem = categoryGO.GetComponent<SCILLCategoryItem>();
                categoryItem.Category = category;
                categoryItem.UpdateChallengeList();
            }
            else
            {
                categoryGO = Instantiate(categoryPrefab);
                categoryGO.transform.SetParent(transform);
                var categoryItem = categoryGO.GetComponent<SCILLCategoryItem>();
                if (categoryItem)
                {
                    categoryItem.Category = category;
                }

                _categoryObjects.Add(category.category_id, categoryGO);
            }
        }
    }

    void UpdateChallenge(Challenge newChallenge)
    {
        Challenge challenge = FindChallengeById(newChallenge.challenge_id);
        if (challenge != null)
        {
            challenge.type = newChallenge.type;
            challenge.user_challenge_current_score = newChallenge.user_challenge_current_score;
            challenge.user_challenge_activated_at = newChallenge.user_challenge_activated_at;
            challenge.user_challenge_unlocked_at = newChallenge.user_challenge_unlocked_at;            
        }
    }

    void OnChallengeWebhookMessage(ChallengeWebhookPayload payload)
    {
        Debug.Log("WEBHOOK MESSAGE");
        Debug.Log(payload);
        
        UpdateChallenge(payload.new_challenge);
    }

    public void UnlockPersonalChallenge(Challenge challenge)
    {
        var response = SCILLManager.Instance.SCILLClient.UnlockPersonalChallenge(challenge.challenge_id);
        if (response.status >= 200 && response.status < 300)
        {
            if (response.challenge != null)
            {
                UpdateChallenge(response.challenge);
            }
        }
    }
    
    public void ActivatePersonalChallenge(Challenge challenge)
    {
        var response = SCILLManager.Instance.SCILLClient.ActivatePersonalChallenge(challenge.challenge_id);
        if (response.status >= 200 && response.status < 300)
        {
            if (response.challenge != null)
            {
                UpdateChallenge(response.challenge);
            }
        }
    }
    
    public void ClaimPersonalChallengeReward(Challenge challenge)
    {
        var response = SCILLManager.Instance.SCILLClient.ClaimPersonalChallengeReward(challenge.challenge_id);
        if (response.status >= 200 && response.status < 300)
        {
            if (response.challenge != null)
            {
                UpdateChallenge(response.challenge);
            }
        }
    }    

    public void CancelPersonalChallenge(Challenge challenge)
    {
        var response = SCILLManager.Instance.SCILLClient.CancelPersonalChallenge(challenge.challenge_id);
        if (response.status >= 200 && response.status < 300)
        {
            if (response.challenge != null)
            {
                UpdateChallenge(response.challenge);
            }
        }
    }        
    // Update is called once per frame
    void Update()
    {
        
    }
}
