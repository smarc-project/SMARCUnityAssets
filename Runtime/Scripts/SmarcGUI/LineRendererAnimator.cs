using System.Collections.Generic;
using UnityEngine;


namespace SmarcGUI
{
    public class LineRendererAnimator : MonoBehaviour
    {
        LineRenderer lineRendererToChange;
        public float speed = 1;
        float movementPerTick => speed * 0.001f;
        public bool animate = true;

        void Start()
        {
            lineRendererToChange = GetComponent<LineRenderer>();
            lineRendererToChange.colorGradient = AddInitialCopy(lineRendererToChange.colorGradient);
        }

        //Source: https://pastebin.com/72vt01HE

        //returns the gradient with a copy of the first key for intersection purposes.
        Gradient AddInitialCopy(Gradient incomingGradient)
        {
            List<GradientColorKey> newColorKeys = new List<GradientColorKey>(incomingGradient.colorKeys);
            Color interSectionColor = newColorKeys[0].color;
            newColorKeys.Insert(0,new GradientColorKey(interSectionColor,0));
            Gradient newInitGradient = new Gradient();
            newInitGradient.colorKeys = newColorKeys.ToArray();
            return newInitGradient;
        }

        //remove first and last keys since they dont shift.
        List<GradientColorKey> RemoveFirstAndLast(Gradient incomingGradient)
        {
            List<GradientColorKey> currentColorKeys = new List<GradientColorKey>(incomingGradient.colorKeys);
            currentColorKeys.RemoveAt(currentColorKeys.Count-1);
            currentColorKeys.RemoveAt(0);
            return currentColorKeys;
        }
    
        Color GetIntersectionColor(List<GradientColorKey> incomingKeys, int lowestIndex, int highestIndex)
        {
            Color firstColor = incomingKeys[lowestIndex].color;
            Color lastColor = incomingKeys[highestIndex].color;
            float distance = 1 - (incomingKeys[highestIndex].time - incomingKeys[lowestIndex].time);
            float colorLerpAmount = (1f-incomingKeys[highestIndex].time) / distance;;
            Color newIntersectionColor = Color.Lerp(lastColor,firstColor,colorLerpAmount);
            return newIntersectionColor;
        }

        void OnGUI()
        {
            if(!animate) return;
            List<GradientColorKey> currentColorKeys = RemoveFirstAndLast(lineRendererToChange.colorGradient);
            float highestTime=0;
            float lowestTime=1;
            int highestIndex = currentColorKeys.Count-1;
            int lowestIndex = 0;
            //Move all inner ones.
            for(int i = 0 ;i<currentColorKeys.Count;i++)
            {
                GradientColorKey tempColorKey = currentColorKeys[i];
                float newTime = tempColorKey.time + movementPerTick;
                
                if(newTime>1)
                {
                    newTime = newTime-1;
                }
                tempColorKey.time = newTime;
                currentColorKeys[i] = tempColorKey;
                if(newTime<lowestTime)
                {
                    lowestTime = newTime;
                    lowestIndex = i;
                }
                if(newTime>highestTime)
                {
                    highestTime = newTime;
                    highestIndex = i;
                }
            }
            Color newIntersectionColor = GetIntersectionColor(currentColorKeys,lowestIndex,highestIndex);
            currentColorKeys.Insert(0,new GradientColorKey(newIntersectionColor,0));
            currentColorKeys.Add(new GradientColorKey(newIntersectionColor,1));
            Gradient tempGradient = lineRendererToChange.colorGradient;
            tempGradient.colorKeys = currentColorKeys.ToArray();
            lineRendererToChange.colorGradient = tempGradient;  
        }
    }
}


