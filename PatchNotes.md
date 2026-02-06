# Z2Randomizer Changelog

## Version 5.0.10 - February 06, 2026

- Added some new sprites. Thanks Varcal, MisterMike, and Irenepunmaster!
- Fixed a bug where logic didn't properly check for secondary locations in towns hidden by 3-eye rock or hidden forest.
- Fixed a crash if your last loaded sprite no longer exists

## Version 5.0.9 - November 29, 2025

### Bug Fixes
- Fixed a bug in the Random Walk palace generator where it believed segmented rooms had more connections than they did.
- Fixed a bug that could cause 3-eye rock to spawn in invalid locations.
- Fixed default window size being too small.
- Fixed or removed some projectile options that were not visible in the dark.

### Improvements
- Geldarms can no longer spawn in the air, and will rerolled into a new enemy if this isn't possible.
- Allow portable mode for settings if portable_mode.txt is found. This means your settings will be saved in a Settings folder inside the program folder.
- Add a portable Windows build and Linux (Ubuntu) & Mac versions to the release assets.
- Added 3 new sprites (Thanks valence)
- Removed swordsman sprite (It was the same as river man).


## Version 5.0.8 - November 15, 2025
- Removed dripper entrances from the 5.0 room pool.

## Version 5.0.7 - November 9, 2025
- Fixed another bug related to segmented room connections.
- Slightly increased the default window height to avoid options not being visible.

## Version 5.0.6 - November 8, 2025
- Fixed an issue where the UI gets confused about what version it is.
- Removed a wall statue from a room to avoid confusion

## Version 5.0.5 - November 06, 2025
- Re-Fixed a bug that caused palaces with multiple item rooms to always use the same room shape for it's item rooms.
- Fixed a bug where drops into segmented rooms could cause invalid exits that take you outside.
- Updated Alucard sprite

## Version 5.0.4 - October 29, 2025

### Bug Fixes
- Improved enemy handling to reduce the likelyhood generators despawn other room elements
- Updated validation on the overworld tab to be more consistent with previous versions.
- Fixed an issue with segmented normal rooms on random walk
- Fixed an issue preventing drop zones from being placed directly below non-drop rooms on reconstructed.
- Blocking rooms can no longer appear in palaces they shouldn't in sequential and reconstructed.
- Fixed an issue that could cause inescapable drops from appearing when they should not, and vice versa.
- Fixed yet further bugs that could cause the sprite preview to not work properly.
- Fixed some issues that could cause screen glitches.
- Fixed helmethead's HP bar to display correctly. His actual health remains unchanged.
- Fixed a bug that could cause hints for items in new kasuto / nabooru to incorrectly display the wrong location name.

### Improvements
- Updated the default flag presets to include the swiss and bracket flags for the 2025 Standard tournament
- Added 2 new sprites (Doomknocker and Moblin)
- Slighly modified drippers when dripper variance is reduced.
- Added some new options for link's projectile and improved some projectile animations when link uses them.
- Moderate performance increase to reconstructed generation times, especially in limited room pools.
- Slightly adjusted the edges of some drop zones to avoid potentially getting stuck in the ceiling.
- Flipped the drop column item room in 5.0 rooms to avoid a potential bug. Please don't jump in the lava.
- Removed floor-level lava from a couple rooms.
- Slightly moved an enemy in a GP room that could instantly hit link on respawn.

## Version 5.0.3 - October 18, 2025

- Fixed a bug that could cause glitching backgrounds on level up on some hardware/emulators.
- Fixed some graphical glitchyness with spell tower.
- Fixed a bug where non-random, non-vanilla drop pools didn't work
- Removed 2 untended variants of an item Room
- Fixed Up/Down stab text when reversed and not in the item pool
- Removed the glove that displayed when hud lag glitching triggered (again)
- Fixed another bug that could cause the sprite list to duplicate

## Version 5.0.2 - October 17, 2025

### New stuff
- Added additional options for handling less important overworld locations
- Updated town text boxes to 6 lines.
	- All shortened item names in text are now full names
- Boss HP bar now shows differently colored boxes for health beyond the vanilla amount.
- Boss HP bars are now more consistent amounts of HP.
- Updated Maze Island river generation
	- River opening can no longer be wide on one end.
	- River forks are now equally likely to appear at all X values
	- River can now open from either end of the maze (or both)
- Added Guma sprite
- Slightly updated the name of some flags to be more accurate.

### Bug fixes
- Fixed several UI bugs and made some minor improvements.
- Fixed vanilla text for stab trainers.
- Fixed a bug preventing overwriting custom flag presets.
- Fixed a bug with the command line version.
- Fixed a bug where valley of death encounters could allow users to walk into the mountain. (For real this time)
- Disabled a new room that was inadvertently set as a 4.0 room.
- Fixed a bug where vertical bubbles in GP could not be blocked.
- Fixed "I know nothing" text for some townsfolk.

## Version 5.0.1 - October 12, 2025

- Added a sprite color picker with more colors.
- Added a 3rd editable sprite color
- Fixed a bug that caused the logic for segmented rooms to be incorrect.
- Fixed a bug that sometimes caused drops to incorrectly be marked as inescapable.
- Fixed the 5.0 GP up elevator entrance, which inadvertently went down.
- Corrected the palace room groups to be labeled 5.0 instead of 4.4

## Version 5.0 - October 9, 2025

### Major Features
- Rebuilt the User Interface - This new interface is more customizable than the old one and should allow more improvements in the future.
- New Web Version - Check it out at https://z2r.app/
- Added over 100 new palace rooms from the 4.4 Room Jam.
	- A searchable visual reference for all the new rooms is available at https://initsu.github.io/Z2R-Rooms/
