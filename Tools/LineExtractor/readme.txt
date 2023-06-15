Paramaters:

extract - Extract the dialogue from files in the Esp folder.
nogame - Skip extraction of dialogue from the base game.
generate - Generate voice lines and mark the lines as done in the selected csv. Requires an ElevenAI API key in GeneratorConfig.json and voices set up on ElevenAI with the format 'Race - Gender', eg. 'Dark Elf - Male'. Must specify the csv to work from using the csv: paramater.
existingonly - If used with generate, will update the csv with files that currently exist but will not generate them itself.
csv: - Specifies the csv to generate lines from, eg. csv:Morrowind.csv

Example commands:

Kezyma.LineExtractor.exe extract nogame
Will extract the dialogue from all esp/esm files in the Esp folder, except the Morrowind game files.

Kezyma.LineExtractor.exe generate existingonly csv:Morrowind.csv
Will update Morrowind.csv and mark any existing files as done.

Kezyma.LineExtractor.exe generate csv:MySpecialMod.csv
Will use ElevenAI to generate lines for MySpecialMod.csv and mark those lines as done in the csv.

