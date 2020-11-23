using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using Newtonsoft.Json;
using RestSharp.Deserializers;
using SCILL;
using SCILL.Api;
using SCILL.Model;
using UnityEngine;
using UnityEngine.PlayerLoop;
using Environment = SCILL.Environment;

public class SCILLManager : MonoBehaviour
{
    public string APIKey;
    public string AppId;
    public SCILL.Environment environment;

    public string AccessToken => _accessToken;

    public string UserId => SystemInfo.deviceUniqueIdentifier;
    public string SessionId => "1234";

    public static SCILLManager Instance; // **<- reference link to SCILL
    public EventsApi EventsApi => _scillClient.EventsApi;
    public ChallengesApi ChallengesApi => _scillClient.ChallengesApi;

    public SCILLClient SCILLClient => _scillClient;

    private SCILLBackend _scillBackend;
    private SCILLClient _scillClient;
    private string _accessToken;

    private WsClient _wsClient;
    
    public delegate void _OnChallengeWebhookMessage(ChallengeWebhookPayload payload);

    public _OnChallengeWebhookMessage OnChallengeWebhookMessage;
    
    private JsonSerializerSettings serializerSettings = new JsonSerializerSettings
    {
        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
    };
    
    async private void Awake()
    {
        Debug.Log("User-ID: " + UserId);
        if (Instance == null) {
            Instance = this;
            
            // This part should be done in the backend if possible to not expose the API key
            _scillBackend = new SCILLBackend(this.APIKey, environment);
            _accessToken = _scillBackend.GetAccessToken(UserId);
            
            _scillClient = new SCILLClient(_accessToken, AppId, environment);

            string env = "production";
            if (environment == Environment.Staging)
            {
                env = "staging";
            } else if (environment == Environment.Development)
            {
                env = "development";
            }
            
            var server = "wss://playground.scillgame.com/scill/ws/challenges/" + AppId + "/" + UserId + "/unity-editor?environment=" + env;
            _wsClient = new WsClient(server);
            await _wsClient.Connect();

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
        // Check if server send new messages
        var cqueue = _wsClient.receiveQueue;
        string msg;
        while (cqueue.TryPeek(out msg))
        {
            // Parse newly received messages
            cqueue.TryDequeue(out msg);
            HandleMessage(msg);
        }
    }
    /// <summary>
    /// Method responsible for handling server messages
    /// </summary>
    /// <param name="msg">Message.</param>
    private void HandleMessage(string msg)
    {
        Debug.Log("Server: " + msg);
        
        var webhookPayload = (ChallengeWebhookPayload)JsonConvert.DeserializeObject(msg, typeof(ChallengeWebhookPayload), serializerSettings);
        Debug.Log(webhookPayload);
        OnChallengeWebhookMessage(webhookPayload);
    }    

    public async void SendEventAsync(string eventName, string eventType = "single", EventMetaData metaData = null)
    {
        Debug.Log("Sending event " + eventName);
        var payload = new EventPayload(UserId, SessionId, eventName, eventType, metaData);
        var response = await EventsApi.SendEventAsync(payload);
        Debug.Log(response);
    }

    public Task<List<ChallengeCategory>> GetPersonalChallengesAsync()
    {
        return _scillClient.ChallengesApi.GetPersonalChallengesAsync(AppId);
    }
}
