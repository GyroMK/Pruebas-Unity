using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;
using System.Collections;
using System.Text.RegularExpressions;

public class WorkshopItemImage : MonoBehaviour
{
    public string itemID;
    public Image itemImage;

    void Start()
    {
        if (string.IsNullOrEmpty(itemID))
        {
            Debug.LogError("La ID del ítem no está establecida.");
            return;
        }

        if (itemImage == null)
        {
            Debug.LogError("El componente de imagen no está asignado.");
            return;
        }

        itemImage.preserveAspect = true;

        StartCoroutine(LoadItemImageFromDescription(itemID));
    }

    IEnumerator LoadItemImageFromDescription(string itemID)
    {
        string url = $"https://steamcommunity.com/sharedfiles/filedetails/?id={itemID}";
        string html = "";

        using (UnityWebRequest www = UnityWebRequest.Get(url))
        {
            yield return www.SendWebRequest();

            if (www.result == UnityWebRequest.Result.Success)
            {
                html = www.downloadHandler.text;
            }
            else
            {
                Debug.LogError("Error al obtener la página del ítem: " + www.error);
                yield break;
            }
        }

        // Expresión regular para encontrar la URL de la imagen en la descripción
        string pattern = @"<img .*?src=""(https://steamuserimages-a.akamaihd.net/ugc/.*?)""";
        var match = Regex.Match(html, pattern);

        if (match.Success)
        {
            string imageUrl = match.Groups[1].Value;
            StartCoroutine(LoadImage(imageUrl));
        }
        else
        {
            Debug.LogError("No se pudo obtener la URL de la imagen de la descripción para el ítem con ID: " + itemID);
        }
    }

    IEnumerator LoadImage(string imageUrl)
    {
        UnityWebRequest www = UnityWebRequestTexture.GetTexture(imageUrl);
        yield return www.SendWebRequest();

        if (www.result == UnityWebRequest.Result.Success)
        {
            Texture2D texture = DownloadHandlerTexture.GetContent(www);
            itemImage.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), new Vector2(0.5f, 0.5f));
        }
        else
        {
            Debug.LogError("Error al cargar la imagen: " + www.error);
        }
    }
}
