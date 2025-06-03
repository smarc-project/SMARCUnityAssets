using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;



namespace SmarcGUI.Connections
{
    public class ExecutingTaskGUI : MonoBehaviour, IPointerExitHandler, IPointerEnterHandler
    {
        [Header("UI Elements")]
        public TMP_Text TaskName;
        public TMP_Text TaskDescription;
        public TMP_Dropdown TaskSignalsDropdown;
        public Button SignalButton;
        public RectTransform HighlightRT;

        RobotGUI robotgui;
        string taskUuid;

        public void SetExecTask(RobotGUI robotgui, string taskName, string taskDesc, string taskUuid, List<string> signals)
        {
            this.robotgui = robotgui;
            TaskName.text = taskName;
            this.taskUuid = taskUuid;
            TaskDescription.text = taskDesc;

            if (signals.Count > 0)
            {
                TaskSignalsDropdown.ClearOptions();
                TaskSignalsDropdown.AddOptions(signals);
                SignalButton.onClick.AddListener(OnSignalButton);
            }
            else
            {
                TaskSignalsDropdown.gameObject.SetActive(false);
                SignalButton.gameObject.SetActive(false);
            }
        }

        void OnSignalButton()
        {
            robotgui.SendSignalTaskCommand(taskUuid, TaskSignalsDropdown.options[TaskSignalsDropdown.value].text);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HighlightRT.gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            HighlightRT.gameObject.SetActive(true);
        }
    }
}