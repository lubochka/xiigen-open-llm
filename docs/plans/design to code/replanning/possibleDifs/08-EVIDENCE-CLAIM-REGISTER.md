# 08 — Source inventory, claim class and remaining audit gaps
**Evidence classes:** CODE = directly inspected source; REPORTED_RUN = saved result, not independently rerun; CONTRACT_DIFF = mismatch to documented intent; HYPOTHESIS = falsifiable possible cause; OPEN = missing current trace. Nothing here is an independent Astra runtime review.

## Cross-root RAG and historical navigation (partial; not exhaustive)
1. [original task](../../original%20task.md): original many→many Unit, learnable Flow, asynchronous Orchestrator, higher-order DynamicGraph, feedback cycle.
2. [WorkLeft RAG usage guide](../../09.09.26WorkLeft-RAG-00-usage-guide.md) and sibling `09.09.26WorkLeft*`: must enumerate and follow links recursively; complete corpus not claimed read.
3. [Recovered design contract](../RECOVERED-DESIGN-CONTRACT.md), [algorithm experiment handoff](../ALGORITHM-EXPERIMENT-HANDOFF.md), [R01–R25 results](../results/), [previous thinking](../previous%20rounds%20thinking%20results.md), [parallel join](../PARALLEL-R01-R03-JOIN.md).
4. [RP01 pack](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/), [RP02 pack](../Kiro-Xiigen-RP02-Domain-Adaptive-Learning-Slice-Pack/), RP03/RP04 design carriers, design-to-code slice roots and original parent designs `llm mvp/dpo legal build` and `llm mvp/llm dynamic flow`.
5. [E09/E15/E16/E17/E29/E31 reconciliation](01-EXPERIMENT-BASELINE-RECONCILIATION.md); note E09 unequal validation indices and E17 incomparable vocab/perplexity.

## Primary current source links
- [DPO trainer](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs), [DPO material](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoExactPairMaterial.cs) (verify exact path at start), [sim import](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs).
- [LoRA forward](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/10.LoRA/LoraLinear.cs), [GenerateCore](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/09.Inference/TextGenerator.cs), [kNN consumer](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs).
- [DynamicGraphContracts](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/00.Core/DynamicGraphContracts.cs), [LearningMachineContracts](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/00.Core/LearningMachineContracts.cs) — source names from recovered design, verify fresh HEAD.

## Claims that are supported vs require tracing
| Claim | Class | Important limitation |
|---|---|---|
| LoRA A/B code exists, frozen base/readback performed | CODE+REPORTED_RUN | Not proof of useful learned behavior |
| DPO response-masked objective exists | CODE | gradient numeric correctness not independently checked |
| kNN chooses hits[0] | CODE | Whether called by failed RP01 free route UNKNOWN |
| E09 positive top1 measured | REPORTED_RUN | Unequal baseline and treatment val sets |
| E15–17 compressions lower perplexity | REPORTED_RUN | Different target vocabulary; cannot compare semantic correctness |
| C#500 ENRU500 show zero correct free gains | REPORTED_RUN | Very small panels, same-family/template limitation |
| Actual DPO→typed Unit/Flow→Graph transfer | OPEN / CONTRACT_DIFF | Needs current source owner, state and consumer |
| Proper domain token semantics, learned k, adaptive law | OPEN | RP02 must establish real consumer effect |
| Production adoption of research adapter | REPORTED_RUN nonadopted | Not an adoption error when quality unproven |

## Explicit audit exclusions / follow-up evidence
Current collection is a source-grounded preliminary triage, not a complete repository crawl, not independent execution of 500 C# samples, not proof that any specific listed defect caused the zero score, and not a final independent CODEX Astra 6 UltraCode assessment. Before implementation, revisit code on pinned branch HEAD, recursively enumerate parent/sibling RAG and all WorkLeft*, map Unit/Flow/Orchestrator/Graph actual callsites, inspect DPO token masks and gradient equations, recovery and consumer identity, do equal-panel ablations. The classification and falsifier are required; do not mark all differences as bugs.