using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace Halloween.Managers
{
    public class HomeCharacterManager : MonoBehaviour
    {
        [SerializeField] Image _characterImage;
        [SerializeField] GameObject _textContainerObject;
        [SerializeField] TextMeshProUGUI _textContainer;
        [SerializeField] Settings.MainCharacterImageState _characterImageSetting;
        [SerializeField] float _interval = 5f;
        [SerializeField] float _hideInterval = 5f;
        [SerializeField] float _preDelay = 5f;
        [SerializeField] Animation _anim;
        private float _time;
        private bool _preDelayed;
        private bool _specialAppeared;
        public void Start()
        {
            _time = 0;
            _specialAppeared = false;
            _preDelayed = false;
            _anim.Play(PlayMode.StopAll);
            SetAppearence();
        }
        public void SetAppearence(Types.CharacterState state, string text)
        {
            Debug.Log($"Set State: {state}");
            _characterImageSetting.SetStateImage(state, _characterImage);
            _textContainerObject.SetActive(!string.IsNullOrEmpty(text));
            _textContainer.text = text;
        }
        public void SetAppearence()
        {
            Debug.Log($"Set State Default");
            _characterImageSetting.SetStateImage(Types.CharacterState.NORMAL, _characterImage);
            _textContainerObject.SetActive(false);
            _textContainer.text = "";
        }
        void FixedUpdate()
        {
            _time += Time.fixedDeltaTime;
            if ((!_preDelayed) && (_time < _preDelay)) return;
            if (_preDelayed)
            {
                if (_time < _hideInterval) return;
                SetAppearence();
                if (_time < _hideInterval + _interval) return;
            }
            _time = 0;
            Types.CharacterState state; string text;
            Settings.GameSettings settings = GameSceneManager.GameSettings;
            if (_specialAppeared)
            {
                NotPlayedGameMessage(settings, out state, out text);
                _specialAppeared = Random.Range(0f, 1f) < 0.7f;
            }
            else
            {
                if (GameSceneManager.TryGetLastResult(out GameResult lastResult))
                {
                    PlayedGameMessage(settings, GameSceneManager.GameData, lastResult, out state, out text);
                }
                else
                {
                    NotPlayedGameMessage(settings, out state, out text);
                }
                _specialAppeared = true;
            }
            _anim.Play(PlayMode.StopAll);
            SetAppearence(state, text);
            _preDelayed = true;
        }
        
        /// <summary>
        /// ゲーム未プレイ時のメッセージを生成します。
        /// 主に世界観の導入、Tips、操作方法の案内を行います。
        /// </summary>
        /// <param name="settings">参照するゲーム設定</param>
        /// <param name="state">出力するキャラクターの表情</param>
        /// <param name="text">出力するセリフ</param>
        void NotPlayedGameMessage(Settings.GameSettings settings, out Types.CharacterState state, out string text)
        {
            // Tipsや操作ヘルプをプレイヤーへ語りかける（世界観重視）
            var messages = new (Types.CharacterState State, string Text)[]
            {
                (Types.CharacterState.SUCCESS, "わーい！ ハロウィン！ お菓子いっぱい作るぞー♪"),
                (Types.CharacterState.SUCCESS, "トリック・オア・トリート！ 準備はいいかな？"),
                (Types.CharacterState.SUCCESS, $"『{settings.mainKey}』で魔法を使うよ！ タイミングが大事！"),
                (Types.CharacterState.RELIEF, "うまくお菓子を渡せるかな…？ ドキドキするね。"),
                (Types.CharacterState.NORMAL, $"{settings.restoreLifeCombo}回連続で喜んでもらえたら、もっと頑張れちゃうんだ♪"),
                (Types.CharacterState.WORRY, $"もし{settings.life}回失敗したら…うぅ、考えたくないかも。")
            };

            // ランダムに選択
            var (selectedState, selectedText) = messages[Random.Range(0, messages.Length)];
            state = selectedState;
            text = selectedText;
        }

        /// <summary>
        /// 直前のゲーム結果に基づいたメッセージを生成します。
        /// 結果に対するフィードバックや独り言をキャラクター設定に沿って行います。
        /// </summary>
        /// <param name="settings">参照するゲーム設定</param>
        /// <param name="data">参照するゲームデータ</param>
        /// <param name="lastResult">参照する直前のゲーム結果</param>
        /// <param name="state">出力するキャラクターの表情</param>
        /// <param name="text">出力するセリフ</param>
        void PlayedGameMessage(Settings.GameSettings settings, GameData data, GameResult lastResult, out Types.CharacterState state, out string text)
        {
            // 結果に対するフィードバック・独り言や共感を求める語りかけ
            
            (Types.CharacterState State, string Text) message;

            // 1. 魔法監視局 (Death) に対応した直後 (最優先)
            if (lastResult.treatedDeath)
            {
                var messages = new (Types.CharacterState, string)[]
                {
                    (Types.CharacterState.RELIEF, "ふぅ、危なかったぁ…。見逃してくれたかな？"),
                    (Types.CharacterState.WORRY, "バレたら、みんなと仲良くなれないもんね…"),
                    (Types.CharacterState.WORRY, "今の、魔女だってバレてないよね…？")
                };
                message = messages[Random.Range(0, messages.Length)];
            }
            // 2. ほぼパーフェクト (コンボが特に多く、失敗が少ない)
            else if (lastResult.maxCombo > 15 && lastResult.failureCount <= 1)
            {
                var messages = new (Types.CharacterState, string)[]
                {
                    (Types.CharacterState.SUCCESS, $"やったぁ！ {lastResult.maxCombo}コンボ！ 魔法の調子、絶好調！"),
                    (Types.CharacterState.SUCCESS, "みんな喜んでくれた！ えへへ、嬉しいな"),
                    (Types.CharacterState.SUCCESS, $"{lastResult.successCount}人ともお友達になれたかな？")
                };
                message = messages[Random.Range(0, messages.Length)];
            }
            // 3. 失敗が多い (ライフの半分以上失敗、または成功より失敗が多い)
            else if (lastResult.failureCount > (settings.life / 2) || lastResult.failureCount > lastResult.successCount)
            {
                var messages = new (Types.CharacterState, string)[]
                {
                    (Types.CharacterState.WORRY, $"うぅ…{lastResult.failureCount}回も失敗…。嫌われちゃったかも…。"),
                    (Types.CharacterState.WORRY, "あんまり喜んでもらえなかったかも…"),
                    (Types.CharacterState.SHOCK, "ごめんなさい…。次はもっと上手に作るから…！")
                };
                message = messages[Random.Range(0, messages.Length)];
            }
            // 4. 好成績 (成功が失敗より多い)
            else if (lastResult.successCount > lastResult.failureCount)
            {
                var messages = new (Types.CharacterState, string)[]
                {
                    (Types.CharacterState.RELIEF, "たくさんのお菓子、喜んでもらえたかな？"),
                    (Types.CharacterState.SUCCESS, $"最大{lastResult.maxCombo}コンボ！ この調子でいこー！"),
                    (Types.CharacterState.NORMAL, $"次は、{lastResult.failureCount}回の失敗、取り戻すぞっ！")
                };
                message = messages[Random.Range(0, messages.Length)];
            }
            // 5. その他 (平均的)
            else
            {
                var messages = new (Types.CharacterState, string)[]
                {
                    (Types.CharacterState.NORMAL, $"{lastResult.totalAlians}人かぁ。もっといっぱい配りたいな♪"),
                    (Types.CharacterState.NORMAL, "成功も失敗もあったけど…ハロウィンは楽しいね！"),
                    (Types.CharacterState.WORRY, $"次は、{lastResult.maxCombo}コンボ以上目指してみようかな？")
                };
                message = messages[Random.Range(0, messages.Length)];
            }

            state = message.State;
            text = message.Text;
        }
    }
}
