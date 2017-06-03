
 Simple UI wrapper for the ZEsarUX emulator (https://sourceforge.net/projects/zesarux/). (Intended for debugging my Spectrum Next creations, so expect the features
to head in that direction for now).

 Relies on the enhanced version of the write-mapped-memory command submitted by Cesar (use latest code checkout of
ZEsarUX - 9160f5194fa0381673c718dfc892684517c6941d or beyond


Instructions

 Load ZEsarUX with --enable-remoteprotocol
 (Note I`m generally doing all my testing with : zesarux.exe --enable-remoteprotocol --mmc-file tbblue.mmc --enable-mmc --enable-divmmc-ports --machine TBBlue)
 On first startup, the windows will be a bit messy, just drag and drop to fit your way of working. Window positions
are saved on exit.
 see http://www.youtube.com/watch?v=KhvBmgtVszo

Limitations

 Sometimes you have to ALT-TAB away on initial boot to have the screen update.
 UI cannot set the registers yet (use set-register PC=0 in the log view).
 Only supports Z80 registers in the Registers view.
 Won't start if ZEsarUX isn't already running.

TODO

 Too numerous to mention - but feel free to contribute ideas/code
