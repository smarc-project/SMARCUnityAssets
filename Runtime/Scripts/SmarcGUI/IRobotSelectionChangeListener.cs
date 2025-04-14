using UnityEngine;

namespace SmarcGUI
{
    public interface IRobotSelectionChangeListener
    {
        public void OnRobotSelectionChange(RobotGUI SelectedRobotGUI);
    }
}