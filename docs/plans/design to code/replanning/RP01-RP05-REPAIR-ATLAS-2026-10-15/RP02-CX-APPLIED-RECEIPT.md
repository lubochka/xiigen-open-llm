# RP02 CX01–CX06 · Applied prompt repair and publication receipt

**15 October 2026 · AUTHOR-REVIEWED DOCUMENTARY REPAIR. INDEPENDENT_ASTRA_NOT_PERFORMED.**

This completes the in-place integration that was still pending in the historical [CX01–CX06 intermediate checkpoint](RP02-CX01-CX06-TARGETED-REPAIR.md), first published as `bfa515b4750fdfbaa161f3cb696e7fec5e7c2fe2`. The effective product-task entry remains [the existing RP02 pack](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/README.md), not this atlas. Findings are not product implementations or measured gains.

## Exactly what this continuation changed

Six existing task packets were replaced with integrated, task-specific detail: four main cards (01B, 01D, 02D, 03A) and two children (02B1/UPDATE, 02B2/LEARN). The effective context and pack README were updated to deliver those details through the existing route. No new task IDs, rounds, dispatcher, experiment IDs, workflows, product tests, optimizer execution or model calls were introduced. The atlas has the intermediate findings file and this applied receipt.

Other RP02 cards, previous R01–R25 results and RP01/RP03/RP04/RP05 work remain owned by their existing assignments. They were not all freshly re-audited or rewritten by this continuation. Concurrent branch changes are not attributed to this work.

## Exact reviewed publication candidate and remote readback

**Candidate P: `6eb4b659b590cb9d7b7883740df803263cb70239`.** Every row below was read back from this immutable commit after its update; returned blob hashes matched the write receipts. The body excerpts containing the new rules were also inspected. Blob equality validates the whole saved file identity; the readback is author verification, not independent code/science acceptance.

All paths below are relative to `Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/`.

| Path | Own update commit | Exact read-back blob |
|---|---|---|
| tasks-v7/children/RP02.02B2-LEARN.md | 25bd3745f5e501cc4883d9b9ade43d31e05f0bc2 | cd28374e0d03a045fc240306d15782d91c882b7e |
| tasks-v7/RP02.01B.md | f510084e18b5c77b0306d53c0d0399974c42c232 | 25db0e3e18ac5afdab8fce3e78185cff364ed7bf |
| tasks-v7/RP02.01D.md | 17200d9b3e61c3a06cb427fd75d120552f89ff20 | b24a31c176037c6b6df487e5ea3eb5db70d27a24 |
| tasks-v7/RP02.02D.md | 8dd09ed4cf748ecfca1e17d463d48f58f60e8e2e | d3f6733833f3730e8030f8f97a968bdc9392b0c3 |
| tasks-v7/RP02.03A.md | 63ee8af07a6cd4acdf24fdcf3ab6b170b4601694 | 6dc3fd1e9116d728da119887d060a6d292d3b691 |
| tasks-v7/children/RP02.02B1-UPDATE.md | 54333fd16ee392b4ecad3d501661bec6aa31c627 | 0e8e0b2f56fcc1e524363988b7bc097b39e47546 |
| RP02-EFFECTIVE-CONTEXT.md | 5944917a6a2dcdce9f263b61273d7a36bc723642 | 7191dbe3b7911fa32e5d963ead3d1be538cb9c18 |
| README.md | 6eb4b659b590cb9d7b7883740df803263cb70239 | fa922aa46298617fa342d88e0eb6416682fc83e8 |

Both `PACKAGE-MANIFEST.json` and `RP02-V7-PACKAGE-MANIFEST.json` were read at P and returned the same unchanged blob `863797be89d62900709b8c6f7e6110fe593eb59d`. They still declare revision 7.3, 18 main tasks, three children, 21 packet files, the existing task directory and first-learning dispatch. Twenty-one packet files are not twenty-one required serial runs.

The repository compare from the intermediate checkpoint to P includes 50 commits because other workers were publishing concurrently. Only the own commit/path rows above are credited here. No force push or sibling file replacement was used. Publication used the connected GitHub contents API with expected current file blobs; a transient read failure was retried successfully and was not interpreted as read-only access.

## What was checked substantively

| Finding | Applied detail | Evidence class / limitation |
|---|---|---|
| CX01 | Explicit action-specific context interaction, feasibility mask, decision-time features and actual outcome provenance in LEARN | Existing prompt admitted a mathematically noncontextual implementation; not an observed implemented policy bug. |
| CX02 | Identify actual TrainingVectorBuilder overload, shared coordinate dictionary and compatible query/index/cold state in 01B | Source counterexample, ordinary caller exposure unverified; no .NET runtime reproduction. |
| CX03 | Separate before/after relation-loss case; preserve raw relation before normalization and reach actual scorer | Source counterexample in one helper, not a claim about all BPE/tokenizer paths. |
| CX04 | Byte/BPE/scalar/word/AST granularity; viable prefix, pending state and actual finalization boundary in 02D | Concrete source-informed discriminator; no incremental decoder experiment performed. |
| CX05 | Candidate-dependent online-computable features, post-filter support and actual data-gradient in 01D/UPDATE | Refines the existing d=0/informativeness rule; no new pair was authored or applied. |
| CX06 | Comparable cold/warm/cache/order/deadline and retained incomplete outcomes in 03A | Proposed scientific refinement, not a finding that previous measurements were biased. |

