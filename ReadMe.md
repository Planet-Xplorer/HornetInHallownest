# Hornet in Hallownest!

This is my repo for a mod that will ideally allow for the user to play as Hornet from Silksong in Hollow Knight.

The main goal for this is so that the player can use their tools, silk skills, etc. when being as hornet. Hornet should be spawning in with all her movement abilities unlocked, following the precedent by the "Knight in Silksong" mod, which did the same thing, since I presume it'll be irritating to also add progression, and generally most people using this mod just want to fight all the bosses in hollow knight as hornet anyway.

# Configuration at this moment:
On any new machine (Windows or Linux), workflow is:

Clone the repo
Create a Local.props file in the project folder (never committed):

<Project>
  <PropertyGroup>
    <HollowKnightRefs>C:\Program Files (x86)\Steam\steamapps\common\Hollow Knight\hollow_knight_Data\Managed\</HollowKnightRefs>
    <ExportDir>C:\HKMods\Exports\</ExportDir>
  </PropertyGroup>
</Project>
Run dotnet build MyHornetMod.csproj
The built DLL auto-copies to the HK Mods folder (the post-build step in the csproj handles it)
Where to place the mod on Windows: [HK_install]\hollow_knight_Data\Managed\Mods\HornetInHallownest\HornetInHallownest.dll — the build step does this automatically if HollowKnightRefs is set correctly in Local.props.

# Shout Outs
Thanks to the people at Lumafly for helping provide the template and some resources.

Thanks to the whole hollow knight community for being cool (and for not making this mod before I was inspired to and stealing my thunder ya'll are real for that).

Most of all, thanks to team cherry for making two awesome games in a row, and inspiring me to make this mod in the first place.