- Added options to add additional items to the item pool.
	- Include spells in shuffle - Wizards can give normal items and spells can be found out in the world
	- Include Sword Techniques in Shuffle - Up/Down stab can be found in the world and their trainers could hand out other items.
	- Include Quest Items in Shuffle - Saria's mirror table, the fountain in Nabooru, and bagu all have any item, and their items could be out in the world.
- New palace styles
	- Sequential - Like reconstructed, rooms are picked from the pool at random and placed into the palace, except in a way where the geometry of the palace makes sense. If you go up -> right -> down -> left, you will always end up back where you started.
	- Random walk - Creates a random dungeon shape, and then fills that shape up with rooms that fit. Like sequential this style has sane geometry. Generally creates palaces with fewer dead ends, and a more centralized shape.
	- Chaos - An evil palace style where any exit can lead to any room. You're guaranteed to be able to get to all rooms, but going back the way you came is not guaranteed to lead you to the room you were in before. This style is never selected with any random option.
	- Random (All Same) - All palaces use the same style, chosen at random.
	- Random (Per Palace) - Each palace selects its own style randomly.
- Added an option to generate a spoiler log. If selected a .spoiler file will be generated with the same name as the output ROM. Seeds generated with spoiler log on will be completely different than seeds with spoilers off.
- Palaces can now have more (or fewer) item rooms. Select 0/1/2 or random in the palaces tab.
- Added an option to add additional copies of important items. These replace minor items as available.
- Added a statistics screen after the ending.

### More New Features
- Starting items/spells can now be limited
	- If the number of selected starting items is greater than the limit, items will be randomly removed down to the limit. 
	- If shuffle starting items is on and there are fewer selected starting items than the limit, the number of starting items may be less than or equal to the limit.
- Random flag rate is now configurable. Any ? flags have the indicated percent chance (either 25/50/75/90%) to be turned on.
- River devil has learned some new tricks! He may now block paths (like vanilla), one of the east caves, or siege one of the towns.
- Added a setting to allow rock blocks in the east.
- Added an option to shorten normal palaces.
- Re-enabled the ability to shorten vanilla palaces.
- Added an option to have palaces continue after bosses at random. 
	- All boss rooms now have a statue at the end if the palace exits, and none if it continues.
- Added an option to limit dripper RNG.
- Added an option to require a minimum length of the path to dark link.
- Added options for random(high/low) for attack/magic/life effectiveness.
- Added an option to change the outline color for player sprites
- Added new sprites. Thanks to the authors for providing them.
	- Alex (River City Ransom)
	- Alucard
	- Barba
	- Beard Link
	- Faxanadu
	- Garfield
	- Geru
	- Gooma
	- Helmethead
	- LittleMagic
	- Mario
	- NinjaGaiden
	- Peach
	- PowerRanger
	- PsycoRanger
	- PunchOut
	- Shadow
	- Shantae
	- SlothLink
	- Sonic
	- TMNT Leo
	- Trogdor
	
### Enhancements / bugfixes
- Improvements to the command line interface to better support other OSes.
- Added the ability to load settings via JSON file instead of a flag string
- Adjusted the damage on some enemies that had unintended damage values in GP
- Innumerable technical improvements to speed up generation time
- Fixed garbled sprites on horsehead / rebonack appearing outside their palaces.
- Enemies that are immune to sword are now always vulnerable to fire.
- Fixed the hitbox on lava so it no longer extends a tile above the lava.

## Version 4.3.19 - April 20, 2025

- Fixed several climate bugs, including a bug that would cause chaos vanillalike DM to never generate.

## Version 4.3.18 - April 15, 2025

- Fixed a rare hidden kasuto issue

## Version 4.3.17 - April 7, 2025

- Fixed yet another hidden palace issue.

## Version 4.3.16 - April 5, 2025

- Fixed a bug with hidden palace caused by the previous fix to hidden palace.

## Version 4.3.15 - April 2, 2025

- Fixed a bug that sometimes causes hidden kasuto/palace to not work properly.

## Version 4.3.14 - March 22, 2025

- Fixed an issue where items required for Reflect could be behind Carock when palaces continue after bosses.
- Fixed an issue where hidden palace/kasuto could be tracked wrong and sometimes not work correctly
- Fixed an issue caused by the previous issue that could cause 3-eyed rock to appear in an incorrect position
- Fixed multiple issues that could cause the raft tile to overlap with a bridge.

- Reduced the amount of lag produced by custom music
- Fixed several custom music related issues
- Added an option to use/disable vanilla music when custom music is enabled

## Version 4.3.13 - October 16, 2024

