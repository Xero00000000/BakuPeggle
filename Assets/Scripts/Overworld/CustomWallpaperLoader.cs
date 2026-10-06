using System;
using System.IO;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class CustomWallpaperLoader : MonoBehaviour
{
    [Header("Referencia a la imagen de fondo")]
    [SerializeField] private Image wallpaperImage;

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    public class OpenFileName
    {
        public int structSize = 0;
        public IntPtr dlgOwner = IntPtr.Zero;
        public IntPtr instance = IntPtr.Zero;
        public string filter = null;
        public string customFilter = null;
        public int maxCustFilter = 0;
        public int filterIndex = 0;
        public string file = null;
        public int maxFile = 0;
        public string fileTitle = null;
        public int maxFileTitle = 0;
        public string initialDir = null;
        public string title = null;
        public int flags = 0;
        public short fileOffset = 0;
        public short fileExtension = 0;
        public string defExt = null;
        public IntPtr custData = IntPtr.Zero;
        public IntPtr hook = IntPtr.Zero;
        public string templateName = null;
        public IntPtr reservedPtr = IntPtr.Zero;
        public int reservedInt = 0;
        public int flagsEx = 0;
    }

    [DllImport("comdlg32.dll", SetLastError = true, CharSet = CharSet.Auto)]
    private static extern bool GetOpenFileName([In, Out] OpenFileName ofn);

    public void OpenFileBrowserAndSetWallpaper()
    {
        Debug.Log("--- BOTÓN CLICKEADO ---");
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
        OpenFileName ofn = new OpenFileName();
        ofn.structSize = Marshal.SizeOf(ofn);
        ofn.filter = "Imagenes (*.png;*.jpg;*.jpeg)\0*.png;*.jpg;*.jpeg\0Todos los archivos (*.*)\0*.*\0";
        ofn.file = new string(new char[256]);
        ofn.maxFile = ofn.file.Length;
        ofn.fileTitle = new string(new char[64]);
        ofn.maxFileTitle = ofn.fileTitle.Length;
        ofn.initialDir = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
        ofn.title = "Selecciona un fondo de pantalla";
        ofn.flags = 0x00080000 | 0x00001000 | 0x00000800 | 0x00000200 | 0x00000008;

        if (GetOpenFileName(ofn))
        {
            ApplyWallpaper(ofn.file);
        }
#else
        Debug.LogWarning("El explorador de archivos nativo solo está disponible para Windows.");
#endif
    }

    private void ApplyWallpaper(string path)
    {
        if (string.IsNullOrEmpty(path) || !File.Exists(path)) return;

        byte[] fileData = File.ReadAllBytes(path);

        Texture2D texture = new Texture2D(2, 2);
        if (texture.LoadImage(fileData))
        {
            Sprite newSprite = Sprite.Create(
                texture,
                new Rect(0, 0, texture.width, texture.height),
                new Vector2(0.5f, 0.5f)
            );

            if (wallpaperImage != null)
            {
                wallpaperImage.sprite = newSprite;
            }
        }
    }

}