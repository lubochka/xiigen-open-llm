# MICRO-N38 — доказать либо опровергнуть потерю структурного урока в ReplayFusionRanker

**Статус 16.10.2026:** executor-ready protocol / experiment `NOT_RUN` / CODEX Astra 6 UltraCode `NOT_INVOKED` / native .NET `NOT_RUN`. Никакого статуса quality PASS на основании этого текста. Только допустимые изменения результата: `xiigen-planning:llm_plan3/.../replanning/possibleDifs`. При исполнении product изменения возможны лишь по отдельному поручению и review, **НЕ** в `llm_mvp_core/docs/plans/replanning-possible-diffs`.

## Вопрос и три независимые гипотезы

`N38-A`: при `UseReplayFusionRanking=true` replay scorer использует lexical Jaccard вместо отдельного learned-vector similarity, из-за чего релевантный старый урок не попадает в top3 на случаях, где одинаковый набор слов кодирует разные законы.

`N38-B`: `pattern=vector` маскирует недостающий domain-structural extractor. Если отдельно дать AST/POS/role/order, повышается ранжирование нужного law ID **и correctness обычного Flow** на новых случаях (доказательство только если обе ступени реально меняются).

`N38-C`: при `Count<=topK` запрос игнорируется ранжированием; только если downstream **зависит от порядка или ещё отсекает** часть candidates это может поменять learned law quality. При order-invariant получателе эффект на качество должен быть нулевым.

**Изолированный нейронный RP01 DPO/LoRA сюда не включать**; его `temperature=0, topK=0, topP=1` и `simulation exact-v2` относятся к другому получателю. Не повторять 500 пар ради этого опыта.

## 0. Предварительная проверка версии и законного входа (перед любыми training changes)

