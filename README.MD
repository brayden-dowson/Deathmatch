# Deathmatch Gamemode NW-API
## Features
- Instant respawn
- Fully customisable loadouts
- Role selection
- Custom killstreak rewards selection
- Safe spawn rooms
- Killfeed with killstreak announcements
- Dynamic facility size (rooms open and close depending on player count)
- Ranks and Experience
- Statistics tracking
- Leader board with multi-column sorting
- Weapon and Attachment blacklists
- Translation config
- First player gets to choose starting area in facility
- Global voice chat toggleable with the Alt key
- Loadouts, Roles and preferences are saved when player leaves
- Round stats displayed at end of round
- Base game features overriden for minimal setup
- Cleanup, automatically cleans up items/corpses 
- Can be run as an event using Cedmod

## Install
### LocalAdmin
To install via LocalAdmin console use "p install brayden-dowson/Deathmatch"

### Manual
To manually install put Deathmatch.dll in PluginAPI/plugins/"your server port number"/ and put everything inside the dependencies.zip inside PluginAPI/plugins/"your server port number"/dependencies/. You can also get the dependencies here [LiteDB.dll](https://github.com/mbdavid/LiteDB), [Glicko2.dll](https://github.com/ikhanage/Glicko2) and [0Harmony.dll](https://github.com/pardeike/Harmony)

## Translations
By default the config will generate a translation.yml for English, Replace or edit the translation.yml for your region
### Avaiable translations
- Chinese [[中文翻译]ZH_translation_config.yml](https://github.com/brayden-dowson/Deathmatch/blob/master/%5B%E4%B8%AD%E6%96%87%E7%BF%BB%E8%AF%91%5DZH_translation_config.yml)

## Run as an Event
To use this plugin as an event you must first install the base plugin and additionally must also install the DeathmatchEvent plugin in /"where you installed cedmod"/CedModEvents. See the [Releases](https://github.com/brayden-dowson/Deathmatch/releases) to get the DeathmatchEvent plugin. You must have Cedmod installed if you want to run the plugin as an event. If you have not installed Cedmod you can get it here [Cedmod](https://github.com/CedModV2/CedMod)
