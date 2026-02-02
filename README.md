<p align="center">
    <img src=".github/assets/logo.png" alt="bianbeat">
</p>
<p align="center">
   <i>Submission for Global Game Jam 2026.</i>
</p>

# Bian Beat
Bian Beat is a rhythm game that's played with the expressions you make! Smile, frown, scowl, and pogface to the beat. Webcam required.

This game is inspired by Bian Lian (變臉), or "face-changing," a 300-year-old, highly skilled dramatic art form in Chinese Sichuan opera, where performers instantly switch vividly coloured, traditional masks to represent different emotions.

# Requirements
# BianLian
Game for Global Game Jam 2026.

A rhythm game where you match facial expressions to hit targets on the beat. Use your webcam to make expressions (Happy, Sad, Angry, Shocked) and match them with incoming mask targets!

### Software
- **Unity Editor**: Version 6000.3.5f2 (Unity 6)
- **Unity Packages**:
  - [MediaPipeUnityPlugin](https://github.com/homuler/MediaPipeUnityPlugin) - For real-time face detection and landmark tracking
  - Unity Barracuda (3.0.2) - For running the BLEM (BianLian Expression Model) neural network inference
  - Universal Render Pipeline (17.3.0) - For rendering
  - Unity Input System (1.17.0) - For input handling

### Hardware
- **Webcam/Camera**: Required for face detection and expression recognition
  - The game uses your webcam feed to detect facial expressions in real-time

## Game Mechanics
### Core Gameplay
1. **Target Spawning**: Mask targets spawn from different spawn points based on the beatmap timing
2. **Expression Matching**: Each target requires a specific facial expression:
   - **Happy** - Smile to match
   - **Sad** - Frown to match
   - **Angry** - Angry expression to match
   - **Shocked** - Surprised expression to match
3. **Target Interaction**: 
   - Targets move toward the center of the screen and scale up as they approach
   - Match your expression when the target reaches the center to score
   - Targets will despawn if you don't match the expression in time
4. **Beatmap System**: 
   - Game uses CSV-based beatmap files (`Mask_Time_Stamp.csv`)
   - Each note contains a timestamp and required expression
   - Music plays in sync with the beatmap

### Face Detection System
- Uses **MediaPipe Face Landmarker** to detect facial features in real-time
- **BLEM (BianLian Expression Model)**: Custom neural network that processes MediaPipe blend shape data to classify expressions
- Expression detection uses confidence thresholds and hysteresis to prevent rapid switching
- Supports 5-frame temporal analysis for smoother expression recognition

## Environment Setup
### Pre-built Executable
A Windows build is available on the [GGJ page](https://globalgamejam.org/games/2026/mask-game-5) 

## Libraries
- [MediaPipeUnityPlugin](https://github.com/homuler/MediaPipeUnityPlugin) - Real-time face detection and landmark tracking
- [Unity Barracuda](https://docs.unity3d.com/Packages/com.unity.barracuda@1.0/manual/index.html) - Provides inference in Unity in real-time