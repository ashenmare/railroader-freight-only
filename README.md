# Freight Only

**Nexus Mods** https://www.nexusmods.com/railroader/mods/1997

This is the source of Freight Only 0.1.5. The ready-made zip is on the Nexus page.

## What it does
Passengers become optional. In the base game, once two connected passenger stations are open, 40% of your
railroad's reputation comes from passenger service (30% network service, 10% passenger car condition). Run a
freight railroad and leave the stations alone, and that part drags your reputation down.
With Freight Only, the reputation always counts freight performance (60%) and safety (40%), the same weights the
game itself uses for a railroad with no passenger service open. Your reputation sets the discount on milestone
(phase) costs, the highest tier new contracts start at, your equipment discount and your repair bonus, so this is
what lets a freight-only railroad thrive.
You can still run passenger trains: fares are paid as usual, they just don't count toward the reputation. With the
mod on, two of the game's own hints no longer apply: the passenger car's "regular passenger service is critical"
tip, and a station's Active or Inactive status.
The reputation is worked out once a game day, so the change shows at the next daily update.
Good to know: freight performance is the game's own score, and the game counts it as full when no industry has an
active contract yet. So with this mod, a railroad with no contracts sits near the top reputation, as a vanilla
railroad without passenger service already does.

## Install
1. Unzip so you have a FreightOnly folder in  Documents\Railroader\Mods\
   (Linux/Proton: the same folder inside the game's Proton prefix).
   Inside should be Info.json, FreightOnly.dll and 0Harmony.dll.
2. Preferences > Features > Experimental > Allow DLL Mods, then restart the game.
3. Main menu > Mods > Manage Mods > tick Freight Only > Apply.
4. On an existing save, when the game asks about mods, choose "Keep My Current Mods".

## Multiplayer
Only the host needs it: the game works out the reputation on the host and sends the result to everyone. Others
don't need it to join. As with any optional mod, if a player's mod list differs from the host's, the game offers to
match the host's mods when they join; that's normal.

## Settings
Preferences (main menu or Esc menu) > Mods > Freight Only: "Freight only", on by default. Off puts passengers back
into the reputation as in the base game, from the next daily update. In multiplayer only the host's setting counts.
Updates never overwrite it.

## Removing it
Untick it or delete the FreightOnly folder. No game data is changed, but the save remembers the mod in its mod
list, so when you load it without the mod the game asks about mods: choose to keep your current mods. From the next
daily update the reputation counts passengers again as in the base game.

## Privacy
Freight Only's own log lines and messages never show your home folder or account name: when settings.json can't be
read or written, the path is shown from the Railroader folder on, as just its file name, or not at all.

## Changes
0.1.5: nothing changes in the mod; the build script's dash check now also works outside a git folder.
0.1.4: settings menu updated to the house template (Revision 9); the setting's tooltip is shorter; no change to the
reputation.
0.1.3: the third-party notice names this mod (it said Beaten Path). Nothing else changes.
0.1.2: privacy: when settings.json can't be read or written, this mod's own log line no longer shows your home folder
or account name, only the path from the Railroader folder on. Unticking it now always puts passengers back from the
next daily update, even if its hook can't be fully removed. Nothing else changes.
0.1.1: texts only, after the audit: the save remembers the mod in its mod list, the multiplayer mod prompt, the
phase cost discount, the two game hints that no longer apply, and the no-contracts note. 0.1.0 was never released.
0.1.0: first release, for 0.6.2. A remake for 0.6 of the idea behind the pre-0.6 "no passengers" mods, written from
scratch on the game's own freight-only weights.

## Building

Needs the .NET SDK (8 or newer) and a Railroader install for the game's dlls. Point the build at the game's
Managed folder (`Railroader_Data/Managed`) with a property or an environment variable.

```
dotnet build FreightOnly.csproj -c Release -p:ManagedPath="<game folder>/Railroader_Data/Managed"
```

or set `RAILROADER_MANAGED` to that folder once and run `dotnet build FreightOnly.csproj -c Release`. The build stops with a
message if the folder is missing. NuGet brings in Harmony and the .NET Framework reference assemblies.

The output is `bin/Release/FreightOnly.dll` and `bin/Release/0Harmony.dll`. To try it, put both beside `Info.json`
in a folder named `FreightOnly` under `Documents/Railroader/Mods`, and turn on Allow DLL Mods in Preferences.

## License

MIT, see LICENSE.
The release zip includes Harmony (MIT), see THIRD-PARTY-NOTICES.txt.

Making a Railroader mod? Use our settings menu in yours: https://github.com/ashenmare/railroader-mod-settings
Made with AI assistance (Claude), every release reviewed and audited in game by a human.
