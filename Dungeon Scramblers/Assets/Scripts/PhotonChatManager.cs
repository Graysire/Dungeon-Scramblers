using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Chat;
using ExitGames.Client.Photon;
using Photon.Pun;

public class PhotonChatManager : MonoBehaviourPunCallbacks, IChatClientListener
{
    //the client used for chat communications
    public ChatClient chatClient;
    //the player's name, replace with players username gather by other scripts
    public string plrName;
    //the state of the player's connection
    public Text connectionState;
    //the message the player would like to send
    public InputField msgInput;
    //the text where past messages are shown
    public Text msgArea;

    string roomChat;

    void Awake()
    {
        //sets this portion of the app to run to ensurem essages are recieved while user is ofcusing another app
        Application.runInBackground = true;
        //check for a chat ID
        if (string.IsNullOrEmpty(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat))
        {
            Debug.LogError("No chat ID provided");
            return;
        }

        //attempt to connect to the chat
        connectionState.text = "Connecting...";
        roomChat = "room";
        Debug.Log("Trying to connect chat");
        chatClient = new ChatClient(this);
        //connect with appID, AppVersion and authentication
        chatClient.Connect(PhotonNetwork.PhotonServerSettings.AppSettings.AppIdChat, PhotonNetwork.PhotonServerSettings.AppSettings.AppVersion, new AuthenticationValues(plrName));
       
    }

    void Update()
    {
        //calls servicing method on chat client
        //if (chatClient != null)
        //{
            chatClient.Service();
        //}
    }

    //Sends the current message to the photon chat server
    public void SendMessage()
    {
        Debug.Log("Send Messages");
        chatClient.PublishMessage(roomChat, msgInput.text);
        msgInput.text = "";
    }

    public void DebugReturn(DebugLevel level, string message)
    {
        //throw new System.NotImplementedException();
    }

    public void OnChatStateChange(ChatState state)
    {
        //throw new System.NotImplementedException();
    }

    public override void OnConnected()
    {
        string[] channels = new string[] { roomChat };
        Debug.Log(channels[0]);


        //throw new System.NotImplementedException();
        connectionState.text = "Connected";
        chatClient.Subscribe(channels);
        //chatClient.ChatRegion = "US";
        Debug.Log(chatClient.CanChatInChannel(roomChat));
        Debug.Log(chatClient.PublicChannels.Count);
        Debug.Log(chatClient.PublishMessage(roomChat, "Live"));
        chatClient.SetOnlineStatus(ChatUserStatus.Online);
    }

    public void OnDisconnected()
    {
        //throw new System.NotImplementedException();
    }

    public void OnGetMessages(string channelName, string[] senders, object[] messages)
    {
        Debug.Log("Get Messages");
        //msgArea.text = "";
        for (int i = 0; i < senders.Length; i++)
        {
            msgArea.text += senders[i] + ": " + messages[i] + "\n";
        }
    }

    public void OnPrivateMessage(string sender, object message, string channelName)
    {
        //throw new System.NotImplementedException();
    }

    public void OnStatusUpdate(string user, int status, bool gotMessage, object message)
    {
        //throw new System.NotImplementedException();
    }

    public void OnSubscribed(string[] channels, bool[] results)
    {
        Debug.Log("Subscribed");
        foreach (string channel in channels)
        {
            chatClient.PublishMessage(channel, "has entered the Dungeon");
        }
    }

    public void OnUnsubscribed(string[] channels)
    {
        foreach (string channel in channels)
        {
            chatClient.PublishMessage(channel, "has abandoned the Dungeon");
        }
    }

    public void OnUserSubscribed(string channel, string user)
    {
        //throw new System.NotImplementedException();
    }

    public void OnUserUnsubscribed(string channel, string user)
    {
        //throw new System.NotImplementedException();
    }
}
