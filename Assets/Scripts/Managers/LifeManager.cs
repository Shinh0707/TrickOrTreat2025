using System;
using System.Collections.Generic;
using UnityEngine;

namespace Halloween.Managers
{
    public class LifeManager : MonoBehaviour
    {
        [SerializeField] GameObject _lifePrefab;
        [SerializeField] Transform _lifeContainer;
        [SerializeField, Tooltip("安全性のための最大値(これ以上はオブジェクトが作られない)")] int _maxLife = 10;
        List<GameObject> _lifes = new();
        public void SetLife(int life)
        {
            if (_lifes.Count < Math.Min(life, _maxLife))
            {
                int need = Math.Min(life, _maxLife) - _lifes.Count;
                for (int i = 0; i < need; i++)
                {
                    _lifes.Add(Instantiate(_lifePrefab, _lifeContainer));
                }
            }
            int lifesCount = _lifes.Count;
            for (int i = 0; i < lifesCount; i++)
            {
                _lifes[i].SetActive(i < life);
            }
        }
    }
}