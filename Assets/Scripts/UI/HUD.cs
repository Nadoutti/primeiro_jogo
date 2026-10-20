using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NeonSurvivor
{
    /// <summary>Interface do jogo: barras, tempo, score, banners, level up, título e fim de jogo.</summary>
    public class HUD : MonoBehaviour
    {
        public PlayerController player;
        public WaveDirector waves;

        UIKit.Bar hpBar, staminaBar, xpBar, bossBar;
        TextMeshProUGUI hpText, timerText, extractText, scoreText, killsText, levelText, bannerText, bossText;
        GameObject bossGroup, titleScreen, endScreen, levelUpScreen;
        TextMeshProUGUI endTitle, endStats;
        Image damageFlash;
        readonly List<(Weapon weapon, Image icon, TextMeshProUGUI label)> weaponSlots = new();

        readonly List<Image> cardFrames = new();
        List<UpgradeOption> options;
        Action<UpgradeOption> onChosen;
        int selected;
        float bannerTime, flashTime, levelUpOpenedAt;

        void Awake() => Build();

        void Build()
        {
            var canvas = gameObject.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = gameObject.AddComponent<CanvasScaler>();
            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(960, 600);
            scaler.matchWidthOrHeight = 0.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            var root = transform;

            damageFlash = UIKit.Image(UIKit.Stretch(root, "DamageFlash"), new Color(1f, 0f, 0.15f, 0f));

            // canto superior esquerdo: vida e stamina
            var tl = new Vector2(0, 1);
            UIKit.Image(UIKit.Rect(root, "HeartIcon", tl, new Vector2(12, -12), new Vector2(24, 20)), Color.white, Art.Get("icon_heart"));
            hpBar = UIKit.MakeBar(root, "HP", tl, new Vector2(42, -12), new Vector2(200, 20), Art.Pink);
            hpText = UIKit.Text(root, "HPText", "100/100", 14, Color.white, tl, new Vector2(42, -12), new Vector2(200, 20));
            UIKit.Text(root, "StaminaLabel", "DASH", 12, Art.Cyan, tl, new Vector2(4, -36), new Vector2(36, 14));
            staminaBar = UIKit.MakeBar(root, "Stamina", tl, new Vector2(42, -36), new Vector2(160, 12), Art.Cyan);

            // centro superior: cronômetro
            var tc = new Vector2(0.5f, 1);
            timerText = UIKit.Text(root, "Timer", "00:00", 40, Color.white, tc, new Vector2(0, -2), new Vector2(240, 48));
            extractText = UIKit.Text(root, "Extraction", "EXTRAÇÃO EM 3:00", 13, Art.Cyan, tc, new Vector2(0, -48), new Vector2(300, 18));

            bossGroup = UIKit.Rect(root, "BossGroup", tc, new Vector2(0, -70), new Vector2(420, 34)).gameObject;
            bossText = UIKit.Text(bossGroup.transform, "BossName", "MECH DE SEGURANÇA", 13, Art.Red, tc, Vector2.zero, new Vector2(420, 16));
            bossBar = UIKit.MakeBar(bossGroup.transform, "BossHP", tc, new Vector2(0, -16), new Vector2(420, 14), Art.Red);
            bossGroup.SetActive(false);

            // canto superior direito: score
            var tr = new Vector2(1, 1);
            scoreText = UIKit.Text(root, "Score", "SCORE 0", 22, Art.Yellow, tr, new Vector2(-12, -8), new Vector2(260, 28), TextAlignmentOptions.Right);
            killsText = UIKit.Text(root, "Kills", "ABATES 0", 14, Color.white, tr, new Vector2(-12, -36), new Vector2(260, 18), TextAlignmentOptions.Right);

            // rodapé: XP + armas
            var bl = new Vector2(0, 0);
            xpBar = UIKit.MakeBar(root, "XP", new Vector2(0.5f, 0), new Vector2(0, 6), new Vector2(940, 12), Art.Yellow);
            levelText = UIKit.Text(root, "Level", "NV 1", 18, Art.Yellow, bl, new Vector2(12, 22), new Vector2(80, 24), TextAlignmentOptions.Left);
            int slot = 0;
            foreach (var w in player.GetComponents<Weapon>())
            {
                var pos = new Vector2(90 + slot * 64, 20);
                var frame = UIKit.Rect(root, "Slot" + slot, bl, pos, new Vector2(56, 34));
                UIKit.Image(frame, new Color(0.08f, 0.04f, 0.16f, 0.85f));
                var icon = UIKit.Image(UIKit.Rect(frame, "Icon", new Vector2(0, 0.5f), new Vector2(4, 0), new Vector2(28, 24)), Color.white, Art.Get(w.IconName));
                var label = UIKit.Text(frame, "Lv", "-", 12, Color.white, new Vector2(1, 0.5f), new Vector2(-3, 0), new Vector2(24, 20), TextAlignmentOptions.Right);
                weaponSlots.Add((w, icon, label));
                slot++;
            }

            bannerText = UIKit.Text(root, "Banner", "", 26, Art.Pink, new Vector2(0.5f, 0.5f), new Vector2(0, 130), new Vector2(900, 50));

            BuildLevelUp(root);
            BuildTitle(root);
            BuildEnd(root);
        }

        void BuildLevelUp(Transform root)
        {
            var screen = UIKit.Stretch(root, "LevelUp");
            UIKit.Image(screen, new Color(0.02f, 0f, 0.06f, 0.78f));
            levelUpScreen = screen.gameObject;
            var c = new Vector2(0.5f, 0.5f);
            UIKit.Text(screen, "Title", "UPGRADE DISPONÍVEL", 34, Art.Yellow, c, new Vector2(0, 190), new Vector2(800, 50));
            UIKit.Text(screen, "Hint", "←/→ ou D-pad para escolher  •  ENTER / A para confirmar  •  1, 2, 3", 14, Color.white, c, new Vector2(0, -190), new Vector2(800, 24));
            for (int i = 0; i < 3; i++)
            {
                var card = UIKit.Rect(screen, "Card" + i, c, new Vector2((i - 1) * 290, 0), new Vector2(260, 300));
                card.pivot = new Vector2(0.5f, 0.5f);
                card.anchoredPosition = new Vector2((i - 1) * 290, 0);
                var frame = UIKit.Image(card, Art.Cyan);
                frame.raycastTarget = true;
                int index = i;
                card.gameObject.AddComponent<Button>().onClick.AddListener(() => Choose(index));
                cardFrames.Add(frame);
                var inner = UIKit.Stretch(card, "Inner");
                inner.offsetMin = new Vector2(4, 4);
                inner.offsetMax = new Vector2(-4, -4);
                UIKit.Image(inner, new Color(0.07f, 0.03f, 0.15f, 1f));
                UIKit.Text(card, "Key", (i + 1).ToString(), 18, Art.Yellow, new Vector2(0, 1), new Vector2(12, -8), new Vector2(30, 24), TextAlignmentOptions.Left);
                UIKit.Image(UIKit.Rect(card, "Icon", new Vector2(0.5f, 1), new Vector2(0, -40), new Vector2(84, 72)), Color.white);
                UIKit.Text(card, "Name", "", 20, Color.white, new Vector2(0.5f, 1), new Vector2(0, -130), new Vector2(230, 56));
                UIKit.Text(card, "Desc", "", 15, new Color(0.8f, 0.85f, 1f), new Vector2(0.5f, 1), new Vector2(0, -190), new Vector2(230, 90));
            }
            levelUpScreen.SetActive(false);
        }

        void BuildTitle(Transform root)
        {
            var screen = UIKit.Stretch(root, "Title");
            UIKit.Image(screen, new Color(0.02f, 0f, 0.06f, 0.85f));
            titleScreen = screen.gameObject;
            var c = new Vector2(0.5f, 0.5f);
            UIKit.Text(screen, "Logo", "NEON SURVIVOR", 72, Art.Pink, c, new Vector2(0, 150), new Vector2(900, 90));
            UIKit.Text(screen, "Sub", "NEO-SÃO PAULO, 2099 — sobreviva 3 minutos até a extração", 18, Art.Cyan, c, new Vector2(0, 90), new Vector2(900, 30));
            UIKit.Text(screen, "Controls",
                "<color=#F9F871>MOVER</color>  WASD / setas / analógico\n" +
                "<color=#F9F871>DASH</color>  Espaço / Shift / A / RT  (gasta stamina, atravessa poças)\n" +
                "Suas armas atiram sozinhas. Colete chips de dados para subir de nível.",
                17, Color.white, c, new Vector2(0, -10), new Vector2(820, 110));
            var press = UIKit.Text(screen, "Press", "PRESSIONE ENTER OU A PARA COMEÇAR", 24, Art.Yellow, c, new Vector2(0, -150), new Vector2(800, 40));
            press.gameObject.AddComponent<Blink>();
        }

        void BuildEnd(Transform root)
        {
            var screen = UIKit.Stretch(root, "End");
            UIKit.Image(screen, new Color(0.02f, 0f, 0.06f, 0.85f));
            endScreen = screen.gameObject;
            var c = new Vector2(0.5f, 0.5f);
            endTitle = UIKit.Text(screen, "EndTitle", "", 56, Art.Pink, c, new Vector2(0, 140), new Vector2(900, 80));
            endStats = UIKit.Text(screen, "Stats", "", 24, Color.white, c, new Vector2(0, 0), new Vector2(700, 180));
            var press = UIKit.Text(screen, "Press", "ENTER OU A PARA JOGAR DE NOVO", 22, Art.Yellow, c, new Vector2(0, -160), new Vector2(800, 40));
            press.gameObject.AddComponent<Blink>();
            endScreen.SetActive(false);
        }

        static string Clock(float seconds)
        {
            int s = Mathf.FloorToInt(seconds);
            return $"{s / 60:00}:{s % 60:00}";
        }

        void Update()
        {
            hpBar.Set(player.Health / player.maxHealth);
            hpText.text = $"{Mathf.CeilToInt(player.Health)}/{Mathf.CeilToInt(player.maxHealth)}";
            staminaBar.Set(player.Stamina / player.maxStamina);
            xpBar.Set((float)player.Xp / player.XpToNext);
            levelText.text = $"NV {player.Level}";
            timerText.text = Clock(waves.Elapsed);
            extractText.text = $"EXTRAÇÃO EM {Clock(waves.Remaining)}";
            extractText.color = waves.Remaining <= 15f && Mathf.Repeat(Time.unscaledTime * 2f, 1f) < 0.5f ? Color.white : Art.Cyan;
            scoreText.text = $"SCORE {GameManager.I.Score}";
            killsText.text = $"ABATES {GameManager.I.Kills}";

            foreach (var (w, icon, label) in weaponSlots)
            {
                icon.color = w.Level > 0 ? Color.white : new Color(1, 1, 1, 0.2f);
                label.text = w.Level > 0 ? $"{w.Level}" : "-";
            }

            var boss = waves.Boss;
            bossGroup.SetActive(boss != null && !boss.IsDead);
            if (boss != null) bossBar.Set(boss.HpFraction);

            if (bannerTime > 0)
            {
                bannerTime -= Time.unscaledDeltaTime;
                var col = bannerText.color;
                col.a = Mathf.Clamp01(bannerTime);
                bannerText.color = col;
                bannerText.transform.localScale = Vector3.one * (1f + Mathf.Max(0, bannerTime - 2.75f));
            }
            if (flashTime > 0)
            {
                flashTime -= Time.unscaledDeltaTime;
                damageFlash.color = new Color(1f, 0f, 0.15f, Mathf.Clamp01(flashTime) * 0.35f);
            }

            if (levelUpScreen.activeSelf) LevelUpInput();
        }

        // --------------------------------------------------------------- API

        public void Banner(string text, Color color)
        {
            bannerText.text = text;
            bannerText.color = color;
            bannerTime = 3f;
        }

        public void FlashDamage() => flashTime = 0.6f;

        public void ShowTitle() => titleScreen.SetActive(true);
        public void HideTitle() => titleScreen.SetActive(false);

        public void ShowLevelUp(List<UpgradeOption> opts, Action<UpgradeOption> chosen)
        {
            options = opts;
            onChosen = chosen;
            selected = 0;
            levelUpOpenedAt = Time.unscaledTime;
            for (int i = 0; i < cardFrames.Count; i++)
            {
                var card = cardFrames[i].transform;
                bool has = i < opts.Count;
                card.gameObject.SetActive(has);
                if (!has) continue;
                card.Find("Icon").GetComponent<Image>().sprite = Art.Get(opts[i].Icon);
                card.Find("Icon").GetComponent<Image>().preserveAspect = true;
                card.Find("Name").GetComponent<TextMeshProUGUI>().text = opts[i].Title;
                card.Find("Desc").GetComponent<TextMeshProUGUI>().text = opts[i].Description;
            }
            levelUpScreen.SetActive(true);
            RefreshSelection();
        }

        public void HideLevelUp() => levelUpScreen.SetActive(false);

        public bool LevelUpOpen => levelUpScreen.activeSelf;
        public void ChooseOption(int index) => Choose(index);

        void LevelUpInput()
        {
            // pequena trava para não escolher sem querer com o botão de dash
            if (Time.unscaledTime - levelUpOpenedAt < 0.35f) return;
            int nav = GameInput.NavPressed();
            if (nav != 0)
            {
                selected = (selected + nav + options.Count) % options.Count;
                RefreshSelection();
                AudioManager.Play("select", 0.6f, 0f);
            }
            int number = GameInput.NumberPressed();
            if (number >= 0 && number < options.Count) Choose(number);
            else if (GameInput.ConfirmPressed()) Choose(selected);
        }

        void RefreshSelection()
        {
            for (int i = 0; i < cardFrames.Count; i++)
            {
                bool sel = i == selected;
                cardFrames[i].color = sel ? Art.Yellow : new Color(0.3f, 0.2f, 0.5f);
                cardFrames[i].transform.localScale = Vector3.one * (sel ? 1.06f : 1f);
            }
        }

        void Choose(int index)
        {
            if (!levelUpScreen.activeSelf || options == null || index >= options.Count) return;
            levelUpScreen.SetActive(false);
            AudioManager.Play("select", 1f, 0f);
            var chosen = options[index];
            options = null;
            onChosen?.Invoke(chosen);
        }

        public void ShowEnd(bool victory)
        {
            endScreen.SetActive(true);
            endTitle.text = victory ? "EXTRAÇÃO CONCLUÍDA" : "VOCÊ FOI DELETADO";
            endTitle.color = victory ? Art.Cyan : Art.Pink;
            var gm = GameManager.I;
            endStats.text =
                $"TEMPO SOBREVIVIDO  <color=#F9F871>{Clock(waves.Elapsed)}</color>\n" +
                $"SCORE FINAL  <color=#F9F871>{gm.Score}</color>\n" +
                $"ABATES  {gm.Kills}     NÍVEL  {player.Level}\n" +
                (gm.BossKilled ? "<color=#05D9E8>MECH DE SEGURANÇA DESTRUÍDO</color>" : "");
        }
    }

    /// <summary>Faz um texto piscar (tempo real, funciona com o jogo pausado).</summary>
    public class Blink : MonoBehaviour
    {
        TMP_Text text;
        void Awake() => text = GetComponent<TMP_Text>();
        void Update()
        {
            var c = text.color;
            c.a = 0.55f + 0.45f * Mathf.Sin(Time.unscaledTime * 5f);
            text.color = c;
        }
    }
}
