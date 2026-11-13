# User-Local Adaptive Intake Schema v1

This note documents Q6-01 only. It defines how client startup material is represented as user-local adaptive references without committing raw private text and without mutating common checkpoints.

## Intake Boundary

Inputs are `AGENTS.md`, `CLAUDE.md`, and skill files from a client project. The committed record stores only source kind, redacted path label, path hash, source file hash, project root hash, capture timestamp, and privacy flags.

Outputs are user-local adaptive references and DPO candidate references. They are not common model rows and they are not common training rows. Q6-01 does not train, continue-train, export, import, backup, share, run host/plugin/runtime paths, or mutate models.

## Redaction Rule

Committed Q6-01 artifacts may contain synthetic examples, schema field names, hashes, redacted refs, and redacted path labels. They must not contain raw source text from client `AGENTS.md`, `CLAUDE.md`, skill files, prompts, corrections, or AI answers.

## Later Hooks

Backup, export, import, and share hooks are named for Q6-05 review only. They are reference hooks in this slice and are not implemented here.

## Gate

Q6-01 passes when the schema and redaction policy parse as JSON, produced files contain no raw client text, and common checkpoint hashes are unchanged before and after the slice.
