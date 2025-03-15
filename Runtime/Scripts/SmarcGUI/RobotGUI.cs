using System.Collections.Generic;
using System.Linq;
using GeoRef;
using SmarcGUI.Connections;
using SmarcGUI.MissionPlanning;
using SmarcGUI.MissionPlanning.Tasks;
using SmarcGUI.MissionPlanning.Params;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using SmarcGUI.KeyboardControllers;
using DefaultNamespace;
using SmarcGUI.WorldSpace;


namespace SmarcGUI
{
    public enum InfoSource
    {
        SIM,
        MQTT,
        ROS
    }


    public class RobotGUI : MonoBehaviour, IPointerClickHandler, IPointerExitHandler, IPointerEnterHandler
    {
        [Header("Params")]
        [Tooltip("Time in seconds before the robot is considered old")]
        public float OldnessTime = 10;

        [Header("UI Elements")]
        public RectTransform HighlightRT;
        public RectTransform SelectedHighlightRT;
        public RectTransform HeartRT;
        public TMP_Text RobotNameText;
        public TMP_Text InfoSourceText;
        public TMP_Dropdown TasksAvailableDropdown;
        public Button AddTaskButton;
        public RectTransform AvailTasksPanelRT;
        public Toggle UserInputToggle;
        public string WorldMarkerName = "WorldMarkers";
        public RectTransform ExecutingTasksScrollContent;
        public RectTransform ExecTasksPanelRT;

        [Header("Prefabs")]
        public GameObject ContextMenuPrefab;
        public GameObject ExecutingTaskPrefab;
        public GameObject RobotGUIOverlayPrefab;

        [Header("Ghost Prefabs")]
        public GameObject GenericGhostPrefab;
        public GameObject SAMGhostPrefab;
        public GameObject EvoloGhostPrefab;


        Transform worldMarkersTF;
        Transform ghostTF;
        Rigidbody ghostRB;
        GameObject simRobotGO;
        Transform simRobotBaseLinkTF;


        public InfoSource InfoSource{get; private set;}
        WaspDirectExecutionInfoMsg directExecutionInfo;
        List<TaskSpec> tasksAvailable => directExecutionInfo.TasksAvailable;
        public List<string> TasksAvailableNames = new();
        public HashSet<string> TasksExecutingUuids = new();

        public bool TSTExecInfoReceived = false;

        public string RobotName => RobotNameText.text;
        string robotNamespace;

        public bool IsSelected{get; private set;}
        GUIState guiState;
        MQTTClientGUI mqttClient;
        GlobalReferencePoint globalReferencePoint;
        MissionPlanStore missionPlanStore;
        RectTransform rt;
        float minHeight;
        float lastHeartbeatTime = -1;
        Color originalColor;
        Image BGImage;
        bool isOld = false;
        GameObject robotOverlayGO;
        KeyboardControllerBase keyboardController;

        void Awake()
        {
            guiState = FindFirstObjectByType<GUIState>();
            mqttClient = FindFirstObjectByType<MQTTClientGUI>();
            missionPlanStore = FindFirstObjectByType<MissionPlanStore>();
            worldMarkersTF = GameObject.Find(WorldMarkerName).transform;
            globalReferencePoint = FindFirstObjectByType<GlobalReferencePoint>();
            AddTaskButton.onClick.AddListener(() => OnTaskAdded(TasksAvailableDropdown.value));
            rt = GetComponent<RectTransform>();
            minHeight = rt.sizeDelta.y;
            AvailTasksPanelRT.gameObject.SetActive(false);
            ExecTasksPanelRT.gameObject.SetActive(false);
            UserInputToggle.gameObject.SetActive(false);
            BGImage = GetComponent<Image>();
            originalColor = BGImage.color;
        }


