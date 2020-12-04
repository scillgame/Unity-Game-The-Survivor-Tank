﻿using System.Collections.Generic;
using System.Threading.Tasks;
using SCILL;
using SCILL.Api;
using SCILL.Model;
using UnityEngine;

[HelpURL("https://developers.scillgame.com")]
public class SCILLManager : MonoBehaviour
{
    // Properties to be set in the Unity inspector
    [Tooltip("Set your API key here. You can get your API-key in the SCILL Admin Panel")]
    public string APIKey;
    [Tooltip("Set your App id here. You need to create an App in the SCILL Admin Panel")]
    public string AppId;
    [Tooltip("You should leave this setting in Production. Sometimes, the SCILL team might ask you to change that.")]
    public SCILL.Environment environment;

    // Getter for the access token
    public string AccessToken => _accessToken;
    
    // In this case, we use a unique devide identifier. Multi device support requires a user account system like
    // Steam, Playfab, etc.
    public string UserId => SystemInfo.deviceUniqueIdentifier;
    
    // Default session id. This is just an example value.
    public string SessionId => "1234";

    // Getter for the singleton instance of this class
    public static SCILLManager Instance; // **<- reference link to SCILL
    
    // Simple wrappers to get SCILL product APIs
    public EventsApi EventsApi => _scillClient.EventsApi;
    public ChallengesApi ChallengesApi => _scillClient.ChallengesApi;
    public BattlePassesApi BattlePassesApi => _scillClient.BattlePassesApi;
    public SCILLClient SCILLClient => _scillClient;

    // Local instances of SCILLClient. Please note, that SCILLBackend should not be used in game clients in production!
    private SCILLBackend _scillBackend;
    private SCILLClient _scillClient;
    private string _accessToken;
    
    private void Awake()
    {
        // Create an instance of this class and make sure it stays (also survives scene changes)
        if (Instance == null) {
            Instance = this;
            
            // This part should be done in the backend if possible to not expose the API key
            _scillBackend = new SCILLBackend(this.APIKey, environment);
            _accessToken = _scillBackend.GetAccessToken(UserId);

            _scillClient = new SCILLClient(_accessToken, AppId, environment);
            
            DontDestroyOnLoad(gameObject);
        }
        else {
            Destroy(gameObject);
        }
    }

    /// <summary>
    /// Unity method called every frame
    /// </summary>
    private void Update()
    {
    }

    // Basic convenience function to send an event. Users global UserId and sessionId
    public async void SendEventAsync(string eventName, string eventType = "single", EventMetaData metaData = null)
    {
        // Please note, in some cases you should change session ids. This is just a simple example where we don't need
        // to do that
        Debug.Log("Sending event " + eventName);
        var payload = new EventPayload(UserId, SessionId, eventName, eventType, metaData);
        var response = await EventsApi.SendEventAsync(payload);
        Debug.Log(response);
    }

    // Basic wrapper for getting personal challenges
    public async Task<List<ChallengeCategory>> GetPersonalChallengesAsync()
    {
        return await ChallengesApi.GetPersonalChallengesAsync(AppId);
    }
}
