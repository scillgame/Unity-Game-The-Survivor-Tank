using System.Collections;
using System.Collections.Generic;
using SCILL;
using SCILL.Model;
using UnityEngine;

public class PersonalChallenges : MonoBehaviour
{
    public GameObject categoryPrefab;
    public bool AutoActivateChallenges = false;
    private List<ChallengeCategory> _categories;

    // Start is called before the first frame update
    async void Start()
    {
        var categories = await SCILLManager.Instance.SCILLClient.GetPersonalChallengesAsync();
        _categories = categories;
        foreach (var category in categories)
        {
            var categoryGO = Instantiate(categoryPrefab);
            var categoryItem = categoryGO.GetComponent<SCILLCategoryItem>();
            if (categoryItem)
            {
                categoryItem.Category = category;
            }
            categoryGO.transform.SetParent(transform);
        }

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
        }

        SCILLManager.Instance.OnChallengeWebhookMessage += OnChallengeWebhookMessage;
    }

    void OnChallengeWebhookMessage(ChallengeWebhookPayload payload)
    {
        foreach (var category in _categories)
        {
            foreach (var challenge in category.challenges)
            {
                if (challenge.challenge_id == payload.new_challenge.challenge_id)
                {
                    challenge.type = payload.new_challenge.type;
                    challenge.user_challenge_current_score = payload.new_challenge.user_challenge_current_score;
                    challenge.user_challenge_activated_at = payload.new_challenge.user_challenge_activated_at;
                    challenge.user_challenge_unlocked_at = payload.new_challenge.user_challenge_unlocked_at;
                }
            }
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
