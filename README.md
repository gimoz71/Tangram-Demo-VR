# Tangram VR Demo - Setup Tecnico & Configurazione VR

## Piattaforma di Destinazione

* **Hardware:** Meta Quest 2 / Meta Quest 3
* **OS:** Android
* **Architettura:** ARM64

## Versione Unity

* **2022.3.52f3 (LTS)**

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

## Architettura Software & Scripting Custom

Il sistema si basa su quattro pilastri: Logica di Gioco, Tracciamento Utente, Modulazione Stress e Data Logging.

### 1. Core Logic & Game Manager

* **`TangramPatternMatcher.cs`:**
    * **Funzione:** Verifica il completamento del puzzle.
    * **Logica Relativa:** Calcola posizione/rotazione dei pezzi rispetto a un pezzo "Anchor" (Capo), permettendo la risoluzione ovunque nello spazio.
    * **Win Condition (Strict):** La vittoria scatta solo se i pezzi sono posizionati correttamente E **tutti i pezzi sono stati rilasciati** (incluso l'Anchor).
    * **Eventi:** Invoca `OnWin` (Audio, FX, Stop Logging, Stop Timer).
* **`TeleportToggler.cs`:**
    * Toggle runtime per abilitare/disabilitare il Teleport Interactor.
    * Forza la separazione rigida dei ruoli joystick (Move vs Turn) all'inizializzazione.
    * Disabilita la logica standard `ActionBasedControllerManager`.

### 2. Modulazione Stress & Behavioral Tracking

* **`TangramTimer.cs`:**
    * **Funzione:** Modulo indipendente progettato per indurre pressione temporale (stressor) nell'utente tramite feedback visivi e uditivi, senza causare hard-lock o interruzioni del gameplay alla scadenza.
    * **Gestione Soglia (Pressure Threshold):** Attivabile a *X* secondi dalla fine (`pressureThreshold`). Cambia dinamicamente il colore del testo UI (rosso) e avvia un tick audio singolo rigorosamente sincronizzato in realtime al calcolo del secondo intero (`Mathf.CeilToInt`).
    * **Tracciamento "Curva di Stress" (Marker CSV):** Il modulo inietta eventi specifici nel `TangramLogger` per mappare l'andamento comportamentale e cognitivo dell'utente sotto pressione:
        * `Pressure_Phase_Started`: Registrato all'innesco della soglia di stress. Consente l'analisi comparativa (A/B) delle metriche motorie (Grab/Gaze) in stato di quiete versus stato di allerta.
        * `Timer_Reached_Zero`: Marcatore di esaurimento del tempo. Isola i dati successivi per l'analisi del comportamento in fase di "post-scadenza" e tolleranza alla frustrazione.
        * `Timer_Stopped_On_Win`: Registrato al completamento del puzzle. Il valore `Duration` associato quantifica il tempo residuo di anticipo (se > 0) o conferma matematicamente la risoluzione in post-scadenza (se = 0.0).

### 3. Data Logging System

* **`TangramLogger.cs`:**
    * **Funzione:** Centralizza la raccolta dati e la scrittura su file CSV.
    * **Gestione Sessioni:** Aggiunge automaticamente un header e una riga vuota se il file esiste già (append mode).
    * **Durata Azioni:**
        * *GRAB:* Calcola la durata (DeltaTime) tra `SelectEntered` e `SelectExited` usando un Dictionary interno.
        * *GAZE:* Riceve la durata dello sguardo dal Tracker.
    * **Safety Switch:** Disabilita qualsiasi scrittura dopo l'evento di Vittoria (`FINE`).
    * **Output Path:** `Application.persistentDataPath/TangramLog.csv`.

### 4. User Tracking (Gaze)

* **`HeadGazeTracker.cs` (Main Camera):**
    * **Funzione:** Raycasting continuo dal centro degli occhi.
    * **Logica:** Rileva oggetti con script `InterestZone`. Calcola il tempo di permanenza dello sguardo su una zona specifica.
    * **Output:** Invia i dati al Logger solo al cambio di zona o distoglimento dello sguardo.
* **`InterestZone.cs`:**
    * Componente "etichetta" da assegnare agli oggetti di interesse (Muri, Tavoli, UI). Richiede Collider.

### 5. Feedback Visivo (Reward)

* **`DecalChanger.cs`:**
    * **Funzione:** Sostituisce la texture di un *URP Decal Projector* alla vittoria.
    * **Fix Shader URP:** Gestisce correttamente i canali colore (evitando che il Rosso venga interpretato come Alpha) e resetta la tinta del materiale a Bianco puro.

## Struttura Dati (CSV Output)

Il file di log utilizza il punto e virgola (`;`) come separatore per compatibilità Excel/Sheets.

| Colonna | Descrizione | Esempio |
| :--- | :--- | :--- |
| **Date** | Data sessione (dd/MM/yyyy) | `13/01/2026` |
| **Time** | Ora evento (fine azione/trigger) | `10:45:01` |
| **Event** | Tipo evento (`GRAB`, `GAZE`, `FINE`, `EVENT`) | `GRAB` |
| **ObjectName** | Nome oggetto, Zona interesse o Nome Evento Custom | `Triangolo_Rosso` (o `Pressure_Phase_Started`) |
| **Duration** | Durata in secondi o Tempo rimanente (2 decimali) | `4.52` (o `15.00`) |

## Note di Sviluppo

* **Legacy VR:** Rimossi script obsoleti basati su `XRSettings.enabled` e tutte le librerie e riferimenti a SteamVR.
* **Assembly Definitions:** Eliminato file `.asmdef` dagli *Starter Assets* per consentire l'accesso agli script interni (`ActionBasedControllerManager`) dal codice utente.