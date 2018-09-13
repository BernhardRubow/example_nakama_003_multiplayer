using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ExtensionMethods;
using newvisionsproject.managers.events;
using System;
using Nakama;
using Nakama.TinyJson;

public class nvpPlayerMover : MonoBehaviour, ISyncedComponent
{

    // +++ public fields ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++    
    // +++ editor fields ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    [SerializeField] private float _speed;
    [SerializeField] float _messageIntervallInMilliseconds;
    [SerializeField, Range(0.0f, 1.0f)] float _lerpIntervall;



    // +++ private fields +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    private nvpNakamaManager _nakama;
    private IUser _user;
    private bool _isLocal;
    private float _timer;
    private float _timerThreshold;
    private Vector3 _networkPosition;
	private Vector3 _currentPosition;





    // +++ unity callbacks ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    void Start()
    {
        Init();
        SubscribeToEvents();

    }

    void Update()
    {
        if (_isLocal)
        {
            DoLocalUpdate();

            _timer += Time.deltaTime;
            if (_timer > _timerThreshold)
            {
                _timer = 0f;
                UpdateNetworkPosition();
            }
        }
        else
        {
            DoRemoteUpdate();
        }



    }




    // +++ event handler ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    /**
	 * Is called when the GameObject, this script is attached to, is destroyed
	 * Used to clean up any manual set event handlers 
	 */
    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

    /**
	 * Is called by nvpNakamaManager-Script when it receives a realtime message.
	 */
    void OnRealtimeMessageReceived(object sender, object msgObject)
    {
        IMatchState msg = msgObject as IMatchState;

        switch (msg.OpCode)
        {
            case (long)NakamaOpCodes.PositionMessage:
                var json = System.Text.Encoding.UTF8.GetString(msg.State);
                _networkPosition = JsonParser.FromJson<SerializableVector3>(json).ToVector3();
				_networkPosition += (_networkPosition - _currentPosition);
                break;

            default:
                break;
        }
    }




    // +++ class methods ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    private void UpdateNetworkPosition()
    {
        var position = this.transform.position.MakeSerializable();
        _nakama.SendRealtimeMessage(
            NakamaOpCodes.PositionMessage,
            position
        );
    }

    private void DoLocalUpdate()
    {
        var h = Input.GetAxis("Horizontal");
        var v = Input.GetAxis("Vertical");

        var movement = new Vector3(
            h * _speed,
            v * _speed,
            0
        );

        this.transform.Translate(movement * Time.deltaTime, Space.World);
    }

    private void DoRemoteUpdate()
    {	
		_currentPosition = transform.position;
        this.transform.position = Vector3.Lerp(
            this.transform.position,
            _networkPosition,
            _lerpIntervall);

    }

    private void Init()
    {
        _nakama = GameObject.Find("nakama").GetComponent<nvpNakamaManager>();
        if (_nakama == null)
        {
            Debug.LogError("Nakama manager not found");
        }

        _networkPosition = this.transform.position;
        _timerThreshold = _messageIntervallInMilliseconds / 1000f;
    }

    private void SubscribeToEvents()
    {
        nvpEventManager.INSTANCE.SubscribeToEvent(GameEvents.OnRealtimeMessageReceived, OnRealtimeMessageReceived);
    }

    private void UnsubscribeFromEvents()
    {
        nvpEventManager.INSTANCE.UnsubscribeFromEvent(GameEvents.OnRealtimeMessageReceived, OnRealtimeMessageReceived);
    }

    public void SetLocalFlag(bool isLocal)
    {
        _isLocal = isLocal;
    }
}