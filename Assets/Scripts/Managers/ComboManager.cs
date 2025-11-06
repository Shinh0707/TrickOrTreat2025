using System.Linq;
using Attributes;
using UnityEngine;
using UnityEngine.UI;

namespace Halloween.Managers
{
    public class ComboManager : MonoBehaviour
    {
        [SerializeField] GameObject _comboLabel;
        [SerializeField] GameObject _numberPrefab;
        [SerializeField] Transform _numberContainer;
        [SerializeField, SpriteSheetPath] string _numberSpriteSheet;
        Image[] _numbers;
        int _addedNumbers;
        Sprite[] _numberSprites;
        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {
            _numberSprites = Resources.LoadAll<Sprite>(_numberSpriteSheet);
            _addedNumbers = 0;
            _numbers = new Image[10];
            SetCombo(999);
            SetCombo(0);
        }

        public void SetCombo(int combo)
        {
            int digits = Utility.Math.Number.GetDigitCountIntOnly(combo);
            if (_addedNumbers < digits)
            {
                int target = digits - _addedNumbers;
                for (int i = 0; i < target; i++)
                {
                    var obj = Instantiate(_numberPrefab, _numberContainer);
                    _numbers[_addedNumbers] = obj.GetComponent<Image>();
                    _addedNumbers++;
                }
            }
            int v = combo;
            _comboLabel.SetActive(v != 0);
            for (int i = 0; i < _addedNumbers; i++)
            {
                if (v == 0)
                {
                    _numbers[i].gameObject.SetActive(false);
                    continue;
                }
                _numbers[i].gameObject.SetActive(true);
                _numbers[i].sprite = _numberSprites[v % 10];
                v /= 10;
            }
        }
    }
}
