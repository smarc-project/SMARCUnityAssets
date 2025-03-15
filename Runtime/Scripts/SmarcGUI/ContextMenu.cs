using UnityEngine;
using UnityEngine.EventSystems;


namespace SmarcGUI
{
    public class ContextMenu : MonoBehaviour, IPointerExitHandler
    {
        RectTransform rt;
        Canvas canvas;

        void Awake()
        {
            rt = GetComponent<RectTransform>();
            canvas = GameObject.Find("Canvas-Over").GetComponent<Canvas>();
        }

        protected void SetOnTop(Vector2 position)
        {
            rt.SetParent(canvas.transform, false);
            rt.position = position;
            rt.SetAsLastSibling();
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            Destroy(gameObject);
        }

    }
}
