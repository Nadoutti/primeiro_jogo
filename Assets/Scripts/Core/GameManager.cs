using UnityEngine;
using UnityEngine.SceneManagement;

namespace NeonSurvivor
{
    public enum GameState { Title, Playing, LevelUp, GameOver, Victory }

    /// <summary>Estado da partida, score e transições entre telas.</summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager I { get; private set; }

        public PlayerController player;
        public WaveDirector waves;
        public UpgradeSystem upgrades;
        public HUD hud;

        public GameState State { get; private set; } = GameState.Title;
        public int Score { get; private set; }
        public int Kills { get; private set; }
        public bool BossKilled { get; private set; }

        float endTime;

        public static bool IsPlaying => I != null && I.State == GameState.Playing;

        void Awake()
        {
            I = this;
            Time.timeScale = 0f;
        }

        void Start()
        {
            hud.ShowTitle();
            AudioManager.I.PlayMusic();
        }

        void Update()
        {
            switch (State)
            {
                case GameState.Title:
                    if (GameInput.ConfirmPressed()) StartRun();
                    break;
                case GameState.GameOver:
                case GameState.Victory:
                    if (Time.unscaledTime - endTime > 1.2f && GameInput.ConfirmPressed())
                        SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                    break;
            }
        }

        public void StartRun()
        {
            State = GameState.Playing;
            Time.timeScale = 1f;
            hud.HideTitle();
            player.GetComponent<PlasmaGun>().LevelUp();
            AudioManager.Play("select");
        }

        public void AddKill(int points)
        {
            Kills++;
            Score += points;
        }

        public void AddScore(int points) => Score += points;

        public void OnBossKilled()
        {
            BossKilled = true;
            Score += 1000;
            hud.Banner("MECH DESTRUÍDO! +1000", Art.Yellow);
        }

        public void OpenLevelUp()
        {
            if (State != GameState.Playing) return;
            State = GameState.LevelUp;
            Time.timeScale = 0f;
            AudioManager.Play("levelup", 0.9f, 0f);
            hud.ShowLevelUp(upgrades.Roll(3), OnUpgradeChosen);
        }

        void OnUpgradeChosen(UpgradeOption option)
        {
            option.Apply();
            player.PendingLevelUps--;
            State = GameState.Playing;
            Time.timeScale = 1f;
            if (player.PendingLevelUps > 0) OpenLevelUp();
        }

        public void GameOver()
        {
            if (State == GameState.GameOver || State == GameState.Victory) return;
            End(GameState.GameOver);
            AudioManager.Play("gameover", 1f, 0f);
        }

        public void Victory()
        {
            if (State == GameState.GameOver || State == GameState.Victory) return;
            Score += Mathf.RoundToInt(player.Health) * 5;
            End(GameState.Victory);
            AudioManager.Play("victory", 1f, 0f);
        }

        void End(GameState state)
        {
            // bônus de tempo: quanto mais tempo vivo, mais pontos
            Score += Mathf.FloorToInt(waves.Elapsed) * 5;
            State = state;
            endTime = Time.unscaledTime;
            Time.timeScale = 0f;
            hud.HideLevelUp();
            hud.ShowEnd(state == GameState.Victory);
        }
    }
}
