# RP10 operational architecture — deterministic + learned, full state, ordinary PC consumer

## На обычном примере

Пользователь просит: «Объясни, как исправить отмену асинхронного C# метода и не потерять данные». Прежде всего обнаруживаем C#/.NET и версию, проверяем закон отмены/согласованности сохранения (детерминированно), строим сохраняющее различие токен/IR/вектор, из памяти находим **несколько** примеров и предупреждений (обучаемое ранжирование допускается после закона), затем составляем допустимые пути и выбираем полезный и компактный. Если модель путает отмену и завершение, можно отдельно готовить законный DPO-урок. Сам полученный ответ ещё не доказывает обучения.

```text
ordinary local request
   → law/intent/stack/version, UNKNOWN/MIXED allowed
   → semantic IR + domain token/vector & versioned space
   → adaptive domain/query kNN: eligible multi-positive + warning evidence
   → typed ILearningUnit<TIn,TOut> many→many (preserve source/association)
   → ILearningFlow (fan-in/out + learned correction; immutable lawful ports)
   → ILearningOrchestrator (async, timeout/cancel, competing lawful routes)
   → ILearningDynamicGraph (higher-order order-2/3 patterns, provenance)
   → existing Student/LoRA chosen checkpoint → ordinary user result
   → permissioned feedback → CODEX selects NEW weakness/case
   → approved open Chinese model authors BOTH final chosen and rejected
   → separate CODEX Astra UltraCode exact-pair judgment → existing RP01 trainer
   → RP09 independent P/C0/T exam → RP10 local promotion decision
   → atomic accepted state pointer + cold reload → next ordinary local request
```

**Rights, domain hard laws, types/schema, port compatibility, candidate eligibility, cancellation, budgets, data separation, replay and rollback are deterministic.** Semantic prediction, ranking, adaptive k/metric, Flow correction, Orchestrator path policy, higher-order Graph authority and Student/Builder parameters can be learned **only where an actual owner, trainable recipient and causal experiment exist**. Do not infer neural training from a new registry row. A short lawful deterministic-only answer may be optimal and MUST NOT be penalized because it is deterministic.

## Required interface *semantics*, not invented C# signatures

| Role | Logical input → output | Mutable state + non-learned invariants | Evidence vs false positive |
|---|---|---|---|
| `ILearningUnit` | (domain, versioned vector/input schema, requested output type, zero-to-many inputs) → typed associated **many→many** results, provenance & confidence | named Unit recipient if proven; immutable type/association law | multiple items without correct relations is not many→many |
| `ILearningFlow` | connected Unit outputs / correction evidence → corrected downstream inputs/outputs | Flow-owned corrections when proven; deterministic port/type compatibility | changed Unit weights mistaken for Flow learning |
| `ILearningOrchestrator` | typed branch DAG + deadline/resource caps → selected/fused async result | actual routing policy if owner; hard stop/cancel/error isolation | fixed sorter or unawaited cancelled work reported as learned |
| `ILearningDynamicGraph` | order-1/2/3 legal motifs, typed route context → generalized pattern / candidate routes | graph relation learning/Builder **separate** from Student; lawful edge provenance | seen-route memorization masquerading as generalization |

## Domain representation, kNN and LoRA are not synonyms
- Domain mapping: multi-axis subject/task/stack/version/rights; `UNKNOWN/MIXED` and abstention explicitly legitimate. Frozen oracle mapping is diagnostic only.
- Semantic IR: preserve discriminating law (e.g., C# `CancellationToken` vs `TaskCompletionSource`; EN→RU negation, SQL isolation); recorded schema/index namespace/hash/adapter version. Domain-conditioned encoder/projection can learn **only** with owned state and separate experiment.
- kNN: choose metric, dynamic `k(x,domain,uncertainty,cost)`, candidate law filter, signed positive/warning, rerank, actual multi-hit consumer. Compare k=1/fixed-k/adaptive-k, exact/ANN, same eligible pool and downstream result; first-hit output despite k=8 is still k=1 consumption.
- LoRA: update frozen-base Student parameters via named low-rank adapter, *not* a guarantee that vectors/tokens changed semantically. Test `LoRA frozen vs new` and `IR/projection frozen vs new` separately, compare student effect, index compatibility and cold loaded artifact.
- Preference training: for immutable x and context, authorized Chinese model authors both final y+ and y−; separate CODEX exact-pair judge must approve law-relevant preference; existing DPO uses reference and response-only policy likelihood with unchanged judged meaningful span. No `Compact` semantic collapse. Pair approval ≠ scientific utility.

## Local acceptance state transaction (RP10 distinct owner seam)
`parentVersion` + `candidateVersion` + `originPairId` + `recipientKeys` + `projectionVersion` + `indexGraphVersions` + `experimentManifest` + `outcomeVerdict` + `AstraReviewReceipt` + `resourceBounds` + `dataRights` → **single atomic selection of accepted pointer**. Only state bundles compatible with the consumer can be selected. Incomplete/unknown-journal or aborted teacher spend does not rewind exposure and cannot be replayed. If RP05 already implements selection, RP10 returns `REUSE_NO_CHANGE` with real cold-consumer proof; otherwise one minimal owner-acknowledged adapter, not an alternative state store.

Required trace: requestId→domainLawVersion→IR/vector/index→retrieved ordered typed hits→Unit associationIds→Flow corrections→Orchestrator branches/cancellation/selected path→Graph motifs→Student adapter/state hashes→answer→optional feedback→no-repeat lesson→P/C0/T→decision→cold answer. Preserve privacy; hashes only when valid identity and consent.

**False claims to falsify**: helper class exists but no ordinary caller; policy says k=6 but only first hit used; positive/negative swapped after CODEX approval; LoRA filename changes without model parameter; offline T beats P but ordinary A reads P; graph route ignores cancellation; invented universal optimal route; one unsafe domain guard overridden by high score.
