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

To add dialogue, once you have generated the audio, it must be placed in the following path inside `Data Files`

`Sound\Vo\AIV\[RACE]\[GENDER]\[Info_Id].mp3`

For example, an imperial male voice line with the info_id of 123456 would be placed as follows:

`Sound\Vo\AIV\imperial\m\123456.mp3`

If the dialogue is for a non-NPC, such as a creature (eg. Vivec), it goes directly into a separate creatures folder. For example, a non-NPC line with the info_id of 123456 would be placed as follows:

`Sound\Vo\AIV\creature\123456.mp3`
