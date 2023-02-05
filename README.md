# Kezyma's Voices of Vvardenfell

AI Generated voices for Morrowind, Tribunal, Bloodmoon and the Official Plugins.

Available on NexusMods: https://www.nexusmods.com/morrowind/mods/52279

## Adding New Dialogue

You can use the LineExtractor tool in this repo to extract dialogue from an esp/esm, which uses the tool from this repo: https://github.com/Greatness7/tes3conv

- Clone this repository.
- Inside the LineExtractor folder, create a folder called 'Esps'
- Place the esp/esm files you wish to extract dialogue from in the folder.
- If the NPC that the dialogue is for comes from another esp/esm, you must also add that too. eg. Patch for Purists edits dialogue from Morrowind.esm, so Morrowind.esm must also be added to the folder.
- Run Kezyma.MorrowindLineExtractor.exe
- A Csvs folder will be created, containing a separate csv file with the dialogue from each esp/esm you included.

To add dialogue, once you have generated the audio, it must be placed in the following path inside `Data Files`

`Sound\Vo\AIV\[RACE]\[GENDER]\[Info_Id].mp3`

For example, an imperial male voice line with the info_id of 123456 would be placed as follows:

`Sound\Vo\AIV\imperial\m\123456.mp3`

If the dialogue is for a non-NPC, such as a creature (eg. Vivec), it goes directly into a separate creatures folder. For example, a non-NPC line with the info_id of 123456 would be placed as follows:

`Sound\Vo\AIV\creature\123456.mp3`
