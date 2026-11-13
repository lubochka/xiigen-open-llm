
# MICRO-N36 — от ложного GraphRAG evidence к честному abstain за один тест

**16.10.2026 · native .NET experiment DESIGN; NOT_RUN.** Источник и source-only numerical demonstration: [N36 audit](36-NEW-GRAPHRAG-SEEDLESS-PAGERANK-FALSE-EVIDENCE-2026-10-16.md), [proxy](36-MICRO-SEEDLESS-PAGERANK-SOURCE-PROXY.mjs), [proxy receipt](36-MICRO-SEEDLESS-PAGERANK-SOURCE-PROXY-RESULT.json). Никакой показанный JS output не является learning/product quality.

## Гипотеза и одна переменная

**ExperimentId N36-EMPTY-SEED-CAUSAL. Tier1: 3 маленьких graph fixtures + 8–12 независимых user queries для одного domain.**

Baseline **A**: текущая цепь IVectorIndex.Search → GlobalSearch.Search → PersonalisedPageRank → HybridRetriever.Retrieve. Enhanced **B**: единственная test-only проверка перед вызовом PPR: сформировать список seed IDs, для которых KnowledgeGraph.TryGetNode истинно; если такой список пуст, GlobalSearch returns empty collection и не предлагает чужой GraphRAG ContextText. При хотя бы одном валидном seed поведение PPR и остальной системы должно быть **абсолютно тем же**, что в A. Не менять encoder, PageRank damping, Graph edges, topK, RawText, weights, model, teacher, или scorer.

Это **проверка контрактной границы**, а не автоматическое решение всех zero-hit queries: допустим другой доказательный retrieval provider. Его trace должен иметь отдельное source происхождение.

## Три конструкции, выполнимые без обучения

**S1 EMPTY_INDEX:** KnowledgeGraph с тремя различными GraphNode Id, Name, Type, RawText (по одному из трех разных source law); IVectorIndex = пустой BruteForceIndex; EmbeddingModel создается из одного допустимого frozen BPE+encoder fixture с непустым query. GlobalSearch.Search возвращает vector-seed count=0. Зафиксировать A.Results count, A.ContextText, GraphScore, actual node IDs; B должен вернуть 0 GraphRAG results при нулевом count. Если real path не существует/не вызывается, verdict NOT_APPLICABLE_TO_CONSUMER.

**S2 STALE_ONLY:** тот же graph, но index содержит 1 вектор с Id=MissingFromGraph, которого нет в KnowledgeGraph. Подать query, который выбирает только этот index hit, и проверить actual validSeedCount=0. A может получить произвольные существующие nodes с нулевым PageRank и сформировать ContextText; B отказывает, не допуская contaminated evidence. Индекс и граф намеренно рассогласованы ТОЛЬКО в test-only fixture, не считать это доказательством реальных production incidents.

**S3 VALID_SEED_NEGATIVE_CONTROL:** тот же graph, index содержит node B с Id, реально присутствующим в графе. A и B ОБЯЗАНЫ дать одинаковые GlobalSearch/HybridRetriever results, score и ContextText. Если нет — guard изменил семантику законного retrieval и эксперимент INVALID.

Дополнительный отрицательный контроль: graph вообще пуст (n=0). Оба варианта ожидаемо не формируют context. Проверить также topK=1 vs topK=3, не выдавать более короткий arbitrary list за исправление.

## Проверки и оценки качества

**L0 SOURCE_CONTRACT:** вызвать настоящие .NET GraphRag.GlobalSearch и HybridRetriever на S1/S2/S3, не JS proxy. Успех L0 требует: S1/S2 A выдаёт graph nodes при нуле **валидных** vector seeds, B не выдаёт; S3 A и B совпадают. Залоггировать nGraph, originalSeedIds, validSeedIds, PPR full ranks, topK, ContextText. Если A вместо этого корректно возвращает empty на реальном коде, наш исходный вывод о reachable branch неверен: записать FALSIFIED_NATIVE.

