# RP Profile Viewer - Profile Monitor

This is a program that is used by the RP Profile Viewer addon for The Elder Scrolls Online.  Like the popular [Tamriel Trade Centre](https://www.esoui.com/downloads/info1245-TamrielTradeCentre.html) addon's client program, the Profile Monitor runs in the background and pulls data from a server for its associated addon.

The profile data that this program uses is hosted on [Google Firebase](https://firebase.google.com/), and comes in JSON format.  The program converts this data into a LUA table that is usable by the RP Profile Viewer addon.

The files for the RP Profile Viewer addon are also included in the repository.

## Using RP Profile Monitor

During operation, the Profile Monitor does not have a form or console window.  It will sit unobtrusively in the user's status area (down by the clock).  The program can be interacted with by right-clicking its status icon.  The user interactions are as follows:

1. **ESO Rollplay Site** - This launches a browser window to https://eso-rollplay.net/, which is where users must create and/or edit their characters.
2. **Update Profiles Now** - This immediately forces the program to download new profile data and save it to the LUA file.
3. **Update Options** - This allows the user to change the *update mode* the program uses.  More on this below.
4. **Exit** - As you have likely guessed, this will close the program.

## Update Modes

The Profile Monitor has 3 modes it can operate in:

1. **No Updates** - In this mode, the program will immediately download profile information, and then it will close after a 10 second grace period.  This grace period is so the user can select another update mode if desired.
2. **Manual Only** - The Profile Monitor will sit idle.  The user may initiate profile updates using *Update Profiles Now*, but the program otherwise does nothing.
3. **Automatic** (default) - The Profile Monitor will download profile information immediately, and then sit idle, listening for the ESO game client.  When it detects that the game client has been started, it will download profile information again.  In addition, it will download profile updates daily at midnight.