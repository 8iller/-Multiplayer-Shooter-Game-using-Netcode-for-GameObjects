using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class cameralook : MonoBehaviour
{
    public float Sensitivity = 100;
    [SerializeField]TMP_InputField SensitivityUI ;
    [SerializeField] Transform CameraLookup = null;
    [SerializeField] GameObject setting;
    [SerializeField] Button joinButton; 
    float XRotation = 0f;
    private void Start()
    {  
        SensitivityUI.text = Sensitivity.ToString();
        Cursor.lockState = CursorLockMode.Locked;
        joinButton.GetComponentInChildren<TextMeshProUGUI>().text = LobbyManager.relayJoinCode;
    }

    
    void Update()   
    {   
        
        if (Input.GetKeyDown(KeyCode.Escape) && (!setting.activeSelf) )
        {   
            setting.SetActive(true);
            Cursor.lockState = CursorLockMode.None;

        }
        else if (Input.GetKeyDown(KeyCode.Escape) && (setting.activeSelf))
        {
            OnBack();

        }
        
        
        float mouseX = Input.GetAxis("Mouse X")*Sensitivity*Time.deltaTime;
        float mouseY = Input.GetAxis("Mouse Y")*Sensitivity*Time.deltaTime;



        XRotation -= mouseY;
        XRotation = Mathf.Clamp(XRotation ,-90f,90f);
        

        transform.localRotation = Quaternion.Euler(XRotation,0f,0f);
        CameraLookup.Rotate(Vector3.up * mouseX);

    }

    public void OnBack()
    {   
        setting.SetActive(false);
        Cursor.lockState = CursorLockMode.Locked;


    }

    public void Leave()
    {
        LobbyManager.LeaveLobby();

    }
    
    public void ChangeSensitivity()
    {
         Sensitivity = float.Parse(SensitivityUI.text);


    }

    public void CopyText()
    {
        string buttonText = joinButton.GetComponentInChildren<TextMeshProUGUI>().text;

        // Check if there is text on the button
        if (!string.IsNullOrEmpty(buttonText))
        {
            // Copy the text to the clipboard
            GUIUtility.systemCopyBuffer = buttonText;
            
        }
    }
}
