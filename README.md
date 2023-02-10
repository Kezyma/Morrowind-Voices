# Kezyma's Voices of Vvardenfell

AI Generated voices for Morrowind, Tribunal, Bloodmoon and the Official Plugins.

Available on NexusMods: https://www.nexusmods.com/morrowind/mods/52279

## Adding New Dialogue

### Extracting Dialogue
To extract dialogue, you can use the LineExtractor tool located in this repository. The tool uses Greatness7's tes3conv (https://github.com/Greatness7/tes3conv) to extract data from esm/esp files and then exports the dialogue, speaker and info_id to csv files for easy reading.

Requires .Net 7 Runtime: https://dotnet.microsoft.com/en-us/download/dotnet/7.0

- Download `Kezyma.MorrowindLineExtractor.exe` from the LineExtractor folder in this repository.
- Run `Kezyma.MorrowindLineExtractor.exe` once to generate required folders.
- Place esp/esm files into the `Esp` folder. You must include both the esp/esm containing the dialogue you wish to extract as well as all dependencies, otherwise speaker information is not available.
- Run `Kezyma.MorrowindLineExtractor.exe` a second time.
- The `Csv` folder will contain csv files for each esp/esm containing all the dialogue and relevant information.

The InfoId column contains a prefix of `id_` which must be removed when naming files later. This is added so that the files can be opened in Excel without the column being formatted and producing the wrong InfoId.

### Adding Dialogue

Dialogue should be added, to the following paths in this priority order:

- For dialogue specific to a single NPC
`Sound\Vo\AIV\[race]\[gender]\[actor id]\[info id].mp3`

- For dialogue shared between NPCs
`Sound\Vo\AIV\[race]\[gender]\[info id].mp3`

- For dialogue specific to a non-NPC
`Sound\Vo\AIV\creature\[actor id]\[info id].mp3`

- For dialogue shared between non-NPCs
`Sound\Vo\AIV\creature\[info id].mp3`

- For faction specific dialogue specific to a single NPC
`Sound\Vo\AIV\[race]\[gender]\[actor id]\[faction id]\[player rank]\[info id].mp3`

- For faction specific dialogue shared between NPCs
`Sound\Vo\AIV\[race]\[gender]\[faction id]\[player rank]\[info id].mp3`

For examples:

- A generic female wood elf line with the info id of 123456 would be
`Sound\Vo\AIV\wood elf\f\123456.mp3`

- An imerial male voice line for Caius Cosades with the info id of 123456 would be
`Sound\Vo\AIV\imperial\m\caius cosades\123456.mp3`

- A dark elf male voice line for Aryon, specific to Retainers in Great House Telvanni with the info id of 123456 would be
`Sound\Vo\AIV\dark elf\m\aryon\Telvanni\1\123456.mp3`

- A Dagoth Ur line with the info id of 123456 would be
`Sound\Vo\AIV\creature\dagoth_ur_1\123456.mp3`
