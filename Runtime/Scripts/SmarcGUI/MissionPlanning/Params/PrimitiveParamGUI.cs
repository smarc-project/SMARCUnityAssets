using TMPro;
using System.Collections.Generic;



namespace SmarcGUI.MissionPlanning.Params
{

    class PrimitiveParamGUI : ParamGUI
    {
        public TMP_InputField InputField;
        public TMP_Dropdown ChoiceDropdown;
        

        protected override void SetupFields()
        {
            switch (paramValue)
            {
                case string s:
                    InputField.gameObject.SetActive(true);
                    InputField.text = s;
                    InputField.contentType = TMP_InputField.ContentType.Standard;
                    InputField.onValueChanged.AddListener(OnInputFieldChanged);
                    break;
                case int i:
                    InputField.gameObject.SetActive(true);
                    InputField.text = i.ToString();
                    InputField.contentType = TMP_InputField.ContentType.IntegerNumber;
                    InputField.onValueChanged.AddListener(OnInputFieldChanged);
                    break;
                case float f:
                    InputField.gameObject.SetActive(true);
                    InputField.text = f.ToString();
                    InputField.contentType = TMP_InputField.ContentType.DecimalNumber;
                    InputField.onValueChanged.AddListener(OnInputFieldChanged);
                    break;
                case bool b:
                    ChoiceDropdown.gameObject.SetActive(true);
                    ChoiceDropdown.ClearOptions();
                    ChoiceDropdown.AddOptions(new List<string>{"True", "False"});
                    ChoiceDropdown.onValueChanged.AddListener(OnChoiceChanged);
                    break;
                default:
                    InputField.gameObject.SetActive(true);
                    InputField.text = $"Non-primitive type: {paramValue.GetType()}";
                    InputField.contentType = TMP_InputField.ContentType.Standard;
                    InputField.interactable = false;
                    break;
            }
        }

        void OnInputFieldChanged(string value)
        {
            switch(InputField.contentType)
            {
                case TMP_InputField.ContentType.Standard:
                    paramValue = value;
                    break;
                case TMP_InputField.ContentType.IntegerNumber:
                    if(int.TryParse(value, out int i))
                        paramValue = i;
                    break;
                case TMP_InputField.ContentType.DecimalNumber:
                    if(float.TryParse(value, out float f))
                        paramValue = f;
                    break;
            }
        }

        void OnChoiceChanged(int index)
        {
            paramValue = bool.Parse(ChoiceDropdown.options[index].text);
        }

    }
}
