using UnityEngine;

namespace Attributes
{
    /// <summary>
    /// [SerializeField]されたstring型のフィールドに
    /// Spriteアセット（スプライトシート）をドラッグアンドドロップして
    /// アセットパスを設定できるようにする属性.
    /// </summary>
    /// <remarks>
    /// この属性自体はマーカーとしてのみ機能し, 
    /// 実際の描画処理は SpriteSheetPathDrawer が担当する.
    /// </remarks>
    public class SpriteSheetPathAttribute : PropertyAttribute
    {
        // 属性にパラメータが必要な場合はここに追加する.
        // 今回はマーカーとしてのみ使用するため空にする.
    }
}