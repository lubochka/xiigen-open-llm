# RP03 V7 repair audit / actual source findings · 15 October 2026

**Scope:** Plan/prompt publication only. P inspected at `78ccad8763adc9fd6bf6499f27260c2e9c4c8bab`; K read at `97e4af3eadd3981320f698d104134a01984a2542`. HEAD may move after this document; executors MUST re-pin. No K edit/model run/independent Astra performed by this audit.

| Finding | Verified source | Effect | V7 repair |
|---|---|---|---|
| A01 P0 contradictory route | RP03 `PACKAGE-MANIFEST.json` had `task_files=tasks/RP03.*` while `active_tasks=tasks/v6/*`, mixed V6.2/V6.3 | Weak model can execute history | Rewrite manifest to single V7 active task list and explicit historical references |
| A02 P0 inherited stale instruction | RP03 `tasks/AGENTS.md` began V6.2; README was V6.3 | Task/agent route conflict | task AGENTS + pack AGENTS + README + START + board all V7 |
| A03 P1 prompt multiplication | `tasks/v6/RP03.02B.md` carries two overlapping V6.3 blocks | Context waste and two sets of directions | New single complete V7 task-local prompts (15), v6 reference-only |
| A04 P0 observational boundary | K `21.AgentInfrastructure/Logical/DynamicGraphRegistryL3SelectedTargetExecutor.cs:ExecuteAsync` calls **flow.ExecuteAsync**; `DynamicGraphEdgeFlow.ExecuteLearnedTargetHandoffAsync` issues handoff and graph `TargetsExecuted=false` | Flow invocation is not proof next typed L2 target ran | 00B/01C/02C demand separate real next Unit.ExecuteAsync + Host method trace |
| A05 P0 learner candidate | K `DynamicGraphEdgeAuthorityExecutionPipeline.ExecuteAsync` executes typed L2 unit; `RegistryDynamicGraphEdgeAuthorityResolver` resolves unit; those methods alone don't show numeric W writer/cold reader | false 'trained' from authority selection | 02A trace or implement minimal adapter, 02B numeric update and independent lawful signal, 02D cold |
| A06 P1 causal data weakness | RP03 V6.3 00C combines early feedback and sealed three-domain exams | models can wait on CERT or invent signal | 00C-A real observed OP_DEV independent signal; 00C-B blinded science parallel; neither conflated |
| A07 P1 domain work overloaded | RP03.03B single three-domain task, with optional H0–H6 | cross-domain gain overclaimed from one C# pair | independently runnable C#/EN→RU/SQL children, explicit OOD vs each-domain actual lesson |
| A08 P1 F1 φ hazard (algorithm analysis) | RP03.02B proposed domain-law-compatibility in φ after F0; legal candidates often share same mask | feature may not distinguish A/B; g=0 | φ from true pre-outcome differentiating source/role/cost/semantic evidence, detect g=0→NO_UPDATE, no fabricated gradient |

**Audit boundaries:** A01–A07 are grounded in the stated current planning/core code; A08 is an *inference and prospective design safeguard*, not observed failure. Code review on actual implementation must confirm or falsify these seams, including other callers not yet examined. Read atlas `01-ORDERED-REPAIRS.md` for 16 prior findings; V7 adds these specific verified prompt/consumer repairs without reopening all earlier runs.

## Cross-slice handoffs retained
RP02: domain mapping, semantic/token/vector law, adaptive k nearest lawful relation (not only hit0). RP18: LoRA output semantics/vector transposition separately. RP01: external open Chinese Teacher both chosen+rejected under approved rights; distinct CODEX Astra delegated case choice and pair judgment; Student DPO never equals Flow. RP04: async Orchestrator and higher-order DynamicGraph. RP05: feedback/continual. RP09: independent exam. All seven WorkLeft tracks are maintained in 00A source crosswalk; only three initial experiments demanded in 03B.

## In-scope actual validation
Check V7 manifest task 15/15 + nine children, every active path exists, links back to true dispatcher, no legacy active task files, no contradictory AGENTS, old V6 dispatch forwards, source and model roles remain honest. This validates **planning publication**, not code/learning. Real separate Astra UltraCode exact-SHA review is still **NOT_PERFORMED**.

## Additional RP03 V7 source findings — 15 October 2026

- **A09 · nine-call AUTHORITY CONTROL FLOW, CODE_VERIFIED:** K `DynamicGraphEdgeAuthorityExecutionPipeline` L2 ExecuteAsync main once + `DynamicGraphEdgeCandidateOrderLeakageGuardUnit` ProbeCount=5 + `DynamicGraphEdgeIdentityLeakageGuardUnit` ProbeCount=3 = 9 possible L2 invocations on full successful authority path; actual runtime count/latency NOT_MEASURED. Source blobs: `70fcdcfbc824d9c0c71f66773136bb6c79fc580f`, `bc1a11655760777eddb638b6207f519b589305a8`, `fbfff288a0783bfb4035c728b3baf6448c3e3b2d`. 02A/02B/03C task-local addenda carry bounded work; preserve identity/order leakage guard integrity.
- **A10 · one-request queryable projection, CODE_VERIFIED:** `LogicalUnitDynamicGraphNode.ExecuteAsync` snapshots one scalar unit result to `GraphNodeCandidate[]` then `AsQueryable()`, blob `781ba5afe61bd3dfa34049133fd4f334a710daa0`. Many-input associations and actual typed next Unit still require 00B/01A/B/C runtime proof.
- **A11 · stale V6 authority, PLAN_VERIFIED:** `SOURCE-AND-ARCHITECTURE.md` and `UNIT-FLOW-ALGORITHM-AND-INTERFACES.md` had active V6.2/V6.1 banners even after V7. Headers now explicitly historical and forwarded to a single V7 canonical dispatcher.
- **A12 · immutable W across probes, HYPOTHESIS ONLY:** concurrent modification/stochastic tie could contaminate comparison; direct code read does not demonstrate that it occurs. Require version snapshot and falsification, not a fabricated bug claim.

See [targeted V7 learning/guard source repair](RP03-V7-TARGETED-LEARNING-AND-GUARD-COST-REPAIR-2026-10-15.md). Product runtime, weights, domain grades, independent Astra not executed by this documentary amendment.

## RP03 N26–N31 current code-source follow-up
[Evidence table, source lines and design risks distinguished](RP03-V7-SOURCE-REPAIRS-AND-7-DOMAIN-EXAMS-2026-10-15.md). New source facts: live resolver IsLearned/Probationary gate; exact latest-ID registry choice before trust/type filtering; node output projector passes CandidateId not typed OutputRefs. New bounded hypotheses: potential latest Candidate shadowing, missing coordinate law binding in the specific EdgeAuthorityInput record, domain-specific Machine training-owner reconciliation. Planning gap repaired by 4 extra independent domain exam children (seven total domain exams), retaining original 15 parent work orders. Runtime training, actual Host gains, computer resource claims, independent Astra remain NOT_RUN / NOT_MEASURED / NOT_PERFORMED.
