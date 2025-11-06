using UnityEngine;

namespace Halloween.Managers
{
    public class AnnotationManager : MonoBehaviour
    {
        [SerializeField] Animator _annotationAnimatior;

        public void Play()
        {
            _annotationAnimatior.SetTrigger("Trigger");
        }
    }
}