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
- Weapon and Attachment blacklists
- Translation config
- First player gets to choose starting area in facility
- Infinite life radio granted (players can opt out in the preferences menu)
- Loadouts, Roles and preferences are saved when player leaves
- Round stats displayed at end of round
- Base game features overriden for minimal setup

## Install
### LocalAdmin
To install via LocalAdmin console use "p install brayden-dowson/Deathmatch" and make the following changes to your config_gameplay.cfg (assuming default config)
1. friendly_fire_multiplier: 1.0
2. disable_decontamination: true
3. custom_gamemode_server: true

### Manual
To manually install get LiteDB.dll from [github](https://github.com/mbdavid/LiteDB) or from the [releases](https://github.com/brayden-dowson/Deathmatch/releases) and place in your PluginAPI/plugins/"your server port number"/dependencies/ folder. Similarly get Glicko2.dll from [github](https://github.com/ikhanage/Glicko2) or from the [releases](https://github.com/brayden-dowson/Deathmatch/releases) and place in the same folder. Place Deathmatch.dll inside the PluginAPI/plugins/"your server port number"/ folder and make the following changes to your config_gameplay.cfg (assuming default config)
1. friendly_fire_multiplier: 1.0
2. disable_decontamination: true
3. custom_gamemode_server: true

