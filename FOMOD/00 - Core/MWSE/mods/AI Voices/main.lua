local metadata = require("AI Voices.metadata")
local version = metadata.version
local config = require("AI Voices.config")

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
		tes3ui.logToConsole(string.format("VoV: Current actor is %s", vovActor.id))
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

--- @param race string
--- @param sex string
--- @param infoId string
--- @return string
local function getPath(race, sex, infoId)
	return string.format("Vo\\AIV\\%s\\%s\\%s.mp3", race, sex, infoId)
end

--- @param race string
--- @param sex string
--- @param infoId string
--- @param facId string
--- @param rank number
--- @return string
local function getFactionPath(race, sex, infoId, facId, rank)
	return string.format("Vo\\AIV\\%s\\%s\\%s\\%s\\%s.mp3", race, sex, facId, rank, infoId)
end

--- @param infoId string
--- @return string
local function getCreaturePath(infoId)
	return string.format("Vo\\AIV\\creature\\%s.mp3", infoId)
end

--- @param race string
--- @param sex string
--- @param infoId string
--- @param actorId string
--- @return string
local function getActorPath(race, sex, infoId, actorId)
	return string.format("Vo\\AIV\\%s\\%s\\%s\\%s.mp3", race, sex, actorId, infoId)
end

--- @param race string
--- @param sex string
--- @param infoId string
--- @param actorId string
--- @param facId string
--- @param rank number
--- @return string
local function getActorFactionPath(race, sex, infoId, actorId, facId, rank)
	return string.format("Vo\\AIV\\%s\\%s\\%s\\%s\\%s\\%s.mp3", race, sex, actorId, facId, rank, infoId)
end

--- @param infoId string
--- @param actorId string
--- @return string
local function getCreatureActorPath(infoId, actorId)
	return string.format("Vo\\AIV\\creature\\%s\\%s.mp3", actorId, infoId)
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

---@param e infoGetTextEventData
local function onInfoGetText(e)
	local info = e.info

	if (config.greetingsOnly) and not (info.type == tes3.dialogueType.greeting) then return end
	tes3ui.logToConsole("VoV: Dialogue item requested, checking if actor is valid.")
	if vovActor then
		tes3ui.logToConsole("VoV: Actor is valid, searching for appropriate voice line.")
		local actorPath = getCreatureActorPath(info.id, vovActor.id)
		local path = getCreaturePath(info.id)
		local npc = vovActorInstance.reference.object
		if vovActor.objectType == tes3.objectType.npc then
			local race = npc.race.id:lower()
			local sex = getActorSex(npc.female)
			local faction = npc.faction
			if faction ~= nil then
				local facid = faction.id
				local prank = faction.playerRank
				if prank >= -1 then
					actorPath = getActorFactionPath(race, sex, info.id, vovActor.id, facid, prank)
					path = getPath(race, sex, info.id, facid, prank)
				end
			end

			if isPathValid(actorPath) == false then
				actorPath = getActorPath(race, sex, info.id, vovActor.id)
			end
			if isPathValid(path) == false then
				path = getPath(race, sex, info.id)
			end
		end

		tes3ui.logToConsole(string.format("VoV: Checking for Actor specific line: %s", actorPath))
		if isPathValid(actorPath) then
			playText(actorPath, npc)
		else
			tes3ui.logToConsole(string.format("VoV: Checking for generic line: %s", path))
			if isPathValid(path) then
				playText(path, npc)
			else
				tes3ui.logToConsole("VoV: No voice line found")
			end
		end
	end
end

---
local function init()
	mwse.log(string.format("AI Voices v%s loaded.", version))
	event.register(tes3.event.infoGetText, onInfoGetText)
	event.register(tes3.event.activate, onActivate)
	event.register(tes3.event["uiActivated"], onDialogActivated, {filter = "MenuDialog"})
end

event.register(tes3.event.initialized, init)

-- Registers MCM menu --
event.register(tes3.event.modConfigReady, function()
    dofile("Data Files\\MWSE\\mods\\Ai Voices\\mcm.lua")
end)
