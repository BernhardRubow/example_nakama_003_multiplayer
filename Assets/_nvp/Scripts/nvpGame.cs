using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Nakama;


public class nvpGame : MonoBehaviour
{

    // +++ public fields ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++    
    // +++ editor fields ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    [SerializeField] private Transform[] _spawnPoints;
    [SerializeField] private GameObject _playerPrefab;





    // +++ private fields +++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    nvpNakamaManager _nakama;



    // +++ unity callbacks ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    void Start()
    {
        Init();

		SpawnPlayers(_nakama.GetConnectedUsers());
    }

    // Update is called once per frame
    void Update()
    {

    }




    // +++ event handler ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
    // +++ class methods ++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++++
	private void SpawnPlayers(List<IUserPresence> userPresences)
    {
		for(int i = 0, n = userPresences.Count; i < n; i++)
		{
			var user = Instantiate(_playerPrefab, _spawnPoints[i].position, _spawnPoints[i].rotation);	
			user.GetComponent<IUser>().SetName("Player " + (i+1).ToString());
		}
	}

	private void Init(){
		_nakama = GameObject.Find("nakama").GetComponent<nvpNakamaManager>();

	}
}