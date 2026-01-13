# Tangram VR Demo: Technical Setup & VR Configuration

## Target Platform

* **Hardware:** Meta Quest 2 / Meta Quest 3
* **OS:** Android
* **Architecture:** ARM64

## Core Packages & Dependencies

* **Render Pipeline:** Universal Render Pipeline (URP)
* **XR Framework:** XR Interaction Toolkit (v2.6.5+)
* **Input System:** Unity Input System (Action-based)
* **XR Plugin Management:** OpenXR / Oculus

## Rendering Configuration (URP Assets)

Impostazioni ottimizzate per standalone VR:

* **HDR:** **Disabled** (OFF) - Per ottimizzazione memoria e performance.
* **Post-Processing:**
* **Tonemapping:** Enabled (Mode: *ACES* su LDR) via Global Volume.
* **Color Adjustments:** Post Exposure / Contrast / Saturation attivi per compensare assenza HDR.


* **Lighting:**
* **Main Light:** Baked (Mixed Lighting).
* **Additional Lights:** Realtime (Spotlights).


* **Shadows:**
* **Soft Shadows:** Enabled.
* **Additional Lights Shadowmap Resolution:** 2048 (per eliminare artefatti su luci dinamiche).
* **Shadow Distance:** Ottimizzata per room-scale (15-20m).



## Baking & Lightmapping

* **Lightmap Resolution:** Low/Medium (globale).
* **Scale in Lightmap:** Aumentata (2x - 4x) specificamente su oggetti di interazione (Tavolo, props statici) per evitare aliasing.
* **Filtering:** Advanced.
* **Compression:** High Quality (o *None* se presenti artefatti critici).

## XR Interaction Setup

Configurazione basata su *ActionBasedControllerManager* (Starter Assets) modificata.

### Interaction Layers

* **Teleport Interactor (Ray):** Mask su layer `Teleport`.
* **Teleport Area/Anchor:** Layer `Teleport`.
* **Physics Ray:** Mask su `Everything` (escluso Teleport se necessario).

### Locomotion Scheme

Gestione input separata per evitare conflitti:

* **Left Controller:**
* *Move:* Enabled (Continuous Move Provider).
* *Turn:* Disabled.
* *Teleport:* Gestito via script custom.


* **Right Controller:**
* *Move:* Disabled.
* *Turn:* Enabled (Snap Turn Provider).
* *Teleport:* Gestito via script custom.



### Custom Scripting

* **`TeleportToggler.cs`:**
* Gestisce l'abilitazione/disabilitazione runtime del Teleport Interactor.
* Forza la separazione dei ruoli dei joystick (Move vs Turn) all'avvio.
* Disabilita la logica `ActionBasedControllerManager` standard per prevenire conflitti di input.



## Note di Sviluppo

* **Legacy VR:** Script obsoleti basati su `XRSettings.enabled` rimossi.
* **Assembly Definitions:** File `.asmdef` degli *Starter Assets* rimosso per permettere l'accesso agli script interni (`ActionBasedControllerManager`) da codice utente.
