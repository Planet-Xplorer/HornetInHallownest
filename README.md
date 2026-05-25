# Hornet in Hallownest!

This is my repo for a mod that will ideally allow for user to play as Hornet from Silksong in Hollow Knight.

The main goal for this is so that player can use their tools, silk skills, etc. when being as hornet. Hornet should be spawning in with all her movement abilities unlocked, following precedent by the "Knight in Silksong" mod, which did the same thing, since I presume it'll be irritating to also add progression, and generally most people using this mod just want to fight all the bosses in hollow knight as hornet anyway.

# Installation

## Per-machine setup (one time only):

**Linux** — add to `~/.bashrc`, then restart your terminal:
```bash
export HK_REFS="/path/to/Hollow Knight_Data/Managed/"
export HK_EXPORT="/path/to/any/folder/you/want/"
```

**Windows** — paste into PowerShell and run, then restart your terminal before building:
```powershell
[System.Environment]::SetEnvironmentVariable("HK_REFS", "C:\path\to\hollow_knight_Data\Managed\", "User")
[System.Environment]::SetEnvironmentVariable("HK_EXPORT", "C:\path\to\any\folder\you\want\", "User")
```

Then build with:
```bash
dotnet build MyHornetMod.csproj
```

## Controls

- **F5** - Toggle between Hornet and Knight sprites
- **G** - Cycle through Hornet's crests (weapon types

# Shout Outs
Thanks to people at Lumafly for helping provide template and some resources.

Thanks to the whole hollow knight community for being cool (and for not making this mod before I was inspired to and stealing my thunder ya'll are real for that).

Thanks to Reddit user Sumwann for posting all silksong assets for me properly extracted! I tried to do it on my own and failed miserably so thanks to you man (even if i didn't end up using these)!
https://www.reddit.com/r/Silksong/comments/1nmzg0p/all_silksong_sprites_sorted_and_renamed/

Thank you to Yuki.kaco on discord for making the main google drive where I got actual sprites in a much more structured and labeled way. Also thanks for help whenever I asked silly questions.

Most of all, thanks to Team Cherry for making two awesome games in a row, and inspiring me to make this mod in the first place.

## File Locations (Reference)

Location of Hollow Knight Game file Executable folder:
/home/fs/Documents/Hollow-Knight/Hollow Knight v1.5.78.11833

Location of Silksong Game File Executable Folder:
/home/fs/snap/steam/common/.local/share/Steam/steamapps/common/Hollow Knight Silksong