- Faster text is now instant text. Rejoice!
- Added options for +0/+4 for Max Hearts
- Added support for Custom Music. See [this wiki entry](https://github.com/Ellendar/Z2Randomizer/wiki/Custom-Music)  for directions on how to set it up. Big thanks to Quantum for adding this.

- Disabled simultaneous L+R inputs.
- Removed the Glove indicator on lag frames when the HUD lag fix was enabled. It will return as an option in the next major release.
- Fixed a bug where enemy projectiles could function incorrectly after killing hard carock
- Fixed an issue where the sleepy bot in saria displayed his vanilla text when helpful hints were on and he didn't have a hint.
- Fixed the upstab/downstab get text when community text is disabled and upstab/downstab are swapped.
- Fixed an issue where items required to get reflect could be behind Carock
- Fixed an issue where starting life level was controlled by the starting magic level selector.
- Fixed an issue where low attack levels 1-6 were 1 damage lower than intended.
- Fixed a bug that could crash the game if you leveled up or died while reading a sign.

## Version 4.3.12 - May 12, 2024

- Fixed a bug where hidden kasuto / 3-eyed rock could incorrectly select locations that were already placed under some circumstances in volcano biomes.

## Version 4.3.11 - May 10, 2024

- Fixed a bug where the basement guard in New Kasuto could tell you an incorrect number of required containers.

## Version 4.3.10 - May 5, 2024

- Fixed a bug where wizards would display text based on their town and not the spell they had when community text was off.
- Fixed an issue where rock blocked caves could "double up" and be offset by 2 with two rocks.
- Added a safety so if a cave exits directly into water, there is a one tile road so you can turn around and go back in.
- Increased the number of allowed room placement failures before a palace is rejected. This should result in fewer seeds where GP takes a long time to generate.
- (Room Jam) Fixed some issues that arise when using only room jam rooms in the pool (still doesn't work properly until we have more GP rooms.
- (Room Jam) Fixed a drop room with invalid exit data.
- (Room Jam) Updated the default custom rooms file to contain more submitted rooms.

## Version 4.3.9 - April 29th, 2024

- Fixed a bug that could result in text/items being incorrect
- Moved "Use Custom Rooms" to the palace tab.
- Fixed handling for 2/3 screen rooms (Room Jam)
- Fixed a few bugs related to Room Jam rooms

## Version 4.3.8 - April 28th, 2024

- Fixed a bug generating text
- Fixed an issue preventing up entrances from working. (for custom rooms)

## Version 4.3.7 - April 28th, 2024

Special thanks to jroweboy for the bugfixes in this patch.

- Fixed an issue where heart/magic container graphics could display incorrectly when custom sprites were used.
- Fixed an issue where enemy shuffle could behave differently if community text was on.
- Fixed an issue where palace generation could fail if there were too many enemies in palaces 1-6
- Fixed an issue where the hash incorrectly did not contain information that could affect the seed.

## Version 4.3.6 - April 23rd, 2024

There is a known issue in this release: If you are using a custom sprite, sometimes heart/magic containers will have incorrect graphics if found in a palace. They will still work normally, the issue is just cosmetic. Please look forward to a fix in the next release.

- Fixed a bug where vanilla attack would cause seeds not to generate.
- Fixed an issue with the installer where updates would cause it to stick on "generating palaces"

## Version 4.3.5 - April 21st, 2024

There is a known issue that may still affect this release. If your first seed after updating gets stuck on "Generating Palaces", uninstalling and reinstalling will fix it.
We will be looking at different installation techniques in the future if this keeps happening.

- Fixed a bug where climate expansion was incorrect.
- Fixed a bug causing artifacts on the title screen for some hardware.
- Fixed a bug where rooms could randomly have unintented walls blocking progress.
- Fixed a bug where up+A in GP could take you to the wrong room.
- Fixed a bug where the incorrect sound would play when picking up some small items.

## Version 4.3.4 - April 18th, 2024

- Fixed a bug where some properties were sometimes randomized outside their valid ranges.
- Fixed a bug where the spell items were sometimes incorrectly assigned based on starting spells.
- Fixed an issue where palace pallettes would sometimes not properly load.
- Fixed an issue the file naming cursor was not displayed properly

## Version 4.3.3 - April 18th, 2024

### New features

- Updated the pause screen to now show icons for up/down stab, trophy/medicine/child, and bagu's note/mirror/water
- Added an option in customize to prevent the HUD from flashing when lag happens. This does not increase or decrease the amount of lag.
- Renamed "Remove spell items" to "Start with spell items". This functions the same but it now shows you the items you start with in the pause screen.
- Updated "Shuffle small items". Now instead of shuffling the small items between different rooms in that palace, small items are selected at random from a weighted list. The current weights are:
	- 35% Small Key
	- 10% Blue Jar
	- 10% Red Jar
	- 10% Small PBag
	- 15% Medium PBag
	- 10% Large PBag
	- 5% XL PBag
	- 5% 1Up
- The Jig is up. ??? Is now properly labeled as "Knockback Randomizer". This will likely be toned down in a future release but for now enjoy the wackyness.
- Slightly updated the default saved flags.

### Bug Fixes

- Fixed "Palaces have extra keys" to once again have extra keys.
- Fixed a bug where high/low attack didn't work properly.
- Slightly adjusted the rouding used in these curves from the old versions.
	- High attack now 3/4/6/9/13/18/27/36
	- Low attack now 1/2/3/4/5/6/9/12
- Fixed several bugs related to hard carock
- Fixed a bug where some rooms with complex requirements we not properly handled.
- Fixed a bug in one of the logic safeties that didn't properly consider up/down stab being available on the west.
- Fixed a rare bug that would break generation if the palace room pool ran out of drop exits.
- Fixed a UI issue related to palace styles enabling/disabling the wrong options.
- Fixed an issue where glove blocks could appear in P5 with "blockers anywhere" off.

## Version 4.3.2 - April 3rd, 2024

- Fixed yet another issue with the L7 room

## Version 4.3.1 - April 1st, 2024

- Fixed an incorrect elevator room in GP
- Fixed an issue where a palace could have an extra P1 boss room
- Fixed an issue where the UI would not save last used flags during batch generation

## Version 4.3.0 - March 31st, 2024

### New features
- Added support for the 2024 Room Jam! Check out the RoomJam channels in the discord for how you can design/submit new rooms.
- Overworld generation now guarantees that all caves will eventually be accessable. It is no longer possible to have unreachable caves that contained unneccessary items.
- Added a separate option for biomes to be random, random (no vanilla), and random (no vanilla/shuffle).
- Significantly reworked the palace generation logic to better support weird or unusual room types. As a result of this, segmented rooms are much more likely to contain both halves.
- Vanilla and community rooms can be independently selected. Try a run with just community rooms and no vanilla.
- Updated the normal/GP palace style settings. You can now select different palace styles for normal/GP palace styles/lengths.
- Added an option for harder carock. (Thanks jroweboy)
- Experimental feature: Climates.
	- Where biomes change the shape of the terrain, climates control the types/sizes/frequency of terrain that appears.
	- This feature is still experimental and may cause generation times to be longer than normal if used.
	- Possible options:
		- Classic: Even weights. Works just like previous versions.
		- Chaos: Terrain shifts quickly and unpredictably. In a future update this will probably include more nonstandard terrain typ
		- Wetlands: Swamps? Check. Deserts? Nope. More swamps? Double check.
		- Great lakes: Bigger lakes, more forests.
		- Scrubland: A nice clear climate free of swamps and high in grass and desert.
- Updated the default preset flags. Saved flags will now be reset to the defaults whenever an update changes the flags format (which includes this update). Sorry for the inconvenience.
- Community hints have been renamed "Community Text" and are no longer part of the flag string. They can be enabled/disabled and the seed will be the same. This should allow people to use them (or not) in races as they like.
- Added community text for "I know nothing" and "Come back when you are ready" replacements.
- Added a bunch of new community text options. Thanks to everyone in the #community-text channel in the discord for their suggestions.
- Added a setting for different kinds of "No duplicate rooms" flags.
	- No Duplicate Rooms (By Layout/Enemies) - Like the old no duplicate rooms. A room is considered to be a duplicate if it has the same layout and the same enemies.
	- No Duplicate Rooms (By Layout) - Only considers the layout of the room to determine what is a duplicate.
	- These options may not be available when the list of available rooms is too small to support the number of required rooms.
- Moved the bagu's woods hint from the bot to the river man (on both sides of the river)
- Added a selector for starting lives.
- ???

### Bug Fixes
- Fixed a bug where palace/overworld items were improperly shuffled. This has several impacts, but most notably fixes the "candle always in palaces" bug.
- Fixed a bug were leaving Maze Island via a cave was not in logic.
- Fixed a bug where canyon biomes in the east were always dry
- Fixed a bug where one of the caves in 4-way Death Mountain groups was sometimes facing the wrong direction.
- Reworked caldera biome generation to fix several possible bugs resulting in caves not exiting properly. This also makes different caldera connection types properly weighted.
- Avoided a bug where leaving a hidden palace that was on the same column as hidden kasuto caused a wrong warp. It is now not possible that these two will have the same X-coordinate.
- Fixed an issue where helpful hints on the sleepy saria bot were sometimes suppressed.

## Version 4.2.7 - September 17th, 2023

- Fixed an issue where items required to get Reflect spell could be behind Carock (thanks TrailZ)
- Fixed a regression where overworld small items would not shuffle properly.

## Version 4.2.6 - August 20th, 2023

- Fixed another bug that would prevent vertical DM from spawning.
- Fixed a UI issue that would cause "Include lava in terrain shuffle" to sometimes not update properly.
- Due to me screwing up the branch, 4.2.X releases will not have source on the github (hopefully this is the last one anyway).

## Version 4.2.5 - August 18th, 2023

- Temporarily disabled Garfield sprite due to potential crashes. If you have already installed 4.2.4 it will still show up in the list, but crashes may result if you use it.
- Fixed a bug where some palace enemies would incorrectly never shuffle. This was a combination of 4.0.4 and 4.2 bugs, so many rooms that haven't shuffled in a long time may be shuffled now. Be warned!
- Fixed a bug where some overworld locations that always contain small items did not properly shuffle when shuffle small items was on.
- Fixed a bug where some DM biomes almost always selected specific vertical/horizonal orientations. All biomes that have vertical/horizonal orientations are now exactly 50/50 for each option.
- Fixed a bug that could cause caves in volcano/caldera biomes to exit into mountains.
- Slightly decreased the maximum distance of caves in volcano/caldera. This now makes volcano and caldera's distance identical to 4.0.4 volcano (which is slightly longer and less random than 4.0.4 caldera)

## Version 4.2.4 - July 27th, 2023

- Fixed a bug where darunia's spell was sometimes not requiring the child
- Fixed a bug where item room bosses were sometimes incorrectly positioned when palace enemy shuffle was off
- Fixed an issue where the UI would not clear lava shuffle when flags were applied that included lava shuffle but not encounter shuffle
- Fixed a bug where encounter rate couldn't be set to "Random"
- Upated Cheese Link sprite. (Thanos Irenepunmaster)
- Added Garfield sprite. (Thanos Irenepunmaster)

## Version 4.2.3 - July 22nd, 2023

- Fixed a bug causing some overworld encounters to spawn enemies with incorrect y-coordinates
- Fixed a bug in palace generation that could cause invalid exits.

## Version 4.2.2 - July 17th, 2023

- Updated flags field to not check flags validity when the box is blank.
- Fixed a crash related to beep frequency: "Off"
- Fixed a bug where the logic would think you can get spells in towns you can't reach
- Fixed an exception when you hit convert with an empty flags box.
- Added a check to swap up/down stab text if up/down stab are swapped and vanilla hints are on.
- Cleaned up the gem pedestal in up/down elevator barba rooms (Thanks mirai/eunos)
- Small corrections to a DM in Palaces room that was incorrect.
- Fixed a bug that sometimes caused the item in one palace to be incorrect
- Updated Hoodie Link sprite to have hood down in palaces/towns. (Thanks Knightcrawler)
- Updated mixed item shuffle to use unbiased shuffling algorithm

## Version 4.2.1 - July 14th, 2023

- Fixed a bug that could cause town items to not correctly randomize.
- Updated the item shuffle algorithm to be less biased.
- Added options to control the threshold / frequency of low health beeps. (Thanks jroweboy!) If you don't like the beeping, set the beep frequency to "off".'
- Moved dash/jump always on to the "Spells" tab.

## Version 4.2.0 - July 13th, 2023

### New features
- Updated community hints to have new / different hints. Have ideas for new texts? (especially for specific spells/locations) Post them to the #community-hints channel on discord.
- Updated the hints system to allow per-location / per-spell hints.
- Added a new sprite preview window to the misc tab that will show off what the current sprite/color combo will look like. (thanks jroweboy!)
- Added new sprites and converted all existing sprites to IPS sprites. If you are a sprite author and have any sprites you want to have added, ping Ellendar on discord.
- Added a system for crediting sprite authors in the UI and in the opening crawl. If you are the author of an existing sprite and would like to be credited, ping Ellendar on discord.
- Updated the installer to automatically clean up old versions when it runs. This will remove all old 4.1 and later versions when you install this update. 4.0.4 and earlier are not affected.
- Fixed the attack randomization procedure to be more statistically accurate. This will result in more normal attack progression and fewer weird outliers, especially at lower levels.
- Reworked the logic for palace routing to more accurately reflect how the rooms are actually constructed.
	- Dash is now allowed in logic, and rooms can be cleared by dashing over pits (possibly with jump as well).
	- Glove is now allowed to be in palaces that contain glove blocks, as long as it is possible to reach the glove without the glove.
	- Several rooms with incorrect logic were updated.
	- Magic key or fairy is still logically required for any palace. (for now...)

### Bug Fixes
- Fixed a bug from 4.1.8 that would cause the split item room to exit outside when you went down the elevator.
- Fixed a bug that would cause minor items in horsehead's item room to despawn when you beat horsehead (Thanks jroweboy!)
- When community rooms are off, all boss rooms will correctly enter from the left.
- When community rooms are off, all item rooms will now be one of the vanilla item rooms.
- Fixed a bunch of small logical bugs that could lead to seeds taking a long time to generate.

## Version 4.1.8 - June 28th 2023

- Fixed an issue where sin wave head generators in palaces 3/4/6 under reconstructed palaces with vanilla enemies caused a lockup.

## Version 4.1.7 - June 17th 2023

- Fixed an issue that could cause the first room of palace 3 to be corrupted.
- Corrected some updated rooms that had gotten reverted to their previous version.
- Added a process to scrub exit data from invalid exits. Note this means ALL unintended exits will now lead outside.

## Version 4.1.6 - June 3rd, 2023

- Updated the wiki/discord buttons to confirm before opening your browser. (Thanks nelyom!)
- Custom / Preset flags boxes are now editable/customizable. Right click them for options. (Thanks nelyom!)
- Fixed two "Death Mountain in palaces" rooms to match what they actually look like in DM.
- Townsfolk will now say their vanilla phrases instead of "I know nothing" when helpful hints are off, but spell hints are on.
- Fixed a bug where sometimes enemies would not be properly updated so rooms would use chunks of the vanilla enemy data isstead of their proper enemies.

## Version 4.1.5 - May 23rd, 2023

- Fixed a validation issue that could cause "No duplicate Rooms' to be disabled.

## Version 4.1.4 - May 21st, 2023

- Fixed a bug that would replace some large enemies with fairies when mixed enemies was off and palace enemy shuffle was on.
- Actually fixed the PalaceStyle bug from before...
- Fixed a bug where enemies would dissapear when palace enemy shuffle is off on reconstructed palaces.
- Fixed a bug where the hash incorrectly did not contain version data.

## Version 4.1.3 - May 20th, 2023

Flag strings have changed (again). Eventually I want to create a system where the flags conversion box works for all old versions of the flags, but for now it will only convert from 4.0.4 to the current version. Any flags from 4.1.2 you'll have to manually update. Sorry.

.NET requirement updated to 7.0 from 6.0 This will download automatically so you should be fine. I promise this is the last .NET update for a while.

### Bug Fixes

- Fixed a bug in the fix to the vanilla shuffle desert tile bug.
- Corrected several item rooms that were not marked as having items, causing slightly incorrect logic.
- "Dash always on" now works correctly. Thanks alewifey for reporting this and Cody for the fix!
- PalaceStyle of Random no longer only chooses vanilla. Thanks TAH for reporting this.
- Fixed a logic bug where seeds couldn't generate if shuffle pbag cave items was off, but extra hearts in the pool were forced into the pbag caves.
- Fixed a bug where community boss/item rooms could appear when community rooms were off.

### New features

- New Flag: "Generators always match" - When this flag is on, enemy generators (the ones that create enemies from the left/right side of the screen) will always match on the left and right side like they do in vanilla.
- New Flag: "No duplicate rooms" - When this flag is on, each palace will not have rooms in it that are duplicates of other rooms in that palace. Each room in the room pool will appear at most once. For rooms that have several versions in the room pool with different enemies (like GP bridges), if multiple of those rooms appear, they will be guaranteed to have at least one enemy different, even after enemy shuffle.
- Linked fire / Replace fire with dash are now a single combined option with options (Vanilla / Link fire with random spell / Replace fire with dash / Random)
- A bunch of tweaks/bugfixes to palace generation, especially for drops.

## Version 4.1.2 - April 16th, 2023

- Added the ability to sideload more sprites. Add some IPS patches to the Sprites folder and they will show up in the sprites list when you launch the randomizer and be applied when you generate the rom.

## Version 4.1.1 - April 15th, 2023

- Fixed an issue where sprite shuffle didn't work.
- Fixed a bug that could incorrectly disable spell hints.
- Updated sprite colors so that "Default" now doesn't set sprite colors if you have your sprite set to "Link". This allows you to preload your sprite to the rom and have it not be overwritten.

## Version 4.1.0 - April 14th, 2023

### Bug Fixes

- Fixed an issue with eastern desert tile in vanilla shuffle that sometimes made the item inaccessible.
- Fixed an bug where helpful spell hints would sometimes incorrectly display the vanilla hint.
- Fixed an issue with some community rooms that would cause the item to despawn if you died or left the room after opening the key door.
- Fixed a rare crash when generating seeds.
- Fixed a bug in a community room where the elevator could despawn if you killed a king bot.
- Fixed a bug where item accessability data would sometimes persist from seed to seed, resulting in incompleteable seeds due to inaccessible items.

### New features

- Reworked the flag generation system. To convert from the old 4.0.4 flags, paste them to the "Old Flags" box and click "Convert"
- Many options now support an indeterminate state that is randomly on/off.
- Items can now be marked as starting items even when starting item shuffle is on. You will always start with those items, and others will be randomized normally.
- You can now set a range of possible starting heart containers.
- Max heart containers can be set to +1/+2/+3 guaranteeing that number of additional containers beyond the starting amount.
- Added an option to include/exclude lava encounters from the encounter shuffle in the east.
- Added an option to swap/shuffle up/down stab locations.
- Number of palaces to complete can now be specified as a min/max.
- Drop pool items can now be forced, even when randomizing. 
- Remove flashing Upon Death now defaults to on. Please be considerate and keep it on for races.
- Added an option to use custom room data. This will change the generated rom/hash. Ask Ellendar for details if you're interested.
- Added a link to the Z2Randomizer Discord. Check us out!

### Other Updates

- Substantially updated the seed verification logic to significantly increase speed.
- Jump+Dash to cross the Saria river is now in logic.

## Version 4.0.4 - August 1st, 2021

- Added a dash always on option
- Fixed a bug where the build number was not included in the game hash

## Version 4.0.3 - December 5th, 2020

- Fixed a broken connection on a room in GP
- Fixed an issue that prevented seeds with vanilla palaces from generating
- Fixed a bug that made Bagu inaccessible in some seeds

## Version 4.0.2 - December 3rd, 2020

- Fixed an issue with improper drop room connections
- Fixed an issue with room layouts in GP
- Fixed a bug that prevented the hidden palace location from appearing in rare situations

## Version 4.0.1 - December 1st, 2020

- Fixed some room connection bugs
- Fixed a bug that didn't check if reflect was blocking an item
- Fixed a bug that prevented seeds from generating when Thunderbird was removed

## Version 4.0 - November 28th, 2020

- Removed items are no longer replaced with a pbag, they can be replaced with any small item
- Implemented a level cap, along with an option to scale xp requirements to the level cap
- Added an option for town signs to tell you what spell is in the town (thanks, Thirwolf!)
- Added an option to reduce the encounter spawn rate or turn them off entirely
- Introduced new exp shuffling levels
- Added an option to allow users to select their starting atk/mag/life level
- Added options to shuffle how continents are connected
- Added new overworld biomes (Islands, Canyon, Caldera, Volcano, Mountainous)
- Added an option to create new palaces
- Added the ability to use community based rooms in the palaces (thanks GTM, Scorpion__Max, eonHck, aaron2u2, TKnightCrawler, Duster, Link_7777)
- Reintroduced vanila maps
- Removed a few more extraneous rooms from spell houses
- Added an option to remap Up+A to controller 1
- Added an option to allow less important locations to be hidden on the overworld
- Added an option to restrict how connection caves are placed
- Added an option to allow connection caves to be boulder blocked
- Added an option to randomize which locations are hidden in New Kasuto/Hidden Palace spots
- Added an option to remove the flashing death screen
- Added an option to randomly select character sprite
- Added an option to randomize the item dropped by bosses
- Added an option to generate Bagu's Woods
- Fixed softlock when dying while talking (thanks eon!)
- Fixed a bug that robbed you of 1 attack power at attack 5 in rare situations
- Added the Yoshi, Dragonlord, Miria, Crystalis sprites (thanks TKnightCrawler!)
- Added the Pyramid sprite (thanks Plan!)
- Added the GliitchWiitch sprite (thanks RandomSaliance!)
- Added the Lady Link sprite (thanks eonHck!)
- Added the Hoodie Link sprite (thanks gtm!)
- Added the Taco sprite (thanks Warlock!)

Double clicking on the box with the flags selects all of the text automatically

- Removed options to shuffle xp of bosses and enemies seperately
- Changed UI for level effectiveness
- Continued my never ending quest to get tooltips right
UI now updates progress of seed generation

## Version 3.19 - August 25th, 2020

- Flag validation is more robust, and will be checked before any seed is generated.
- The hash actually works now.

## Version 3.18 - August 25th, 2020

- Loaded a set of flags when the program first starts
- Added version number to the hash so that it will be different for each version number

## Version 3.17 - August 25th, 2020

- The interface does a better check of the flags to make sure they are valid
- Fixed a bug that didn't shuffle the HP of all enemies (thanks, TheCowness!)
- Fixed a bug that sometimes did not generate helpful spell hints
- Updated preset buttons for the upcoming 2020 Standard Tournament

## Version 3.16 - July 11th, 2020

- Added a hash at the top of the title screen that shows whether or not racers are on the same seed.

## Version 3.15 - July 7th, 2020

- Fixed a bunch of dumb stuff that I broke in the last version.

## Version 3.14 - July 6th, 2020

- Fixed a bug that prevented spell item hints from generating (thanks ....?)
- Fixed a bug that prevented seeds from generating if spell hints were selected and you started with those spells
- Fixed a bug that put the user in the wrong spot upon exiting new kasuto/p6 sometimes (thanks fon2d2)
- Fixed a bug that prevented attack levels from being generated properly (thanks TheCowness!)
- Updated preset buttons
- Added a feature that shuffles pbag xp values (thanks CF207!)

## Version 3.13 - May 24th, 2018

- Fixed a bug with random hidden palace/new kasuto
- Fixed a bug that should hopefully speed up seed generation a bit
- Added an option to allow users to select their beam sprite
- Added a whole bunch of new playable character sprites (thanks Knightcrawler, Lufia, and Plan!)
- Added two forms of helpful hints
- Items in the drop pool are now guaranteed to appear in the pool at least once
- Added an option to standardize drops
- Added an option to randomize which items are in the item pool
- Added some additional community hints (too many people to thank for this one)
- Various UI tweaks and fixes

## Version 3.12 - May 17th, 2018

- Fixed a few minor graphical glitches when using custom character sprites
- Fixed a really dumb bug with the heart containers that never should have happened but it did so whatever
- The application will now remember some of your preferences (rom path, custom flags, tunic color, sprite, etc.) even after updating the application

## Version 3.11 - May 16th, 2018

- Fixed multiple bugs preventing hidden palace from appearing
- Added Zelda sprite (thanks KnightCrawler!)
- Added Iron Knuckle sprite (thanks Jackimus!)
- Added options to allow users to manually select tunic colors
- Added the ability to save custom flag sets
- Added a new Drops tab that can be used to configure enemy drops
- Migrated update server
- If you start with a spell, the corresponding spell item is replaced with a 500 pbag
- Added an option to remove spell items

## Version 3.10 - May 13th, 2018

- Added an option to include three eye rock on the overworld, hiding a palace (thanks CF207!)
- Added an option to hide new kasuto behind a forest tile (thanks CF207!)
- Added an option to randomize what items enemies will drop
- Adjusted the tunic randomization so that shield and normal tunic colors will always be different
- The GUI will now remember some miscellaneous settings (tunic, low health beep, beam sprite, fast spells)
- Fixed low health beep (thanks Myria!)
- Added an option to disable in game music (thanks JillyRabbit!)

## Version 3.06 - July 7th, 2017

- Fixed a bug that sometimes didn't spawn a maze island bridge

## Version 3.05 - June 29th, 2017

- Fixed a bug that sometimes generated multiple maze island bridges
- Updated sprite palette shuffle to be more random than it previously was
- Updated presets (thanks AngryLarry!)
- Fixed a couple of minor UI bugs

## Version 3.04 - May 27th, 2017

- Added an option to shuffle which enemy spawns from a dripper
- Added an option to shuffle sprite palettes
- Fixed a bug that incorrectly tracked shuffled spells
- Fixed a bug that sometimes gave extra keys upon palace completion
- Ensured that all item caves are reachable, regardless of whether or not they have an important item
- Updated some tooltips

## Version 3.03 - May 20th, 2017

- Fixed a bug that generated unbeatable seeds if certain items were placed in New Kasuto

## Version 3.02 - May 19th, 2017

- Fixed a bug that crashed the game on tbird if gp wasn't in the east
- Fixed a bug that didn't display items properly in some palaces
- Fixed some minor UI stuff

## Version 3.01 - May 17th, 2017

- Added an option that will allow palace 7 to spawn outside the valley of death.
- Added an option to allow batch seed creation
- Changing palace palettes will now affect palace 7
- Minor UI fixes and updates

## Version 3.0 - May 17th, 2017

- Items can now appear in any position:
- Spell items can be anywhere
- The randomizer will check each palace to see what items/spells are blocking the item.
- Added an option to place items in pbag caves
- Added option to specify the maximum number of hearts in a seed
- Modified seed generation logic to (hopefully) improve seed generation times
- Any items that you start with will now be replaced with pbags
- Added additional community hints (thanks BuzzThunder!)
- It is now guaranteed that you will be able to find all heart containers
- Fixed a bug that made some projectiles invisible
- Fixed a bug that spawned boulders in front of death mountain caves
- Fixed a bug that sometimes spawned locations right next to the ocean
- Fixed a bug that sometimes put the river on top of the bridge
- Fixed a bug that sometimes prevented OHKO enemies from working
- Fixed a bug that sometimes crashed the randomizer when placing the raft
Reduced the "high attack" attack power just a smidge

## Version 2.30 - March 27th, 2017

- Added a "Remove Thunderbird" option. If "Remove Thunderbird" and "Thunderbird Required" are both unchecked, then Thunderbird will still be in Great Palace, but may or may not be required to get to dark link.
- Added an option that will make Link's beam sword permanent, regardless of how much health he has.
- Added an option that will shuffle the sprite for Link's beam sword. This option (like tunic color) does not affect seed generation.
- Modified the road generation algorithm for West Hyrule. This was done to increase variety and to (hopefully) help reduce seed generation times.
- Updated overworld generation so that it ensures that the raft spots and the maze island bridge will always be adjacent to a walkable land tile.

## Version 2.29

- Fixed a UI bug
- Fixed bug that prevented seeds from generating

## Version 2.28

- Removed Old Overworld Options
- Modified Overworld Generation Algorithm
- If "Thunderbird Required" is unchecked, thunderbird will be removed from the game
- Added an option that combines Fire with other spells
- Randomize tunic color now affects the shield spell
- Fixed a bug that caused crashes during seed generation

## Version 2.27

- Fixed a bug with Maze Island item locations (thanks darkmagician1184!)

## Version 2.25

- Added an option to change the text for selected in game NPCs. Thank you to everyone who submitted text suggestions!
- Added an option that shuffles how many containers are required for the item in New Kasuto

## Version 2.24

- Made text even faster (thanks lemmypi!)
- Reduced the size of maze island
- Added a rock to death mountain that will always contain an item
- Added an option that generates a random tunic color for link

## Version 2.23

- Fixed a UI bug that didn't always update the flags
- Fixed a bug with up+a in the eastern continent towns
- Fixed a bug that broke up+a after saving
- Fixed a bug that overlapped important locations on maze island
- Tweaked rock placements
- Added a feature that reduces the number of rooms in great palace

## Version 2.22

- Fixed a UI bug that didn't always update the flags
- Fixed a bug with up+a in the eastern continent towns
- Fixed a bug that broke up+a after saving
- Fixed a bug that overlapped important locations on maze island
- Tweaked rock placements
- Added a feature that reduces the number of rooms in great palace

## Version 2.21

- Added an option to allow you to restart in the current palace when you game over or up+a
- Fixed a bug with the UI that did not update buttons correctly when flags were changed

## Version 2.2

- Tweaked enemy experience amounts
- Starting attack power will never be less than 2 (same as in original game)
- Fixed a bug that spawned too many boulders on some maps.
- Minor changes to valley of death generation.
- Revamped Overworld tab and Levels and Spells Tab
- Introduced five new modes: high attack, low attack, high defense, high spell cost, low spell cost
- Removed some presets and updated remaing presets
- You can now cast a spell as many times as you want without opening the spell menu
- Text displays much faster (thanks lemmypi!)
- Wizard text is now correct when spells are shuffled (thanks lemmypi!)

## Version 2.13

- If you opt to start with an item, that item will be replaced with a pbag in the game
- Fixed a bug where I was checking for palace 4 instead of palace 3 (yes really, i know, i'm dumb)

## Version 2.12

- Fixed a bug in the sanity checks (sorry Eunos!)

## Version 2.11

- Palaces can be swapped across continents
- Fixed Tooltips
- Fixed a bug that spawned the ocean encounter in hard to see places

## Version 2.01

- Fixed a bug that trapped the player behind a rock coming out of caves
- Fixed a hilarious bug that turned the valley of death into a river

## Version 2.0

Can now generate new overworld maps
- Added option to shuffle palace palettes (thanks questwizard!)
- Fixed exp softlock (thanks myria!)
- Updated enemy placements
- Tank Mode - makes Link invincible
- OHKO Link Mode - Link will die in one hit
- Wizard Mode - Link has infinite magic
- Added an option to shuffle pbag drop frequency

## Version 1.17

- Fixed a bug that generated improper spell costs
- Moved raft animation to actual raft spot
- Fixed music glitch on raft/maze island bridge tiles
- Fixed a bug that shuffled overworld locations improperly
- Adjusted flying enemy placements
- Simplified overworld shuffle options
- Added an option to insert extra keys into palaces

## Version 1.16

- Fixed the sprite bug with the medicine and the kid. No, really. It's fixed. I promise.
- Fixed sanity checks for upstab/downstab

## Version 1.15

- Fixed bugs that trapped players behind rocks or in the ocean
- Fixed a sanity check involving palace 2
- Updated sanity checks to take disabled magic container requirements into account
- Removed dumb moblins because I hate them and they're dumb
- Attempted to fix a bug that causes the kid and medicine to be swapped but its probably still a little broken
- Added version number to title (thanks jkoper!)

## Version 1.12

- Fixed a bug that counted magic containers incorrectly (thanks Eunos!)
- Added a feature that disables the magic container requirements for spells

## Version 1.11

### Bug Fixes:

- Fixed a bug that started you with the wrong spells if spell shuffle is on

## Version 1.1

### Bug Fixes:

- Made sure you can never get stuck behind a rock without a hammer
- Fixed a bug that cause spell costs to be too low (thanks Eunos!)
- Fixed a graphical glitch that affects ropes in palace 3
- Fixed OHKO bugs with rebonack and large skull bubbles
- Fixed Life spell randomization
### New Features:

- Added item shuffle (thanks lemmypi!)
- Added spell shuffle (thanks fcoughlin!)

## Version 1.0

Version 1.0 is finished! Thanks to everyone who helped with testing and bug reports. I couldn't have done it without you! To celebrate the release of version 1.0, there will be a race on February 13th 2016 at 1PM Eastern. I hope to see you there!

- Fixed OHKO enemies - all enemies now die in one hit
- Fixed mago/wizzrobe spawns - these enemies will always spawn on the floor
- Fixed barba HP overflow bug - barba was spawning with too little HP
- Fixed bug that crashes GP on some seeds
- Fixed bug that shuffles caves even when option is not selected

## Version 0.95

Z2Randomizer is now in public beta!

- Added descriptions to each option that can be viewed by hovering the mouse over them
- Added buttons with preset options selected
- Added a button to go to the wiki
- Improved enemy spawn locations for magos, wizzrobes, and skull bubbles
- Fixed a bug that prevented palaces from being shuffled in certain cases (thanks Jaylow7!)
- Fixed an odd bug that caused unbeatable seeds in East Hyrule (thanks Kingdahl!)

## Version 0.94 
- Z2Randomizer is now in public beta!

- Don't spawn magos and wizzrobes in stalfos spots (thanks warlock82!)
- Fixed spawns for stalfos, skull heads, and megmets
- Can no longer enter Thunderbird room from the right (thanks breastickle!)
- Permenantly unhid palace 6 tile

## Version 0.93

- Adjusted the level efficiency randomization to more closely match the original game.
- Fixed a bug that prevented overworld shuffle from working in some situations (thanks Skavenger216!)

## Version 0.92 Lots of bug fixes:

- Recategorized a bunch of enemies so they spawn in more appropriate locations
- Fixed a bug that made spells cost more than what was displayed on the menu (thanks, Jaylow7!)
- Fixed blue stalfos HP shuffling
- Fixed a bug that replaced red jars in p7 with enemies
- Fixed a bug that could cause a softlock at the palace 6 overworld location (thanks GTM604!)
- Added a new option for enemy shuffle. You can now choose to mix large and small enemies or not.

## Version 0.91

- Bug fix that created unbeatable seeds when terrain mixing was off. Thanks to Jaylow7 for reporting the bug!

## Version 0.9

- Beta release of Z2Randomizer!
