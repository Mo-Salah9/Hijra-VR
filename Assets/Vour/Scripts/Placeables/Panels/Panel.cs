using UnityEngine;

namespace CrizGames.Vour
{
    
    public enum PanelDisplayType
    {
        _2D,
        _3D,
    }
    
    public abstract class Panel : MonoBehaviour
    {
        protected Transform _panel = null;
        public Transform panel
        {
            get
            {
                if (_panel == null || !Application.isPlaying)
                    _panel = GetPanel();
                return _panel;
            }
        }

        public virtual Transform panelContainer => panel.parent;

        protected virtual void Start()
        {
            InitPanel();
        }

        public abstract void InitPanel();

        protected abstract Transform GetPanel();


        protected virtual void UpdateSize(Vector2 size)
        {
            RectTransform t = panel.GetComponent<RectTransform>();
            t.sizeDelta = new Vector2(size.x / size.y * t.sizeDelta.y, t.sizeDelta.y);

            // Adjust video UI width
            RectTransform vidUIT = t.GetChild(1).GetComponent<RectTransform>();
            vidUIT.sizeDelta = new Vector2(t.sizeDelta.x / vidUIT.localScale.x - 3, vidUIT.sizeDelta.y);
        }
    }
}
