# Z2Randomizer Changelog

## Version 4.1.4 - May 21st, 2023

- Fixed a bug that would replace some large enemies with fairies when mixed enemies was off and palace enemy shuffle was on.
- Actually fixed the PalaceStyle bug from before...

## Version 4.1.3 - May 20th, 2023

Flag strings have changed (again). Eventually I want to create a system where the flags conversion box works for all old versions of the flags, but for now it will only convert from 4.0.4 to the current version. Any flags from 4.1.2 you'll have to manually update. Sorry.

.NET requirement updated to 7.0 from 6.0. This will download automatically so you should be fine. I promise this is the last .NET update for a while.

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
