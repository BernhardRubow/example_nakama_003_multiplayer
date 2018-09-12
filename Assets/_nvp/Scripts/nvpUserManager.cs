using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nvpUserManager: MonoBehaviour, IUserManager {

	// +++ public fields ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++    
	// +++ editor fields ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
	// +++ private fields +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
	private string _playerId;
    
    
    
    
    // +++ unity callbacks ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
	void Awake () {
		if (!PlayerPrefs.HasKey("id")){
			PlayerPrefs.SetString("id", System.Guid.NewGuid().ToString());
		}
		_playerId = PlayerPrefs.GetString("id");
	}




    // +++ event handler ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
	// +++ interface methods ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
	public string GetPlayerId() => _playerId;


	
    // +++ class methods ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

    

}