# Cooperative 2D Platformer

This project is a synchronized multiplayer **2D platformer** prototype built with:

- **Netcode for GameObjects** (Networking)
- **Unity Relay** (Connection handling)
- **Unity Lobby** (Session management)
- **UniTask** (Async workflow)

## Key Features

### 1. Multiplayer Infrastructure
- **Anonymous Authentication**: seamless sign-in via Unity Gaming Services.
- **Relay & Lobby Integration**: host creates a room, clients join via a **Lobby Code** or a dynamic room list.
- **NetworkBootstrap**: centralized manager for the network lifecycle (Auth -> Relay -> Lobby).

### 2. Player Controller
- **2D Platformer Logic**: supports horizontal movement and jumping (`NetworkPlayerController`).
- **Responsiveness**: uses local authority for movement to ensure zero-latency input for the player.
- **Server Verification**: uses `ServerRpc` for jump triggers to maintain synchronization across all clients.

### 3. Developer Tools
- **AutoSceneSetup**: an Editor tool that builds a complete, playable network scene in one click.
- Automates the creation of `NetworkManager`, `EventSystem`, `Lobby UI`, and player prefabs.

### 4. Dynamic UI
- **Lobby Management**: UI for hosting, joining, and browsing active rooms.
- **Gameplay HUD**: real-time display of connected players and synchronization of session state.
