using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;


namespace ARCoreRecordingPlaybackUtil.Scripts
{
    public class LongPressEventTrigger : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerExitHandler
    {
        public UnityEvent OnLongPressed = new();
        [SerializeField]
        [Tooltip("How long must pointer be down on this object to trigger a long press")]
        private float _holdTime = 1f;

        public void OnPointerDown(PointerEventData eventData)
        {
            Invoke(nameof(OnLongPress), _holdTime);
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            CancelInvoke(nameof(OnLongPress));
        }

        public void OnPointerExit(PointerEventData eventData)
        {
            CancelInvoke(nameof(OnLongPress));
        }

        private void OnLongPress()
        {
            OnLongPressed.Invoke();
        }
    }
}
