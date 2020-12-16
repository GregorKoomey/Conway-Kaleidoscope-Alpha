using UnityEngine;

[RequireComponent(typeof(ConwayMain))]
public class BrushDropDown : MonoBehaviour
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
//                Debug.Log("Glider Gun");
                main.ClearMap();
                main.CenterBrush(BrushFactory.GliderGun());
                main.UpdateDisplay();
                break;
            case 1:
                Debug.Log("Simple Glider");
                main.ClearMap();
                main.CenterBrush(BrushFactory.SimpleGlider());
                main.UpdateDisplay();
                break;
            case 2:
                Debug.Log("Tiled Glider");
                main.ClearMap();
                CellOffset[] tempBrush = BrushFactory.SimpleGlider();
                main.SetBrushAt(0, 0, BrushFactory.TiledBrush(main.ColumnCount, main.RowCount, 3, BrushFactory.SimpleGlider()));
                main.UpdateDisplay();
                break;
            case 3:
                Debug.Log("RPentimeno");
                main.ClearMap();
                main.CenterBrush(BrushFactory.RPentomino());
                main.UpdateDisplay();
                break;
            case 4:
                Debug.Log("Acorn");
                main.ClearMap();
                main.CenterBrush(BrushFactory.Acorn());
                main.UpdateDisplay();
                break;
            case 5:
                Debug.Log("Randomize");
                main.RandomizeState();
                main.UpdateDisplay();
                break;
            case 6:
                Debug.Log("Clear Map");
                main.ClearMap();
                main.UpdateDisplay();
                break;
        }
    }
}
