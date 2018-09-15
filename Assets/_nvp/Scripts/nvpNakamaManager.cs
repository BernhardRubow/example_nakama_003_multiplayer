using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using Nakama.TinyJson;
using System;
using newvisionsproject.managers.events;

/**
 * Class for handling all communication which the nakama backend server.
 *
 * Usage:
 * On instantiation a connection to the nakama backend server is build up.
 * 
 * Use the public method 'SendRealtimeMessage' to send data to the connected players
 *
 * Subscribe to the 'GameEvents.OnRealtimeMessageReceived' event, which is invoked
 * by the nvp event dispatcher.
 */
public class nvpNakamaManager : MonoBehaviour
{

    // +++ public fields ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++   
    public const int NUMPLAYERS = 2;

    // +++ editor fields ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
	[SerializeField] private string _key = "defaultkey";
	[SerializeField] private string _host = "127.0.0.1";
	[SerializeField] private int _port = 7350;




    // +++ private fields +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    private const string PrefKeyName = "nakama.session";
    private IClient _client;
    private ISession _session;
	private ISocket _socket;
	private IMatchmakerTicket _matchMakerTicket;
	private IMatch _match;
	private IUserPresence _self;
	private string _matchId;
	private string _playerId;
	private IUserManager _userManager;
	private List<IUserPresence> _connectedUsers;
    




    // +++ unity callbacks ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    async void Start()
    {
        Init();

		// create client
		_client = new Client(_key, _host, 7350, false);
        _playerId = _userManager.GetPlayerId();

		// create session
        _session = await _client.AuthenticateDeviceAsync(_playerId);

		// Init List of connected users
        _connectedUsers = new List<IUserPresence>(0);

		// create socket
        _socket = _client.CreateWebSocket();

		// subscribe to socket events
        _socket.OnMatchmakerMatched += OnMatchmakerMatched;
        _socket.OnConnect += OnConnect;
        _socket.OnDisconnect += OnDisconnect;
        _socket.OnMatchPresence += OnMatchPresence;
        _socket.OnMatchState += OnMatchState;

		// wait for socket connection
        await _socket.ConnectAsync(_session);
		Debug.Log("Socket connected");

		// wait for match maker ticket
		_matchMakerTicket = await _socket.AddMatchmakerAsync(
            "*",
            NUMPLAYERS,
            NUMPLAYERS);
		Debug.Log("Matchmaker ticket received");

		// wait for 2 players to connect
		StartCoroutine(WaitForNumberOfPlayers(NUMPLAYERS));
    }

    // Update is called once per frame
    void Update()
    {

    }

	// +++ coroutines +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    /**
     * This coroutine check in short intervalls, if the desired bumber of
     * players have joined the game. If so, the method exits by throwning
     * an OnAllPlayersJoined event
     *
     * Paramter:
     * numberOfPlayerToMatch (int): Number of players to match to exit this
     *                              Method
     */
	IEnumerator WaitForNumberOfPlayers(int numberOfPlayerToMatch){
		while(_connectedUsers.Count != numberOfPlayerToMatch){
			Debug.LogFormat("Players in Game: {0}", _connectedUsers.Count);
			yield return new WaitForSeconds(1.0f);
		}
		
        nvpEventManager.INSTANCE.InvokeEvent(
            GameEvents.OnAllPlayersJoined,
            this,
            _connectedUsers
        );
	}




    // +++ event handler ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    async void OnMatchmakerMatched(object sender, IMatchmakerMatched e)
    {
        Debug.Log("Match found");

		_match = await _socket.JoinMatchAsync(e);
		
        Debug.Log("match joined");

		 // persisting own presence
        _self = _match.Self;
        _matchId = _match.Id;

		_connectedUsers.AddRange(_match.Presences);
    }

    /**
     * Eventhandler which is reasponsible for handling
     * realtime in game messages
     * 
     * Parameter:
     * sender (object): the boxed object, which invoked this event
     * msg (IMatchState): A nakama objece, which contains information
     *                    about the state received like:
     *                    - State (byteArray): the unencoded match state as byte array
     *                    - UserPresence (IUserPresence): Information about the user, 
     *                                                    who sent the state.
     */
    private void OnMatchState(object sender, IMatchState msg)
    {
        nvpEventManager.INSTANCE.InvokeEvent(
            GameEvents.OnRealtimeMessageReceived, 
            msg.OpCode, 
            msg);
    }

    /**
     * Eventhandler is called when the composition of the players in
     * the match has changed either by joining of new player or 
     * of player left the match.
     */
    private void OnMatchPresence(object sender, IMatchPresenceEvent e)
    {
        Debug.Log("OnMatchPresence");
		_connectedUsers.AddRange(e.Joins);
        foreach (var leave in e.Leaves)
        {
            _connectedUsers.RemoveAll(item => item.SessionId.Equals(leave.SessionId));
        };
    }

    /**
     * Event is thrown by nakama, when this client disconnect from the
     * nakama server
     */
    private void OnDisconnect(object sender, EventArgs e)
    {
        Debug.Log("OnDisconnect");
    }

    /**
     * Event is thrown by nakama, when this client connects to a 
     * nakama server
     */
    private void OnConnect(object sender, EventArgs e)
    {
        Debug.Log("OnConnect");
    }




	// +++ public class methods +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
	public List<IUserPresence> GetConnectedUsers() => _connectedUsers;
    public IUserPresence GetSelf() => _self;

    /**
     * This methode is used to a serializable state message (object or struct)
     * to the other players connected in this match. The messages are sent in
     * JSON-Format
     * 
     * Parameter:
     * opCode (NakamaOpCodes-Enum): An operation code identifier, which classifies 
     *                              the type of the message. Is available on the
     *                              receiving clients.
     * messageDto(<T>): a generic item (struct or class), which has to be serializable
     *                  Will be received on connected clients in form of a byte Array;       
     */
    public void SendRealtimeMessage<T>(NakamaOpCodes opCode, T messageDto){
        var sMessage = messageDto.ToJson();
        _socket.SendMatchState(_matchId, (int)opCode, sMessage);
    }




    // +++ private class methods ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
	void Init(){
		_userManager = GameObject.Find("managers").GetComponent<IUserManager>();
        if(_userManager == null){
            Debug.LogWarning("No user manager found");
        }
		_connectedUsers = new List<IUserPresence>();
	}
}

public enum NakamaOpCodes{
    PositionMessage
}