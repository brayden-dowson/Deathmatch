#Deathmatch Gamemode NW-API
##Features
- Instant respawn
- Fully customisable loadouts
- Role selection
- Custom killstreak rewards selection
- Safe spawn rooms
- Killfeed with killstreak announcements
- Dynamic facility size (rooms open and close depending on player count)
- First player gets to choose starting area in facility
- Infinite life radio granted (players can opt out in the preferences menu)
- Loadouts, Roles and preferences are saved when player leaves
- Round stats displayed at end of round
- Base game features overriden for minimal setup

##Install
Requiers [LiteDB.dll](https://github.com/mbdavid/LiteDB) - place in the dependencies folder. Changes need to be made in config_gameplay.cfg for the plugin to work properly (assumes default config)
1. friendly_fire_multiplier: 1.0
2. disable_decontamination: true
3. custom_gamemode_server: true
4. heavily_modded_server: true

To install via LocalAdmin console use "p install brayden-dowson/Deathmatch"