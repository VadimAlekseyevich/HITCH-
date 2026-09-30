# scenes/

Godot scenes will live here after project bootstrap.

Scenes compose engine-facing objects and presentation.

They must not become the only source of truth for gameplay simulation.

Expected eventual categories may include:

- bootstrap/main development scene;
- movement laboratory;
- MVP arena;
- player presentation;
- debug/test scenes.

Do not build a deep scene taxonomy before real scenes exist.

Gameplay rules belong in simulation code; scene nodes adapt/render them.