**L1 FINAL_QUALITY только после actual consumer trace:** собрать 4–8 полностью законных seed lessons/knowledge nodes и 8–12 sealed source-family-disjoint queries по C#/SQL/Translation либо DTC. Раздельно воспроизвести 2–4 queries с нулем/устаревшим seed. В сравнении A/B подать downstream product **тот же** user request; модель, retrieved local results, decoder, tokenizer, verifier и Rubric не меняются. CODEX/Astra независимый blind judge не должен знать A/B. Primary = false-accepted critical answers на запросах без evidence; secondary = law-correct answered, legitimate coverage, abstention, wrong abstention, context contamination, latency. Не объявлять «качество выросло», если B просто отказался от всех задач, где можно было ответить.

| Домен | Изолированное ложное evidence | Критическая ошибка, которую измерить |
| --- | --- | --- |
| C# | узел другой функции/overload со схожим именем, а правильный node отсутствует в индексе | unsafe null guard, неверный return type |
| SQL | узел несовместимой schema/dialect без matching seed | утверждение неверной семантики JOIN/NULL |
| EN→RU | lesson противоположного direction или с отрицанием вместо утверждения | смена смысла или адресата |
| DTC/IDE | устаревшая Angular/SDK версия в графе, нет matching seed | недопустимый API/CSS и false implementation confidence |

**Falsifiers:** (1) code/actual consumer не вызывает GlobalSearch; (2) все реальные data paths гарантируют хотя бы один валидный seed и это подтверждено наблюдением; (3) per-case final quality A и B одинакова без contamination; (4) B подавил реальные верные ответы. SOURCE_MATCH не равно PRODUCT_GAIN.

## Минимальное исполнение CODEX

Read-before-edit: [GlobalSearch.cs](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/08.GraphRag/GlobalSearch.cs), [PageRankCalculator.cs](https://github.com/lubochka/llm_mvp_core/blob/216a1f3d6af3f40ceef081dc168b07e4ce3934f9/07.GraphEngine/PageRankCalculator.cs), GraphRag HybridRetriever/LocalSearch, VectorSearch.BruteForceIndex, GraphEngine.KnowledgeGraph/GraphNode, existing GraphRag test helper, actual call chain. Создать isolated test-only native scenario по фактическим namespace and config; один targeted local dotnet test без Windows CI и без 500 pairs.

Test-only variant B должен обрезать **только invalid seed IDs** до PPR и возвращать empty, если осталось ноль; при nonzero valid seeds не изменять порядок существующих и не менять их веса. Запрещено «чинить» проблему изменением числа hidden dimensions, tokenizer, оптимизатора, DPO weights, KNN k или semantic oracle одновременно.

**Work order:** CURRENT TASK N36 deterministic zero-valid-seed contrast and real consumer law evaluation; NOT THE TASK RP01 model retrain or broad refactor. EVIDENCE ONLY source snapshot and N36 proxy; required skills: architect-behavior-classifier-SKILL.md, intent-to-plan-SKILL.md, phase-planning-SKILL.md, plan-review-SKILL.md, audit-protocol-SKILL.md, claim-verification-SKILL.md, code-examination-SKILL.md, generated-code-review-SKILL.md, experiment-design-SKILL.md, prompt-patch-authoring-SKILL.md, human-readable-architecture-audit-SKILL.md, human-communication skill. Allowed files = isolated test/probe and xiigen-planning possibleDifs receipts after approval; forbidden files = core/docs/plans/replanning-possible-diffs, frozen RP01 evidence, pretrained weights. Allowed commands = source-read, one native focused run, diff/sha checks; forbidden commands = Windows CI, broad training, unrelated fixes. Good output = paired final law and exact candidate-bound independent review; bad output = assume PageRank topK means relevant, or measure only test PASS.

**Return packet:** source SHA + test diff SHA; actual creation-call details; S1/S2/S3 logs and JSON case IDs; originalSeedIds/validSeedIds; A/B candidates, scores, context; CODEX blind judge results and counts; negative controls; changed/unmodified paths; claims marked C/P/NATIVE/H; independent review PASS/NEEDS_FIX bound to reviewed commit; next falsifier. claim_scope/evidence_scope/count_claimed/independent_evidence_count/subagent_id_or_NONE/downgrade_needed.

**DPO governance unchanged:** human role lesson selection = CODEX with Astra 6 UltraCode; approved external open Chinese model creates both final chosen and rejected; CODEX with Astra 6 UltraCode independently judges DPO pair and observed model output. N36 does not require teacher generation; none was called.
