using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface ISyncedComponent {

	void SetLocalFlag(bool isLocal);
	void HandleMessage();
}