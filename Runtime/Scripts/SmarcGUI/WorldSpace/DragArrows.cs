using UnityEngine;

namespace SmarcGUI.WorldSpace
{
    public class DragArrows : MonoBehaviour
    {
        [Header("Arrows")]
        public Transform PX;
        public Transform NX, PY, NY, PZ, NZ;

        Transform[] arrows;
        Vector3[] basePositions;

        void Awake()
        {
            arrows = new Transform[] { PX, NX, PY, NY, PZ, NZ };
            basePositions = new Vector3[arrows.Length];
            for (int i = 0; i < arrows.Length; i++)
            {
                basePositions[i] = arrows[i].localPosition;
            }
        }

        public void SetArrowSize(float len, float width)
        {
            foreach (var arrow in arrows)
            {
                arrow.localScale = new Vector3(width, width, len);
            }
        }

        public void SetInnerCube(float side)
        {
            for(int i=0; i<arrows.Length; i++)
            {
                arrows[i].localPosition = basePositions[i] + new Vector3(0, 0, side / 2);
            }
        }
        
    }
}