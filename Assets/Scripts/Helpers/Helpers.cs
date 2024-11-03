using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;

namespace Helpers
{
    public static class Helpers
    {

        // Is Mouse over a UI Element? Used for ignoring World clicks through UI
        public static bool IsPointerOverUI()
        {
            if (EventSystem.current.IsPointerOverGameObject())
            {
                return true;
            }
            else
            {
                PointerEventData pe = new PointerEventData(EventSystem.current);
                pe.position = Input.mousePosition;
                List<RaycastResult> hits = new List<RaycastResult>();
                EventSystem.current.RaycastAll(pe, hits);
                return hits.Count > 0;
            }
        }

        public static Vector3 ApplyRotationToVectorXZ(Vector3 vec, float angle)
        {
            return Quaternion.Euler(0, angle, 0) * vec;
        }


        // Create Text in the World
        public static TextMesh CreateWorldText(Transform parent, string text, Vector3 localPosition, int fontSize, Color color, TextAnchor textAnchor, TextAlignment textAlignment, int sortingOrder)
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMesh));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            TextMesh textMesh = gameObject.GetComponent<TextMesh>();
            textMesh.anchor = textAnchor;
            textMesh.alignment = textAlignment;
            textMesh.text = text;
            textMesh.fontSize = fontSize;
            textMesh.color = color;
            textMesh.GetComponent<MeshRenderer>().sortingOrder = sortingOrder;
            return textMesh;
        }

        public static TextMeshPro CreateWorldTextPro(Transform parent, string text, Vector3 localPosition, float fontSize, Color color)
        {
            GameObject gameObject = new GameObject("World_Text", typeof(TextMeshPro));
            Transform transform = gameObject.transform;
            transform.SetParent(parent, false);
            transform.localPosition = localPosition;
            TextMeshPro textMesh = gameObject.GetComponent<TextMeshPro>();
            textMesh.verticalAlignment = VerticalAlignmentOptions.Middle;
            textMesh.alignment = TextAlignmentOptions.Center;
            textMesh.rectTransform.sizeDelta = new Vector2(fontSize, fontSize);
            textMesh.rectTransform.rotation = Quaternion.Euler(Vector3.right * 90f);
            textMesh.text = text;
            textMesh.fontSizeMin = fontSize;
            textMesh.enableAutoSizing = true;
            textMesh.color = color;
            //textMesh.enableWordWrapping = false;
            return textMesh;
        }
    
    }
}
