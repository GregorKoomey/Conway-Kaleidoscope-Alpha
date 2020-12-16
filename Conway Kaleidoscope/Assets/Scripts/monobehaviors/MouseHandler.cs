using UnityEngine;
[RequireComponent(typeof(ConwayMain))]
public class MouseHandler : MonoBehaviour
{
    private ConwayMain main;
    
    void Start()
    {
        main = transform.GetComponent<ConwayMain>();
    }   

    void Update()
    {

        if (Input.GetMouseButtonUp(0))
        {
            Vector3 mousePos = Input.mousePosition;
            Vector3 worldPosition = Camera.main.ScreenToWorldPoint(mousePos);
            Vector3 gridOriginOffset = main.DisplayOriginVector();

            Vector3 gridMouseCoords = worldPosition - gridOriginOffset;

            int indexX = Mathf.FloorToInt(gridMouseCoords.x + .5f);
            int indexY = Mathf.FloorToInt(gridMouseCoords.y + .5f);
            //Debug.Log("MouseHandler indexX: " + indexX + " indexY: " + indexY);
            
            if (!((indexX >= main.ColumnCount)||(indexY >= main.RowCount)))
                main.ToggleStateAt(indexX,indexY);
        }
    }
}
