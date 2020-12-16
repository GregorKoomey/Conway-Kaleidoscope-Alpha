using UnityEngine;

[RequireComponent(typeof(ConwayMain))]
public class PauseDropDown : MonoBehaviour
{
    private ConwayMain main;
    
    void Start()
    {
        main = transform.GetComponent<ConwayMain>();
    }
    
    public void HandleInputData(int choice)
    {
        switch (choice)
        {
            case 0:
                Debug.Log("Paused");
                main.SetTimerOn(false);
                break;
            case 1:
                Debug.Log("Running");
                main.SetTimerOn(true);
                break;
        }
    }
}
