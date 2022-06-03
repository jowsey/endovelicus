using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.UI;

public class DragSelect : MonoBehaviour
{
    private Vector3 dragOrigin;
    public Image selectBox;
    public static readonly List<Unit> selectedUnits = new List<Unit>();

    // Update is called once per frame
    private async void Update()
    {
        // Start drag whenever the mouse is pressed
        if(Input.GetMouseButtonDown(0))
        {
            dragOrigin = Input.mousePosition;

            // Unselect any currently-selected units if not holding shift
            if(!Input.GetKey(KeyCode.LeftShift))
            {
                foreach (var unit in selectedUnits.Where(u => u != null))
                {
                    unit.selected = false;
                }

                selectedUnits.Clear();
            }
        }

        // If mouse has been held down for more than 1 frame
        if(Input.GetMouseButton(0) && !Input.GetMouseButtonDown(0))
        {
            // If the mouse has moved since the start of the drag
            if(Input.mousePosition != dragOrigin)
            {
                // Set box to active if it's not already
                if(!selectBox.gameObject.activeSelf) selectBox.gameObject.SetActive(true);

                var sf = UIManager.instance.canvas.scaleFactor;

                var min = new Vector2(Mathf.Min(Input.mousePosition.x, dragOrigin.x), Mathf.Min(Input.mousePosition.y, dragOrigin.y));
                var max = new Vector2(Mathf.Max(Input.mousePosition.x, dragOrigin.x), Mathf.Max(Input.mousePosition.y, dragOrigin.y));

                selectBox.rectTransform.anchoredPosition = min / sf;
                selectBox.rectTransform.sizeDelta = (max - min) / sf;
            }
            else
            {
                // Set box to inactive if it currently is
                if(selectBox.gameObject.activeSelf) selectBox.gameObject.SetActive(false);
            }
        }

        if(Input.GetMouseButtonUp(0))
        {
            // If the box doesn't exist: AKA if we just clicked
            if(!selectBox.gameObject.activeSelf)
            {
                var ray = UIManager.instance.cam.ScreenPointToRay(Input.mousePosition);
                var hit = Physics2D.Raycast(ray.origin, ray.direction);

                if(hit.collider && hit.collider.TryGetComponent<Unit>(out var unit) && unit.ownership == UnitOwnership.Friendly)
                {
                    selectedUnits.Add(unit);
                    unit.selected = true;
                }

                return;
            }

            // Get two opposite corners of the box in world-space
            var position = selectBox.rectTransform.position;
            var size = selectBox.rectTransform.sizeDelta * UIManager.instance.canvas.scaleFactor;

            var c1 = UIManager.instance.cam.ScreenToWorldPoint(new Vector3(position.x, position.y, 0));
            var c2 = UIManager.instance.cam.ScreenToWorldPoint(new Vector3(position.x + size.x, position.y + size.y, 0));


            // Get all objects with PlayerUnit tag within the box
            var units = FindObjectsOfType<Unit>().Where(unit =>
            {
                var pos = unit.transform.position;

                return pos.x >= c1.x && pos.x <= c2.x &&
                       pos.y >= c1.y && pos.y <= c2.y &&
                       unit.ownership == UnitOwnership.Friendly;
            });

            foreach (var unit in units)
            {
                unit.selected = true;
                selectedUnits.Add(unit);
            }

            selectBox.gameObject.SetActive(false);
        }

        if(Input.GetMouseButtonDown(1))
        {
            var intendedPosition = UIManager.instance.cam.ScreenToWorldPoint(Input.mousePosition + new Vector3(0f, 0f, 10f));

            // Don't let player send units within 4 units of the fog
            if(intendedPosition.x > FogManager.instance.fogBeginX - 4f) return;
            
            // Sort units by distance to destination
            var sorted = selectedUnits.Where(u => u != null).OrderBy(u => Vector3.Distance(u.transform.position, intendedPosition)).ToList();

            for (var i = 0; i < sorted.Count; i++)
            {
                var unit = sorted[i];
                unit.mainDestination = intendedPosition;
                unit.orderType = Input.GetKey(KeyCode.LeftControl) ? UnitOrder.DirectMove : UnitOrder.AttackMove;

                // By adding stopping distance based on distance from destination, this means the units closest will remain the closest
                // (staying in their position in the group, stopping them from pushing between eachother to get in front/behind)
                unit.navMeshAgent.stoppingDistance = 1f + 0.25f * i;
                
                await Task.Delay(Random.Range(0, 150));
            }
        }

        if(selectedUnits.Count > 0)
        {
            RelevantControlsList.instance.Add(ControlTips.UnitControls);
        }
        else
        {
            RelevantControlsList.instance.Remove(ControlTips.UnitControls);
        }
    }
}
