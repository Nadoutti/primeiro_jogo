using System.Linq;
using Platformer.Mechanics;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Platformer.UI
{
    /// <summary>
    /// The in-game HUD: token counter (top left), timer (top right) and the
    /// victory screen with the final time. Everything is built at runtime, and
    /// the HUD adds itself to any scene that has tokens.
    /// </summary>
    public class GameHUD : MonoBehaviour
    {
        /// <summary>
        /// The token controller to read the counts from. If empty, it is found in the scene.
        /// </summary>
        public TokenController tokenController;

        public string tokenLabel = "Moedas";
        public int fontSize = 36;
        public Color color = new Color(1f, 0.85f, 0.2f);

        /// <summary>
        /// Seconds since the level started. Does not advance while the game is paused.
        /// </summary>
        public float ElapsedTime { get; private set; }

        /// <summary>
        /// Has the player won? Stops the timer.
        /// </summary>
        public bool Finished { get; private set; }

        TextMeshProUGUI tokenText, timerText, victoryText;
        GameObject victoryPanel;
        int shownCollected = -1;
        int shownTotal = -1;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void CreateIfMissing()
        {
            if (Object.FindFirstObjectByType<GameHUD>() != null) return;
            var tokens = Object.FindFirstObjectByType<TokenController>();
            if (tokens != null)
                tokens.gameObject.AddComponent<GameHUD>().tokenController = tokens;
        }

        void Start()
        {
            if (tokenController == null)
                tokenController = Object.FindFirstObjectByType<TokenController>();

            var canvasObject = new GameObject("HUD Canvas", typeof(Canvas), typeof(CanvasScaler));
            canvasObject.transform.SetParent(transform, false);
            var canvas = canvasObject.GetComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvasObject.GetComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080);

            tokenText = CreateText(canvasObject.transform, new Vector2(0, 1), new Vector2(24, -16), fontSize, TextAlignmentOptions.TopLeft);
            timerText = CreateText(canvasObject.transform, new Vector2(1, 1), new Vector2(-24, -16), fontSize, TextAlignmentOptions.TopRight);

            //victory screen: dark overlay with the final time, hidden until the player wins.
            victoryPanel = new GameObject("Victory Panel", typeof(RectTransform), typeof(Image));
            victoryPanel.transform.SetParent(canvasObject.transform, false);
            var panelRect = victoryPanel.GetComponent<RectTransform>();
            panelRect.anchorMin = Vector2.zero;
            panelRect.anchorMax = Vector2.one;
            panelRect.offsetMin = panelRect.offsetMax = Vector2.zero;
            victoryPanel.GetComponent<Image>().color = new Color(0, 0, 0, 0.6f);
            victoryText = CreateText(victoryPanel.transform, new Vector2(0.5f, 0.5f), Vector2.zero, fontSize * 2, TextAlignmentOptions.Center);
            victoryText.rectTransform.sizeDelta = new Vector2(1200, 400);
            victoryPanel.SetActive(false);

            //hide the HUD together with the other gameplay canvases when the menu opens.
            var metaGameController = Object.FindFirstObjectByType<MetaGameController>();
            if (metaGameController != null)
                metaGameController.gamePlayCanvasii = metaGameController.gamePlayCanvasii.Append(canvas).ToArray();
        }

        TextMeshProUGUI CreateText(Transform parent, Vector2 anchor, Vector2 position, float size, TextAlignmentOptions alignment)
        {
            var textObject = new GameObject("Text", typeof(RectTransform));
            textObject.transform.SetParent(parent, false);
            var rect = textObject.GetComponent<RectTransform>();
            rect.anchorMin = rect.anchorMax = rect.pivot = anchor;
            rect.anchoredPosition = position;
            rect.sizeDelta = new Vector2(400, 60);

            var text = textObject.AddComponent<TextMeshProUGUI>();
            text.fontSize = size;
            text.fontStyle = FontStyles.Bold;
            text.alignment = alignment;
            text.color = color;
            text.outlineWidth = 0.2f;
            text.outlineColor = Color.black;
            return text;
        }

        /// <summary>
        /// Stop the timer and show the victory screen with the final time.
        /// </summary>
        public void ShowVictory()
        {
            if (Finished) return;
            Finished = true;
            timerText.text = FormatTime(ElapsedTime);
            victoryText.text = $"VOCÊ VENCEU!\nTempo: {FormatTime(ElapsedTime)}";
            victoryPanel.SetActive(true);
        }

        /// <summary>
        /// Format seconds as mm:ss.cc
        /// </summary>
        public static string FormatTime(float seconds)
        {
            var centiseconds = (int)(seconds * 100);
            return $"{centiseconds / 6000:00}:{centiseconds / 100 % 60:00}.{centiseconds % 100:00}";
        }

        void Update()
        {
            if (!Finished)
            {
                ElapsedTime += Time.deltaTime;
                timerText.text = FormatTime(ElapsedTime);
            }

            if (tokenController == null) return;

            var collected = tokenController.CollectedCount;
            var total = tokenController.TotalCount;
            if (collected == shownCollected && total == shownTotal) return;

            shownCollected = collected;
            shownTotal = total;
            tokenText.text = $"{tokenLabel}: {collected}/{total}";
        }
    }
}
