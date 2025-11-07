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
                _specialAppeared = Random.Range(0f, 1f) < 0.55f;
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

        void SelectMessage((Types.CharacterState State, string Text)[] messages, out Types.CharacterState state, out string text)
        {
            var (selectedState, selectedText) = messages[Random.Range(0, messages.Length)];
            state = selectedState;
            text = selectedText;
        }

        (Types.CharacterState State, string Text) SelectMessage((Types.CharacterState State, string Text)[] messages)
        {
            return messages[Random.Range(0, messages.Length)];
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
            SelectMessage(new[]{
                (Types.CharacterState.SUCCESS, "ハッピーハロウィン！ お菓子いっぱい作るよ！"),
                (Types.CharacterState.SUCCESS, "トリック・オア・トリート！ 準備はいいかな？"),
                (Types.CharacterState.SUCCESS, $"『{settings.mainKey}』で魔法を使うよ！ タイミングが大事！"),
                (Types.CharacterState.RELIEF, "うまくお菓子を渡せるかな…？ ドキドキするね…"),
                (Types.CharacterState.NORMAL, $"{settings.restoreLifeCombo}回連続で喜んでもらえたら、もっと頑張れちゃうんだ"),
                (Types.CharacterState.WORRY, $"もし{settings.life}回失敗したら…うぅ、考えたくないかも…")
            }, out state, out text);
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
            // コンボ・成功・新記録というゲームの言葉は使わない
            // パティ目線のセリフ
            // ポジティブな内容で！が入ってたら基本SUCCEESS(喜び・希望)
            // ネガティブな内容で！が入ってたら基本SHOCK(ショック・驚き)
            // ホッとする,静かに何かを喜んだり感じたりするときはRELIEF(セリフに…や？が入ると良い感じに合う)
            // 不安,心配な時はWORRY(セリフに…や？が入ると良い感じに合う)
            // それ以外はNORMAL
            // 各パターン最低３セリフ

            (Types.CharacterState State, string Text) message;

            var lastEval = data.LastEvaluate();
            // 成長を一緒に喜ぼうと語りかける (必ず各セリフにlastResult.maxCombo, succcessCountを自然に含め、今までで一番良かったことを示唆)
            if (((lastEval & GameDataEvaluate.MAXCOMBO) != 0) && (lastResult.maxCombo >= settings.restoreLifeCombo))
            {
                if (((lastEval & GameDataEvaluate.MAXSUCCESS) != 0) && (lastResult.successCount >= settings.restoreLifeCombo))
                {
                    // 前より成功回数が多いし、連続成功数も多い (調子が良いし,いろんな人に出会えて嬉しい)
                    message = SelectMessage(new[]{
                        (Types.CharacterState.SUCCESS, $"{lastResult.maxCombo}回も連続でできたし、{lastResult.successCount}人とも出会えて嬉しい！"),
                        (Types.CharacterState.SUCCESS, $"初めて{lastResult.maxCombo}回も続けられて、{lastResult.successCount}人にも出会えた！私、今すごく調子いいみたい！"),
                        (Types.CharacterState.SUCCESS, $"初めて{lastResult.maxCombo}回も連続で魔法を使えて{lastResult.successCount}人とも仲良くなれたかも！")
                    });
                }
                else
                {
                    // 前より連続成功数が多い (調子が良い, たくさん作れそう)
                    message = SelectMessage(new[]{
                        (Types.CharacterState.SUCCESS, $"初めて{lastResult.maxCombo}回も続いた！今までで一番調子がいいかも！"),
                        (Types.CharacterState.SUCCESS, $"初めて{lastResult.maxCombo}回も連続でできた！次も、もっと作れるかな！"),
                        (Types.CharacterState.RELIEF, $"{lastResult.maxCombo}回も続いた…みんなともっと仲良くなれるかな")
                    });
                }
            }
            else if (((lastEval & GameDataEvaluate.MAXSUCCESS) != 0) && (lastResult.successCount >= settings.restoreLifeCombo))
            {
                // 前より成功回数が多い(いろんな人に出会えてうれしい)
                message = SelectMessage(new[]{
                    (Types.CharacterState.SUCCESS, $"初めて{lastResult.successCount}人とも出会えた！喜んでもらえたかな！"),
                    (Types.CharacterState.SUCCESS, $"{lastResult.successCount}人と出会えた！こんなに喜んでもらえたの初めてかも！"),
                    (Types.CharacterState.SUCCESS, $"{lastResult.successCount}人と出会えちゃった！もっとお菓子を作ろうかな！"),
                });
            }
            // 連続で成功したけど Deathで失敗してしまったか, 最後の最後で連続失敗してしまった (悔しい))
            else if ((lastResult.maxCombo >= settings.restoreLifeCombo) && (lastResult.maxCombo >= (lastResult.successCount + lastResult.safeCount)))
            {
                message = SelectMessage(new[]{
                    (Types.CharacterState.WORRY, $"{lastResult.maxCombo}回もずっと上手くいってたから悔しいね…"),
                    (Types.CharacterState.SHOCK, $"{lastResult.maxCombo}回もずっと連続でできて、調子よかったのに！"),
                    (Types.CharacterState.WORRY, $"悔しいけど、少し休もうかな？")
                });
            }
            // お菓子を欲しがってる人が来なかった(あれ？だれも来なかったね？みたいなことを言う)
            else if ((lastResult.successCount == 0) && (lastResult.safeCount > 0))
            {
                message = SelectMessage(new[]{
                    (Types.CharacterState.WORRY, "あれ…？ 誰も来てくれなかった…"),
                    (Types.CharacterState.WORRY, "みんな、もうお菓子いらないのかな？"),
                    (Types.CharacterState.RELIEF, "次は誰かと出会えると嬉しいな…")
                });
            }
            else if (lastResult.maxCombo >= settings.restoreLifeCombo)
            {
                message = SelectMessage(new[]{
                    (Types.CharacterState.SUCCESS, $"{lastResult.maxCombo}回も連続で作れちゃった！"),
                    (Types.CharacterState.SUCCESS, $"{lastResult.maxCombo}回も続いた！コツがわかってきたかも！"),
                    (Types.CharacterState.RELIEF, $"ふふ、{lastResult.maxCombo}回連続成功…。なんだか嬉しいね")
                });
            }
            else if (lastResult.successCount >= settings.restoreLifeCombo)
            {
                message = SelectMessage(new[]{
                    (Types.CharacterState.RELIEF, $"{lastResult.successCount}人に出会えて嬉しい…次は誰に出会えるかな？"),
                    (Types.CharacterState.SUCCESS, $"{lastResult.successCount}人に渡せたね！喜んでくれたかな？"),
                    (Types.CharacterState.RELIEF, $"みんなと仲良くなれるかもって思えて嬉しい…")
                });
            }
            // 魔法監視局に見つかって逃げた
            else if (lastResult.treatedDeath)
            {
                message = SelectMessage(new[]{
                    (Types.CharacterState.RELIEF, "ふぅ、危なかったぁ…さっきの言葉には気をつけないとね…"),
                    (Types.CharacterState.WORRY, "とっても怖かった…あの言葉には注意だね"),
                    (Types.CharacterState.WORRY, "今の、魔女だってバレてないよね…？")
                });
            }
            // 全部失敗した ({settings.allowableBeatStartIndex+1}番目の言葉の後に{settings.mainkey}を押してみて のようなことを伝える, 悲しんだりはしない)
            else if ((lastResult.successCount + lastResult.safeCount == 0) && (lastResult.failureCount > 0))
            {
                message = SelectMessage(new[]{
                    (Types.CharacterState.WORRY, "うーん…ちょっと遅れただけで、みんな帰っちゃうね…"),
                    (Types.CharacterState.NORMAL, $"{settings.allowableBeatStartIndex+1}番目の言葉の後に、おもてなしだね"),
                    (Types.CharacterState.SUCCESS, $"よーし、もう一回！{settings.allowableBeatStartIndex+1}番目の言葉の後に合わせてみよう！")
                });
            }
            else
            {
                message = SelectMessage(new[]{
                    (Types.CharacterState.RELIEF, $"次は何人に出会えるかな？"),
                    (Types.CharacterState.RELIEF, "成功も失敗もあったけど、ハロウィンは楽しいね"),
                    (Types.CharacterState.NORMAL, $"次は何を目指してみようかな")
                });
            }
            state = message.State;
            text = message.Text;
        }
    }
}
