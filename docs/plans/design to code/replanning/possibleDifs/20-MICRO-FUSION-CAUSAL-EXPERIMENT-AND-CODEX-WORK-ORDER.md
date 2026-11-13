# MICRO-FUSION-20 — маленький причинный опыт: исчезновение старого навыка после нового примера

<!-- POST117-CURRENT-2026-11-03:START -->
## Текущий вход после P117

[текущая сверка после P117](codex-2026-11-03-POST117-TRANSFER-AND-EXPERIMENT-AUDIT.md) отделяет уже исполненный P115 cost contrast и опубликованный cache от ещё самостоятельной H20 candidate-pool гипотезы. Сначала читать сохранённый опыт и текущего consumer; не повторять PRIMARY ради L3 или документа. До отдельного native задания допустимы пересчёт raw, source audit и чистые проверки формата/чисел/кардинальности, без модели.

Для cache нужен actual corpus→однозначный hash→trusted revision/tokenizer→полный набор score/output rows. Для H20 требуется прежняя неизменность корпуса и raw old-candidate features; source-family split охватывает BPE/нормировку/подбор scale. False/missing/NaN-контроль обязан менять общий verdict/exit. Copyable work order ниже обновлён внутри команды; исторические числа L0 и проектные дозы L1–L3 не являются выполнением этих стадий.
<!-- POST117-CURRENT-2026-11-03:END -->


**Дата:** 16.10.2026. **Статус:** план native-пробы / NOT_RUN; единственная выполненная стадия — локальный JS-прокси формулы, опубликованный отдельно. **Core pin:** lubochka/llm_mvp_core, ветка codex/dpo-implementation, SHA 83590a462b2f8aedc9d3ab4cb775193eb9c25ce5. **Planning:** llm_plan3. Нет независимого CODEX/Astra-verdict. В исходный core код, веса, исходные 500 DPO, frozen exams и thresholds изменений не вносилось.

## Для архитектора — что проверяем и что НЕЛЬЗЯ смешивать

В гибридном TransformerEmbeddingKnnLogicalUnit окончательный ответ вычисляется после поиска соседей: HNSW candidate IDs → 3 сигнала (cosine, lexical BM25-like, structural pattern) → нормировка lexical на max среди **этого** набора → fused 0.45/0.35/0.20 → один выбранный output. Новый пример, который проиграл по итоговому fused, может занизить лексические компоненты старых соседей и **переключить победителя**. Проверяется качество выбранного *предметно правильного* ответа при замороженных старых данных, не только точность самого индекса и не optimizer update LoRA.

Source [unit ExecuteAsync and fused score L184–243](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L184-L243). Исторический E34V2 имеет **другую** формулу и unmatched val subsets. Соседняя ветка language_translate уже содержала эту нормализацию: это не доказанный RP01 regression. См. [20 — анализ причин](20-NEW-GAP-NONLOCAL-FUSION-AND-BM25-SCALING-2026-10-16.md).

## Экспериментальный контракт

- **Hypothesis H20:** при неизменных A/B признаках и метках добавление высоколексического C, который сам не становится top1, приводит к неверной смене старого ответа только из-за candidate-set-dependent maxLexical. Отдельная прикладная гипотеза: на source-disjoint запросах заранее замороженный масштаб улучшает lawful answer accuracy и retention.
- **Baseline A:** исходная формула ScoreCandidates — делить lexical каждого найденного кандидата на maxLexical текущего найденного пула.
- **Treatment B:** весь тот же scorer с тем же retrieval, raw cosine/BM25/pattern и весами, но делить lexical на **заранее объявленную константу** lexicalScale из train/dev (не из sealed heldout), clip [0,1]. Это только диагностика; такой фиксированный масштаб не считается автоматически production решением.
- **Placebo P:** тот же третий кандидат с низким raw lexical, остальные фиксированы.
- **Negative N:** те же frozen candidates/ranks/raw scores, но outputs у учебных ID переставлены; правильное качество должно исчезать или становиться хуже. Не обучать Student/DPO ради негативного контроля.
- **Primary metric:** парная доля lawful final answers на **одних 12 замороженных source-disjoint heldout**; count(A correct,B wrong), count(A wrong,B correct), отдельно incorrect-critical-law, abstain/coverage, latency и hit@k. Вторичная — число rank flips A↔B и число новой неверной генерализации после C.
- **Данные:** один конкретный домен сначала — 6–12 TRAIN случаев, 4 DEV для предварительного выбора lexicalScale и 12 HELDOUT с новыми источниками и, желательно, попарно зеркальными отрицаниями/отношениями; H01…H12 sealed до измерений. Тексты, law/label и разделение фиксируются до первого сравнения. Ни expected answer, ни law label не попадают в metadata retrieval/scorer; CODEX/Astra валидирует значения независимо от scorer.

