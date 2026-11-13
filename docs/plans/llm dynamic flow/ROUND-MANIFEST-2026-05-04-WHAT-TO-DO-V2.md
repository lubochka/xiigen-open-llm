# Round Manifest - What To Do / What Not To Do v2

## Round Name

What-to-do / what-not-to-do guide correction and canonicalization.

## Source Request

Review `WHAT-TO-DO-WHAT-NOT-TO-DO-v1.md`, assess it, and produce the corrected
v2 version with the requested fixes.

## Files Included

- `WHAT-TO-DO-WHAT-NOT-TO-DO-v2.md`
- `SESSION-START-PROMPT-ROUND-ARTIFACT-ZIPPING-v1.md`
- `ROUND-MANIFEST.md`

## What Changed

- Created v2 beside v1.
- Marked D-15 as locked rather than pending.
- Clarified the distinction between `PermissionGate.Check("call_external_ai")`
  and `ExternalAiStateGuards.ShouldRequestExternalAiApproval(...)`.
- Scoped the warning about the dynamic-flow architecture plan so it applies to
  gathered-document training-set work, not every architecture maintenance task.
- Reworded the plain-language rule so formal runtime outputs may still use
  predicates, arbiter names, feature fields, and canonical identifiers.
- Added the round-zip packaging rule to general session behavior.

## Verification

Text searches confirmed:

- `D-15` is present without "not yet locked".
- The old approval contradiction phrases are absent.
- The v2 corrections section is present.
- The round-zip rule is present.

## Remaining Gap

None for this small correction round.
