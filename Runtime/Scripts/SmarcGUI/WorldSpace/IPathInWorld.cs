using System.Collections.Generic;
using UnityEngine;

namespace SmarcGUI.WorldSpace
{
    public interface IPathInWorld
    {
        List<Vector3> GetWorldPath();
    }
}