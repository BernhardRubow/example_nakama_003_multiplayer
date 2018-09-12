using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;
using System;
using newvisionsproject.managers.events;


public class nvpNakamaManager : MonoBehaviour
{

    // +++ public fields ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++    
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
		_matchMakerTicket = await _socket.AddMatchmakerAsync("*", 2, 2);
		Debug.Log("Matchmaker ticket received");

		// wait for 2 players to connect
		StartCoroutine(WaitForPlayersToJoin());

    }


    // Update is called once per frame
    void Update()
    {

    }

	// +++ coroutines +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
	IEnumerator WaitForPlayersToJoin(){
		while(_connectedUsers.Count != 2){
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
    private void OnMatchState(object sender, IMatchState e)
    {

        


        Debug.Log("OnMatchState");
        string sMsg = System.Text.Encoding.UTF8.GetString(e.State);
        nvpEventManager.INSTANCE.InvokeEvent(OnMatchState, e.OpCode, )

    }

    private void OnMatchPresence(object sender, IMatchPresenceEvent e)
    {
        Debug.Log("OnMatchPresence");
		_connectedUsers.AddRange(e.Joins);
        foreach (var leave in e.Leaves)
        {
            _connectedUsers.RemoveAll(item => item.SessionId.Equals(leave.SessionId));
        };
    }

    private void OnDisconnect(object sender, EventArgs e)
    {
        Debug.Log("OnDisconnect");
    }

    private void OnConnect(object sender, EventArgs e)
    {
        Debug.Log("OnConnect");
    }




	// +++ public class methods +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
	public List<IUserPresence> GetConnectedUsers() => _connectedUsers;
    public IUserPresence GetSelf() => _self;



    // +++ private class methods ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
	void Init(){

		_userManager = GameObject.Find("managers").GetComponent<IUserManager>();
        if(_userManager == null){
            Debug.LogWarning("No user manager found");
        }
		_connectedUsers = new List<IUserPresence>();
	}


}