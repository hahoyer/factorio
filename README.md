Program to manage different sets of mods and save files ("mmasf") for [factorio](https://www.factorio.com/ "Official factorio website") on Windows 7 systems.
The program looks for factorio installation and for factorio application data for the current user. The latter is expected under "%appdata%\Factorio".

The program is developed for steam version of factorio, but it should work with non steam version too.

At the moment the program is more or less an emtpy envelope. 
More functions will follow soon.

Functions so far:

* handle two mmasf-sets, "current" and "original"
* dumb synchronise of mods and save files
* installer

Functions planned:

* multiple mmasf-sets
* master-slave relations between sets
* detection of mod usage of save files and masking of incompatible ones
* managing a inheritance tree for save files
* silent persisting of auto saves
* detection of achievements a save file contains
* synchronisation between differnt computers (using services like drop box)
* user interface
* logging
* multiplatform (need help for this)
