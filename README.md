## TTGL Mod for Risk of rain 2
- TTGL themed mod that currently adds Lagann as a playable survivor in Risk of rain 2
- Currently wip.

[![](https://cdn.discordapp.com/attachments/194257452374425600/840096334576746496/unknown.png)]()

[![](https://cdn.discordapp.com/attachments/194257452374425600/840096968524169216/unknown.png)]()

feel free to ping/dm me with any questions / suggestion / complaints on the modding discord- @Mico27#0642

## Changelog

`0.2.9`
- Nerfed Skill++ skill upgrades, limited to 25 upgrades for each skill until the duplication skill point bug is fixed.
- Added new primary skill for Lagann: Drill spike (still working for a new icon for it)
- Optimized Yoko rifle skill's code, Also it now ricochet faster (From 0.5 sec to 0.1 sec), nerfed damage from 250% to 200%.
- Yoko's rifle nerfed to ricochet only up to 6 times (Except when using Skill++ it will start at 2 times)
- Finally got around and added item displays on Lagann, Gurren and Gurren Lagann. (You can disable item display for each of them in config)
- Nerfed Gurren Lagann starting base damage, base health, base health regen and base armor stats by 25%

`0.2.8`
- Lowered the volume and attenuated the sound effects
- Combine and gigadrill break sound effect are now local to the user/spectator
- Implemented skill leveling support for the mod Skill++ (There's currently a bug that duplicates the skill point when you transform back and forth but it should be eventualy fixed by the skill++ devs)
- Yoko's rifle nerfed to ricochet only up to 6 times (Except when using Skill++ it will start at 2 times)
- Lagann impact will now be able to hit the same enemy multiple times on rebound
- Gurren Lagann will now be less "floaty"

`0.2.7`
- Fixed a bug where sometimes after using giga drill break, you would fall into the void forever in multiplayer.
- Fixed Gurren Lagann's throwing shades that was doing x4 the amount of damage intended. Also the shades will also now hit an enemy for a second time on the way back.
- Nerfed the price of activating Gurren on the map from 5 lunar coins to 2 lunar coins.

`0.2.6`
- Combine and GigaDrill Animation cinematic is now displayed only for the user or for anyone spectating the user in Multiplayer (Will no longer "freeze" everything else during it).
- Combine and GigaDrill Animation cinematic can be skipped by pressing the interact button ("e" or whatever you have it bind to).
- TTGL music will now also be played for any spectator spectating Lagann or GurrenLagann.

`0.2.5`
- Fixed bug where some skills depending on spiral power amount like Combine and GigaDrill Maximum wouldnt activate correctly after combining/spliting multiple times or if there were multiple of the same character in multiplayer.

`0.2.4`
- Added death ragdoll on Lagann, Gurren and GurrenLagann (Finaly!)
- Fixed getting Gurren in multiplayer which was still bugged when you werent host
- Removed Gurren from selection screen, loadout isnt very good so I'll keep it as just an AI you can find on the map during a run.
- If you play as Lagann (Or started playing as GurrenLagann and Gurren dies), you will always be able to find Gurren on the first stage (or the next stage after Gurren dies)

`0.2.3`
- Added Drill Blaster move for Gurren
- Added a bunch of actual tips/description in the summary of Lagann, Gurren and Gurren Lagann.
- Buffed a bit Gurren and Gurren Lagann's melee attacks.

`0.2.2`
- Temporarily removed part of the implementation of BetterUI (Forgot I wasnt using a released version for testing)
- Forgot to remove some multiplayer degging stuff

`0.2.1`
- Fixed a bug where the Gurren placed on the map was only showing for the host in multiplayer
- Gurren will now reappear on a following map if it was killed (You'll have to repurchase it)
- Added the move boulder throw to Gurren
- Implemented Gurren's passive
- You can now edit the placement of the spiral gauge in the config
- Added support for BetterUI (Adds descriptions to buffs on hover. you can also add $spiralrate and $spiralamount in StatString in BetterUI-StatsDisplay.cfg)

`0.2.0`
- Added Unlockables
- Added some new icons for Gurren (still WIP but "playable")
- You can now randomly find Gurren in a run if you play as Lagann (cost 5 Lunar coins to activate).
- Combine requires now 1 spiral gauge and require Gurren to be in the party.
- GigaDrill Maximum consumes half a spiral gauge
- GigaDrill Breaker now forces you to revert to Lagann
- You can now specify the frequency of the combine cinematic animation in the config (Always, Once per run, Never)

`0.1.11`
- Added Combine move WIP

`0.1.10`
- Fixed giga drill break camera not working all the time in multiplayer
- Forgot to remove some debugging stuff

`0.1.9`
- Fixed mod because an update broke it
- Added Gurren Lagann's Giga Drill Break!!! (throw shades at a teleporter boss and press your secondary again to activate it)

`0.1.8`
- Fixed multiplayer bug not showing spiral combo animations for other players and throwin shades not moving for the non-host player

`0.1.7`
- Added secret skin

`0.1.6`
- Fixed an issue where gurren lagann couldnt open pickup menus such as the scrapper and artifact of command
- Buffed Gurren lagann throwing shades hitbox

`0.1.5`
- Added Gurren Lagann (wip)
- Added some new icons
- Fixed some mod conflicts

`0.1.4`
- forgot to add dependencies ...

`0.1.3`
- Fixed some perfomance bugs
- Added an alpha version of Gurren lagann

`0.1.2`
- Removing R2API dependency (Hopefully this works)

`0.1.1`
- Attempting Fixing HHook conflict

`0.1.0`
- Fixed mod to work with the anniversary update

`0.0.4`
- Fixed visual bug with Poison affix effect
- Added Toggle Canopy utility skill
- Made some new icons

`0.0.3`
- Made spiral power networkable
- Fixed bug with full spiral power music and plays only localy
- Added Ancient Scepter effect to upgrade Yoko's rifle skill

`0.0.2`
- Added custom sounds
- Added full spiral energy state
- Refined Yoko rifle skill visual and sound effects
- Added a variant of Yoko rifle skill using explosive rounds
- Fixed model scaling issues which made some overlay effects look blown out
- Implemented the Ancient Scepter item effect, modifying Yoko rifle skill to be hitscan, ricochet on crit, have improved dmg AND explode on hit.

`0.0.1`
- Initial release