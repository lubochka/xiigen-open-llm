# 00 — Context Load Report
## External Sources Examination — Phase 0
## Date: 2026-05-04

---

## Files Read and Confirmed Present

| File | Location | Status |
|------|----------|--------|
| EXTERNAL-SOURCE-REGISTRY-v1.0.md | plan the learning set/ | Present — governing source policy |
| EXTERNAL-SOURCES-EXAMINATION-PLAN-v2.md | plan the learning set/ | Present — execution procedure |
| EXAMINATION-CONTINUATION-PLAN-v1.0.md | plan the learning set/ | Present — 116-round analysis plan |
| DYNAMIC-FLOW-ARCHITECTURE-PLAN-v1_0-DRAFT.md | llm dynamic flow/ | Present — pipeline architecture |
| learning-sets/flow-01-training-sets-v1/00-INDEX.md | learning-sets/ | Present — existing internal corpus |

---

## Eight Dataset Families (confirmed binding)

| Family | Function name | Edge in pipeline |
|--------|--------------|-----------------|
| 1 | `userPrompt_to_machineCommands` | Raw user input → structured command list |
| 2 | `machineCommand_to_machineActionItems` | Command → decomposed action items |
| 3 | `machineActionItem_to_domainResult` | Action item → domain-specific result |
| 4 | `domainResultsArray_to_resultI` | Result array → single merged result |
| 5 | `arbitrateResultI_with_resultsContext` | Result + context → arbiter verdict |
| 6 | `resultI_to_finalMachineResults` | Arbiter output → final machine result |
| 7 | `finalMachineResults_to_userFormat` | Final result → user-facing format |
| 8 | `historyAndVersions_to_reprocessingDecision` | History + versions → reprocess/accept/rollback |

---

## Existing Row Count per Family (internal corpus baseline)

From `flow-01-training-sets-v1/` and the dynamic flow plan:
- Family 1: ~25 rows (domain splitter training data)
- Family 2: ~20 rows (decomposer and scheme planner)
- Family 3: ~30 rows (applier sets per domain)
- Family 4: ~15 rows (result translator, aggregator)
- Family 5: **CRITICAL GAP** — ~8 rows (arbiter units, arbiter selector)
- Family 6: ~10 rows (result translator to final)
- Family 7: ~12 rows (formatter, style adapter)
- Family 8: **CRITICAL GAP** — ~6 rows (reprocessing decisions, rollback)

**Families 5 and 8 are the highest priority for external source extraction.**

---

## Source Categories to Add (from EXTERNAL-SOURCE-REGISTRY-v1.0.md)

Priority 1 — Security and governance rows (families 5, 8):
- OWASP-ASVS (CC-BY-SA 4.0) — 24 control groups
- OWASP-CHEAT-SHEETS (CC-BY-SA 4.0) — 70+ cheat sheets
- NIST-SSDF (public domain) — 4 practice groups
- CWE-TOP25 (free commercial use with notice) — 25 weakness entries
- IETF-RFCS (IETF Trust provisions) — OAuth2, JWT, TLS 1.3

Priority 2 — Architecture and orchestration rows (families 1, 2, 5, 8):
- KUBERNETES-KEPS (Apache 2.0) — KEP (Kubernetes Enhancement Proposal) archive
- OPENTELEMETRY-SPEC (Apache 2.0) — signal specifications
- OPENAPI-SPEC (Apache 2.0) — API design decisions
- GOOGLE-AIP (CC-BY 4.0) — API improvement proposals
- MICROSOFT-LEARN-ARCH (CC-BY 4.0) — Azure cloud design patterns

Priority 3 — Bug-fix and code-agent rows (families 2, 3, 4, 6):
- SWE-BENCH (MIT) — 2,294 real GitHub issues + patches

Priority 4 — Proposal archives (families 1, 5, 8):
- RUST-RFCS, PYTHON-PEPS, GO-PROPOSALS, KAFKA-KIPS, CLOUDEVENTS-SPEC

Priority 5 — OSS architecture narratives (families 1, 3, 5):
- AOSA-VOL1, AOSA-VOL2, DOTNET-MICROSERVICES, SYSTEM-DESIGN-PRIMER

Priority 6 — Stack-specific architecture (families 1, 3, 5):
- NESTJS-ADVANCED, REACT-ADVANCED, ANGULAR-STYLE, DOCKER-BEST-PRACTICES

---

## Gap Statement (mandatory per v2 plan)

**The current rowized dataset is not final.**

Families 5 (arbitration) and 8 (reprocessing decisions) are critically
under-covered. No row in either family has been produced from external
sources. The internal corpus provides ~14 combined rows across both families —
far below what is needed for reliable arbiter behavior.

Exact source spans and accepted/rejected example pairs are missing from
most internal rows. External source rows must provide both.

---

## Acceptance Gate Result

This report is written. The gap statement is present.
Phase 1 (source registry) and Phase 2 (examination files) may now proceed.
