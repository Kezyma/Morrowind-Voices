![Voices of Vvardenfell](img/Header.png)

Voices of Vvardenfell is a project to fully voice Morrowind using [ElevenAI](https://beta.elevenlabs.io/) and [MWSE](https://github.com/MWSE/MWSE). Easily extensible to add compatibility with other mods with no knowledge of scripting or the CS and no additional esp.

> **Requires:** the latest version of [MWSE](https://github.com/MWSE/MWSE/releases) to run.

- [Installation](#installation)
  - [00 - Core](#00---core)
  - [01 - Patch for Purists](#01---patch-for-purists)
- [Adding Voices](#adding-voices)
  - [File Paths](#file-paths)
  - [Examples](#examples)
  - [Extracting Dialogue](#extracting-dialogue)
- [Uninstallation](#uninstallation)

## Installation

To install Voices of Vvardenfell, you will first need to download it, either from the Nexus or from the Github releases page for Voices of Vvardenfell. Make sure to download the latest version.

Download from [Nexus Mods](https://www.nexusmods.com/morrowind/mods/52279) or [GitHub](https://github.com/Kezyma/AI-Voices/releases).

The best way to install Voices of Vvardenfell is through a mod manager, such as [Mod Organizer 2](https://www.nexusmods.com/skyrimspecialedition/mods/6194).

If installing manually, unpack the downloaded file. Each folder inside is a separate module which should be installed independently in order. To install, simply extract the contents of each module into the `Data Files` directory in your Morrowind game folder.

### 00 - Core

The core package includes the MWSE script required for the mod to function. It also includes voice lines for Morrowind, Tribunal, Bloodmoon and the Official Plugins.

### 01 - Patch for Purists

This package contains voice lines to add support for users of [Patch for Purists](https://www.nexusmods.com/morrowind/mods/45096). Do not install this package if you are not using Patch for Purists.

## Adding Voices

Voices of Vvardenfell includes a framework that allows voice lines to be added and played without any additional esp. Anyone can add a voice line to Morrowind simply by placing the file in the correct place.

### File Paths

Dialogue should be placed in the following paths depending on each situation.

- `Sound\Vo\AIV\[race]\[gender]\[actor id]\[info id].mp3`
  - Dialogue specific to a single NPC.
- `Sound\Vo\AIV\[race]\[gender]\[info id].mp3`
  - Generic dialogue shared by NPCs.
- `Sound\Vo\AIV\creature\[actor id]\[info id].mp3`
  - Dialogue specific to a single non-NPC.
- `Sound\Vo\AIV\creature\[info id].mp3`
  - Generic dialogue shared by non-NPCs.
- `Sound\Vo\AIV\[race]\[gender]\[actor id]\[faction id]\[player rank]\[info id].mp3`
  - Faction dialogue specific to a single NPC.
- `Sound\Vo\AIV\[race]\[gender]\[faction id]\[player rank]\[info id].mp3`
  - Faction dialogue shared by NPCs.

### Examples

- `Sound\Vo\AIV\wood elf\f\123456.mp3`
  - A generic wood elf female voice line with an info id of 123456.
- `Sound\Vo\AIV\imperial\m\caius cosades\123456.mp3`
  - A Caius Cosades voice line with an info id of 123456.
- `Sound\Vo\AIV\dark elf\m\aryon\Telvanni\1\123456.mp3`
  - An Aryon voice line played only for members of Great House Telvanni at the rank of Retainer with an info id of 123456.
- `Sound\Vo\AIV\creature\dagoth_ur_1\123456.mp3`
  - A Dagoth Ur voice line with an info id of 123456.

### Extracting Dialogue

To determine the actor id, faction id, info id and player rank, dialogue must first be extracted from an esp or esm file. An optional tool to extract this is available from Github or from the Optional Files section on NexusMods.

Download from [Nexus Mods](https://www.nexusmods.com/morrowind/mods/52279) or [GitHub](https://github.com/Kezyma/AI-Voices/tree/main/Tools/LineExtractor).

This tool uses Greatness7's [tes3conv](https://github.com/Greatness7/tes3conv) to extract data from esm/esp files and then exports them to a csv for easy reading and manipulation.

> **Requires:** [.Net 7.0](https://dotnet.microsoft.com/en-us/download/dotnet/7.0) or newer.

Steps to extract dialogue from esp/esm files:

1. Download and extract `Kezyma.MorrowindLineExtractor.exe` to a folder.
2. Run `Kezyma.MorrowindLineExtractor.exe` once, this will generate the required folders.
3. Place esp/esm files into the new `Esp` folder. You must include the esp/esm that contains the dialogue you wish to extract as well as any esp/esm that it depends upon, including the base game esm/esp files.
4. Run `Kezyma.MorrowindLineExtractor.exe` a second time, this will generate a set of csv files in the `Csv` folder.

Each generated csv file will contain the Info Id (with an added prefix of `id_` to prevent Excel from incorrectly formatting the column), Actor Id, Actor Name, Actor Faction, Actor Faction Rank, Required Player Rank and Dialogue text.

Two additional csv files will also be included. `NPCs.csv` will contain a full list of all NPCs in all of the esp/esm files and `Factions.csv` will contain a full list of all Factions with Ids and Ranks.

<!-- The original page also showed a live voice-line Progress table (Morrowind & Patch for Purists). That was generated by JavaScript (vov.progress.js / DataTables) and is intentionally omitted from this static document. -->

## Uninstallation

If using a Mod Manager, simply disable Voices of Vvardenfell.

If uninstalling manually, remove the following folders:

- `Data Files\Sound\Vo\AIV\`
- `Data Files\MWSE\mods\AI Voices\`
- `Data Files\Textures\AI Voices\`
