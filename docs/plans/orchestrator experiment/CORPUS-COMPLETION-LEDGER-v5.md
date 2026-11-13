# Corpus Completion Ledger v5
# Date: 2026-05-05

---

## Answer

The full corpus v4 is still not complete.

New information since v4:

- CB4 Stage 5A replaced the first internal station inside the security-verification flow.
- Node-08 arbiter behavior now executes through `SecurityArbiterUnit`, an `ILogicalUnit`.
- The unit is anchored to real CB4 node-08 pair ids:
  - `CB4-3.4-001` for PASS;
  - `CB4-3.4-005` for REPROCESS;
  - `CB4-3.4-009` for CONCERN.
- The S1/S2/S3 oracle verdicts stayed stable.

---

## Current Proven Surfaces

| Surface | Status |
|---|---|
| WDA-00 through WDA-06 deterministic suite | passing |
| WDA-01 real router seed proof | passing |
| 31 seed source rows | ready |
| CB4 pair corpus for 3.4 | accepted |
| CB4 Stage 5 deterministic runtime flow | passing |
| CB4 node-08 unit replacement | passing |
| CB4 node-01 through node-07 replacement | not started |
| CB4 node-09 replacement | not started |
| full v4 training document quota | not complete |
| full held-out quota | not complete |
| transition held-out quota | not complete |

---

## Phase 85 Runtime Result

| Item | Value |
|---|---|
| replaced station | node-08 arbiter |
| unit id | `unit-cb4-security-arbiter-v1` |
| role | `node-08-arbiter` |
| domain tag | `security-verification-3.4` |
| confidence | 0.94 |
| S1 verdict | PASS |
| S1 pair anchor | `CB4-3.4-001` |
| S2 verdict | REPROCESS |
| S2 pair anchor | `CB4-3.4-005` |
| S3 verdict | CONCERN |
| S3 pair anchor | `CB4-3.4-009` |
| full suite | 1172/1172 passing |
| solution build | passing |

---

## What This Proves

The CB4 security flow no longer keeps all verdict authority inside one
flow-level fixture. Node-08 is now a unit-shaped station:

```text
SecurityVerification34FlowAdapter
  -> SecurityArbiterUnit : ILogicalUnit<SecurityArbiterInput, SecurityArbiterVerdict>
```

The S2 REPROCESS and S3 CONCERN distinction survived the replacement, which is
the highest-risk behavior for this batch.

---

## What This Does Not Prove

The node-08 unit is still deterministic and small. It is pair-backed by named
CB4 examples, but it is not yet a broad learned arbiter over all 446 base pairs.

The next phases must replace additional stations and then widen the unit over
more CB4 pair anchors.

---

## Next Gate

Phase 86:

```text
CB4 Stage 5B - replace node-07 arbiter selector
```

Recommended reason:

- Node-07 determines whether the correct arbiter reaches node-08.
- S2 must select `ASVS-V10-OAuth-Arbiter`.
- S3 must select `ASVS-V7-Session-Arbiter`.
- A correct node-08 cannot protect the flow if node-07 selects the wrong arbiter
  or no arbiter.

Required:

1. keep node-08 as the unit from Phase 85;
2. replace only node-07 selection behavior;
3. keep S1 PASS, S2 REPROCESS, and S3 CONCERN stable;
4. expose selected arbiter ids in metrics;
5. run assistant, WDA, full suite, and solution build;
6. produce manifest and zip.
