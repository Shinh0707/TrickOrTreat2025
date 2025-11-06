using UnityEngine;
using TMPro;
using Halloween.Settings;

namespace Halloween.Managers
{
    public class TipsManager : MonoBehaviour
    {
        [SerializeField] TextMeshProUGUI _textMesh;
        [SerializeField] TipsData _tipsData;

        public void SetTips()
        {
            _textMesh.text = _tipsData.RandomSelect();
        }
    }
}