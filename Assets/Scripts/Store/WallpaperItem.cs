using UnityEngine;

[CreateAssetMenu(fileName = "NewWallpaper", menuName = "Tienda/Wallpaper Item")]
public class WallpaperItem : ScriptableObject
{
    public string wallpaperId;      
    public Sprite wallpaperSprite; 
    public int price;              
}