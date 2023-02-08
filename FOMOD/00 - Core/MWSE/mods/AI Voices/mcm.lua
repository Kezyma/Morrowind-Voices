local configPath = "AI Voices"
local config = require("Ai Voices.config")
local metadata = require("AI Voices.metadata")
local VERSION = metadata.version

local function registerVariable(id)
    return mwse.mcm.createTableVariable {
        id = id,
        table = config
    }
end

local template = mwse.mcm.createTemplate {
    name = "AI Voices",
    headerImagePath = "\\Textures\\AI Voices\\kezyma_ai_logo.dds" }

local mainPage = template:createPage { label = "Main Settings", noScroll = true }
mainPage:createCategory {
    label = "Kezyma's AI Voices " .. VERSION .. " by Kezyma and tewlwolow.\nAI generated voiced dialogue framework.\nSettings:\n",
}

mainPage:createYesNoButton {
    label = "Greetings only mode?",
    variable = registerVariable("greetingsOnly"),
    restartRequired = true
}

template:saveOnClose(configPath, config)
mwse.mcm.register(template)