## L0 — дешёвый арифметический механизм (ВЫПОЛНЕНО, только JS)

Локально исполнен Node v22, файл [20-MICRO-FUSION-CANDIDATE-SET-PROXY.mjs](20-MICRO-FUSION-CANDIDATE-SET-PROXY.mjs), [raw JSON](20-MICRO-FUSION-CANDIDATE-SET-PROXY-RESULT.json). Synthetic frozen inputs:

| Candidate | raw cosine | lexical | pattern | intended |
| --- | ---: | ---: | ---: | --- |
| A | 0.4 | 2.0 | 0 | lawful |
| B | 0.7 | 0.5 | 0 | unlawful |
| C | -1.0 | 12.0 | 0 | losing distractor |

A/B-only → winner A, fused A=0.6650, B=0.4700. С C и исходной динамической нормировкой → winner B, fused B=0.3970833, A=0.3733333, C=0.3500. С C и заранее замороженной шкалой lexicalScale=2.0 → winner A (A=0.6650). С placebo lexical(C)=0.01 → winner A, fused C=0.00175. Assertions=true. **Это только математическое свойство текущей формы score**, цифры raw BM25 придуманы для контрпримера, .NET не выполнялся, empirical prevalence и качество не оценивались.

## L1 — native C# проверка актуального пути, без training (НЕ ВЫПОЛНЕНО)

1. CODEX берёт исходники по точному pinned SHA и фиксирует выбранный тестовый проект/harness. Сначала читает actual ScoreCandidates / ComputeBm25LikeScore, отдельно Build/Execute/serving. Не менять основной Unit и не создавать новую production магию.
2. В test-only harness воспроизвести формулу **из actual private ScoreCandidates**, при возможности вызвать существующий метод через controlled reflection на сконструированном Unit, либо сделать минимальный test-only seam с diff. Подать одни и те же реальные raw features и candidate IDs; снять raw lexical/vec/pattern, divisor, fused, winner до и после появления C в пуле. Для прямого контроля причинности держать полный example corpus и BM25 IDF/averageLength frozen; менять только список кандидатов, иначе добавление corpus item одновременно меняет IDF и averageLength.
3. Проверить четыре ветви: текущая динамическая, фиксированный знаменатель, placebo низкого C.lexical, тот же пул но permuted output labels. Проверить, что raw old A/B features идентичны по битам, candidate IDs стабильны, закон правильности не засасывается в алгоритм.
4. **Требование честности:** объявить L1 FORMULA_PARITY только если актуальный native путь действительно вычисляет приведённые численные значения/соответствующий flip. Если использован лишь скопированный JS/C# helper, пометить SHADOW_FORMULA_ONLY. При невозможности построить native harness — NOT_RUN с командой и blocker, не считать маленький JS actual C#.

## L2 — реальная узкая quality проверка (НЕ ВЫПОЛНЕНО)

1. Заморозить одну уже существующую модель + tokenizer SHA, train-only/documents, retrieval scores, example IDs, source family/split, controlled seeds, 12 exam cases. Ни новых 500 DPO, ни Windows CI, ни массового benchmark.
2. Материализовать для каждого запроса один common candidate pool, raw cosine/BM25/pattern **однократно**, затем A/B/P/N **из одного snapshot**. Уровень 1 — outcome от нормировки на фиксированном пуле (один фактор). Уровень 2 — отдельная честная проверка «добавили учебный C в память»: здесь corpus/BM25 statistics и HNSW candidates могут меняться; декомпозировать их как дополнительные факторы, не приписывать весь эффект одной нормировке.
3. Снимать у каждого запроса полный before/after: hash text, model/tokenizer/index SHA, neighbor IDs, raw features, maxLexical, three fused components, selected output hash и predeclared law verdict. Для H20 корректные/неверные outputs должны быть действительно различными; если одна метка для A/B, ранговая инверсия ещё не влияет на качество.
4. B выигрывает на реальном sealed panel без роста critical wrong и negative N не выигрывает → **EXPLORATORY_HELDOUT_QUALITY_SIGNAL**; значение 12 слишком мало для доказательства широкой generalization, далее новый holdout из других families. Если лишь rank flips без semantic change → RANK_MECHANISM_ONLY; если нет качественного gain → NO_OBSERVED_GAIN, не менять production код.
5. Если L2 положителен, проверить actual user consumer: trace request → route → unit.ExecuteAsync → candidates → selected output → ordinary final answer; никакой claim о RP01, если он обслуживается через TextGenerator.Generate, который этот Unit не вызывает. Сравнить cold-restored state и holdout ответ, не «засчитывать» Unit-only изменение как качество Student.