        public void SetRobot(string robotname, InfoSource infoSource, string robotNamespace)
        {
            InfoSource = infoSource;
            this.robotNamespace = robotNamespace;

            RobotNameText.text = robotname;
            InfoSourceText.text = $"({infoSource})";

            if(infoSource == InfoSource.SIM)
            {
                HeartRT.gameObject.SetActive(false);
                UserInputToggle.gameObject.SetActive(true);
                simRobotGO = GameObject.Find(robotname);
                simRobotBaseLinkTF = Utils.FindDeepChildWithName(simRobotGO, "base_link").transform;
                keyboardController = simRobotGO.GetComponent<KeyboardControllerBase>();
            }

            if(infoSource == InfoSource.MQTT) 
            {
                mqttClient.SubToTopic(robotNamespace+"tst_execution_info");
                mqttClient.SubToTopic(robotNamespace+"direct_execution_info");
                mqttClient.SubToTopic(robotNamespace+"sensor_info");
                mqttClient.SubToTopic(robotNamespace+"sensor/position");
                mqttClient.SubToTopic(robotNamespace+"sensor/heading");
                mqttClient.SubToTopic(robotNamespace+"sensor/course");
                mqttClient.SubToTopic(robotNamespace+"sensor/speed");
                AvailTasksPanelRT.gameObject.SetActive(true);
                ExecTasksPanelRT.gameObject.SetActive(true);
                rt.sizeDelta = new Vector2(rt.sizeDelta.x, minHeight + AvailTasksPanelRT.sizeDelta.y + ExecTasksPanelRT.sizeDelta.y);
                HeartRT.gameObject.SetActive(true);
            }

            if(infoSource != InfoSource.SIM && worldMarkersTF != null)
            {
                if(robotname.ToLower().Contains("sam")) ghostTF = Instantiate(SAMGhostPrefab).transform;
                if(robotname.ToLower().Contains("evolo")) ghostTF = Instantiate(EvoloGhostPrefab).transform;
                else ghostTF = Instantiate(GenericGhostPrefab).transform;
                ghostTF.name = $"{robotname} (Ghost)";
                ghostTF.SetParent(worldMarkersTF);
                ghostTF.gameObject.SetActive(false);
                ghostRB = ghostTF.GetComponent<Rigidbody>();

                robotOverlayGO = Instantiate(RobotGUIOverlayPrefab);
                robotOverlayGO.name = $"{robotname}_Overlay";
                var robotOverlay = robotOverlayGO.GetComponent<RobotGUIOverlay>();
                robotOverlay.SetRobot(ghostTF, infoSource);
                robotOverlay.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
            }
        }
        

        /////////////////////////////////////////
        // MQTT STUFF
        /////////////////////////////////////////
        public void SendPing()
        {
            var pingCommand = new PingCommand();
            switch(InfoSource)
            {
                case InfoSource.SIM:
                    guiState.Log($"Ping! -> {RobotName} in SIM");
                    break;
                case InfoSource.MQTT:
                    mqttClient.Publish(robotNamespace+"exec/command", pingCommand.ToJson());
                    break;
                case InfoSource.ROS:
                    guiState.Log($"Ping! -> {RobotName} in ROS");
                    break;
            }
        }

        public void SendSignalCommand(string taskUuid, string signal)
        {
            var signalCommand = new SigntalTaskCommand(taskUuid, signal);
            switch(InfoSource)
            {
                case InfoSource.SIM:
                    guiState.Log($"Sending signal {signal} to {RobotName} in SIM");
                    break;
                case InfoSource.MQTT:
                    mqttClient.Publish(robotNamespace+"exec/command", signalCommand.ToJson());
                    break;
                case InfoSource.ROS:
                    guiState.Log($"Sending signal {signal} to {RobotName} in ROS");
                    break;
            }   
        }

        public StartTaskCommand SendStartTaskCommand(Task task)
        {
            var startTaskCommand = new StartTaskCommand(task, RobotName);
            switch(InfoSource)
            {
                case InfoSource.SIM:
                    guiState.Log($"Sending StartTaskCommand {task} to {RobotName} in SIM");
                    break;
                case InfoSource.MQTT:
                    mqttClient.Publish(robotNamespace+"exec/command", startTaskCommand.ToJson());
                    break;
                case InfoSource.ROS:
                    guiState.Log($"Sending StartTaskCommand {task} to {RobotName} in ROS");
                    break;
            }   
            return startTaskCommand;
        }

