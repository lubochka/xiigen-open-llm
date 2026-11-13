# Codex Non-Heavy Mergeability Proof - 2026-07-07

Evidence note only. This records the accepted non-heavy mergeability proof for
`DesignToCode-clean`; it is not a plan edit, full dirty proof, product
completion claim, or authorization to commit, push, PR, merge, switch, reset, or
clean.

## Scope

- Branch checked: `DesignToCode-clean`.
- Layer checked: non-heavy dirty layer only.
- Bases checked: `origin/main` and `origin/codex/chat-gpt-5.5`.
- Source branch mutation: none.

## Committed Merge Conflicts

Both bases had the same 4 committed conflict families:

| Path |
| --- |
| `AGENTS.md` |
| `CLAUDE.md` |
| `docs/skills/claim-verification-SKILL.md` |
| `docs/skills/plan-review-SKILL.md` |

Stage-0 temp-index resolution used the current working versions of those four
paths. For both bases, unmerged entries were reduced from 12 to 0.

## Tracked Dirty-Rest Proof

The four stage-0 paths were excluded from the dirty patch because they were
already staged as manual resolutions.

| Base | Dirty-rest paths | Unmerged before apply | `git apply --cached --check` | Cached apply count |
| --- | ---: | ---: | ---: | ---: |
| `origin/main` | 322 | 0 | exit 0 | 663 |
| `origin/codex/chat-gpt-5.5` | 322 | 0 | exit 0 | 322 |

## Untracked Non-Heavy Proof

Untracked classification:

| Class | Count |
| --- | ---: |
| Total untracked | 20072 |
| Included non-heavy | 490 |
| `TestResults/**` | 19065 |
| `artifacts/**` | 517 |
| `bin/**` | 0 |
| `obj/**` | 0 |

Collision checks:

| Base | Included non-heavy paths | Exact collisions | Directory/file collisions |
| --- | ---: | ---: | ---: |
| `origin/main` | 490 | 0 | 0 |
| `origin/codex/chat-gpt-5.5` | 490 | 0 | 0 |

## Heavy Boundary

- `TestResults/**` is classified as generated test/browser/runtime output and
  excluded from the non-heavy proof.
- Untracked `artifacts/**` is mixed generated evidence/runtime/cache and is
  excluded by default from non-heavy mergeability unless a specific curated
  evidence fixture is intentionally selected.
- This note does not claim full dirty proof for the heavy layer.

## Remaining Risks / Next Work

- Full proof still needs an explicit heavy policy or curated artifacts decision.
- No commit, push, PR, merge, branch switch, checkout, reset, or clean was
  performed as part of this note.
