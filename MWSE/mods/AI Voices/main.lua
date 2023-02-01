local menuActive
local npc

--- @param path string
local function playText(path)
	local ref = npc.reference
	tes3.removeSound { reference = ref }
	tes3.say {
		volume = 0.9,
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

	if not menuActive then return end
	if not npc then return end

	local race = npc.race.id:lower()
	local sex = getActorSex(npc.female)
	local info = e.info

	-- Leave it for later use
	-- e.text = e:loadOriginalText()
	-- debug.log(info.id)
	-- debug.log(e.text)

	local path = getPath(race, sex, info.id)
	if isPathValid(path) then
		playText(path)
	end
end

---
local function onDialogueMenuEnter()
	local actor = tes3ui.getServiceActor()
	if actor then
		npc = actor.object
	end
	menuActive = true
end

local function onDialogueMenuExit()
	menuActive = false
	npc = nil
end

---
local function init()
	debug.log("AI Voices loaded.")
	event.register(tes3.event.infoGetText, onInfoGetText)
	event.register(tes3.event.menuEnter, onDialogueMenuEnter, {filter = "MenuDialog"})
	event.register(tes3.event.menuExit, onDialogueMenuExit, {filter = "MenuDialog"})
end

event.register(tes3.event.initialized, init)