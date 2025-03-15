using UnityEngine;
using TMPro;
using System.Collections.Generic;

using System.IO;
using System;
using Newtonsoft.Json;
using UnityEngine.UI;
using System.Collections;
using SmarcGUI.MissionPlanning.Tasks;
using SmarcGUI.MissionPlanning.Params;
using SmarcGUI.Connections;


namespace SmarcGUI.MissionPlanning
{
    [RequireComponent(typeof(GUIState))]
    public class MissionPlanStore : MonoBehaviour
    {
        GUIState guiState;

        [Tooltip("Path to store mission plans")]
        public string MissionStoragePath;
        public List<TaskSpecTree> MissionPlans = new();

        [Header("Misison GUI Elements")]
        public Transform MissionsScrollContent;
        public Button NewMissionPlanButton;
        public Button LoadMissionsButton;
        public Button SaveMissionsButton;
        

        [Header("Mission Control Elements")]
        public Button RunMissionButton;


        [Header("Tasks GUI Elements")]
        public Transform TasksScrollContent;
        public TMP_Dropdown TaskTypeDropdown;
        public Button AddTaskButton;
        public List<string> BasicTaskTypes = new(){"move-to", "move-path", "custom"};


        [Header("Prefabs")]
        public GameObject TSTPrefab;
        public GameObject TaskPrefab;
        public GameObject PrimitiveParamPrefab;
        public GameObject GeoPointParamPrefab;
        public GameObject ListParamPrefab;

        [Header("State of mission planning GUI")]
        public TSTGUI SelectedTSTGUI;


        Image RunMissionButtonImage;
        Color RunMissionButtonOriginalColor;
        TMP_Text RunMissionButtonText;



        void Awake()
        {
            guiState = GetComponent<GUIState>();
            // Desktop on win, user home on linux/mac
            MissionStoragePath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Personal), Path.Combine("SMaRCUnity", "MissionPlans"));
            Directory.CreateDirectory(MissionStoragePath);
            LoadMissionPlans();

            NewMissionPlanButton.onClick.AddListener(OnNewTST);
            LoadMissionsButton.onClick.AddListener(LoadMissionPlans);
            SaveMissionsButton.onClick.AddListener(SaveMissionPlans);
            RunMissionButton.onClick.AddListener(() => guiState.SelectedRobotGUI.SendStartTSTCommand(SelectedTSTGUI.tst));
            AddTaskButton.onClick.AddListener(() => SelectedTSTGUI.OnTaskAdded(new TaskSpec(TaskTypeDropdown.options[TaskTypeDropdown.value].text, null)));

            TaskTypeDropdown.ClearOptions();
            TaskTypeDropdown.AddOptions(BasicTaskTypes);

            RunMissionButtonImage = RunMissionButton.GetComponent<Image>();
            RunMissionButtonText = RunMissionButton.GetComponentInChildren<TMP_Text>();
            RunMissionButtonOriginalColor = RunMissionButtonImage.color;
        }

        void OnGUI()
        {
            RunMissionButton.interactable = SelectedTSTGUI != null &&
                                            guiState.SelectedRobotGUI != null &&
                                            guiState.SelectedRobotGUI.TSTExecInfoReceived;
            AddTaskButton.interactable = SelectedTSTGUI != null;
        }


        void LoadMissionPlans()
        {
            var existingPlans = new Dictionary<string, TaskSpecTree>();
            foreach(var plan in MissionPlans)
            {
                existingPlans[plan.GetKey()] = plan;
            }

            var i=0;
            foreach (var file in Directory.GetFiles(MissionStoragePath))
            {
                if(!file.EndsWith(".json")) continue;
                var json = File.ReadAllText(file);
                try
                {
                    var plan = JsonConvert.DeserializeObject<TaskSpecTree>(json);
                    // Json does not know about _classes_ so we need to recover the types
                    // by checking for simple fields, and matching them to known classes
                    // Most of the work is done in the Task class
                    plan.RecoverFromJson();
                    if(existingPlans.ContainsKey(plan.GetKey()))
                    {
                        guiState.Log($"Skipping existing mission plan:{plan.GetKey()}. If you want to load this from file, either delete or modify the description of the one in the GUI.");
                        continue;
                    }
                    MissionPlans.Add(plan);
                    var tstGUI = Instantiate(TSTPrefab, MissionsScrollContent).GetComponent<TSTGUI>();
                    tstGUI.SetTST(plan);
                    i++;
                }
                catch (Exception e)
                {
                    guiState.Log($"Failed to load mission plan from {file}! Check debug logs.");
                    Debug.LogError(e);
                    continue;
                }
            }
            guiState.Log($"Loaded {i} mission plans");
        }

        void SaveMissionPlans()
        {
            var i=0;
            foreach (var plan in MissionPlans)
            {
                var json = JsonConvert.SerializeObject(plan, Formatting.Indented);
                var path = Path.Combine(MissionStoragePath, $"{plan.GetKey()}.json");
                File.WriteAllText(path, json);
                i++;
            }
            guiState.Log($"Saved {i} mission plans");
        }


        public void OnNewTST()
        {
            var newPlan = new TaskSpecTree();
            MissionPlans.Add(newPlan);
            var tstGUI = Instantiate(TSTPrefab, MissionsScrollContent).GetComponent<TSTGUI>();
            tstGUI.SetTST(newPlan);
            tstGUI.Select();
        }

        public void OnTSTDelete(TaskSpecTree tst)
        {
            var index = MissionPlans.IndexOf(tst);
            if (index >= 0 && index < MissionsScrollContent.childCount)
            {
                MissionPlans.RemoveAt(index);
                Destroy(MissionsScrollContent.GetChild(index).gameObject);
            }
        }

        public void OnTSTUp(TaskSpecTree tst)
        {
            var index = MissionPlans.IndexOf(tst);
            if(index == 0) return;
            MissionPlans.RemoveAt(index);
            MissionPlans.Insert(index-1, tst);
            // Swap the two TaskGUI objects
            var tstGO = MissionsScrollContent.GetChild(index).gameObject;
            var prevTSTGO = MissionsScrollContent.GetChild(index - 1).gameObject;
            tstGO.transform.SetSiblingIndex(index - 1);
            prevTSTGO.transform.SetSiblingIndex(index);
        }

        public void OnTSTDown(TaskSpecTree tst)
        {
            var index = MissionPlans.IndexOf(tst);
            if(index == MissionPlans.Count-1) return;
            MissionPlans.RemoveAt(index);
            MissionPlans.Insert(index+1, tst);
            // Swap the two TaskGUI objects
            var tstGO = MissionsScrollContent.GetChild(index).gameObject;
            var nextTSTGO = MissionsScrollContent.GetChild(index + 1).gameObject;
            tstGO.transform.SetSiblingIndex(index + 1);
            nextTSTGO.transform.SetSiblingIndex(index);
        }


        public void OnTSTSelected(TSTGUI tstGUI)
        {
            foreach(Transform child in MissionsScrollContent)
            {
                var tst = child.GetComponent<TSTGUI>();
                if(tst != tstGUI) tst.Deselect();
            }
            SelectedTSTGUI = tstGUI;
        }

        

        public GameObject GetParamPrefab(object paramValue)
        {
            return paramValue switch
            {
                string or int or float or bool => PrimitiveParamPrefab,
                GeoPoint => GeoPointParamPrefab,
                IList => ListParamPrefab,
                _ => PrimitiveParamPrefab,
            };
        }
        

        

    }
}