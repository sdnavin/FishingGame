using UnityEngine;
using UnityEngine.Events;
using System;
using System.Collections;
using NativeWebSocket;

[System.Serializable]
public class GameMessage
{
    public string type;
    public string gameId;
    public int slotNumber;
    public int userId;

    public GameMessage(string messageType, string gameId, int slotNumber=-1,int userId=-1)
    {
        this.type = messageType;
        this.gameId = gameId;
        this.slotNumber = slotNumber;
        this.userId = userId;
    }
}

public class WebSocketClient : MonoBehaviour
{
    [SerializeField]
    public WorldScript[] worldScripts;
    private WebSocket websocket;
    public  string serverUrl = "ws://localhost:8080"; // Change this to your WebSocket server URL
    private bool isConnecting = false;
    private bool Connected = false;
    private float reconnectDelay = 5f;
    private bool shouldReconnect = true;

    bool gameStart;

    [SerializeField]
    private bool autoReconnect = true;

    public static DataIn[] dataIn;
    public DataIn dataInVis;
    public GameState gameState;

    private float lastUpdateTime; // Tracks the last time data was updated
    private float resetTimeThreshold = 0.5f; // 500ms threshold for resetting

    private bool isConnectedtoJoystick = false;
    [SerializeField]
    UnityEvent OnConnected;
    [SerializeField]
    string uniqueID;
    [SerializeField]
    int slotNumber;
    private void Start()
    {
        dataIn = new DataIn[4];
        uniqueID = SystemInfo.deviceUniqueIdentifier;
        Debug.Log("Device Unique ID: " + uniqueID);

        InitializeWebSocket();
        StartCoroutine(ConnectToServer());
    }

    private void InitializeWebSocket()
    {
        websocket = new WebSocket(serverUrl);

        websocket.OnOpen += () =>
        {
            Debug.Log("[WebSocket] Connected to server!");

            isConnecting = false;
            Connected = true;
        };

        websocket.OnError += (e) =>
        {
            Debug.LogError("[WebSocket] Error: " + e);
            isConnecting = false;
            Connected = false;
        };

        websocket.OnClose += (e) =>
        {
            Debug.Log("[WebSocket] Connection closed!");
            isConnecting = false;
            Connected = false;
            if (autoReconnect && shouldReconnect)
            {
                StartCoroutine(ReconnectWithDelay());
            }
        };

        websocket.OnMessage += (bytes) =>
        {
            var message = System.Text.Encoding.UTF8.GetString(bytes);
            ProcessReceivedData(message);
        };
    }

    private IEnumerator ConnectToServer()
    {
        while (true)
        {
            if (websocket.State == WebSocketState.Closed && !isConnecting)
            {
                isConnecting = true;
                Debug.Log("[WebSocket] Attempting to connect...");

                    yield return new WaitForSeconds(0.1f); // Small delay before connecting
                    websocket.Connect();
            }
            yield return new WaitForSeconds(1f);
        }
    }

