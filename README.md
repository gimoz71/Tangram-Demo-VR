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
    * *Move:** Abilitato (Continuous Move Provider).
    * *Turn:** Disabilitato.
    * *Teleport:** Gestito via script custom.
* **Right Controller:**
    * *Move:** Disabilitato.
    * *Turn:** Abilitato (Snap Turn Provider).
    * *Teleport:** Gestito via script custom.

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
    * **Funzione:** Modulo indipendente progettato per indurre pressione temporale (stressor) nell'utente tramite feedback visivi e uditivi.
    * **Gestione Soglia (Pressure Threshold):** Attivabile a *X* secondi dalla fine (`pressureThreshold`). Cambia dinamicamente il colore del testo UI (rosso) e avvia un tick audio singolo **rigorosamente sincronizzato** in realtime al calcolo del secondo intero (`Mathf.CeilToInt`).
    * **Tracciamento "Curva di Stress" (Marker CSV):** Il modulo inietta eventi specifici nel `TangramLogger` per mappare l'andamento comportamentale sotto pressione:
        * `Pressure_Phase_Started`: Registrato all'innesco della soglia di stress.
        * `Timer_Reached_Zero`: Marcatore di esaurimento del tempo; isola i dati per l'analisi del comportamento in fase di "post-scadenza".
        * `Timer_Stopped_On_Win`: Registrato al completamento del puzzle.

### 3. Data Logging System & Networking (Update Standalone)

* **`TangramLogger.cs`:**
    * **Funzione:** Centralizza la raccolta dati gestendo salvataggio locale e trasmissione API asincrona.
    * **Export CSV Locale:** Registra con precisione al **millisecondo** (`HH:mm:ss.fff`) tutti gli eventi, inclusi i marker psicologici del timer.
    * **Comunicazione Standalone (FastAPI):**
        * **IP Addressing:** Configurato per puntare all'IP statico del server nella rete locale (LAN) invece di `localhost`.
        * **Security:** Abilitato `usesCleartextTraffic` nel Manifest per comunicazioni HTTP verso host locali.
        * **Timeout & Async:** Gestione asincrona tramite `UnityWebRequest` con timeout di 5s per evitare stutter nel visore.
    * **Filtro di Sicurezza:** Il payload API scarta i marker del timer inviando solo gli eventi core (`GAZE`, `GRAB`, `FINE`) per conformità al database.
    * **Output Path Locale:** `Application.persistentDataPath/TangramLog.csv`.

### 4. User Tracking (Gaze)

* **`HeadGazeTracker.cs` (Main Camera):** Raycasting continuo dal centro degli occhi. Rileva oggetti con script `InterestZone`. Invia i dati al Logger al cambio di zona o distoglimento dello sguardo.
* **`InterestZone.cs`:** Componente "etichetta" assegnato agli oggetti di interesse (Muri, Tavoli, UI).

### 5. Feedback Visivo (Reward)

* **`DecalChanger.cs`:** Sostituisce la texture di un *URP Decal Projector* alla vittoria. Gestisce correttamente i canali colore e resetta la tinta del materiale a Bianco puro.

## Struttura Dati

Il file di log CSV locale utilizza il punto e virgola (`;`) come separatore.

| Colonna | Descrizione | Esempio |
| :--- | :--- | :--- |
| **Date** | Data sessione (dd/MM/yyyy) | `16/03/2026` |
| **Time** | Ora evento con millisecondi (HH:mm:ss.fff) | `10:36:58.180` |
| **Event** | Tipo evento (`GRAB`, `GAZE`, `FINE`, `EVENT`) | `GRAB` |
| **ObjectName** | Nome oggetto, Zona interesse o Marker Custom | `Triangolo_Rosso` |
| **Duration** | Durata in secondi o Tempo rimanente | `4.52` |

### Payload API Server (JSON)
```json
{
  "session_id": "5391",
  "filename": "Tangram_Session_5391.csv",
  "events": [
    {
      "date": "16/03/2026",
      "time": "10:36:58.180",
      "event_type": "GAZE",
      "object_name": "Tangram",
      "duration": 0.1949230432510376
    }
  ]
}