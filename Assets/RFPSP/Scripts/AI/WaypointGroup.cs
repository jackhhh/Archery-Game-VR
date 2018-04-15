//WaypointGroup.cs by Azuline Studios© All Rights Reserved
//Stores waypoints in an array and draws waypoint placement helper gizmos in editor.
using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class WaypointGroup : MonoBehaviour {
	[Tooltip("True if waypoints in this group and lines between them should be drawn in editor.")]
	public bool drawWaypoints = true;
	[HideInInspector]
	public List<Transform> wayPoints = new List<Transform>();//array of waypoints in this group, populated on scene load
	private Transform myTransform;
	
	void Start (){
		myTransform = transform;
		wayPoints.Clear();
		for (int i = 0; i < myTransform.childCount; i++){
			wayPoints.Add(myTransform.GetChild(i));
		}
	}

	void OnDrawGizmos (){

		if(drawWaypoints){
			myTransform = transform;
			wayPoints.Clear();
			for (int i = 0; i < myTransform.childCount; i++){
				wayPoints.Add(myTransform.GetChild(i));
			}

			if(wayPoints != null){
				if(wayPoints.Count != 0){
					for (int i = 0; i < wayPoints.Count; i++){
						if(wayPoints[i] != null){
							Gizmos.color = Color.green;
							Gizmos.DrawWireSphere(wayPoints[i].position, 0.3f);
							if(i != wayPoints.Count -1){
								if(wayPoints[i + 1] != null && wayPoints.Count > 1){
									if (Physics.Linecast(wayPoints[i].position, wayPoints[i + 1].position)) {
										Gizmos.color = Color.red;
										Gizmos.DrawLine (wayPoints[i].position, wayPoints[i + 1].position);
									} else {
										Gizmos.color = Color.green;
										Gizmos.DrawLine (wayPoints[i].position, wayPoints[i + 1].position);
									}
								}
							}
						}
					}
				}
			}
		}
	}

}
