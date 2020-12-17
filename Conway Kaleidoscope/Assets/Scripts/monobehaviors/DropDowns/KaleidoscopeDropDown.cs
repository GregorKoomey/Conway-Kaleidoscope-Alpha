using UnityEngine;

[RequireComponent(typeof(ConwayMain))]
public class KaleidoscopeDropDown : MonoBehaviour
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
                Debug.Log("Kaleidoscope");
                main.SetKaleidoscope(true);
                break;
            case 1:
                Debug.Log("Simple Conway");
                main.SetKaleidoscope(false);
                break;
        }
    }
}
