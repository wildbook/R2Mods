Have you ever thought "Damn, this game doesn't give me enough things to kill", or "Would be nice if I had a few more friends so I could steal their loot"?  
Ever been stuck wondering "How would the game scale if I got 150 players ingame?"

**Congratulations. This mod is for you.**

----

By faking a playercount higher than your actual playercount, this mod allows you to multiply your loot by any amount.  
It consists of like three lines of code, and all you need to know is that it most likely works.

To set a custom multiplier, use `mod_wb_set_multiplier X` in the ingame console.  
`mod_wb_set_multiplier 4` is default and makes the game scale loot and enemies as if you were playing with four times as many players as you are.

Setting a very high multiplier will make the game choke for a few minutes before you get ingame. Highest tested so far was 2 players and x250, effectively scaling as 500 players. It took around 5 minutes to generate a map, and once we were in there was loot *everywhere*.  
Enemies stop spawning at around 140 effective players, meaning 1x150, 3x50, and so on.

----
**Changenotes**

v1.3.1

  - Restructured project a bit
  - [NOT PUBLISHED YET DUE TO NO NOTICABLE CHANGES]

v1.3.0

  - Now supports configuration, meaning it'll remember the multiplier you set between game restarts

v1.2.1

  - Add a notification on mod_wb_**g**et_multiplier if used with an argument, will hopefully prevent players from using get instead of set by mistake
  - Added a BepInEx badge to the icon

v1.2.0

  - Fix teleporter charge requiring far more time than intended

v1.1.0

  - Fix compatibility with other mods adding console commands (more specifically [RoR2Cheats](https://thunderstore.io/package/Morris1927/RoR2Cheats/))
    
v1.0.1

  - Fix typos in command description and README.md

v1.0.0

  - Initial release

----

Credit for this mod's name and icon goes entirely to [Sipondo](https://thunderstore.io/package/Sipondo/), check out their mods as well.