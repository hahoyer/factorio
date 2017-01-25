Program to manage different sets of mods and save files ("mmasf") for [factorio](https://www.factorio.com/ "Official factorio website: https://www.factorio.com/") on Windows 7 systems.
The program looks for factorio installation and for factorio application data for the current user. The latter is expected under "%appdata%\Factorio".
There it looks at the root configuration file "%appdata%\Factorio\config\config.ini". This file contains the information, where all the other user specific configurations are located, like login data, mods and saves.
Normally this will also be "%appdata%\Factorio". 
But it is possible to have different sets of such configurations, mainly to accomplish different combinations of mods (until now together with saves that fit to these mod combinations - lets see, what version 0.15 of factorio will improve here.)
To mamange these different configurations, Mmasf will help.

The program is developed for steam version of factorio, but it should work with non steam version too.

More functions will follow soon.

Functions so far:

* installer
* presenting different factorio configurations
* select current configuration

Functions planned:

* creating/deleting configurations
* master-slave relations between configurations
* detection of mod usage of save files and masking of incompatible ones
* managing a inheritance tree for save files
* silent persisting of auto saves
* detection of achievements a save file contains
* synchronisation between differnt computers (using services like drop box)
* logging
* multiplatform (need help for this)
