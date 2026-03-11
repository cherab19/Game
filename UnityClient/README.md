# Unity Client (UI Toolkit + REST + WSS)

This module provides a Unity C# frontend designed to integrate with a Lovable-hosted backend and the backend implementation in this repository.

## Included

- JWT authentication client for Lovable `/auth/register` and `/auth/login`
- Matchmaking client for Lovable `/matchmaking/join` and `/matchmaking/start`
- WebSocket realtime client for Lovable `/ws/game`
- Leaderboard client for Lovable `/leaderboards/top` and `/leaderboards/update`
- UI Toolkit controllers and starter UXML/USS
- Match result sync service that converts realtime gameplay activity into `/leaderboards/update` submissions
- Multiplayer stack abstraction with adapters for:
  - Unity Netcode
  - Mirror
  - Photon

## Folder Layout

- `Assets/Scripts/Config`: backend URL and security settings
- `Assets/Scripts/Contracts`: JSON contracts matching backend responses
- `Assets/Scripts/Services`: REST and WebSocket clients
- `Assets/Scripts/Networking`: pluggable multiplayer adapters
- `Assets/Scripts/UI`: UI Toolkit controllers + realtime publisher
- `Assets/UI/UXML`: screen structure
- `Assets/UI/USS`: styles

## Unity Setup

1. Create or open your Unity project.
2. Copy `UnityClient/Assets` into your Unity project `Assets` folder.
3. Install package: `com.unity.nuget.newtonsoft-json`.
4. Create `BackendConfig` asset:
   - `Create > Game > Backend Config`
   - Set REST URL (HTTPS): `https://your-lovable-backend-host`
   - Set WebSocket URL (WSS): `wss://your-lovable-backend-host/ws/game`
5. Create a scene with:
   - `UIDocument` using `Assets/UI/UXML/GameClient.uxml`
   - A GameObject with `GameClientBootstrap`
   - GameObjects with `AuthViewController`, `MatchmakingViewController`, `LeaderboardViewController`, `RealtimeHudController`
6. Reference the same `GameClientBootstrap` and `UIDocument` in each controller.

## Secure Transport

- Production should use HTTPS for REST and WSS for WebSocket.
- `BackendConfig` blocks non-secure endpoints by default.
- For localhost testing only, enable `Allow Insecure Localhost`.

## Multiplayer Stack Sync

`GameClientBootstrap` selects the adapter:

- `UnityNetcodeAdapter`
- `MirrorAdapter`
- `PhotonAdapter`

Current adapters are integration stubs with compile guards (`UNITY_NETCODE_GAMEOBJECTS`, `MIRROR`, `PHOTON_UNITY_NETWORKING`) so you can bind your project-specific spawn, ownership, and state synchronization code while reusing backend-auth/matchmaking flow.

## Realtime Events

Inbound and outbound event types:

- `player_joined`
- `player_moved`
- `shot_fired`
- `collision`
- `powerup_spawned`

`RealtimeGameplayPublisher` exposes helper methods to publish movement, shooting, collisions, and power-up spawns.

`MatchResultSyncService` tracks gameplay events (shots, collisions, powerups, kills, deaths) and posts aggregate leaderboard updates through `/leaderboards/update`.

## Verification Checklist

1. Authenticate via UI (login/register) and confirm JWT session starts.
2. Join and start a match from matchmaking UI.
3. Publish gameplay events through `RealtimeGameplayPublisher` hooks in your gameplay loop.
4. Trigger `Post Match Result` and confirm leaderboard reflects the update.
5. Verify backend persistence by querying PostgreSQL (`players`, `lobbies`, `games`, `events`, `leaderboards`).
