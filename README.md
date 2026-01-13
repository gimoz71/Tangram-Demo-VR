# Tangram VR Demo - Setup Tecnico & Configurazione VR

## Piattaforma di Destinazione

* **Hardware:** Meta Quest 2 / Meta Quest 3
* **OS:** Android
* **Architettura:** ARM64

## Pacchetti Core & Dipendenze

* **Render Pipeline:** Universal Render Pipeline (URP)
* **XR Framework:** XR Interaction Toolkit (v2.6.5+)
* **Input System:** Unity Input System (Action-based)
* **XR Plugin Management:** OpenXR / Oculus

## Configurazione Rendering (Asset URP)

Impostazioni ottimizzate per VR standalone:

* **HDR:** **Disabilitato** (OFF) - Critico per ottimizzazione memoria e performance.
* **Post-Processing:**
* **Tonemapping:** Abilitato (Modalità: *ACES* su LDR) via Global Volume.
* **Color Adjustments:** Post Exposure / Contrast / Saturation attivi per compensare l'assenza di HDR.

* **Lighting:**
* **Main Light:** Baked (Mixed Lighting).
* **Additional Lights:** Realtime (Spotlights).

* **Shadows (Ombre):**
* **Soft Shadows:** Abilitate.
* **Additional Lights Shadowmap Resolution:** 2048 (per eliminare artefatti/aliasing su luci dinamiche).
* **Shadow Distance:** Ottimizzata per room-scale (15-20m).



## Baking & Lightmapping

* **Lightmap Resolution:** Bassa/Media (Globale).
* **Scale in Lightmap:** Aumentata (2x - 4x) specificamente su Tavoli interattivi/Props per evitare ombre scalettate (aliasing).
* **Filtering:** Advanced.
* **Compression:** High Quality (o *None* se persistono artefatti visivi).

## Setup Interazione XR

Configurazione basata su *Starter Assets* modificati.

### Interaction Layers

* **Teleport Interactor (Ray):** Mask impostata solo su layer `Teleport`.
* **Teleport Area/Anchor:** Layer impostato su `Teleport`.
* **Physics Ray:** Mask impostata su `Everything` (escluso Teleport).

### Schema di Locomozione

Gestione input separata per evitare conflitti:

* **Left Controller:**
* *Move:* Abilitato (Continuous Move Provider).
* *Turn:* Disabilitato.
* *Teleport:* Gestito via script custom.

* **Right Controller:**
* *Move:* Disabilitato.
* *Turn:* Abilitato (Snap Turn Provider).
* *Teleport:* Gestito via script custom.

### Scripting Custom

* **`TeleportToggler.cs`:**
* Toggle runtime per abilitare/disabilitare il Teleport Interactor.
* Forza la separazione rigida dei ruoli joystick (Move vs Turn) all'inizializzazione.
* Disabilita la logica standard `ActionBasedControllerManager` per prevenire override degli input.

* **`TangramLogger.cs`:**
* **Funzione:** Traccia le interazioni utente con i pezzi del Tangram.
* **Eventi:** Iscrizione a `SelectEntered` (Presa/Grab) e `SelectExited` (Rilascio/Release) su oggetti `XRGrabInteractable`.
* **Output:** Aggiunge dati a file CSV in `Application.persistentDataPath`.
* **Formato Dati:** `Timestamp, EventType (GRAB/RELEASE), ObjectName`.


## Note di Sviluppo

* **Legacy VR:** Rimossi script obsoleti basati su `XRSettings.enabled` e tutte le librerie e riferimenti a SteamVR
* **Assembly Definitions:** Eliminato file `.asmdef` dagli *Starter Assets* per consentire l'accesso agli script interni (`ActionBasedControllerManager`) dal codice utente.
