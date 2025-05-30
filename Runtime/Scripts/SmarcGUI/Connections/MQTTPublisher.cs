using UnityEngine;

namespace SmarcGUI.Connections
{
    public abstract class MQTTPublisher : MonoBehaviour
    {
        protected bool publish = false;
        protected MQTTClientGUI mqttClient;

        public abstract void StartPublishing();
        public abstract void StopPublishing();
    }
}