Previously recorded top-1 neighbor use, duplicate normalized IDs, holdout leakage risk, paid-probe cost, cosine/calibration and DPO role/state distinctions were reused, not counted as new bugs. NO_GAIN remains a legitimate outcome of a properly executed lesson; it does not authorize changing the hidden rubric or replaying an applied lesson.

## Actual source and skill scope

Core source used: complete `21.AgentInfrastructure/Logical/KnnLogicalUnit.cs` and `23.SemanticCompilerAdapter/TrainingVectorBuilder.cs`; BpeTokenizer opening/Train/Encode/Decode and the initial persistence/validation section. The two latter source identities were rechecked at K `025c42c74b1d30ffa4660f9824a52cf8fcaf7c88`: TrainingVectorBuilder blob `d1fc22d7823f377da497881c0e540218dd22c6a2`; BpeTokenizer blob `d7c5e69308a32bf832ec4b087097a35b26f51d64`. BpeTokenizer was not read in its entirety. HNSW details already present in the atlas are reused bounded evidence, not a fresh complete HNSW audit here.

Primary-document passages actually followed: original task/four roles; previous-round opening with R07 repair limitation; PARALLEL-R01-R03-JOIN current concurrency/source contract; WorkLeft MASTER opening and usage guide; V/RAG-06 cross-links/design inventory → F/ARBITER-KNN Decision, positive/negative/warning and final authority; WorkLeft RESEARCH-03 R16 exact-pair/no-repeat/state/functional examination. No claim of reading every RAG family, NDJSON record, WorkLeft file or all 25 research results is made.

Requested six skill areas were applied from K/docs/skills: trainable-architecture-decision, phase-planning, plan-review, generated-code-review, prompt-patch-authoring, conversation-intent-and-self-reflection; code-examination opening/protocol also read. The applied sections concern source/consumer boundaries, current authority, exact candidate, specific counterexamples and clear communication. This is not a whole-skill read or a 30/50-pass/independent-subagent claim. Historical quotas and coverage-as-truth rules were not imported into this scoped repair.

## First learning and retained functional breadth

Earliest path remains `00A + ONE ready 00B passport + minimal 01D + 02B1/S + 03A/S → existing scorer reuse or CODE → one authorized UPDATE → independent 03A/E C0/T`. Confirmed missing wiring alone invokes 00C. The initial scorer must already respect the relevant domain laws; generic nearest-neighbor similarity is not a substitute. It does not wait for a universal tokenizer, all domains, learned-k, LoRA, all sibling slices or Windows CI.

Later claims retain the existing C01–C08 owners: domain axes; representation; θ learning; contextual/learned search effort; exact/ANN; optional typed fusion; actual next-role/token/AST/action; Student×ranker interaction. C#/EN→RU/SQL are distinct examination panels. B0/B1/B2 require novel authorized lessons, matched parent/challenger comparisons, retention and complete cost. A small first pilot does not certify all domains, Unit/Flow/Orchestrator/Graph, DTC/IDE or PC feasibility.

## Actors, minimal checks and final boundary

**CODEX requested `astra 6 ultracode` performs delegated human case selection. The admitted external open Chinese model authors BOTH final chosen and rejected. A separate CODEX requested `astra 6 ultracode` invocation judges the exact unchanged pair.** Student evaluation and independent review are separate real invocations. Requested identity is not evidence of an actual runtime/model call.

Source/prompt/review/no-change adds zero product tests. Existing checks protect changed rights/type/schema/finite-update/once/restore boundaries; only a demonstrated missing changed risk can justify a small additional engineering check in future code work. Learning quality is established by independent domain experiments, not coverage. U does not grade or promote; E does not train or promote.

**Performed here:** source/design review, six task-packet repairs, effective-context/README integration, connected GitHub publication and exact-candidate remote readback.

**Not performed here:** product code changes, .NET runtime reproductions, Windows Actions, deletion/addition of product tests, DPO author/judge calls, Student/ranker training, domain exams, hardware benchmarking or independent Astra review.

`CORE_UNCHANGED_BY_THIS_REPAIR; PRODUCT_TESTS_ADDED=0; TRAINING_NOT_RUN; DOMAIN_GAIN_NOT_MEASURED; PC_NOT_MEASURED; INDEPENDENT_ASTRA_NOT_PERFORMED`.
