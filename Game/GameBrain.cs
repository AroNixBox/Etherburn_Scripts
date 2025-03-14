using System;
using Extensions.FSM;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Game {
    public class GameBrain : Extensions.Singleton<GameBrain> {
        [Header("References")]
        [Title("Systems")]
        [Required] public Player.Input.InputReader inputReader;
        [Required] public Camera menuCamera;
        
        [Title("User Interface")]
        // Reactivate the Game Over Image to layer on top of everything
        [SerializeField, Required] Canvas fadeOutCanvas;
        [Required] public UnityEngine.UI.Image fadeOutImage;
        public bool PauseToggleTriggered { get; set; }
        public bool GameOverTriggered { get; set; }
        public bool HomePressed { get; set; }
        public bool QuitTriggered { get; set; }
        public bool PlayTriggered { get; set; }
        
        StateMachine _stateMachine;
        static bool isQuitting;
        protected override bool ShouldPersist => true;

        protected override void Awake() {
            base.Awake();
            
            inputReader.InitializeInputActionAsset();
            // Check if Playeractions are enabled
            inputReader.EnablePlayerActions();
        }
        
        void OnEnable() {
            // Prevent Hooking into the Player Death Event w/ Game Over Screen when quitting
            Application.quitting += SetQuitting;
        }

        void Start() {
            fadeOutCanvas.gameObject.SetActive(false);
            
            inputReader.Pause += PauseGame;
            
            _stateMachine = new StateMachine();
            _stateMachine.OnDebugStateChanged += DebugStates;
            
            var menuState = new State.MenuState(inputReader, this);
            var gameOverState = new State.GameOverState(inputReader, this);
            var playState = new State.PlayState(inputReader, this);
            var pauseState = new State.PauseState(inputReader, this);
            
            At(menuState, playState, () => PlayTriggered);
            At(menuState, pauseState, () => PauseToggleTriggered);
            
            At(playState, pauseState, () => PauseToggleTriggered && !playState.IsGameUnInitializing);
            At(playState, gameOverState, () => GameOverTriggered);
            
            At(pauseState, playState, () => PauseToggleTriggered && SceneLoader.Instance.IsInLevel() && !pauseState.IsGameUnInitializing);
            At(pauseState, menuState, () => (PauseToggleTriggered && !SceneLoader.Instance.IsInLevel()) || pauseState.ReadyForMainMenu && !pauseState.IsGameUnInitializing);
            At(pauseState, gameOverState, () => GameOverTriggered);
            
            At(gameOverState, menuState, () => QuitTriggered);
            
            IState initialState = menuState;
            
            #if UNITY_EDITOR
            
            // If we are in the editor and not in the Bootstrapper Scene..
            if (SceneManager.GetActiveScene().name != "LevelBootstrapper") {
                initialState = playState;
            }
            
            #endif
            
            _stateMachine.SetInitialState(initialState);
            
            return;
            
            void Any(IState to, Func<bool> condition) => _stateMachine.AddAnyTransition(to, condition);
            void At(IState from, IState to, Func<bool> condition) => _stateMachine.AddTransition(from, to, condition);
        }
        
        void PauseGame() => PauseToggleTriggered = true;
        void SetQuitting() => isQuitting = true;
        void DebugStates(string state) {
            Debug.Log($"Current State: {state}");
        }
        public bool IsGamePaused => _stateMachine.GetCurrentState() is State.PauseState;
        void Update() {
            _stateMachine.Tick();
        }

        void FixedUpdate() {
            _stateMachine.FixedTick();
        }
        
        public async System.Threading.Tasks.Task UninitializeGame() {
            if (isQuitting) return;
            
            await FadeOutAsync(2f);
            
            var sceneLoader = SceneLoader.Instance;
            if (sceneLoader == null) {
                Debug.LogError("SceneLoader is not set in the inspector");
                return;
            }
            
            await sceneLoader.UnloadScenes(SceneData.ELevelType.Level_One);
            
            // Reset the Game Over Image
            ResetFadeOutImage();
            
            GameOverTriggered = true;
        }
        
        public async System.Threading.Tasks.Task UninitializeGame(bool isGameOver) {
            if (isQuitting) return;
                
            await FadeOutAsync(1);
            
            var sceneLoader = SceneLoader.Instance;
            if (sceneLoader == null) {
                Debug.LogError("SceneLoader is not set in the inspector");
                return;
            }
            
            await sceneLoader.UnloadScenes(SceneData.ELevelType.Level_One);

            if (isGameOver) {
                GameOverTriggered = true;
            }
            // Reset the Game Over Image
            ResetFadeOutImage();
        }

        async System.Threading.Tasks.Task FadeOutAsync(float duration) {
            fadeOutCanvas.gameObject.SetActive(true);
            
            var elapsedTime = 0f;
            var color = fadeOutImage.color;
            color.a = 0;
            fadeOutImage.color = color;

            while (elapsedTime < duration) {
                elapsedTime += Time.deltaTime;
                color.a = Mathf.Lerp(0f, 1f, elapsedTime / duration);
                fadeOutImage.color = color;
                await System.Threading.Tasks.Task.Yield();
            }
            
            fadeOutCanvas.gameObject.SetActive(false);
        }
        void ResetFadeOutImage() {
            var color = fadeOutImage.color;
            color.a = 0;
            fadeOutImage.color = color;
        }

        void OnDestroy() {
            if (inputReader != null) {
                inputReader.Pause -= PauseGame;
            }
            
            if (_stateMachine != null) {
                _stateMachine.OnDebugStateChanged -= DebugStates;
            }
            
            Application.quitting -= SetQuitting;
        }
    }
}
