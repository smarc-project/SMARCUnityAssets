using UnityEngine;
using UnityEngine.UI;


namespace SmarcGUI
{
    public class ListItemContextMenu : ContextMenu
    {
        public Button DeleteButton;
        public Button MoveUpButton;
        public Button MoveDownButton;

        IListItem item;

        public void SetItem(Vector2 position, IListItem item)
        {
            DeleteButton.onClick.AddListener(OnItemDelete);
            MoveUpButton.onClick.AddListener(OnItemUp);
            MoveDownButton.onClick.AddListener(OnItemDown);
            this.item = item;
            SetOnTop(position);
        }

        void OnItemDelete()
        {
            item.OnListItemDelete();
            Destroy(gameObject);
        }

        void OnItemUp()
        {
            item.OnListItemUp();
            Destroy(gameObject);
        }

        void OnItemDown()
        {
            item.OnListItemDown();
            Destroy(gameObject);
        }
        

    }
}