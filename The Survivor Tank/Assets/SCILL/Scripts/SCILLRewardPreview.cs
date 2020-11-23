using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SCILLRewardPreview : MonoBehaviour
{
    private SCILLReward _scillReward;
    private GameObject _rewardModel;

    public GameObject photoBox;
    public Text rewardName;
    public Text rewardDescription;

    public SCILLReward reward
    {
        get => _scillReward;
        set
        {
            _scillReward = value;
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
    }

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
