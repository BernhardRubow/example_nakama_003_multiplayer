using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class nvpUserManager: MonoBehaviour {

	// +++ public fields ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++    
	public string playerId;
	// +++ editor fields ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
	// +++ private fields +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    
    
    
    
    // +++ unity callbacks ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
	void Awake () {
		if (!PlayerPrefs.HasKey("id")){
			PlayerPrefs.SetString("id", System.Guid.NewGuid().ToString());
		}
		playerId = PlayerPrefs.GetString("id");
	}
	
	// Update is called once per frame
	void Update () {
		
	}
    
    
    
    
  // +++ event handler ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
  // +++ class methods ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++

}