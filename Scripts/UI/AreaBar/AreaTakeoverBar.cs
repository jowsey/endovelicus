using UnityEngine;
using UnityEngine.AddressableAssets;

public class AreaTakeoverBar : MonoBehaviour
{
    // Start is called before the first frame update
    private void Start()
    {
        MapManager.instance.onFinishedLoading.AddListener(() =>
        {
            for (var i = 0; i < GameManager.instance.areaCount; i++)
            {
                var item = Addressables.InstantiateAsync("Prefabs/UI/AreaTakeoverBarItem", transform)
                    .WaitForCompletion()
                    .GetComponent<AreaTakeoverBarItem>();

                item.area = MapManager.instance.areas[i];
            }
        });
    }
}