        public StartTSTCommand SendStartTSTCommand(TaskSpecTree tst)
        {
            var startTSTCommand = new StartTSTCommand(tst, RobotName);
            switch(InfoSource)
            {
                case InfoSource.SIM:
                    guiState.Log($"Sending StartTSTCommand {tst} to {RobotName} in SIM");
                    break;
                case InfoSource.MQTT:
                    mqttClient.Publish(robotNamespace+"exec/command", startTSTCommand.ToJson());
                    break;
                case InfoSource.ROS:
                    guiState.Log($"Sending StartTSTCommand {tst} to {RobotName} in ROS");
                    break;
            }   
            return startTSTCommand;
        }

        public void OnHeartbeatReceived()
        {
            HeartRT.localScale = new Vector3(1.5f, 1.5f, 1.5f);
            lastHeartbeatTime = Time.time;
        }

        public void OnSensorInfoReceived(WaspSensorInfoMsg msg)
        {
            return;
        }


        void UpdateTasksDropdown(WaspDirectExecutionInfoMsg msg)
        {
            var tasksAvailable = msg.TasksAvailable;
            TasksAvailableDropdown.options.Clear();
            foreach (TaskSpec taskSpec in tasksAvailable)
            {
                TasksAvailableDropdown.options.Add(new TMP_Dropdown.OptionData() { text = taskSpec.Name });
            }
            TasksAvailableDropdown.RefreshShownValue();
        }

        void UpdateExecutingTasks(WaspDirectExecutionInfoMsg msg)
        {
            // bunch of loops here, but usually the item count is < 3 for all of them
            var newTasks = msg.TasksExecuting;

            HashSet<string> newUuids = new();
            foreach (var task in newTasks)
            {
                var taskUuid = task["uuid"];
                newUuids.Add(taskUuid);
            }

            // old tasks that arent executing anymore
            var outdatedUuids = TasksExecutingUuids.Except(newUuids);
            // new tasks that are now executing, that werent before
            var newUuidsSet = newUuids.Except(TasksExecutingUuids);

            // nuke outdated tasks
            foreach (var taskUuid in outdatedUuids)
            {
                var index = TasksExecutingUuids.ToList().IndexOf(taskUuid);
                Destroy(ExecutingTasksScrollContent.GetChild(index).gameObject);
            }

            // create new ones
            foreach (var taskUuid in newUuidsSet)
            {
                var task = newTasks.Find(t => t["uuid"] == taskUuid);
                var taskName = task["name"];
                var execTaskGO = Instantiate(ExecutingTaskPrefab, ExecutingTasksScrollContent);
                var execTaskGUI = execTaskGO.GetComponent<ExecutingTaskGUI>();
                var taskSpec = msg.TasksAvailable.Find(t => t.Name == taskName);
                List<string> signals = new();
                if(taskSpec != null) signals = new List<string>(taskSpec.Signals);
                execTaskGUI.SetExecTask(this, taskName, taskUuid, signals);
            }

            TasksExecutingUuids = newUuids;
        }

        public void OnDirectExecutionInfoReceived(WaspDirectExecutionInfoMsg msg)
        {
            UpdateTasksDropdown(msg);
            UpdateExecutingTasks(msg);
            directExecutionInfo = msg;

            TasksAvailableNames = msg.TasksAvailable.Select(t => t.Name).ToList();
        }

        public void OnTSTExecutionInfoReceived(WaspTSTExecutionInfoMsg msg)
        {
            // the information about the executing TST is a different message sent
            // as a reply to the start-command. so this is literally just
            // a "yes we can start"...
            TSTExecInfoReceived = true;
            return;
        }


        public void OnPositionReceived(GeoPoint pos)
        {
            if(globalReferencePoint == null) return;
            if(ghostTF == null) return;
            if(ghostTF.gameObject.activeSelf == false) ghostTF.gameObject.SetActive(true);
            var (x,z) = globalReferencePoint.GetUnityXZFromLatLon(pos.latitude, pos.longitude);
            ghostTF.position = new Vector3(x, pos.altitude, z);
        }

