# Codex Plan 14 - Stage References

Artifact: `codex-plan-14-stage-references.md`
Status: PLAN-ONLY, Codex-owned.
Encoding recovery note: recovered after cycle-42 status-sync encoding fault.

## Authority Anchor

```text
CURRENT TASK: Preserve source/reference planning and finish 50 real review cycles.
NOT THE TASK: No implementation, tests, builds, git, training, Claude-file edits, or false cycle claims.
EVIDENCE-ONLY: Claude benchmark and old packets.
```

## Canon

The planning library must maintain source inventory, source-to-plan mapping,
source-to-review mapping, ownership/evidence-only manifest, and population proof.
Reference docs are not implementation authority unless the current plan maps them
through a requirement and gate.

Required inventories: Reference Inventory, Source-to-Plan Map, Source-to-Review Map, Knowledge Library Manifest, Population Proof, Pass Ledger.

## Canonical Ownership

`codex-plan-14-stage-references.md` is the canonical owner for:

- Reference Inventory.
- Source-to-Plan Map.
- Source-to-Review Map.
- Knowledge Library Manifest.
- Ownership/Evidence-Only Manifest.
- Population Proof.
- Pass Ledger.

The master plan, requirements matrix, and execution board only project these
maps. If they conflict with plan-14, plan-14 wins for source/reference ownership
and the projecting artifact must be repaired.

### Ownership / Evidence-Only Manifest Schema

Each row must carry:

| Field | Meaning |
|---|---|
| `artifact_path` | Exact file path or stable artifact locator. |
| `artifact_kind` | plan, guide, board, matrix, benchmark, recon, source, packet, or other named type. |
| `actual_author` | Codex, Claude, Luba, sub-agent id, external source, or unknown. |
| `owner_lane` | canonical_owner, projection, benchmark_evidence, recovery_note, or blocked_decision. |
| `editable_status` | Codex-editable, evidence-only-do-not-edit, or external-read-only. |
| `evidence_only_reason` | Required when `editable_status != Codex-editable`. |
| `destination_section` | Target section that consumes the artifact. |
| `gate_b` | Measurable gate or decision id that makes the artifact useful. |
| `completion_evidence` | File/line, packet id, or proof field required before marking complete. |
| `conflict_policy` | Which artifact wins if projections conflict. |
| `branch_authority_ref` | `R21` for repo/source rows that need latest-updated-branch pre-flight evidence. |

Plan-14 does not duplicate the full R21 branch-authority table. It stores only
`branch_authority_ref=R21`; selected branch, timestamp/ref evidence, divergence
note, and future branch name `DPOLegalLearning` remain in the board/matrix
pre-flight gate.

R15 reserve domains are: small-model, language-translate, local-wiki, extensions, documentation-learning, session-start.

They map to reserve rows F10.3.1 through F10.3.6 and stay outside baseline-59 until Q41 Variant B is accepted.

## Gate B

- Every accepted reference has a destination section, gate, and completion evidence.
- Rejected/deferred benchmark items name the conflict or missing decision.
- R16 host/source mapping stays blocked by final Luba decision.
- FLOW-38 feedback event mapping stays blocked by legal origin decision refs Q37/Q38.
- Current comparison-run packet records: 50/50; C49-C50 returned and integrated.

## Final Questions Only

1. Should Q41 Variant B activate all six R15 reserve domains now?
2. Should R16 default hosted/client material to `PersonalAdaptive` unless common proof is present?
