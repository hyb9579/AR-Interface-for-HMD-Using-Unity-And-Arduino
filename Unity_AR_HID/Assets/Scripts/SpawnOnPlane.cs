using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;

public class SpawnOnPlane : MonoBehaviour
    //, IPointerClickHandler
{
    #region Editor public fields

    [SerializeField]
    Camera mainCamera;

    [SerializeField]
    GameObject crosshairPrefab;

    [SerializeField]
    Color crosshairInsidePlaneColor = Color.green;

    [SerializeField]
    Color crosshairOutsidePlaneColor = Color.yellow;

    [SerializeField]
    GameObject prefabToSpawn;

    [SerializeField]
    ARRaycastManager arraycastManager;

    [SerializeField]
    float scaleMinimum = 0.2f;

    [SerializeField]
    float scaleMaximum = 1f;

    [SerializeField]
    GameObject Bluetooth;

    [SerializeField]
    GameObject Controller;
    
    private Boolean isTouched;

    manager manager_cs;

    #endregion


    #region Private fields

    Vector3 screenCenter;

    GameObject crosshair;
    Renderer crosshairRenderer;

    Ray ray;

    readonly List<ARRaycastHit> arraycastHits = new List<ARRaycastHit>();

    bool raycast;
    bool arraycast;

    RaycastHit hitob;

    Quaternion hitob_rot;

    int shaderColorId;

    int layerMask;

    private GameObject rotob;

    #endregion

    #region Unity methods

    void Start() {

        layerMask = 1 << LayerMask.NameToLayer("Object");

        manager_cs = Bluetooth.GetComponent<manager>();
        screenCenter = new Vector3(mainCamera.pixelWidth / 2, mainCamera.pixelHeight / 2, 0f);
        
        crosshair = Instantiate(crosshairPrefab);
        crosshairRenderer = crosshair.GetComponentInChildren<Renderer>();
        crosshair.SetActive(false);

        shaderColorId = Shader.PropertyToID("_Color");
    }

    void Update() {

        isTouched = manager_cs.GetTouch();

        ray.origin = Controller.transform.position;
        ray.direction = Controller.transform.forward;

        raycast = Physics.Raycast(ray, out hitob, 10, layerMask);

        arraycast = arraycastManager.Raycast(ray, arraycastHits, TrackableType.PlaneWithinBounds) ||
                arraycastManager.Raycast(ray, arraycastHits, TrackableType.PlaneWithinInfinity);
                   
        if (arraycast)
        {

            var hit = arraycastHits[0];
            var pose = hit.pose;

            crosshair.SetActive(true);
            crosshair.transform.position = pose.position;
            crosshair.transform.up = pose.up;

            if (crosshairRenderer != null)
            {
                var color = hit.hitType == TrackableType.PlaneWithinBounds
                    ? crosshairInsidePlaneColor
                    : crosshairOutsidePlaneColor;
                crosshairRenderer.material.SetColor(shaderColorId, color);
            }

            //객체 조작시
            if (isTouched == true)
            {
                if (raycast)
                {
                    hitob.transform.position = pose.position;

                    hitob_rot = hitob.transform.rotation;
                    hitob.transform.rotation = Quaternion.Euler(hitob_rot.eulerAngles.x, Controller.transform.rotation.eulerAngles.z, hitob_rot.eulerAngles.z);
                }
                else
                {
                    //새 객체 생성

                    pose = arraycastHits[0].pose;
                    var spawnedObject = Instantiate(prefabToSpawn, pose.position, pose.rotation);

                    // Adjust the spawned object to look towards the camera (while staying
                    // perpendicular to the plane of a general orientation).
                    // Project the camera position to the tracked plane:
                    Vector3 distance = mainCamera.transform.position - spawnedObject.transform.position;
                    Vector3 normal = spawnedObject.transform.up.normalized;
                    Vector3 projectedPoint = mainCamera.transform.position
                        - (normal * Vector3.Dot(normal, distance));

                    // Rotate the spawned object towards the projected position:
                    Vector3 newForward = projectedPoint - spawnedObject.transform.position;
                    float angle = Vector3.Angle(spawnedObject.transform.forward, newForward);
                    spawnedObject.transform.Rotate(spawnedObject.transform.up, angle, Space.World);

                    //var randomScale = UnityEngine.Random.Range(scaleMinimum, scaleMaximum);
                    //spawnedObject.transform.localScale = Vector3.one * randomScale;
                }
            }
        }
        else
        {
            crosshair.SetActive(false);
        }
    }

    #endregion
}