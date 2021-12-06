using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class ARTapToPlaceObject : MonoBehaviour
{
    public GameObject placementIndicator;
    public GameObject arObjectToSpawn;

    private GameObject spawnedObject;
    private ARRaycastManager arOrigin;
    private Pose placementPose;
    private bool placementPoseIsValid = false;
    private float initialDistance;
    private Vector3 initialScale;

    void Start()
    {
        arOrigin = FindObjectOfType<ARRaycastManager>();
    }

    // Update placement indicator, placement pose and spawn
    void Update()
    {
        if(placementPoseIsValid && Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Began){
            ARPlaceObject(); //Spawns the object
        }

        //Scaling using pinch involves two touches
        if(Input.touchCount == 2){
            var touchZero= Input.GetTouch(0);
            var touchOne = Input.GetTouch(1);

            if(touchZero.phase == TouchPhase.Ended || touchZero.phase == TouchPhase.Canceled || 
                touchOne.phase == TouchPhase.Ended || touchOne.phase == TouchPhase.Canceled) {
                return;
            }

            if(touchZero.phase == TouchPhase.Began || touchOne.phase == TouchPhase.Began){
                initialDistance = Vector2.Distance(touchZero.position, touchOne.position);
                initialScale = spawnedObject.transform.localScale;
                Debug.Log("Initial Distance: " + initialDistance + "GameObject Name: " + arObjectToSpawn.name);
            }
            else{
                var currentDistance = Vector2.Distance(touchZero.position, touchOne.position);

                if(Mathf.Approximately(initialDistance, 0)){
                    return; //do nothing because the change is too small
                }
                
                var factor = currentDistance / initialDistance;
                spawnedObject.transform.localScale = initialScale * factor;
            
            }

        }



        UpdatePlacementPose();
        UpdatePlacementIndicator();
    }

    private void UpdatePlacementIndicator(){
        if (placementPoseIsValid){
            placementIndicator.SetActive(true);
            placementIndicator.transform.SetPositionAndRotation(placementPose.position, placementPose.rotation);
        }
        else{
            placementIndicator.SetActive(false);
        }
    }

    private void UpdatePlacementPose(){
        var screenCenter = Camera.current.ViewportToScreenPoint(new Vector3(0.5f, 0.5f));
        var hits = new List<ARRaycastHit>();
        arOrigin.Raycast(screenCenter, hits, TrackableType.Planes);

        placementPoseIsValid = hits.Count > 0;
        if (placementPoseIsValid){
            placementPose = hits[0].pose;

            var cameraForward = Camera.current.transform.forward;
            var cameraBearing = new Vector3(cameraForward.x, 0, cameraForward.z).normalized;
            placementPose.rotation = Quaternion.LookRotation(cameraBearing);
        }
    }

    void ARPlaceObject(){
        spawnedObject = Instantiate(arObjectToSpawn, placementPose.position, placementPose.rotation);
    }
}
