using System;
using Extensions.FSM;
using Sirenix.OdinInspector;
using UnityEngine;

namespace Game {
    public class GameBrain : Extensions.Singleton<GameBrain> {
        [Header("References")]
        [Title("Systems")]
        [Required] public Player.Input.InputReader inputReader;
        [Required] public Camera menuCamera;
        
        [Title("User Interface")]
        [SerializeField, Required] UI.Menu.OptionMenuNavigation optionMenuNavigation;
        [Required] public UnityEngine.UI.Image fadeOutImage;
        public bool PauseToggleTriggered { get; set; }
        public bool GameOverTriggered { get; set; }
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
            Application.quitting += () => isQuitting = true;
        }

        void Start() {
            inputReader.Pause += () => PauseToggleTriggered = true;
            
            _stateMachine = new StateMachine();
            _stateMachine.OnDebugStateChanged += state => Debug.Log($"Current State: {state}");
            
            var menuState = new State.MenuState(inputReader, this);
            var gameOverState = new State.GameOverState(inputReader, this);
            var playState = new State.PlayState(inputReader, this);
            var pauseState = new State.PauseState(inputReader, this, optionMenuNavigation);
            
            At(menuState, playState, () => PlayTriggered);
            At(menuState, pauseState, () => PauseToggleTriggered);
            
            At(playState, pauseState, () => PauseToggleTriggered);
            At(playState, gameOverState, () => GameOverTriggered);
            
            At(pauseState, playState, () => PauseToggleTriggered && SceneLoader.Instance.IsInLevel());
            At(pauseState, menuState, () => PauseToggleTriggered && !SceneLoader.Instance.IsInLevel());
            
            At(gameOverState, menuState, () => QuitTriggered);
            
            _stateMachine.SetInitialState(menuState);
            
            return;
            
            void Any(IState to, Func<bool> condition) => _stateMachine.AddAnyTransition(to, condition);
            void At(IState from, IState to, Func<bool> condition) => _stateMachine.AddTransition(from, to, condition);
        }
        
        public bool IsGamePaused => _stateMachine.GetCurrentState() is State.PauseState;

        void Update() {
            _stateMachine.Tick();
        }

        void FixedUpdate() {
            _stateMachine.FixedTick();
        }
        
        public async void UninitializeGame() {
            if (isQuitting) return;
            
            await FadeOutAsync();
            
            var sceneLoader = SceneLoader.Instance;
            if (sceneLoader == null) {
                Debug.LogError("SceneLoader is not set in the inspector");
                return;
            }
            
            sceneLoader.UnloadScenes(SceneData.ELevelType.Level_One);
            
            // Trigger Game Over State
            GameOverTriggered = true;
        }

        async System.Threading.Tasks.Task FadeOutAsync() {
            var duration = 1f;
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
            
            // Reset the Game Over Image
            ResetFadeOutImage();
        }
        void ResetFadeOutImage() {
            var color = fadeOutImage.color;
            color.a = 0;
            fadeOutImage.color = color;
        }
    }
}
