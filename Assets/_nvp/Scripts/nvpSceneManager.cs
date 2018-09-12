using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using newvisionsproject.managers.events;


public class nvpSceneManager : MonoBehaviour
{

    // +++ public fields ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++    
    // +++ editor fields ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    // +++ private fields +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++




    // +++ unity callbacks ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    void Start()
    {
        SceneManager.LoadScene("_nakama", LoadSceneMode.Additive);

        SubscribeToEvents();
    }

    // Update is called once per frame
    void Update()
    {

    }




    // +++ event handler ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    void OnDestroy()
    {
        UnsubscribeFromEvents();
    }

	void OnAllPlayersJoined(object e, object s){
		SceneManager.LoadScene("_game", LoadSceneMode.Additive);
	}

    // +++ class methods ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
	void SubscribeToEvents(){
		nvpEventManager.INSTANCE.SubscribeToEvent(GameEvents.OnAllPlayersJoined, OnAllPlayersJoined);
	}

	void UnsubscribeFromEvents(){
		nvpEventManager.INSTANCE.UnsubscribeFromEvent(GameEvents.OnAllPlayersJoined, OnAllPlayersJoined);
	}
}