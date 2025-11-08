using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Halloween.Managers
{
    public class PhaseProgressBarManager : MonoBehaviour
    {
        [SerializeField] GameObject _phasePrefab;
        [SerializeField] int _maxPhases = 32;
        [SerializeField] RectTransform _barPhaseContainer;
        [SerializeField] Image _lNode;
        [SerializeField] Image _rNode;
        [SerializeField] Color _unprogressedColor;

        [Header("Progressed Colors")]
        [SerializeField] Color _successColor;
        [SerializeField] Color _failureColor;
        [SerializeField] Color _lateColor;
        [SerializeField] Color _deathColor;
        [SerializeField] Color _safeColor;

        [Header("Labels")]
        [SerializeField] TextMeshProUGUI _lLabelTextMesh;
        [SerializeField] TextMeshProUGUI _rLabelTextMesh;
        private Dictionary<Types.ResultType, Color> _progressedColors = new();
        private Image[] _phases = new Image[0] { };
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        public void ManualStart()
        {
            _phases = new Image[_maxPhases];
            for (int i = 0; i < _maxPhases; i++)
            {
                var obj = Instantiate(_phasePrefab, _barPhaseContainer);
                _phases[i] = obj.GetComponent<Image>();
                obj.SetActive(false);
            }
            _progressedColors[Types.ResultType.SUCCESS] = _successColor;
            _progressedColors[Types.ResultType.FAIL] = _failureColor;
            _progressedColors[Types.ResultType.LATE] = _lateColor;
            _progressedColors[Types.ResultType.SAFE] = _safeColor;
            _progressedColors[Types.ResultType.DEATH] = _deathColor;
        }

        public void Setup(int label, int numPhases)
        {
            int i;
            int actives = (numPhases > _maxPhases) ? _maxPhases : numPhases;
            for (i = 0; i < actives; i++)
            {
                _phases[i].gameObject.SetActive(true);
                _phases[i].color = _unprogressedColor;
            }
            for (; i < _maxPhases; i++)
            {
                _phases[i].gameObject.SetActive(false);
            }
            _lLabelTextMesh.text = label.ToString();
            _rLabelTextMesh.text = (label + 1).ToString();
            _lNode.color = _rNode.color;
            _rNode.color = _unprogressedColor;
        }

        public void InProgress(int progressPhase, Types.TreatResult result)
        {
            if ((0 <= progressPhase) && (progressPhase < _maxPhases))
            {
                _phases[progressPhase].color = _progressedColors[result.resultType];
            }
        }
        
        public void EndProgress(Types.TreatResult result)
        {
            _rNode.color = _progressedColors[result.resultType];
        }
    }
}