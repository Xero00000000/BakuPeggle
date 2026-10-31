using UnityEngine;

[CreateAssetMenu(fileName = "NewMarbeast", menuName = "Shop/Marbeast")]
public class MarbeastItem : ScriptableObject
{
    public string marbeastId;          // unique, e.g. "bomb"
    public string displayName;
    public int price;
    public Sprite icon;
    [Tooltip("Index used by PegLauncher.ChangeBall: 0 default, 1 other, 2 clone, 3 bomb")]
    public int ballIndex;
    public bool unlockedByDefault;     // tick for the normal ball
}