    private IEnumerator ReconnectWithDelay()
    {
        Debug.Log($"[WebSocket] Attempting to reconnect in {reconnectDelay} seconds...");
        yield return new WaitForSeconds(reconnectDelay);

        if (websocket.State == WebSocketState.Closed)
        {
            InitializeWebSocket();
        }
    }
    int activeWorlds = 0;
    private void ProcessReceivedData(string data)
    {
        if (slotNumber == 0)
        {
            try
            {
                gameState = JsonUtility.FromJson<GameState>(data);
                if (gameState.gameStatus.Length > 0)
                {
                    slotNumber = int.Parse(gameState.slotNumber);
                }
            }
            catch (Exception e)
            {
                Debug.LogError("[WebSocket] Error processing data: " + e.Message);
            }
        }
        try
        {
            //Debug.Log("[WebSocket] Received: " + data);

            dataInVis = JsonUtility.FromJson<DataIn>(data);
           
            if (dataInVis.status == "userJoined")
            {
                OnConnected.Invoke();
                isConnectedtoJoystick = true;
                print(dataInVis.user.slotId);
                activeWorlds = (dataInVis.user.slotId)-1;
                print(activeWorlds);
                UIHandler.instance.closeUI(activeWorlds);

                worldScripts[activeWorlds].BringItOn();
            }
            //if (dataInVis.received.type!=null&&dataInVis.received.type.Length>0)
            //{
            //    OnConnected.Invoke();
            //    isConnectedtoJoystick = true;
            //}
            if (dataInVis.data != null&& dataInVis.data.slotId>0)
            {
                dataIn[dataInVis.data.slotId - 1] = dataInVis;
            }
            lastUpdateTime = Time.time;
            // Add your data processing logic here
            // Example: Parse JSON data
            // var parsedData = JsonUtility.FromJson<YourDataType>(data);
        }
        catch (Exception e)
        {
            Debug.LogError("[WebSocket] Error processing data: " + e.Message);
        }
    }



    public async void SendMessage(string message)
    {
        if (websocket.State == WebSocketState.Open)
        {
            try
            {
                await websocket.SendText(message);
                Debug.Log("[WebSocket] Sent: " + message);
            }
            catch (Exception e)
            {
                Debug.LogError("[WebSocket] Error sending message: " + e.Message);
            }
        }
        else
        {
            Debug.LogWarning("[WebSocket] Cannot send message - connection is not open");
        }
    }

    private void Update()
    {
        if (websocket != null)
        {
            websocket.DispatchMessageQueue();
        }
        if (Input.GetKeyDown(KeyCode.Space))
        {
            OnConnected.Invoke();
        }
        if (!gameStart && Connected)
        {
            gameStart = true;
            SendGameStartMessage();
        }
        //if (Time.time - lastUpdateTime > resetTimeThreshold)
        //{
        //    dataInVis = null;
        //    dataIn = dataInVis;
        //}
    }

    private async void OnDisable()
    {
        shouldReconnect = false; // Prevent auto reconnection when disabled

        if (websocket != null && websocket.State == WebSocketState.Open)
        {
            await websocket.Close();
        }
    }

    private void OnApplicationQuit()
    {
        shouldReconnect = false;
        StopAllCoroutines();
    }

    // Public methods for external control
    public bool IsConnected()
    {
        return websocket != null && websocket.State == WebSocketState.Open;
    }

    public void Disconnect()
    {
        shouldReconnect = false;
        StopAllCoroutines();
        if (websocket != null)
        {
            websocket.Close();
        }
    }


    public void SetAutoReconnect(bool value)
    {
        autoReconnect = value;
    }
    public void SendGameStartMessage()
    {
        GameMessage message = new GameMessage("gameStart", uniqueID, slotNumber);
        string jsonMessage = JsonUtility.ToJson(message);
        SendMessage(jsonMessage);
    }
    public void SendGameEndMessage()
    {
        GameMessage message = new GameMessage("gameEnd", uniqueID);
        string jsonMessage = JsonUtility.ToJson(message);
        SendMessage(jsonMessage);
    }

    public void UserJoinedMessage()
    {
        GameMessage message = new GameMessage("userJoin", uniqueID, slotNumber, -1);
        string jsonMessage = JsonUtility.ToJson(message);
        SendMessage(jsonMessage);
    }

    public void UserLeftMessage()
    {
        GameMessage message = new GameMessage("userLeft", uniqueID, slotNumber, -1);
        string jsonMessage = JsonUtility.ToJson(message);
        SendMessage(jsonMessage);
    }

    public void SetServerUrl(string newUrl)
    {
        if (serverUrl != newUrl)
        {
            serverUrl = newUrl;
            if (websocket != null && websocket.State == WebSocketState.Open)
            {
                Disconnect();
                InitializeWebSocket();
                StartCoroutine(ConnectToServer());
            }
        }
    }
}