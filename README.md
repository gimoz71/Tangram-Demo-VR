# Tangram VR Demo - Setup Tecnico & Configurazione VR

## Piattaforma di Destinazione

* **Hardware:** Meta Quest 2 / Meta Quest 3
* **OS:** Android (Standalone)
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

## File di Configurazione Unificato (`server_config.txt`)

Il sistema utilizza un singolo file di testo per configurare dinamicamente sia i parametri di rete che le variabili del timer, senza necessità di ricompilare la build.

Il file **deve** contenere esattamente 4 righe strutturate in questo modo:

```text
192.168.178.48   <-- [Riga 1] Indirizzo IP del Server (FastAPI)
60               <-- [Riga 2] Tempo totale del timer in secondi
2                <-- [Riga 3] Delay iniziale in secondi prima del countdown
15               <-- [Riga 4] Soglia di pressione in sec. (innesca testo rosso e audio)
```

* **In Editor:** Cerca/Crea il file nella cartella `Assets/`.
* **Su Visore:** Cerca/Crea il file in `\Android\data\[PackageName]\files\server_config.txt`.

## Architettura Software & Scripting Custom

Il sistema si basa su quattro pilastri: Logica di Gioco, Tracciamento Utente, Modulazione Stress e Data Logging.

### 1. Core Logic & Game Manager

* **`TangramPatternMatcher.cs`:**
    * **Funzione:** Verifica il completamento del puzzle.
    * **Logica Relativa:** Calcola posizione/rotazione dei pezzi rispetto a un pezzo "Anchor" (Capo), permettendo la risoluzione ovunque nello spazio.
    * **Win Condition (Strict):** La vittoria scatta solo se i pezzi sono posizionati correttamente E **tutti i pezzi sono stati rilasciati** (incluso l'Anchor).
    * **Eventi:** Invoca `OnWin` (Audio, FX, Stop Logging, Stop Timer, Nasconde UI Rinuncia).
* **`TeleportToggler.cs`:**
    * Toggle runtime per abilitare/disabilitare il Teleport Interactor.
    * Forza la separazione rigida dei ruoli joystick (Move vs Turn) all'inizializzazione.
    * Disabilita la logica standard `ActionBasedControllerManager`.

### 2. Modulazione Stress & Behavioral Tracking

* **`TangramTimer.cs`:**
    * **Funzione:** Modulo indipendente progettato per indurre pressione temporale (stressor) nell'utente tramite feedback visivi e uditivi. Legge i parametri operativi direttamente dal file `server_config.txt`.
    * **Fase di Start (Blinking Phase):** All'avvio della scena, il timer rimane in stato di pausa per un tempo `initialDelay` (lampeggiante). Questa fase è tracciata per isolare le interazioni avvenute prima dell'inizio effettivo della prova.
    * **Gestione Soglia (Pressure Threshold):** Attivabile a *X* secondi dalla fine (`pressureThreshold`). Cambia dinamicamente il colore del testo UI (rosso) e avvia un tick audio sincronizzato.
    * **Gestione Rinuncia:** Se l'utente interrompe la sessione, il metodo `StopTimerOnGiveUp()` congela visivamente il countdown e arresta lo stressor sonoro.
    * **Tracciamento "Curva di Stress" (Marker CSV):** Il modulo inietta eventi specifici nel `TangramLogger`:
        * `Timer_Blinking_Phase_Started`: Inizio della fase di attesa iniziale.
        * `Timer_Countdown_Started`: Fine del delay, il tempo inizia a scalare.
        * `Pressure_Phase_Started`: Innesco della soglia di stress (testo rosso/audio tick).
        * `Timer_Reached_Zero`: Esaurimento del tempo.
        * `Timer_Stopped_On_Win`: Completamento del puzzle.

### 3. Data Logging System & Networking (Update Standalone)

* **`TangramLogger.cs`:**
    * **Funzione:** Centralizza la raccolta dati gestendo salvataggio locale e trasmissione API asincrona.
    * **Export CSV Locale:** Registra con precisione al **millisecondo** (`HH:mm:ss.fff`) tutti gli eventi.
        * **Percorso Log (Quest):** `\Internal shared storage\Documents\TangramVR_Logs\`.
    * **Gestione Rinuncia (UX):** Flusso a due fasi attivato dal giocatore. Al 1° click registra l'evento `RINUNCIA`, disabilita il tracciamento di ulteriori interazioni, invia il payload JSON e mostra a schermo il Session ID. Al 2° click riporta l'utente al Main Menu.
    * **Diagnostica Connessione (`ServerConnectionCheck.cs`):**
        * Esegue un **Heartbeat Check** (POST) all'avvio nella scena Start (leggendo esclusivamente la riga 1 del config).
        * Fornisce feedback visivo immediato sull'IP utilizzato e sullo stato (Online/Offline) del server FastAPI sulla porta 80.
    * **Comunicazione Standalone (FastAPI):**
        * **Security:** Abilitato `usesCleartextTraffic` nel Manifest per comunicazioni HTTP verso host locali.
    * **Filtro API Allargato:** Il payload API include ora gli eventi interattivi (`GAZE`, `GRAB`, `FINE`), l'abbandono (`RINUNCIA`) e i marker della curva di stress del timer (`EVENT`).

### 4. User Tracking (Gaze)

* **`HeadGazeTracker.cs` (Main Camera):** Raycasting continuo dal centro degli occhi. Rileva oggetti con script `InterestZone`. Invia i dati al Logger al cambio di zona o distoglimento dello sguardo.
* **`InterestZone.cs`:** Componente "etichetta" assegnato agli oggetti di interesse (Muri, Tavoli, UI).

### 5. Feedback Visivo (Reward)

* **`DecalChanger.cs`:** Sostituisce la texture di un *URP Decal Projector* alla vittoria. Gestisce correttamente i canali colore e resetta la tinta del materiale a Bianco puro.

## Struttura Dati

Il file di log CSV locale utilizza il punto e virgola (`;`) come separatore.

| Colonna | Descrizione | Esempio |
| :--- | :--- | :--- |
| **Date** | Data sessione (dd/MM/yyyy) | `17/03/2026` |
| **Time** | Ora evento con millisecondi (HH:mm:ss.fff) | `14:05:03.110` |
| **Event** | Tipo evento (`GRAB`, `GAZE`, `FINE`, `RINUNCIA`, `EVENT`) | `EVENT` |
| **ObjectName** | Nome oggetto, Zona interesse o Marker Custom | `Timer_Countdown_Started` |
| **Duration** | Durata in secondi o Tempo rimanente | `120.00` |

### Payload API Server (JSON)

```json
{
  "session_id": "5391",
  "filename": "Tangram_Session_5391.csv",
  "events": [
    {
      "date": "17/03/2026",
      "time": "14:05:05.450",
      "event_type": "GRAB",
      "object_name": "Triangolo_Grande",
      "duration": 1.20
    }
  ]
}
```