- Freeze `core branch + commit SHA`, blob SHAs двух файлов `18.CapabilityTraining/ReplayFusionRanker.cs` и `DefaultLogicalCapabilityTrainer.cs`, версия признаков, настройки `UseReplayFusionRanking` и `topK=3`, исходные hashes manifests. Source anchors: [Rank L13–88](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/ReplayFusionRanker.cs#L13-L88), [caller L490–544 and L1850–1883](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L1850-L1883).
- Нельзя подставить недоверенный искусственный manifest непосредственно в production training: использовать существующий verified source + attestation; синтетические **shadow fixtures** только в изолированном тестовом инструменте без mutation authority. Внутрь sealed blind exam ни train, ни selector не заглядывают.
- Восстановить вызов `SelectAndValidateReplaySnapshot` и downstream `AutonomousLearningReplayMixer` / `BuildKnnTrainingCorpus`; осмотреть actual consumer `Executable Flow`. Если конкретный planned user flow не читает этот learned Unit, классифицировать `NOT_WIRED`, а не «плохая точность KNN».
- Отдельно зарегистрировать действующие источники правды о domain-specific features: Roslyn AST для C#, SQL parse tree для SQL, POS/dep parser для EN↔RU. Это **опциональные экспериментальные признаки**, не универсальное изменение Student tokenizer.

## 1. Tier-0: тест контракта scoring без дорогого обучения

**Одна короткая нативная .NET диагностика, 4–6 manifested candidate records одного domainAdapterTag, `topK=3`**. Три старых кандидата не равны трём независимым обучающим семействам; каждый manifest имеет настоящий distinct law/sourceFamily/source hash.

- **T0A:** 2 verified manifest. Старый A точно соответствует compiled query, новый B лексически чужой. Вызвать `Rank([B,A],query,3)` и вывести ordered IDs; затем `Rank([A,B],query,3)`. По исходному коду результаты совпадают с input order. Здесь **membership не меняется** — это отрицательный контроль на ложное заявление «урок потерян».
- **T0B:** добавить C,D до 4 manifests, включая один `wrong-law lexical twin` (те же нормализованные roots, другая syntax/role), и включить fusion. Записать raw `BM25Like`, `Jaccard`, separate `pattern`, общий `score`, `rankingTop3`; контроль `UseReplayFusionRanking=false` и shuffled manifest order. Прямая source-predicate: `pattern==vector` и `score == .65*vector+.35*lexical` (с погрешностью float double не более 1e-12).
- **T0C (контрфакт без product patch):** на **тех же** candidate IDs заменить только третью функцию scoring pattern из «копии vector» на проверенный domain predicate feature; нельзя менять BM25, Jaccard, train texts, admission, capacity или priority. Для N38-A отдельно заменить `vector` на действительную frozen embedding cosine; третий pattern остаётся прежним. Вести эти интервенции раздельно: `A=current`, `B=embedding only`, `C=syntax pattern only`; допустим exploratory factorial `D=both` только после A/B/C.
- **Negative controls:** запрос с уникальными лексическими термами, regex/exact identifier, 2 кандидата без downstream order sensitivity; если pattern всегда «улучшает» и эти контроли, искать leakage/feature-gate ошибку.

**Tier-0 result:** только `RANK_SEAM_REPRODUCED` / `RANK_SEAM_FALSIFIED` / `NOT_EXECUTED`. Не повышать до LEARNING_GAIN.

## 2. Tier-1: недорогой доменный causal A/B с реальным learned consumer

| Домен | Учебные manifest (4–6 в одном домене) | SEALED новый запрос (8–12) | Проверяемая ошибка |
| --- | --- | --- | --- |
| C# / Angular TS | пары same identifiers с разным `if/await` и caller/parent AST | новые методы/файлы/проект с тем же law, но другим именованием | error handling / cancellation / wrong callsite |
| SQL | два закона `LEFT JOIN ... ON ...` vs `WHERE ...` с одинаковым набором roots и разным деревом | новые таблицы и значения, 2 adversarial `NULL` случая | row-loss / неверное значение отфильтровано |
| EN→RU / RU→EN | `Anna follows Boris` vs `Boris follows Anna`; scope `not / never`, grammatical role | другие имена, времена и тематики, source-family-disjoint | субъект-объект/отрицание меняет истину |
| Regex/exact-control | source exact keys и порядок нейтрален | 4 контрольных запроса | неожиданный false-positive от syntactic gate |

### Фиксированные факторы

Один Student snapshot, tokenizer/version, модель/LoRA, набор manifests, seed, candidate budget, источник labels, evaluator, cold-restore. Для A/B изменяется **только replay selection strategy**. Важно: не создавать «правильный» test manifest из sealed gold после просмотра ответов.

1. Изолированная копия исходного state. `Arm A`: текущий `ReplayFusionRanker` и downstream existing acceptance. `Arm B`: тот же код, но test-only structural pattern для релевантного домена при неизменном остальных. `Arm C`: отдельный test-only actual embedding-vector вместо lexical placeholder, pattern прежний. Если training grant не позволяет без product patch, выполнять сначала read-only shadow и маркировать quality as `NOT_RUN`.
2. Зафиксировать `query ID, sourceFamily, domain, lawId, sourceVersion, structural-feature-version, candidateId+sha, score parts, actual selected replayIDs, replayDpoIds, trainedKnnExampleIds, generatedUnitId`. Не копировать приватный chosen/gold в публичные отчёты.
3. Запустить одну минимальную disposable training campaign на настоящем owner (без повторного запуска прежних consumed pair IDs) при разрешённом admission. Сравнить до/после **обычный конечный Flow**, а не только `Rank()` или train metrics. Fresh-process cold restore обоих arms и identity/hash check.
4. Независимый CODEX/Astra 6 UltraCode evaluator получает blinded outputs A/B и доменные обязательные clauses; первичная метрика = доля полностью соблюдённых обязательных законов (или заранее frozen clause score), макро по независимым families, отдельно `criticalErrorRate`, `retrievalRelevantLaw@3`, `source-disjoint top1` и старый law retention. Показывать wins **и losses**.
5. Вердикт: `OBSERVED_SMALL_GAIN` при хотя бы одном source-disjoint реально исправленном законе, положительном net paired effect, отсутствии новых critical regressions; `NO_OBSERVED_GAIN` при отсутствии улучшения; `INVALID_EVIDENCE` при leakage/неправильном consumer/grant; `MECHANISM_ONLY` если разница ранжирования есть, но Flow не меняется. Это exploratory и не статистическая гарантия (N=8–12 на домен).

### Роли, не подменять программой или данным авторством

**CODEX с Astra 6 UltraCode выполняет роль человека — выбирает конкретный TRAIN case** и доменные критерии до train. **Внешняя открытая китайская модель** независимо порождает **обе** фактические стороны DPO `chosen` и `rejected` при необходимости новых DPO; receipt должен указывать её действительный model/revision/id, не Astra simulated и не DeterministicGenerator. **Отдельный CODEX c Astra 6 UltraCode** проверяет *обе* стороны, их законность и семантический контраст до обучения; ещё один независимый evaluator CODEX/Astra 6 UltraCode оценивает blind before/after ответы. Если уроки reused как verifiable stored existing manifests, новый DPO author не вызывается и эти роли не симулируются. Аудитор ChatGPT здесь не выполняет эти CODEX/Chinese вызовы.

## 3. Falsification / stop / decision

- **N38-A отвергнуть для measured consumer**, если source-only lexical score не влияет на selected replay IDs на проверенной панели либо доступный реальный embedding не даёт отличающей информации/ухудшает quality. Не путать «нет в ReplayFusionRanker» с «нет нигде в проекте».
- **N38-B отвергнуть как causal quality gain**, если AST/POS signal меняет score, но не selected manifest/learned KNN entry/ordinary answer; либо на новом домене ошибок больше.
- **N38-C оставить как contract difference без quality harm**, если при ≤3 конечная пара/Unit не зависит от порядка, а вся обучающая информация сохранена. Не проектировать patch ради чистого изменения сортировки.
- Никаких новых Windows CI циклов или 500 DPO ради проверки; одна прицельная compile/runtime integrity проверка при фактическом изменении кода, только затем доменный A/B. Время/память измерять после действительного запуска, не выдумывать.

## 4. Где записывать результат

Создать отдельный machine/human-readable receipt с полями `coreSha`, `planningSha`, `domain`, `caseFamilyIds`, `arms`, `sourceHashes`, `actualCallChain`, `legalGrantId`, `queryFeatures`, `selectedBeforeAfter`, `learnedStateHashes`, `freshProcessRestored`, `blindJudgeReceipt`, `pairedLawScores`, `criticalFailures`, `qualityVerdict`, `unknowns`. Текущая карточка — план, а не receipt. Связанный finding: [N38 lexical shadow](38-NEW-REPLAY-FUSION-LEXICAL-SHADOW-PATTERN-ALIAS-AND-SHORTLIST-GATE-2026-10-16.md).
