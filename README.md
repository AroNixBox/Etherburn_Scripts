# Etherburn: Game Scripts Collection for Unity

![GitHub Forks](https://img.shields.io/github/forks/AroNixBox/Etherburn_Scripts)  ![GitHub Contributors](https://img.shields.io/github/contributors/AroNixBox/Etherburn_Scripts)  ![GitHub Stars](https://img.shields.io/github/stars/AroNixBox/Etherburn_Scripts)  ![GitHub Repo Size](https://img.shields.io/github/repo-size/AroNixBox/Etherburn_Scripts)  

## Overview

**Etherburn** is a comprehensive collection of scripts tailored for building an action-oriented game. This repository includes behaviors for enemies, weapons, sensors, and UI components, making it an essential toolkit for game developers who wish to create an engaging player experience. The focus of this project is to provide ready-to-use game mechanics that help developers implement interactive and responsive gameplay in Unity.

## Features

- **Enemy Behavior Scripts**: Define enemy states and reactions based on player actions. Includes features like enemy detection, attack behaviors, and state transitions.
- **Player Actions**: Manage player interactions, including movement, attacks, and usage of special abilities.
- **Weapon System**: Implements different weapon behaviors, including melee and ranged weapon systems like bows and katanas.
- **Sensors**: Detect environmental changes, collisions, and interactions. This includes weapon hit detection and enemy proximity sensors.
- **UI Components**: Customizable health bars, radial selection tools, and other UI elements to keep players informed and engaged.

## Getting Started

### Prerequisites

To get started with **Etherburn**, ensure you have the following tools installed:
- Unity 6 or later
- C# development environment (e.g., Visual Studio or Rider)

### Installation

1. Clone the repository using the command:
   ```bash
   git clone https://github.com/Ddemon26/Etherburn_Scripts.git
   ```
2. Open the project in Unity.
3. Attach the desired scripts to appropriate GameObjects in your scene to implement their functionality.

## Usage Examples

### 1. Enemy Behavior

The **EnemyState** script can be used to manage different behaviors for an enemy character, such as patrolling, attacking, or fleeing. To use it:
- Attach the `EnemyState.cs` script to your enemy GameObject.
- Define the desired states in the Inspector view within Unity.

Example setup:
```csharp
public class EnemyState : MonoBehaviour {
    void Start() {
        // Initialize state (e.g., idle, patrol)
    }
    void Update() {
        // Handle transitions based on player proximity or health
    }
}
```

### 2. Player Weapon System

The **WeaponSO** scriptable objects manage weapon stats, such as damage and type. To implement a new weapon:
- Create a new ScriptableObject from the `WeaponSO` template.
- Assign weapon attributes (e.g., damage, attack speed).
- Attach the `BaseMeeleWeaponHitSensor.cs` to detect collisions with enemies.

### 3. UI Elements

The **AttributeBar** script allows you to display player health or stamina bars effectively. To use it:
- Attach `AttributeBar.cs` to a UI GameObject in your canvas.
- Link the UI slider component to update in real-time with player stats.

Example:
```csharp
public class AttributeBar : MonoBehaviour {
    public Slider healthBar;
    public void UpdateHealth(float currentHealth) {
        healthBar.value = currentHealth;
    }
}
```

## Folder Structure (General Overview)

- **Behavior/**: Contains scripts related to enemy AI and game events.
- **Player/**: Includes player movement, attacks, and weapon control scripts.
- **Sensor/**: Scripts that handle detection for enemies, weapons, and other gameplay elements.
- **UI/**: User interface elements like health bars and radial menus.

## Contributing

We welcome contributions to improve Etherburn. To contribute:
1. Fork the repository.
2. Create a feature branch (`git checkout -b feature-branch`).
3. Commit your changes (`git commit -m 'Add new feature'`).
4. Push to the branch (`git push origin feature-branch`).
5. Create a Pull Request.

## License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## Acknowledgements

Special thanks to all the contributors who have helped build and improve this project!

