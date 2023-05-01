using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace TheRiptide.Features
{
    public class TranslationConfig
    {
        [Description("Hitbox")]
        public string Body = "Body";
        public string Limb = "Limb";
        public string Head = "Head";

        [Description("\nkillfeed messages")]
        public string FirearmKill = "{killer} shot  {victim} in  the  {hitbox} with  {gun}";
        public string ExplosionKill = "{killer} fragged {victim}";
        public string ExplosionSelfKill = "{victim} humiliated  themselves  with  a  <b><color=#eb0d47>Frag grenade</color></b>";
        public string JailbirdHeadKill = "{killer} bonked {victim} on the {hitbox} with the <b><color=#eb0d47>Jailbird</color></b>";
        public string JailbirdNormalKill = "{killer} slapped {victim} on the {hitbox} with the <b><color=#eb0d47>Jailbird</color></b>";
        public string Scp018Kill = "{killer} pummeled {victim} with <b><color=#eb0d47>SCP 018</color></b>";
        public string Scp018SelfKill = "{victim} humiliated themselves by failing to catch their own ball";
        public string DistruptorKill = "{killer} atomised {victim} in the {hitbox} with the <b><color=#eb0d47>Particle disruptor</color></b>";
        public string DistruptorSelfKill = "{victim} humiliated themselves with the <b><color=#eb0d47>Particle disruptor</color></b>";
        public string CustomReasonKill = "{victim} slain: <b><color=#43BFF0>{reason}</color></b>";
        public string FailedFirstGrade = "{victim} <b><color=#eb0d47>could not read so they left the match humiliating themselves</color></b>";
        public string SelfKill = "<b><color=#eb0d47>{victim}</color></b> humiliated  themselves";

        [Description("killstreak")]
        public string GlobalKillstreak = "<b><color=#43BFF0>{name}</color></b> is on a <b><color=#FF0000>{streak}</color></b> kill streak";
        public string PrivateKillstreak = "Kill streak <b><color=#FF0000>{streak}</color></b>";
        public string GlobalKillstreakEnded = "<b><color=#43BFF0>{killer}</color></b> ended <b><color=#43BFF0>{victim}'s </color></b>" + "<b><color=#FF0000>{streak}</color></b> kill streak";

        [Description("loadout")]
        public string CustomisationHint = "<b>CHECK INVENTORY! <color=#FF0000>Right Click O5 to select gun</color></b>";
        public List<string> CustomisationDenied = new List<string>() {
            "<color=#f8d107>Loadout can not be customised after shooting gun/using item</color>",
            "<color=#43BFF0>Wait until next respawn</color>" };
        public string RadioDisableHint = "<color=#FF0000>Radio can be disabled in</color> <b><color=#43BFF0>[MAIN MENU]</color> -> <color=#43BFF0>[PREFERENCES]</color> -> <color=#eb0d47>[GUARD]</color></b>";

        [Description("\nlobby")]
        public string Teleport = "<color=#43BFF0>you will be teleported after selecting a gun</color>";
        public List<string> WaitingForPlayers = new List<string>(){
            "<color=#43BFF0>Waiting for 1 player to join</color>",
            "<color=#43BFF0>You get to choose the starting area!</color>"};
        public string Respawn = "<b><color=#FFFF00>Left/Right click to respawn</color></b>";
        public string Attachments = "<b><color=#FF0000>Tab to edit attachments/presets</color></b>";
        public string Teleporting = "<color=#43BFF0>Teleporting in 7 seconds</color>";
        public string TeleportCancel = "<color=#43BFF0>Open [MAIN MENU] to cancel</color>";
        public string FastTeleport = "<color=#43BFF0>loadout set, teleporting in 3 seconds</color>";
        public string SpectatorMode = "spectator mode is currently bugged, you may need to leave and rejoin to respawn";

        [Description("\nmain menu")]
        public string MainMenu = "<b><color=#43BFF0>[MAIN MENU]</color></b> <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>";
        public string BackToMainMenu = "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[MAIN MENU]</color></b>";
        public string SaveAndExit = "<color=#5900ff>[O5]</color> = <b><color=#5900ff>Save and Exit</color></b>";
        public string CustomiseLoadout = "<color=#e7d77b>[SCIENTIST]</color> = <b><color=#FF0000>Customise Loadout - </color><color=#43BFF0>[GUN SLOT]</color></b>";
        public string KillstreakRewardSystem = "<color=#e1ab21>[RESEARCH SUPERVISOR]</color> = <b><color=#43BFF0>[KILLSTREAK REWARD SYSTEM]</color></b>";
        public string Preferences = "<color=#bd8f86>[CONTAINMENT ENGINEER]</color> = <b><color=#43BFF0>[PREFERENCES]</color></b>";
        public string Role = "<color=#bd1a4a>[FACILITY MANAGER]</color> = <b><color=#43BFF0>[ROLE]</color></b>";

        [Description("\ngun slot menu")]
        public string GunSlotMenu = "<b><color=#43BFF0>[GUN SLOT]</color></b> <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>";
        public string Primary = "<color=#bd1a4a>[FACILITY MANAGER]</color> = <b><color=#FF0000>Primary - </color><color=#43BFF0>[GUN CLASS]</color></b>";
        public string Secondary = "<color=#217b7b>[ZONE MANAGER]</color> = <b>Secondary - <color=#43BFF0>[GUN CLASS]</color></b>";
        public string HeavyPrimary = "<color=#1b43cb>[CAPTAIN]</color> = <b><color=#FF0000>Primary - </color><color=#43BFF0>[GUN CLASS]</color></b>";
        public string HeavySecondary = "<color=#177dde>[SERGEANT]</color> = <b>Secondary - <color=#43BFF0>[GUN CLASS]</color></b>";
        public string HeavyTertiary = "<color=#accfe1>[PRIVATE]</color> = <b>Tertiary - <color=#43BFF0>[GUN CLASS]</color></b>";

        [Description("\ngun class menu")]
        public string GunClassMenu = "<b><color=#43BFF0>[GUN CLASS]</color></b> <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>";
        public string MtfGuns = "<color=#1b43cb>[CAPTAIN]</color> = <b><color=#1b43cb>[MTF GUNS]</color></b>";
        public string ChaosGuns = "<color=#008f1c>[CHAOS]</color> = <b><color=#008f1c>[CHAOS GUNS]</color></b>";

        [Description("\ngun menu")]
        public string MtfGunMenu = "<b><color=#1b43cb>[MTF GUNS]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>";
        public string ChaosGunMenu = "<b><color=#008f1c>[CHAOS GUNS]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>";
        public string GunSelected = "<color=#43BFF0>{gun}</color> added to your loadout as the <color=#FF0000>{slot}</color> weapon";

        [Description("\nkillstreak reward system menu")]
        public string KillstreakRewardMenu = "<b><color=#43BFF0>[KILLSTREAK REWARD SYSTEM]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>";
        public string Easy = "<color=#eb0d47>[HEAVY ARMOR]</color> = <b><color=#5900ff>Easy</color>: low risk low reward, good loadout but bad killstreak rewards</b>";
        public string Standard = "<color=#eb0d47>[COMBAT ARMOR]</color> = <b><color=#43BFF0>Standard</color>: medium risk medium reward, ok loadout and ok killsteak rewards</b>";
        public string Expert = "<color=#eb0d47>[LIGHT ARMOR]</color> = <b><color=#36a832>Expert</color>: high risk high reward, bad loadout but good killstreak rewards </b>";
        public string Rage = "<color=#eb0d47>[COM 45]</color> = <b><color=#FF0000>RAGE</color> - [DATA EXPUNGED]</b>";
        public string KillstreakSelected = "{killstreak} selected as your killstreak reward system";

        [Description("\nrole menu")]
        public string RoleMenu = "<b><color=#43BFF0>[ROLE]</color></b> <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>";
        public string ClassD = "<color=#bdafe4>[JANITOR]</color> = <b><color=#FF8E00>Class-D</color></b>";
        public string Scientist = "<color=#e7d77b>[SCIENTIST]</color> = <b><color=#FFFF7C>Scientist</color></b>";
        public string Guard = "<color=#5B6370>[GUARD]</color> = <b><color=#5B6370>Facility Guard</color></b>";
        public string Private = "<color=#accfe1>[PRIVATE]</color> = <b><color=#accfe1>NTF Private</color></b>";
        public string Sergeant = "<color=#177dde>[SERGEANT]</color> = <b><color=#177dde>NTF Sergeant</color></b>";
        public string Captain = "<color=#1b43cb>[CAPTAIN]</color> = <b><color=#1b43cb>NTF Captain</color></b>";
        public string Chaos = "<color=#008f1c>[CHAOS]</color> = <b><color=#008f1c>Chaos</color></b>";
        public string RoleSelected = "{role} selected as role";

        [Description("\npreferences menu")]
        public string PreferencesMenu = "<b><color=#43BFF0>[PREFERENCES]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>";
        public string ToggleRadio = "<color=#eb0d47>[GUARD]</color> = <b>Toggle Loadout Radio</b>";
        public string Stats = "<color=#eb0d47>[SCIENTIST]</color> = <b><color=#43BFF0>[STATS]</color></b>";
        public string Spectator = "<color=#eb0d47>[FLASH LIGHT]</color> = <b>Enable spectator mode</b>";
        public string EnableRage = "<color=#eb0d47>[COIN]</color> = <b>Enable [DATA EXPUNGED]</b>";
        public string RadioToggled = "<b><color=#43BFF0>Loadout Radio: </color></b> {state}";

        [Description("\nstats menu")]
        public string StatsMenu = "<b><color=#43BFF0>[STATS]</color></b> - <b><color=#FF0000>RIGHT CLICK TO SELECT</color></b>";
        public string BackToPreferences = "<color=#5900ff>[O5]</color> = <b>Back to <color=#43BFF0>[PREFERENCES]</color></b>";

        [Description("\nrooms")]
        public string SecondPlayerJoined = "Player <color=#43BFF0>{name}</color> joined, waiting for them to select a loadout";
        public string SecondPlayerHelp = "<color=#43BFF0>{name}</color> is struggling to set his loadout please help him with your radio";
        public string Decontaminating = "<color=#FF0000>DECONTAMINATNG!</color>";
        public string Caution = "<color=#FFFF00>Caution! Room decontamination in {time}</color>";
        public string Warning = "<color=#FF8000>Warning! Room decontamination in {time}</color>";
        public string Danger = "<color=#FF0000>DANGER! DECONTAMINATION IMMINENT! {time}</color>";

        [Description("\nstats")]
        public string DeathMsgKiller = "\n{killer} <color=#43BFF0>HP: {health}</color>";
        public string DeathMsgDamageReduction = " <color=#5900ff> DR: {reduction}%</color>";
        public string DeathMsgBodyshotReduction = " <color=#e7d77b> BSR: {reduction}%</color>";
        public string DeathMsgDamageDelt = "<color=#43BFF0> DMG: {damage}</color> <color=#FF0000>HS: {head_shots}</color> <color=#36a832>BS: {body_shots}</color> <color=#43BFF0>LS: {limb_shots}</color>";
        public string DeathMsgDamageOther = " Other: {other_hits}";
        public string PlayerStatsLine1 = "<color=#76b8b5>Kills:</color> <color=#FF0000>{kills}</color>    <color=#76b8b5>Deaths:</color> <color=#FF0000>{deaths}</color>    <color=#76b8b5>K/D:</color> <color=#FF0000>{kd}</color>    <color=#76b8b5>Highest Killstreak:</color> <color=#FF0000>{top_ks}</color>" + "</color>    <color=#76b8b5>Score:</color> <color=#FF0000>{score}</color>";
        public string PlayerStatsLine2 = "<color=#76b8b5>Hs Kills:</color> <color=#FF0000>{hsk}%</color>    <color=#76b8b5>Hs:</color> <color=#FF0000>{hs}%</color>    <color=#76b8b5>Accuracy:</color> <color=#FF0000>{accuracy}%</color>    <color=#76b8b5>Dmg Delt:</color> <color=#FF0000>{dmg_delt}</color>    <color=#76b8b5>Dmg Taken:</color> <color=#FF0000>{dmg_taken}</color>";
        public string HighestKillstreak = "<b><color=#43BFF0>{name}</color></b> <color=#d4af37>had the highest killstreak of</color> <b><color=#FF0000>{streak}</color></b>";
        public string HighestKills = "<b><color=#43BFF0>{name}</color></b> <color=#c0c0c0>had the most kills</color> <b><color=#FF0000>{kills}</color></b>";
        public string HighestScore = "<b><color=#43BFF0>{name}</color></b> <color=#a97142> was the best player with a score of </color> <b><color=#FF0000>{score}</color></b>";

    }
}
