using System;
using Interfaces.Attribute;
using Player.Ability;
using Player.Audio;
using Player.Cam;
using Player.Input;
using Sirenix.OdinInspector;
using UI;
using UnityEngine;
using UnityEngine.Serialization;

namespace Player {
    /* @ Explanation
     * Nothing in this class needs to be saved!
     * Everything assigned is not changed during runtime.
     */
    public partial class References : MonoBehaviour {
        [Title("References")]
        public InputReader input;
        public Mover mover;
        public Weapon.WeaponManager weaponManager;
        public Animation.AnimationController animationController;
        public OrbitalController orbitalController;
        [FormerlySerializedAs("abilityTargetProvider")] public AbilityTargetQuery abilityTargetQuery;
        public CapsuleCollider collider;

        
        [Header("Body Parts")]
        public Transform modelRoot;
        public Transform weaponSocket;
        [Tooltip("Position where Swash Particle FX Should be spawned")]
        public Transform vfxSpawnPointRight;
        
        [Header("Attributes")]
        [ValidateInput("@Extensions.ClassExtensions.IsClass<IHealth, MonoBehaviour>(health)", "The assigned object must implement IHealth.")]
        [SerializeField] MonoBehaviour health;
        public IHealth HealthAttribute { get; private set; }
        [ValidateInput("@Extensions.ClassExtensions.IsClass<IEnergy, MonoBehaviour>(stamina)", "The assigned object must implement IEnergy.")]
        [SerializeField] MonoBehaviour stamina;
        public IEnergy StaminaAttribute { get; private set; }
        [ValidateInput("@Extensions.ClassExtensions.IsClass<IEnergy, MonoBehaviour>(ultimate)", "The assigned object must implement IEnergy.")]
        [SerializeField] MonoBehaviour ultimate;
        public IEnergy UltimateAttribute { get; private set; }

        [Header("UI")]
        [Tooltip("WeaponWheel UI")] 
        public RadialSelection radialSelection;
        
        [Header("Audio")]
        public AudioSource weapon2DSource;
        public PlayerSounds playerSounds;


        [Title("Player Stats")] 
        [Tooltip("How fast do we start/ stop => running/ walking, the bigger the value, the slower")] 
        public float speedLerpRate = 10f;
        [Tooltip("How fast do we change direction, the bigger the value, the slower")]
        public float directionLerpRate = 5f;
        public float stopLerpSpeedWhenNoInputEnabled = 2.5f;
        [Header("Stamina Costs")]
        public float runStaminaCostPerSecond = 5f;
        public float dodgeStaminaCost = 10f;
        
        [Header("Movement Speed: Controlled by RM Animations")]
        [Tooltip("WalkSpeed in the Animator Controller")]
        [ReadOnly] public float walkSpeedInAnimator = 0.5f;
        [Tooltip("RunSpeed in the Animator Controller")]
        [ReadOnly] public float runSpeedInAnimator = 1f;
        [Range(0, 1)] [Tooltip("The Threshold where Camera Rotation influences players walking direction" +
                               "The Speed is the Animator Speed")]
        public float minPlayerSpeedWhereCameraRotatesModel = 0.25f;
                
        [Header("Vision Cone")]
        public float detectionRadius = 10f;
        public float visionConeAngle = 60f;
        
        [Header("Target Check")]
        public Transform[] rayCheckOrigins;
        public int maxTargetToCheckAround = 10;
        
        
        [Header("Motion Warp")]
        [Tooltip("This is not used for any maths, this is only for a check to see if we can reach the target with the warp."
                 + "Because on long distance the warp doesnt hit the target perfectly, this would be too far then."
                 + "This basically multiplies the root motion distance from start to end-warpframe with the multiplier to deny the warp.")]
        public float warpRootMotionMultiplier = 8f;

        void Awake() {
            StaminaAttribute = (IEnergy) stamina;
            UltimateAttribute = (IEnergy) ultimate;
            HealthAttribute = (IHealth) health;
        }

        void Start() {
            // Player Input Events
            input.Move += OnMove;
            input.Run += OnRun;
            input.Dodge += OnDodge;
            input.Attack += OnAttack;
            input.SecondAttack += OnSecondAttack;
            input.Ultimate += OnUltimate;
            
            // Global Events (Always Active)
            input.MiddleClickUI += OnMiddleClickUI;
        }
    }

    // Internal References, also doesnt need to be saved
    public partial class References {
        // This is used to check which attack clip we need to use
        // If we have two attacks one after another we need to animation states with different clips, to not cause bugs in replacing the Animations
        // We can not hold this in the AttackState, because we have two Instances of that State, so two different bools, which causes bugs
        public bool UseFirstAttackClip { get; set; }
        #region Input Playermap caching
        public bool DodgeKeyPressed { get; private set; }
        public bool RunKeyPressed { get; private set; }
        public bool AttackKeyPressed { get; private set; }
        public bool SecondAttackKeyPressed { get; private set; }
        public bool UltimateKeyPressed { get; private set; }
        public Vector2 MovementInput { get; private set; }

        void OnRun(bool isKeyPressed) => RunKeyPressed = isKeyPressed;
        void OnMove(Vector2 movementInput) => MovementInput = movementInput;
        void OnDodge(bool isKeyPressed) => DodgeKeyPressed = isKeyPressed;
        void OnAttack(bool isKeyPressed) => AttackKeyPressed = isKeyPressed;
        void OnSecondAttack(bool isKeyPressed) => SecondAttackKeyPressed = isKeyPressed;
        void OnUltimate(bool isKeyPressed) => UltimateKeyPressed = isKeyPressed;
        #endregion

        #region Input UIMap caching
        public bool MiddleKeyPressed { get; private set; }
        void OnMiddleClickUI(bool isKeyPressed) => MiddleKeyPressed = isKeyPressed;
        #endregion

        #region Animation Event Bools
        public bool DodgeEnded { get; set; }
        public bool LandEnded { get; set; }
        public bool InAnimationWarpFrames { get; set; }
        public bool ExecutionEnded { get; set; }
        public bool AttackEnded { get; set; }
        public bool GetHitEnded { get; set; }

        #endregion

        #region Animation Event Events
        public Action SpawnParticles { get; set; } = delegate { };
        public Action EnableHitDetection { get; set; } = delegate { };
        public Action DisableHitDetection { get; set; } = delegate { };

        #endregion
    }
}