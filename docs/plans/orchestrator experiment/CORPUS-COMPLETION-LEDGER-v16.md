# Corpus Completion Ledger v16
# Date: 2026-05-05

---

## Current State

The orchestrator machinery is functional for the registered proof paths, and the
CB4 security-verification executable proof has been broadened through the full
ext24 cross-source file.

Current verified code state:

```text
Full suite: 1175/1175 passing
WDA lifecycle checks: 7/7 passing
LogicalOrchestratorAssistant checks: 18/18 passing
Security verification flow: node-01 through node-09 all represented as logical units
```

---

## Newly Closed In Phase 96

The `security-verification-3.4` flow now supports every base pair from
`node-cross-source-set-ext24.md`:

```text
CB4-3.4-819 -> S7  -> REPROCESS
CB4-3.4-820 -> S8  -> PASS
CB4-3.4-821 -> S9  -> CONCERN
CB4-3.4-822 -> S10 -> PASS
CB4-3.4-823 -> S11 -> REPROCESS
CB4-3.4-824 -> S12 -> PASS
CB4-3.4-825 -> S13 -> REPROCESS
CB4-3.4-826 -> S14 -> REPROCESS
CB4-3.4-827 -> S15 -> REPROCESS
CB4-3.4-828 -> S16 -> PASS
```

This is the first external-source cross-source file whose full base-pair range
is executable through the registered logical-unit flow.

---

## What Is Proven

The registered security flow can preserve:

```text
Combined MUST plus MUST failures
Combined PASS plus PASS controls
CONCERN plus CONCERN without false escalation
CONCERN absorbed by a stronger REPROCESS condition
Training-gap causal evidence as part of a security verdict
Compound token risk across signing and expiry
Comprehensive all-domain pass evidence
```

The most important design lesson remains detector specificity. Higher-complexity
cross-source cases must be detected before their simpler component cases.

---

## What Is Not Complete

The full corpus v4 target is still not complete.

The external Wave 1 source folder provides 828 base pairs / 1656 instances, but
only selected proof slices are executable. The development lifecycle corpus still
needs real training and held-out rows across all 31 sub-phases.

Remaining corpus work includes:

```text
Register more Action 3 external-source families as executable flows
Produce real quota batches for the 31 development sub-phases
Create held-out rows and conformance gates per sub-phase
Create transition-evaluation rows for phase and sub-phase carry-forward
Run inspector-visible executions over representative user prompts
```

---

## Next Gate

Recommended next gate:

```text
Use node-pipeline-chain-set-ext25.md to compare full 9-node traces against the
current executable flow for the ext31 cases.
```

Reason:

The flow now classifies ext24 cross-source verdicts correctly. The next question
is whether its per-node outputs match the accepted full pipeline-chain traces,
not only whether the final verdict and pair anchors match.
