using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Client.Options;
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

    private List<IMqttClient> _mqttClients = new List<IMqttClient>();
    private MqttFactory _mqttFactory = new MqttFactory();

    private IMqttClient _challengesMqttClient;
    private IMqttClient _battlePassMqttClient;
    
    public delegate void ChallengeWebhookMessageHandler(ChallengeWebhookPayload payload);

    public event ChallengeWebhookMessageHandler OnChallengeWebhookMessage;
    
    private JsonSerializerSettings serializerSettings = new JsonSerializerSettings
    {
        ConstructorHandling = ConstructorHandling.AllowNonPublicDefaultConstructor
    };

    private void Awake()
    {
        Debug.Log("User-ID: " + UserId);
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
