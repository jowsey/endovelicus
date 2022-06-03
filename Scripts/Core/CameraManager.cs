using System.Linq;
using ElRaccoone.Tweens;
using UnityEngine;

public class CameraManager : MonoBehaviour
{
    public static Camera cam;

    public const float minZoomSize = 3f;
    public const float maxZoomSize = 32f;
    public const float moveSpeed = 3f;
    public const float zoomSpeed = 12f;

    private Vector3 lastMiddleMousePosition;

    private int maxWidth;
    private int maxHeight;

    // Start is called before the first frame update
    private void Start()
    {
        MapManager.instance.onFinishedLoading.AddListener(() =>
        {
            cam = GetComponent<Camera>();

            // Set game bounds
            maxWidth = GameManager.instance.areaCount * GameManager.instance.areaWidth;
            maxHeight = GameManager.instance.areaHeight;

            // Set default zoom 
            transform.TweenValueFloat(Mathf.Lerp(minZoomSize, maxZoomSize, 0.33f), 1f, val => cam.orthographicSize = val)
                .SetFrom(cam.orthographicSize)
                .SetEaseCubicInOut();

            // Gets the center-most village owned by the player
            var startVillage = MapManager.instance.villages
                .Where(v => v.ownership == Ownership.Friendly)
                .OrderBy(v => Mathf.Abs(v.centerPosition.y - GameManager.instance.areaHeight / 2))
                .First();

            transform.TweenPosition(startVillage.centerPosition + new Vector3(0f, 0f, -10f), 1f).SetEaseCubicInOut();
        });
    }

    // Update is called once per frame
    private void Update()
    {
        if (!MapManager.instance.loadingFinished || PauseMenu.isPaused || ConsoleManager.isInConsole) return;

        // Zooming
        // inspired by http://answers.unity.com/answers/1784515/view.html
        var mouseWorldPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        var scroll = Input.GetAxis("Mouse ScrollWheel") * zoomSpeed;

        cam.orthographicSize = Mathf.Clamp(cam.orthographicSize - scroll, minZoomSize, maxZoomSize);

        var changedMouseWorldPosition = cam.ScreenToWorldPoint(Input.mousePosition);
        var mouseDelta = mouseWorldPosition - changedMouseWorldPosition;

        transform.position += new Vector3(
            mouseDelta.x,
            mouseDelta.y,
            0
        );

        // Movedir set by WASD - speed multiplied by how zoomed out the camera is (1x at zoomed in, 2x at zoomed out)
        var moveDir = new Vector2(Input.GetAxisRaw("Horizontal"), Input.GetAxisRaw("Vertical")) *
                      (cam.orthographicSize + minZoomSize / (maxZoomSize + minZoomSize) + 1f);

        transform.Translate(moveDir * (moveSpeed * Time.unscaledDeltaTime));

        // Pan camera with middle-mouse if it's dragged
        if (Input.GetMouseButtonDown(2))
        {
            lastMiddleMousePosition = cam.ScreenToWorldPoint(Input.mousePosition);
        }
        else if (Input.GetMouseButton(2))
        {
            var direction = lastMiddleMousePosition - cam.ScreenToWorldPoint(Input.mousePosition);
            cam.transform.position += direction;
        }
    }

    private void LateUpdate()
    {
        if (!MapManager.instance.loadingFinished || PauseMenu.isPaused || ConsoleManager.isInConsole) return;

        // Restrict camera to game area
        var camPos = transform.position;
        camPos.x = Mathf.Clamp(camPos.x, 0, maxWidth);
        camPos.y = Mathf.Clamp(camPos.y, 0, maxHeight);
        transform.position = camPos;
    }
}