## L3 — исторический проект проверки стоимости; нынешний P115 рассмотрен выше

Source [ComputeBm25LikeScore L246–281](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L246-L281). Исходный проект16.10 предусматривал корпуса16/32/64/128 с одним tokenizer и frozen queries; baseline пересчитывал корпус, treatment кэшировал statistics той же revision. Эти размеры сохраняются как история проекта, не становятся новым бюджетом P115. Текущая первая работа — сверка сохранённых4 документов P115 и уже опубликованного product cache: полные raw/fused scores, QueryId, candidate sets, outputs/confidence, cold/warm cost и lineage. Расширение корпусов возможно лишь отдельным назначенным исследованием. Риск задержки и стоимость памяти отделены от семантического обучения; L2 и L3 не объединяются в скрытый рефакторинг.

## P0 executable CODEX work order (короткий)

- CURRENT TASK: прочитай эту карточку полностью и120§1/40§2/42/44 из живого core; зафиксируй current pin и текущего получателя. Сначала сверь P115 saved raw/receipts/state и полный расход24 чтения, не повторяя PRIMARY. Уточни exposure BPE, full row/output parity, finite/revision/key-set controls, однозначный hash корпуса и связь всех обязательных исходов с verdict/exit. L1/L2 H20 и расширенный L3 остаются отдельными будущими исследованиями; выполняй только явно назначенный сейчас срез с его прежней дозой/gold, без автоматического перехода к следующему.
- NOT THE TASK: 500 DPO, глобальный LoRA repair, повышение покрытия тестов, Windows GitHub workflow, изменение frozen results/checkpoints, произвольное подключение KNN к ProductHost.
- REQUIRED SKILLS: docs/skills/architect-behavior-classifier-SKILL.md, human-readable-architecture-audit-SKILL.md, phase-planning-SKILL.md, plan-review-SKILL.md, code-examination-SKILL.md, generated-code-review-SKILL.md, prompt-patch-authoring-SKILL.md, experiment-design-SKILL.md, learning-path-audit-SKILL.md, claim-verification-SKILL.md, output-skepticism-SKILL.md и project AGENTS instructions. Применить architecture → plan → independent code/plan review → report and Git publication цикл.
- SOURCE/OUTPUT: два GitHub pinned SHAs, exact raw A/B/C, зафиксированный corpus/split, во время L1 test-only instrumentation без изменения active code; локальный JSON каждого case + manifest + exact runner command + diff + report в possibleDifs. Не записывать fictional PASS/NO_EFFECT, если не запускали.
- CHECKS: отсутствие oracle leakage у всех preprocessing-потребителей, включая BPE и scale; frozen before/after candidate features; exact QueryId/candidate sets/full outputs и finite confidence; actual corpus и trusted revision; same-count подмена target, ghost-document, NaN и delimiter в Text/Output должны быть обнаружены. Каждый missing/false/unknown обязательный исход меняет общий verdict/exit. Все diagnostic/retention/physical calls входят в общий ledger. DPO provenance не использовать для synthetic fixtures; no mutation production state; independent review привязать к SHA конкретного diff/commit; remote readback.
- STOP: не удалось идентифицировать actual Unit consumer; не зафиксировать входные hash; C вызывает одновременно неконтролируемую смену embedder/IDF/ANN; arm B выбирает scale по sealed holdout; невозможна независимая law-оценка; native .NET недоступен. В STOP результат — blocker + точный способ воспроизведения, не подмена настоящего исполнения.
- RETURN PACKET: observed pin; paths/lines; one-factor diff; running command; cases and raw score rows; before/after lawful/critical/abstain; consumer route evidence; CPU/wall/RAM; PASS/NO_GAIN/INCONCLUSIVE; changes review PASS or NEEDS_FIX bound to SHA; publication SHA; no-review marked REVIEW_PENDING.

**DPO governance, даже если позднее потребуется расширение данных:** delegated human choice = **CODEX на Astra 6 UltraCode**; final chosen **и** rejected самостоятельно создаёт **разрешённая внешняя открытая китайская модель**; независимое pair-quality judgement = **CODEX на Astra 6 UltraCode**, не Student. Проверка Student post-training — отдельная стадия. В MICRO-FUSION-20 DPO вообще НЕ создаются, teacher/codex model calls НЕ совершались.

**Исторический статус редакции16.10:** formula shadow EXECUTED (локально), source seam CODE_CONFIRMED, independent native L1 NOT_RUN, source-disjoint quality L2 NOT_RUN, cost L3 NOT_RUN, ordinary consumer NOT_VERIFIED, independent review NOT_PERFORMED. Последующий P115 и нынешний product cache разобраны в актуальном блоке выше; H20 candidate-pool не принимается как production root cause RP01 без своей цепочки.
