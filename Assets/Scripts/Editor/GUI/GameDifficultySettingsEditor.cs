#if UNITY_EDITOR
using UnityEngine;
using UnityEditor; // Editor スクリプトには必須

namespace HalloweenEditor.GUI
{
    /// <summary>
    /// GameDifficultySettings のカスタムInspectorエディタ
    /// </summary>
    [CustomEditor(typeof(Halloween.Settings.GameDifficultySettings))]
    public class GameDifficultySettingsEditor : Editor
    {
        // パフォーマンス向上のため SerializedProperty をキャッシュする
        private SerializedProperty _bgm;
        private SerializedProperty _phaseAlians;
        private SerializedProperty _phaseSpeed;
        private SerializedProperty _treatRate;
        private SerializedProperty _trickRate;

        // GUIStyle は高コストなため, static でキャッシュする
        private static GUIStyle _orangeLabelStyle;
        private static GUIStyle _purpleLabelStyle;
        private static GUIStyle _blackLabelStyle;

        /// <summary>
        /// Inspectorが有効になったときに呼び出される
        /// </summary>
        private void OnEnable()
        {
            // プロパティを名前で検索してバインドする
            _bgm = serializedObject.FindProperty("bgm");
            _phaseAlians = serializedObject.FindProperty("phaseAlians");
            _phaseSpeed = serializedObject.FindProperty("phaseSpeed");
            _treatRate = serializedObject.FindProperty("treatRate");
            _trickRate = serializedObject.FindProperty("trickRate");

            // スタイルが初期化されていない場合のみ初期化する
            if (_orangeLabelStyle == null)
            {
                _orangeLabelStyle = new GUIStyle(EditorStyles.label);
                _orangeLabelStyle.normal.textColor = Color.orange;
            }

            if (_purpleLabelStyle == null)
            {
                _purpleLabelStyle = new GUIStyle(EditorStyles.label);
                // Color.purple は暗すぎるため, magenta を使用する
                _purpleLabelStyle.normal.textColor = Color.magenta;
            }

            if (_blackLabelStyle == null)
            {
                _blackLabelStyle = new GUIStyle(EditorStyles.label);
                _blackLabelStyle.normal.textColor = Color.black;
                _blackLabelStyle.fontStyle = FontStyle.Bold;
            }
        }

        /// <summary>
        /// InspectorのGUIを描画する
        /// </summary>
        public override void OnInspectorGUI()
        {
            // serializedObject の更新を開始する
            serializedObject.Update();

            // デフォルトのプロパティフィールドを描画
            EditorGUILayout.PropertyField(_bgm);
            EditorGUILayout.PropertyField(_phaseAlians);
            EditorGUILayout.PropertyField(_phaseSpeed);

            EditorGUILayout.Space(); // 空白行
            EditorGUILayout.LabelField(
                "Rate Settings (Total 1.0)", EditorStyles.boldLabel);

            DrawRateSliders();
            serializedObject.ApplyModifiedProperties();
        }

        /// <summary>
        /// Treat, Trick, Death のレートを調整するスライダーを描画する
        /// </summary>
        private void DrawRateSliders()
        {
            float currentTreatRate = _treatRate.floatValue;
            float currentTrickRate = _trickRate.floatValue;

            float totalRate = currentTreatRate + currentTrickRate;

            // もし合計が 1.0 を超えていたら, 比率を維持して正規化する
            if (totalRate > 1.0f)
            {
                float normalizationFactor = 1.0f / totalRate;
                currentTreatRate = currentTreatRate * normalizationFactor;
                currentTrickRate = currentTrickRate * normalizationFactor;
                _treatRate.floatValue = currentTreatRate;
                _trickRate.floatValue = currentTrickRate;
            }

            // 1. Treat Rate のスライダー
            EditorGUI.BeginChangeCheck();
            // 最大値は (1.0 - trickRate)
            float maxTreatRate = 1.0f - currentTrickRate;
            currentTreatRate = EditorGUILayout.Slider(
                "Treat Rate",
                currentTreatRate,
                0.0f,
                maxTreatRate);
            if (EditorGUI.EndChangeCheck())
            {
                _treatRate.floatValue = currentTreatRate;
            }

            // 2. Trick Rate のスライダー
            EditorGUI.BeginChangeCheck();
            // Treat Rate の変更を反映した最大値 (1.0 - treatRate)
            float maxTrickRate = 1.0f - _treatRate.floatValue;
            currentTrickRate = EditorGUILayout.Slider(
                "Trick Rate",
                currentTrickRate,
                0.0f,
                maxTrickRate);
            if (EditorGUI.EndChangeCheck())
            {
                _trickRate.floatValue = currentTrickRate;
            }

            // 3. Death Rate (残り) の計算
            float deathRate = 1.0f - _treatRate.floatValue - _trickRate.floatValue;
            // 浮動小数点誤差を考慮し, 0 未満にならないようクランプする
            if (deathRate < 0.0f)
            {
                deathRate = 0.0f;
            }

            EditorGUILayout.Space();

            // GUILayout を使用してレートのバーを描画する
            DrawRateBar(_treatRate.floatValue, _trickRate.floatValue, deathRate);

            EditorGUILayout.Space();

            // 各レートをラベルで表示（色付き）
            string treatLabel = $"Treat Rate: {_treatRate.floatValue:P1}";
            EditorGUILayout.LabelField(treatLabel, _orangeLabelStyle);

            string trickLabel = $"Trick Rate: {_trickRate.floatValue:P1}";
            EditorGUILayout.LabelField(trickLabel, _purpleLabelStyle);

            string deathLabel = $"Death Rate: {deathRate:P1}";
            EditorGUILayout.LabelField(deathLabel, _blackLabelStyle);
        }

        /// <summary>
        /// レートの合計を視覚化するバーを描画する
        /// </summary>
        private void DrawRateBar(float treatRate, float trickRate, float deathRate)
        {
            // Inspector の幅いっぱいに Rect を確保する
            Rect barRect = EditorGUILayout.GetControlRect(
                false,
                EditorGUIUtility.singleLineHeight);

            // Treat (Orange)
            Rect treatRect = barRect;
            treatRect.width = barRect.width * treatRate;
            EditorGUI.DrawRect(treatRect, Color.orange);

            // Trick (Purple / Magenta)
            Rect trickRect = barRect;
            trickRect.x += treatRect.width;
            trickRect.width = barRect.width * trickRate;
            EditorGUI.DrawRect(trickRect, Color.magenta);

            // Death (Black)
            Rect deathRect = barRect;
            deathRect.x += treatRect.width + trickRect.width;
            // 誤差で幅がマイナスにならないよう Mathf.Max を使用
            deathRect.width = Mathf.Max(0.0f, barRect.width * deathRate);
            EditorGUI.DrawRect(deathRect, Color.black);
        }
    }
}
#endif