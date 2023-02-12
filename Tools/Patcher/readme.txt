To use the patcher, place the exe file in either the Morrowind folder or the Data Files folder.
Call the patcher with the following arguments:

Kezyma.MorrowindCompatibilityPatcher.exe "EspToPatchFor.esp" "OutputFolder"

The patcher will check the esp for any dialogue that exists in the base game but that has had it's info_id changed, it'll then create copies of the dialogue, structured to work with VoV, in the output folder.

If you want to patch directly into the game itself, use an empty string as the paramater:

Kezyma.MorrowindCompatibilityPatcher.exe "EspToPatchFor.esp" ""

This will only work if the dialogue is not changed from the base game, only the info_id. 

If the file you are patching for is NOT "Patch for Purists.esm" AND you have Patch for Purists installed, it will be included and considered part of the game files.