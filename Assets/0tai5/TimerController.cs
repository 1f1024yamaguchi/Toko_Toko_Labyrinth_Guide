using UnityEngine;
using TMPro; // TextMeshPro用

namespace Otai5
{
    /// <summary>
    /// 画面上部中央に表示するカウントアップタイマー
    /// </summary>
    public class TimerController : MonoBehaviour
    {
        [Header("UI Reference")]
        [Tooltip("時間を表示するTextMeshProテキスト（未設定なら自動検索します）")]
        [SerializeField] private TextMeshProUGUI timerText;

        [Header("Settings")]
        [Tooltip("ゲーム開始時に自動で計測を開始するか")]
        [SerializeField] private bool autoStart = true;

        [Tooltip("小数点以下の秒（00:00.00）も表示するか")]
        [SerializeField] private bool showMilliseconds = false;

        private float elapsedTime = 0f;
        private bool isRunning = false;

        public float ElapsedTime => elapsedTime;
        public bool IsRunning => isRunning;

        private void Awake()
        {
            // インスペクターで未設定の場合、自分自身または子要素から自動取得
            if (timerText == null)
            {
                timerText = GetComponent<TextMeshProUGUI>();
            }
        }

        private void Start()
        {
            UpdateTimerDisplay();
            if (autoStart)
            {
                StartTimer();
            }
        }

        private void Update()
        {
            if (!isRunning) return;

            elapsedTime += Time.deltaTime;
            UpdateTimerDisplay();
        }

        /// <summary>
        /// 画面のタイマー表示を「00:00」形式に更新
        /// </summary>
        private void UpdateTimerDisplay()
        {
            if (timerText == null) return;

            int minutes = Mathf.FloorToInt(elapsedTime / 60f);
            int seconds = Mathf.FloorToInt(elapsedTime % 60f);

            if (showMilliseconds)
            {
                int fraction = Mathf.FloorToInt((elapsedTime * 100f) % 100f);
                timerText.text = string.Format("{0:00}:{1:00}.{2:00}", minutes, seconds, fraction);
            }
            else
            {
                timerText.text = string.Format("{0:00}:{1:00}", minutes, seconds);
            }
        }

        /// <summary>
        /// タイマー開始
        /// </summary>
        public void StartTimer()
        {
            isRunning = true;
        }

        /// <summary>
        /// タイマー一時停止（ポーズ時など）
        /// </summary>
        public void PauseTimer()
        {
            isRunning = false;
        }

        /// <summary>
        /// タイマー再開
        /// </summary>
        public void ResumeTimer()
        {
            isRunning = true;
        }

        /// <summary>
        /// タイマーを0にリセット
        /// </summary>
        public void ResetTimer()
        {
            elapsedTime = 0f;
            UpdateTimerDisplay();
        }
    }
}
