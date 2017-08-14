using System.Collections;
using System.Collections.Generic;
using UnityEngine;
[RequireComponent(typeof(ComputeSwarm))]
public class DemoOptions : MonoBehaviour {
    [HideInInspector]
    public static bool activated = false;
    public static bool FPSactivated = false;
    public GameObject FramerateWatcher,MemoryWatcher;
    public UnityStandardAssets.Characters.FirstPerson.RigidbodyFirstPersonController firstPersonController;
    public ComputeSwarm swarm;
    #region GUI
    Vector2 scrollPosition = Vector2.zero;
    int hSliderValue = 1;
    bool usingGPU = false;
    bool usingFixedUpdate = false;
    #endregion
    void Start () {
        hSliderValue = swarm.instanceCount;
        usingGPU = swarm.usingGPU;
        usingFixedUpdate = swarm.usingFixedUpdate;
        StartCoroutine(augmentInstanceCount());
    }
	
	void Update () {
		if(Input.GetKeyDown(KeyCode.F1))
        {
            activated = !activated;
            if(activated)
            {
                firstPersonController.enabled = false;
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            else
            {
                firstPersonController.enabled = true;
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            FPSactivated = !FPSactivated;
            if(FPSactivated)
            {
                FramerateWatcher.SetActive(true);
                MemoryWatcher.SetActive(true);
            }
            else
            {
                FramerateWatcher.SetActive(false);
                MemoryWatcher.SetActive(false);
            }
        }
        // Updating values
        swarm.instanceCount = hSliderValue;
        swarm.usingGPU = usingGPU;
        swarm.usingFixedUpdate = usingFixedUpdate;
    }
    private void OnGUI()
    {
        if(activated)
        {
            scrollPosition = GUI.BeginScrollView(new Rect(100, 100, Screen.width-200, Screen.height-200), scrollPosition, new Rect(0, 0, Screen.width-100, Screen.height));
            GUI.Label(new Rect(400, 0, 100, 100), "MENU");
            GUI.Label(new Rect(0, 70, 500, 30), "InstanceCount : " + hSliderValue);
            hSliderValue = Mathf.RoundToInt( GUI.HorizontalSlider(new Rect(0, 100, 500, 30), hSliderValue, 0, swarm.maxInstance));
            GUI.Label(new Rect(0, 120, 500, 30), "USING GPU ?");
            usingGPU = GUI.Toggle(new Rect(0, 150, 30, 30), usingGPU,"");
            GUI.Label(new Rect(0, 170, 500, 30), swarm.computeTime.ToString());
            GUI.Label(new Rect(0, 200, 500, 30), "USING Fixed Update ?");
            usingFixedUpdate = GUI.Toggle(new Rect(0, 230, 30, 30), usingFixedUpdate, "");
            GUI.EndScrollView();
        }
    }
    IEnumerator augmentInstanceCount()
    {
        while (true)
        {
            hSliderValue+=100;
            CSVExport.LogCSV(new string[] { Time.time.ToString(),Time.deltaTime.ToString() });
            yield return new WaitForSeconds(0.1f);
        }
    }
}