        public void OnHeadingReceived(float heading)
        {
            ghostTF.rotation = Quaternion.Euler(0, heading, 0);
        }

        public void OnCourseReceived(float course)
        {
            var speed = ghostRB.linearVelocity.magnitude;
            // waraps really isnt made for things that move in 3D space, so we'll just set the velocity in the xz plane...
            ghostRB.linearVelocity = speed * new Vector3(Mathf.Sin(course * Mathf.Deg2Rad), 0, Mathf.Cos(course * Mathf.Deg2Rad));
        }

        public void OnSpeedReceived(float speed)
        {
            if(ghostRB.linearVelocity.sqrMagnitude == 0) ghostRB.linearVelocity = ghostRB.transform.forward * speed;
            else ghostRB.linearVelocity = ghostRB.linearVelocity.normalized * speed;
        }



        /////////////////////////////////////////
        // GUI STUFF
        /////////////////////////////////////////
        void OnKBControl()
        {
            IsSelected = true;
            OnSelectedChange(true);
            keyboardController.Enable();
        }


        public void OnPointerClick(PointerEventData eventData)
        {
            if(eventData.button == PointerEventData.InputButton.Right)
            {
                var contextMenuGO = Instantiate(ContextMenuPrefab);
                var contextMenu = contextMenuGO.GetComponent<RobotContextMenu>();
                contextMenu.SetItem(eventData.position, this);
            }

            if(eventData.button == PointerEventData.InputButton.Left)
            {
                IsSelected = !IsSelected;
                OnSelectedChange(true);
            }
        }

        void OnSelectedChange(bool notify = false)
        {
            SelectedHighlightRT?.gameObject.SetActive(IsSelected);
            if(notify) guiState.OnRobotSelectionChanged(this);
        }

        public void Deselect()
        {
            IsSelected = false;
            OnSelectedChange();
            keyboardController?.Disable();
        }

        void OnTaskAdded(int index)
        {
            var taskSpec = tasksAvailable[index];
            missionPlanStore.SelectedTSTGUI?.OnTaskAdded(taskSpec);
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            HighlightRT?.gameObject.SetActive(false);
        }

        public void OnPointerEnter(PointerEventData eventData)
        {
            HighlightRT?.gameObject.SetActive(true);
        }

        public void LookAtRobot()
        {
            Transform tf;
            if(InfoSource != InfoSource.SIM)
            {
                if(ghostTF == null) return;
                tf = ghostTF;
                
            }
            else
            {
                if(simRobotBaseLinkTF == null) return;
                tf = simRobotBaseLinkTF;
            }
            guiState.SelectDefaultCamera();
            var cam = guiState.CurrentCam;
            cam.transform.position = tf.position + new Vector3(0, 10, 0);
            cam.transform.LookAt(tf);
        }

        public void FollowRobot()
        {
            Transform tf;
            if(InfoSource != InfoSource.SIM)
            {
                if(ghostTF == null) return;
                tf = ghostTF;
            }
            else
            {
                if(simRobotBaseLinkTF == null) return;
                tf = simRobotBaseLinkTF;
            }
            guiState.SelectDefaultCamera();
            var cam = guiState.CurrentCam;
            cam.GetComponent<SmoothFollow>().target = tf;
        }


        public void OnGUI()
        {
            AddTaskButton.interactable = missionPlanStore.SelectedTSTGUI != null;
            if(keyboardController != null) keyboardController.enabled = UserInputToggle.isOn && InfoSource == InfoSource.SIM;

            
            if(InfoSource != InfoSource.SIM && lastHeartbeatTime > 0)
            {
                HeartRT.localScale = Vector3.Lerp(HeartRT.localScale, Vector3.one, Time.deltaTime * 10);
                isOld = Time.time - lastHeartbeatTime > OldnessTime;
                AddTaskButton.interactable = !isOld;
                TasksAvailableDropdown.interactable = !isOld;
                BGImage.color = isOld ? Color.yellow : originalColor;
            }

            if(isOld)
            {
                guiState.Log($"Robot {RobotName} is old!");
                TSTExecInfoReceived = false;
            }
        }

    }
}