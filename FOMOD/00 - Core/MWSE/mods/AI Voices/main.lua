local metadata = require("AI Voices.metadata")
local version = metadata.version
local config = require("AI Voices.config")
local basePath = "Vo\\AIV\\"

--- @param e uiActivatedEventData
local function onDialogActivated(e)
	local ref = tes3ui.getServiceActor()
	e.element:registerAfter(
		tes3.uiEvent.destroy,
		function()
			tes3.removeSound { reference = ref, sound = nil }
			vovActor = nil
			vovActorInstance = nil
		end
	)
end

--- @param e activateEventData
local function onActivate(e)
    if (e.activator ~= tes3.player) then
        return
    end
    if (e.target.object.objectType == tes3.objectType.npc or e.target.object.objectType == tes3.objectType.creature) then
		vovActor = e.target.baseObject
		vovActorInstance = e.target.object
		return
    end
end

--- @param path string
local function playText(path, npc)
	local ref = npc.reference
	tes3.removeSound { reference = ref }
	tes3.say {
		volume = 1.0,
		soundPath = path,
		reference = ref
	}
end

--- @param isFemale boolean
--- @return string
local function getActorSex(isFemale)
	if isFemale then return "f" else return "m" end
end

--- @param path string
--- @return boolean
local function isPathValid(path)
	return lfs.fileexists("Data Files\\Sound\\" .. path)
end

local function constructVoicePath(race, sex, infoId, actorId, factionId, factionRank)
	local path = basePath
	if (race) then
		path = path .. "\\" .. race
	else
		path = path .. "\\creature"
	end
	if (sex) then
		path = path .. "\\" .. sex
	end
	if (actorId) then
		path = path .. "\\" .. actorId
	end
	if (factionId) then
		path = path .. "\\" .. factionId
	end
	if (factionRank and factionRank >= 0) then
		path = path .. "\\" .. factionRank
	end
	if (infoId) then
		path = path .. "\\" .. infoId .. ".mp3"
	end
	return path
end

local function getVoicePath(race, sex, infoId, actorId, factionId, factionRank)
	-- Check the most specific path first.
	local primaryPath = constructVoicePath(race, sex, infoId, actorId, factionId, factionRank)
	if (isPathValid(primaryPath)) then return primaryPath end

	-- Find every possible fallback path.
	local secondaryPaths = {
		constructVoicePath(race, sex, infoId, actorId, factionId, nil),
		constructVoicePath(race, sex, infoId, actorId, nil, nil),
		constructVoicePath(race, sex, infoId, nil, factionId, factionRank),
		constructVoicePath(race, sex, infoId, nil, factionId, nil),
		constructVoicePath(race, sex, infoId, nil, nil, nil),
		constructVoicePath(nil, nil,  infoId, actorId, factionId, factionRank),
		constructVoicePath(nil, nil,  infoId, actorId, factionId, nil),
		constructVoicePath(nil, nil, infoId, actorId, nil, nil),
		constructVoicePath(nil, nil,  infoId, nil, factionId, factionRank),
		constructVoicePath(nil, nil,  infoId, nil, factionId, nil),
		constructVoicePath(nil, nil, infoId, nil, nil, nil)
	}

	-- Return the first path in the list that is valid.
	for ix, path in pairs(secondaryPaths) do
		if (isPathValid(path)) then
			return path
		end
	end

	-- If there's no line, return the most specific path for logging purposes.
	return primaryPath
end

---@param e infoGetTextEventData
local function onInfoGetText(e)
	local info = e.info
	if (info.type == tes3.dialogueType.voice or info.type == tes3.dialogueType.journal) then return end
	if (config.greetingsOnly) and not (info.type == tes3.dialogueType.greeting) then return end
	if vovActor then
		local infoId = info.id
		local actorId = vovActor.id
		local race = nil
		local sex = nil
		local factionId = nil
		local factionRank = nil
		if not (vovActorInstance) or not (vovActorInstance.reference) then return end
		local npc = vovActorInstance.reference.object
		if vovActor.objectType == tes3.objectType.npc then
			race = npc.race.id:lower()
			sex = getActorSex(npc.female)
			local faction = npc.faction
			if faction ~= nil then
				factionId = faction.id
				factionRank = faction.playerRank
			end
		end
		local voicePath = getVoicePath(race, sex, infoId, actorId, factionId, factionRank)
		if isPathValid(voicePath) then
			tes3ui.logToConsole(string.format("VoV: Playing Line at %s", voicePath))
			playText(voicePath, npc)
		else
			tes3ui.logToConsole(string.format("VoV: Missing Line at %s", voicePath))
		end
	end
end

---
local function init()
	mwse.log(string.format("AI Voices v%s loaded.", version))
	event.register(tes3.event.infoGetText, onInfoGetText)
	event.register(tes3.event.activate, onActivate)
	event.register(tes3.event.uiActivated, onDialogActivated, {filter = "MenuDialog"})
end

event.register(tes3.event.initialized, init)

-- Registers MCM menu --
event.register(tes3.event.modConfigReady, function()
    dofile("Data Files\\MWSE\\mods\\Ai Voices\\mcm.lua")
end)
