using UnityEngine;
using System.Collections;

public class UIDrag : UIEventTrigger {

    public GameObject dragWindow;
    
    Vector3 offSet;

	public void OnDragStart()
    {
        offSet = 
            dragWindow.transform.position - UICamera.mainCamera.ScreenToWorldPoint(Input.mousePosition);
    }
    public void OnDrag(Vector2 delta)
    {
        dragWindow.transform.position =
            UICamera.mainCamera.ScreenToWorldPoint(Input.mousePosition) + offSet;
    }
}
