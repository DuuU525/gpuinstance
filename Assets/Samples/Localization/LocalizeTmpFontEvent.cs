using TMPro;
using UnityEngine.Localization.Events;

namespace UnityEngine.Localization.Components
{
    [AddComponentMenu("Localization/Asset/Localize TmpFont Event")]
    public class LocalizeTmpFontEvent : LocalizedAssetEvent<TMPro.TMP_FontAsset, LocalizedTmpFont, UnityEventTmpFont>
    {
        private TextMeshProUGUI text;
        private void Awake()
        {
            text = GetComponent<TextMeshProUGUI>();
        }

        protected override void UpdateAsset(TMP_FontAsset localizedAsset)
        {
            base.UpdateAsset(localizedAsset);
            text.font = localizedAsset;
        }
    }
}
