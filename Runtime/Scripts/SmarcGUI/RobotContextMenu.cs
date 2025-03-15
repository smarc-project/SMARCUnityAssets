using UnityEngine;
using UnityEngine.UI;


namespace SmarcGUI
{
    public class RobotContextMenu : ContextMenu
    {
        public Button PingButton;
        public Button LookAtButton;
        public Button FollowButton;

        RobotGUI item;
        public void SetItem(Vector2 position, RobotGUI item)
        {
            this.item = item;
            SetOnTop(position);
            PingButton.onClick.AddListener(OnPing);
            LookAtButton.onClick.AddListener(OnLookAt);
            FollowButton.onClick.AddListener(OnFollow);
        }

        void OnPing()
        {
            item.SendPing();
            Destroy(gameObject);
        }

        void OnLookAt()
        {
            item.LookAtRobot();
            Destroy(gameObject);
        }

        void OnFollow()
        {
            item.FollowRobot();
            Destroy(gameObject);
        }

    }
}
