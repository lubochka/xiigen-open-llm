# CB4 Stage 5 Security Flow Registration Plan v1
# Date: 2026-05-05

---

## Purpose

Define the next executable gate for the accepted CB4 corpus material.

The CB4 pair files exist and are license-cleared. The remaining gap is runtime
use: the orchestrator must be able to run a `security-verification-3.4` flow
against the oracle scenarios.

---

## Flow To Register

```text
flow_id: security-verification-3.4
scenario_id: cb4-security-verification
primary_phase_id: phase-3-technical-design
primary_sub_phase_id: 3.4-security-and-authorization-model
```

---

## Minimal Stage 5 Shape

Do not attempt to ingest all 892 instances in the first code round.

Stage 5 begins with a deterministic flow adapter that proves the orchestrator
surface and result contract:

1. register `security-verification-3.4`;
2. run three oracle scenarios from OWASP-ASVS;
3. produce one PASS, one REPROCESS, and one CONCERN result;
4. preserve source attribution;
5. preserve arbiter authority;
6. keep WDA and full suite green.

After that deterministic registration passes, replace one station at a time
with pair-backed units.

---

## Oracle Scenarios

| Scenario | Expected verdict | Required behavior |
|---|---|---|
| S1 | PASS | valid security evidence is accepted without extra concern |
| S2 | REPROCESS | violating security evidence is blocked for correction |
| S3 | CONCERN | boundary or partial evidence remains visible without false block |

The scenario source is:

```text
wave-1/OWASP-ASVS/stage-1/OWASP-ASVS-EXECUTOR-CONTRACT-v1.md
```

Mandatory human-result checks come from:

```text
wave-1/OWASP-ASVS/stage-1/OWASP-ASVS-HUMAN-RESULT-STANDARDS-v1.md
```

---

## Runtime Units

The final learned flow will need all nine node roles:

```text
node-01 domain splitter
node-02 domain translator
node-03 scheme planner
node-04 domain processor
node-05 applier
node-06 result translator
node-07 arbiter selector
node-08 arbiter units
node-09 formatter
```

The first Stage 5 code round may use one deterministic adapter for the whole
flow. It must expose the same final result shape that later units will replace.

---

## Acceptance Gates

| Gate | Required value |
|---|---|
| flow registered | true |
| selected flow id | `security-verification-3.4` |
| scenario count | 3 |
| PASS scenario accepted | true |
| REPROCESS scenario accepted as reprocess | true |
| CONCERN scenario preserved as concern | true |
| attribution visible for OWASP ASVS | true |
| external AI calls | 0 |
| WDA tests | pass |
| full suite | pass |

---

## Failure Classification

| Failure | Meaning |
|---|---|
| FLOW_NOT_REGISTERED | orchestrator cannot select the 3.4 security flow |
| VERDICT_DRIFT | PASS/REPROCESS/CONCERN scenario returns wrong verdict |
| ARBITER_AUTHORITY_LEAK | REPROCESS evidence reaches accepted result |
| CONCERN_COLLAPSE | CONCERN is incorrectly converted to PASS or REPROCESS |
| ATTRIBUTION_MISSING | license-sensitive source output lacks attribution |

---

## Next Action

Start Phase 84 implementation:

```text
Register deterministic security-verification-3.4 logical flow and prove the
three oracle verdicts through the orchestrator.
```
