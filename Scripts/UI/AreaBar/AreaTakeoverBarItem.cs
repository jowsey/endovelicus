using System;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.UI;

public class AreaTakeoverBarItem : MonoBehaviour
{
    public Area area;
    
    public Image barImage;
    
    private void Start()
    {
        // Create linked UI elements for each village in the area
        foreach (var v in area.villages)
        {
            var villageIcon = Addressables.InstantiateAsync("Prefabs/UI/AreaTakeoverVillageIcon", transform)
                .WaitForCompletion()
                .GetComponent<AreaTakeoverVillageIcon>();
            
            var villageDetails = Addressables.InstantiateAsync("Prefabs/UI/VillageDetailsDisplay", UIManager.instance.villageDetailsCanvas.transform)
                .WaitForCompletion()
                .GetComponent<VillageDetailsDisplay>();
            
            villageIcon.village = villageDetails.village = v;
            v.detailsDisplay = villageDetails;
        }
        
        area.onOwnershipChanged.AddListener(UpdateBar);
        UpdateBar();
    }
    
    private void UpdateBar()
    {
        barImage.color = area.ownership switch
        {
            Ownership.Friendly => Constants.friendlyColour,
            Ownership.Roman => Constants.enemyColour,
            Ownership.Neutral => Constants.neutralColour,
            _ => throw new ArgumentOutOfRangeException()
        };
    }
}
