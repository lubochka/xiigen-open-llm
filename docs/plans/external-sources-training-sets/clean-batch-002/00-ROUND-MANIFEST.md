# Round 002 Manifest — external-learning-round-002-schema-and-translator

## Round goal

Produce Node 2 (Domain Translator) pairs, Node 4 (Domain Processor) pairs, and Node 7 (Arbiter Selector) pairs.
Node 2 was the bottleneck blocking formal input to Node 3. This round resolves it for four domain labels.

---

## Pair counts per node

| Node | File | Pairs (incl. Phrasing B) |
|---|---|---|
| 2 — Domain Translator | node-02-domain-translator-set.md | 8 |
| 4 — Domain Processor | node-04-domain-processor-set.md | 8 |
| 7 — Arbiter Selector | node-07-arbiter-selector-set.md | 4 |
| **Total** | | **20** |

---

## Sources used

| Source | License | Pairs |
|---|---|---|
| SWE-bench (princeton-nlp/SWE-bench) | MIT | CB2-02-002, CB2-04-001, CB2-07-001 |
| react.dev/learn | CC-BY 4.0 | CB2-02-001 |
| eShopOnContainers (dotnet-architecture) | MIT | CB2-02-003 |
| kubernetes/enhancements keps/sig-autoscaling | Apache 2.0 | CB2-02-004 |
| open-telemetry/opentelemetry-specification | Apache 2.0 | CB2-04-002 |
| dotnet/efcore | MIT | CB2-04-003 |
| docs.nestjs.com | MIT | CB2-04-004 |
| DapperLib/Dapper | Apache 2.0 | CB2-07-002 |

---

## Validation results

Gate B — Phase 1 (Node 2): grep -c "^Pair CB2-02-" node-02-domain-translator-set.md → 8 PASS
Gate B — Phase 2 (Node 4): grep -c "^Pair CB2-04-" node-04-domain-processor-set.md → 8 PASS
Gate B — Phase 3 (Node 7): grep -c "^Pair CB2-07-" node-07-arbiter-selector-set.md → 4 PASS
Gate B — Phase 4 (index): grep -c "clean-batch-002" 00-INDEX.md → 1 PASS

Pre-promotion checks (all five): PASS — see 10-SIMULATION-CHECK.md

---

## What this round enables

- Node 2 now translates D-database-query-efficiency, C-frontend-rendering-performance,
  A-api-topology-selection, and A-node-provisioning-strategy fragments to formal descriptions.
- Node 4 now produces enumerated success, failure, and wait-state outcomes for four sub-task types.
- Node 7 now selects arbiters correctly for the Django N+1 scenario and passes through
  results cleanly for the Dapper connection reuse scenario.
- CB2-04-001 and CB2-07-001 will activate immediately when Node 3 is trained on the
  Django N+1 scheme plan pair.

---

## Next-round prompt

Priority 1: Node 3 — Django N+1 scheme plan pair. This resolves the last cascade block on the
simulation scenario and allows CB2-04-001 (select_related sub-task) and CB2-07-001 (N+1 arbiter
selection) to activate in the full pipeline flow.

Secondary priorities: Node 5 (Applier) pairs for sub-tasks already covered in Node 4;
Node 6 (Result Translator) pairs; Node 2 pairs for E-orm-usage-pattern and B-backend-response-latency.
