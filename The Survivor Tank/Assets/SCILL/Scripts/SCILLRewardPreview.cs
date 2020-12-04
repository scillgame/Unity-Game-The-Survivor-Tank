using System.Collections;
using System.Collections.Generic;
using SCILL.Model;
using UnityEngine;
using UnityEngine.UI;

public class SCILLRewardPreview : MonoBehaviour
{
    [Header("Required connections")]
    [Tooltip("Connect to a text field to render the reward")]
    public Text rewardName;
    [Tooltip("Connect to a text field to render a description of the reward")]
    public Text rewardDescription;
    [Tooltip("Connect to a claim button which should have a Button attached. This item is hidden unless the reward can be claimed and has not been yet claimed")]
    public GameObject claimButton;
    
    [Header("Optional connections")]
    [Tooltip("Connect to a Reward Photobox which will be used to render a 3D representation of the reward")]
    public GameObject photoBox;
    
    private SCILLReward _scillReward;
    private GameObject _rewardModel;
    private BattlePassLevel _selectedBattlePassLevel;

    public BattlePassLevel SelectedBattlePassLevel
    {
        get => _selectedBattlePassLevel;
        set
        {
            _selectedBattlePassLevel = value;
            SetRewardId(_selectedBattlePassLevel.reward_amount);
        }
    }

    private void SetRewardId(string rewardId)
    {
        _scillReward = Resources.Load<SCILLReward>(rewardId);
        if (_scillReward)
        {
            UpdateScillReward();
        }
    }

    void UpdateScillReward()
    {
        if (_rewardModel)
        {
            DestroyImmediate(_rewardModel);
        }

        if (_scillReward.prefab)
        {
            _rewardModel = Instantiate(_scillReward.prefab, photoBox.transform);
            //_rewardModel.transform.localPosition = Vector3.zero;
        }

        if (rewardDescription)
        {
            rewardDescription.text = _scillReward.description;
        }

        if (rewardName)
        {
            rewardName.text = _scillReward.name;
        }

        if (claimButton)
        {
            if (_selectedBattlePassLevel.activated_at == null || _selectedBattlePassLevel.level_completed == false)
            {
                claimButton.SetActive(false);
            }
            else
            {
                if (_selectedBattlePassLevel.reward_claimed == false)
                {
                    claimButton.SetActive(true);
                }
                else
                {
                    claimButton.SetActive(false);
                }
            }
        }
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public async void OnClaimButtonPassRewardButtonClicked()
    {
        var response = await SCILLManager.Instance.SCILLClient.ClaimBattlePassLevelRewardAsync(_selectedBattlePassLevel.level_id);
        Debug.Log(response.ToJson());
        if (response != null && response.message == "OK")
        {
            _selectedBattlePassLevel.reward_claimed = true;
        }
        UpdateScillReward();
    }
}
