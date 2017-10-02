# Hero Defense 

A tower defense-like game, where enemies spawn, run through the level and try to reach some end point and thus make the player lose the game. The difference is that the player has a first person view of the environment and can move around and place various traps and use spells instead of the regular bird eye view and tower placement. The main inspiration is a game Orcs Must Die, by Robot Entertainment.


## Roadmap 

Gameplay
* Saving/loading on start of persistent data, like unlocked levels and player's level and chosen spells
* Audio - sound effects and background audio
* Custom enemy models with animations and audio
* More levels - currently there is only a "tutorial level"
* More traps
* Spells for the player
* Leveling system - currently the player gains experience and level, but those number affect nothing
* Gameplay balance 

Code specific
* Player movement script - which will replace the standard assets' First Person Controller
* WaveData and spawning system rework, so that a designer has more control over how the waves are created and how the level plays out
* Trap refactor: derived class for each targeting system
