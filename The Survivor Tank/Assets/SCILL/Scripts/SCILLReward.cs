using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "Reward", menuName = "SCILL/Reward", order = 1)] 
public class SCILLReward : ScriptableObject
{
    public Sprite image;
    public string name;
    [TextArea]
    public string description;

    public GameObject prefab;
}
