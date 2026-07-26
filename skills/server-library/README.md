# XIIGen Skill Selection Server Library Boundary

This directory records the common server-side smart-skill library boundary for
Phase40 extension use. It is not a host package and does not contain Claude,
Codex, or Cursor extension implementation logic.

Common assets are repo-owned in `llm_mvp_core`:

- `models/skill-selection/skill-selection-v1/manifest.json`
- `training-data/skill-selection/skill-selection-v1/training-manifest.json`
- `training-data/skill-selection/skill-selection-v1/corpus-pointer.json`

Adaptive user or project state is separate from common assets. It belongs in a
project/user-local `.xiigen` state root, not under `models`, `training-data`,
`skills/server-library`, or any `[platform]-xiigen-smart-skills` host package.

This boundary forbids private corpus commits, external AI, image generation, and
deterministic/static evidence masquerading as learned skill-selection behavior.
