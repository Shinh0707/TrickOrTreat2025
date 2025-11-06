using UnityEngine;

namespace Halloween.Managers
{
    public class AnimationManager : MonoBehaviour
    {
        [SerializeField] Animator _animator;
        public Animator Animator => _animator;
    }
}