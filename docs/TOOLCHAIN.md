# HITCH! Toolchain

This file pins the development toolchain used by the MVP bootstrap.

## Pinned versions

- **Godot:** 4.7.2 stable, .NET/C# build, Windows x86_64
- **.NET SDK:** 8.0.425
- **Target framework:** `net8.0`
- **3D physics backend:** Jolt Physics
- **Initial development/CI platform:** Windows x86_64 for local development, `windows-latest` for GitHub Actions

## Why these versions

Godot 4.7.2 is the current stable maintenance release selected for the MVP bootstrap.

Godot 4.7 supports C# on .NET 8 and higher. The project intentionally targets `net8.0` and pins SDK 8.0.425 so local development, the bootstrap script, and CI agree on one compiler/runtime toolchain.

Jolt is built into Godot and is the default 3D physics engine for newly created projects in current Godot 4.x releases. HITCH! still sets the backend explicitly in `project.godot` so a future engine default change does not silently change gameplay behavior.

## Windows bootstrap

The repository root contains `build_and_run.bat`.

It is designed to make a clean Windows clone runnable without a global Godot or .NET installation:

1. create a repository-local `.tools/` directory;
2. install .NET SDK 8.0.425 into `.tools/dotnet/` using Microsoft's official `dotnet-install.ps1`;
3. download the official Godot 4.7.2 .NET Windows x86_64 archive into `.tools/`;
4. extract it locally;
5. restore/build the project;
6. run tests;
7. launch the Godot project.

`.tools/` is not committed.

## Official references

- Godot 4.7.2 archive: https://godotengine.org/download/archive/4.7.2-stable/
- Godot Windows downloads: https://godotengine.org/download/windows/
- Godot 4.7 features / C# support: https://docs.godotengine.org/en/4.7/about/list_of_features.html
- Godot Jolt documentation: https://docs.godotengine.org/en/4.7/tutorials/physics/using_jolt_physics.html
- .NET 8 downloads: https://dotnet.microsoft.com/download/dotnet/8.0

## Upgrade policy

Do not silently upgrade Godot or .NET in an unrelated task.

A toolchain upgrade must update, together:

- this file;
- `global.json`;
- `HITCH.csproj` if its Godot SDK package changes;
- `build_and_run.bat`;
- CI;
- `docs/DECISIONS.md`;
- any affected task/status documentation.

Run the clean-checkout verification again after an upgrade.
