-- local sha = require("sha2")
local metadata = require("AI Voices.metadata")
local version = metadata.version

local config = require("AI Voices.config")


--- @param e uiActivatedEventData
local function onDialogActivated(e)
	local ref = tes3ui.getServiceActor().object.reference
	e.element:registerAfter(
		tes3.uiEvent.destroy,
		function()
			tes3.removeSound { reference = ref }
		end
	)
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

	local actor = info.actor

	if actor then

		local actorPath = getCreatureActorPath(info.id, info.actor.id)
		local path = getCreaturePath(info.id)
		local npc = info.actor.reference.object

		if actor.objectType == tes3.objectType.npc then
			local race = npc.race.id:lower()
			local sex = getActorSex(npc.female)

			--e.text = e:loadOriginalText()
			--local ctxt = string.gsub(string.gsub(e.text, "@", ""), "#", "")
			--local hash = string.upper(sha.md5(ctxt))
			actorPath = getActorPath(race, sex, info.id, info.actor.id)
			path = getPath(race, sex, info.id)
		end

		if isPathValid(actorPath) then
			playText(actorPath, npc)
		elseif isPathValid(path) then
			playText(path, npc)
		end
	end
end

---
local function init()
	mwse.log(string.format("AI Voices v%s loaded.", version))
	event.register(tes3.event.infoGetText, onInfoGetText)
	event.register(tes3.event["uiActivated"], onDialogActivated, {filter = "MenuDialog"})
end

event.register(tes3.event.initialized, init)

-- Registers MCM menu --
event.register(tes3.event.modConfigReady, function()
    dofile("Data Files\\MWSE\\mods\\Ai Voices\\mcm.lua")
end)