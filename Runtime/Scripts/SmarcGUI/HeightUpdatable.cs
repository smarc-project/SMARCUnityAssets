using UnityEngine;

namespace SmarcGUI
{
    public class HeightUpdatable : MonoBehaviour, IHeightUpdatable
    {
        RectTransform rt;

        void Awake()
        {
            rt = GetComponent<RectTransform>();
            UpdateHeight();
        }

        public void UpdateHeight()
        {
            float selfHeight = 5;
            foreach(Transform child in transform)
                selfHeight += child.GetComponent<RectTransform>().sizeDelta.y;
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, selfHeight);
        }

    }
}