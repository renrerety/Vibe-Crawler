Okay, adopting a decisive stance and selecting a specific, robust stack built around a framework for your PC/Mobile 2D roguelike, using Cursor as the IDE.

```markdown
# Technology Stack: Aetherium Depths

**Version:** 4.0
**Date:** 2023-10-27

## 1. Introduction

This document specifies the definitive technology stack for the development of "Aetherium Depths," a 2D top-down action roguelike targeting PC (Windows, macOS, Linux) and Mobile (iOS, Android). Development will utilize the **Cursor AI-enabled IDE**. A framework-based approach is mandated, avoiding full game engines. This stack prioritizes robustness, cross-platform capability, and effective integration with C# development workflows.

## 2. Core Technology Stack

### 2.1. Integrated Development Environment (IDE)

*   **Tool:** **Cursor** (Cursor.sh / Cursor.so)
*   **Rationale:** Specified requirement. Leverages AI assistance for C# code development, management, and debugging.

### 2.2. Core Game Development Framework

*   **Framework:** **MonoGame**
*   **Rationale:** Mature, open-source C# implementation of the XNA 4 API. Provides the necessary foundation for cross-platform 2D game development, including graphics rendering (`SpriteBatch`), audio management, input handling (Keyboard, Mouse, Gamepad, Touch), and a content pipeline for asset management. Offers direct control suitable for framework-based development while supporting PC and Mobile targets effectively.

### 2.3. Programming Language

*   **Language:** **C# (C-Sharp)**
*   **Rationale:** Native language for the MonoGame framework. A powerful, modern, object-oriented language well-suited for game logic development and supported by robust tooling, including the Cursor IDE.

### 2.4. Physics & Collision

*   **Approach:** **Custom Implementation**
*   **Rationale:** For core functionality, collision detection (e.g., Axis-Aligned Bounding Boxes - AABB) and simple physics responses (e.g., knockback) will be implemented manually. This avoids introducing external physics library dependencies initially, maintaining simplicity. If highly complex physics simulation becomes necessary later, integration of a library like Velcro Physics (Farseer Physics Fork) can be evaluated.

### 2.5. UI (User Interface)

*   **Approach:** **Custom Implementation using `SpriteBatch`**
*   **Rationale:** All UI elements (buttons, health bars, text displays) will be drawn directly using MonoGame's `SpriteBatch` capabilities and texture assets. Input logic for UI interaction will be handled within the custom UI system. This provides maximum control and avoids external UI library dependencies.


## 3. Conclusion

This technology stack establishes **MonoGame** as the core framework, utilizing **C#** for development within the **Cursor IDE**. Essential systems like physics and UI will be custom-built initially, leveraging MonoGame's primitives. **Git** hosted on **GitHub** ensures robust version control and project management. Standard, effective tools like **Aseprite** and **Audacity** are designated for asset creation. This stack provides the necessary components and control for building "Aetherium Depths" across PC and Mobile platforms under the framework-only constraint.
```