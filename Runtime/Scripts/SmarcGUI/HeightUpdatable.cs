using UnityEngine;

namespace SmarcGUI
{
    public class HeightUpdatable : MonoBehaviour, IHeightUpdatable
    {
        [Tooltip("If set, instead of using the height of the children, the height will be set contain the children of this object")]
        public Transform ParentObject;
        RectTransform rt;

        void Awake()
        {
            rt = GetComponent<RectTransform>();
            UpdateHeight();
        }

        public void UpdateHeight()
        {
            float selfHeight = 5;
            var parent = ParentObject != null ? ParentObject : transform;
            foreach(Transform child in parent)
            {
                if(child.gameObject.activeSelf)
                    selfHeight += child.GetComponent<RectTransform>().sizeDelta.y;
            }
            
            rt.sizeDelta = new Vector2(rt.sizeDelta.x, selfHeight);
        }

    }
}