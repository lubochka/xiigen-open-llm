# N31 — Два новых разрыва continual learning: порог включения relevance и псевдоструктурные 20% в replay selector

**Дата:** 2026-10-16. **Источник:** `lubochka/llm_mvp_core@79082340f01b744cdf08185616f2bd818dbd06b8`, ветка `codex/dpo-implementation`; исторический контраст — `DesignToCode` (старый файл с тем же условием). **Единственная область записи:** `xiigen-planning/llm_plan3/llm mvp/design to code session/replanning/possibleDifs`. Директория `llm_mvp_core/docs/plans/replanning-possible-diffs` — строго read-only.

**Статус:** `SOURCE_CONFIRMED` для ветвления и формулы, `JS_PROXY_EXECUTED` для двух конструктивных примеров, `NATIVE_DOTNET_NOT_RUN`, `PAIRED_CONSUMER_QUALITY_NOT_MEASURED`, `INDEPENDENT_REVIEW_NOT_RUN`. Это самостоятельная гипотеза потери *исторически обученных уроков capability/Replay*, **не установленная причина** отрицательных Student-only free-response RP01 C#500 `0/3→0/3` и EN→RU500 `0/4→0/4`.

## Для человека: почему очередной правильный урок может поменять старый ответ

Раньше система обучалась не только нейросетью: были успешные, но ограниченные по валидности, retrieval/bigram/replay эксперименты. У нового capability-trainer есть память прошлых уроков, которую он собирает в список `replay manifests`. Правильный урок может сохраниться на диске, но **не попасть в текущую выборку**, от которой зависит следующее обучение и последующий KNN.

Найдены два отдельных свойства *конкретного селектора*, ранее не выведенные явно из общего ограничения replay `top3`:

1. Пока в одном домене 1–3 урока, включённый флаг семантического ранжирования не приводит к семантической сортировке **вообще**. На четвёртом уроке сортировка внезапно появляется. Поэтому алгоритм, обучающийся непрерывно, имеет **нелинейную смену политики** прямо на границе размера памяти.
2. Формула заявляет три признака: similarity + BM25 + pattern; но `pattern` в реализации — **повтор той же similarity**, не синтаксическое дерево, не роли, не порядок. Значит вес `0.20` не даёт второго независимого структурного сигнала. Особенно рискованно для C#/SQL и языка, где перестановка субъекта/объекта или области `not` меняет смысл.

## N31-R1: relevance-ranking выключен ровно до появления (k+1)-го урока

### Непосредственные факты

- [`DefaultLogicalCapabilityTrainer.SelectAndValidateReplaySnapshot` L1798–1814](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L1798-L1814): проверяет прошлые manifest того же `DomainTag` и их принадлежность topology. Нельзя подать произвольную неаттестованную строку в production.
- [Тот же caller L1870–1880](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L1870-L1880): сначала сортирует доступные manifest по `CreatedAtUtc DESC`, затем при `UseReplayFusionRanking=true` и непустом `compiledText` вызывает `ReplayFusionRanker.Rank(..., topK:3)`.
- **Ключевой шов:** [`ReplayFusionRanker.Rank` L13–37](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/18.CapabilityTraining/ReplayFusionRanker.cs#L13-L37), дословная ветка условия: `if (string.IsNullOrWhiteSpace(compiledText) || manifests.Count <= topK) return manifests.Take(topK).ToArray();`. Следовательно при непустом запросе и `Count=3, topK=3` программа **не вычисляет** relevance score, хотя флаг активен. При `Count=4` — вычисляет. Это конкретная смена алгоритма, не гипотеза.
- [Main training call L490–543](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/18.CapabilityTraining/DefaultLogicalCapabilityTrainer.cs#L490-L543): выбранный список реально передаётся в `AutonomousLearningReplayMixer.BuildPreferences`. Ранее зафиксированное соседними агентами различие получателей DPO vs KNN сохраняется; наш эффект находится **до** этого различия.
- [Исторический `DesignToCode` ReplayFusionRanker L8–33](https://github.com/lubochka/llm_mvp_core/blob/DesignToCode/18.CapabilityTraining/ReplayFusionRanker.cs#L8-L33) имеет **ту же границу**. Это inherited policy seam, **не** вновь внесённая RP01 регрессия.

### Что это предсказывает и чего НЕ предсказывает

- Если текущий источник возвращает три `created DESC` записи `[новый посторонний, средний посторонний, старый релевантный]`, selector без scoring возвращает тот же порядок; forced-scoring только тех же трёх выдаёт `[старый релевантный, ...]`. Факт: критерий «самый важный для нового задания» не применяется при малой памяти.
- Добавление **четвёртой** посторонней записи пересекает порог и меняет стратегию. Состав корпуса для BM25 тоже меняется; поэтому фактическое изменение качества после `3→4` нельзя приписывать одному `if` без контрольного `3×alwaysScore`.
- Для `Count <=3` **все три** урока остаются в replay selection. Нельзя писать, что один из трёх был удалён. Риск — приоритет/далее применяемые replay budgets и дальнейшие изменения при росте корпуса.
- Не относить seam к RP01 LoRA / isolated Student без доказанного call graph `user output→capability replay`; этот путь создаёт *другие* train recipients.

### Быстрый source-equivalent расчёт

Пара `[31-MICRO source JS](31-MICRO-REPLAY-SCORE-AND-ORDER-SOURCE-PROXY.mjs)` / `[результат JSON](31-MICRO-REPLAY-SCORE-AND-ORDER-PROXY-RESULT.json)` проверена выполнением JavaScript proxy 16 октября:

| Arm | Всего manifest | Первое место | Что фактически сравнивалось |
| --- | ---: | --- | --- |
| A: current | 3 | `recent-irrelevant` | `Take(3)` от `created DESC`; relevance даже не рассчитывается |
| B: shadow alwaysScore | те же 3 | `old-relevant` | тот же корпус, query и BM25, удалён **только early return** |
| C: current, плюс посторонний | 4 | `old-relevant` | начинается рейтинг; **конфунд с новым corpus/idf** |

Здесь качественные `relevant/irrelevant` — авторские метки **синтетической fixture**, не результаты проверки Student или пользовательских задач. В прокси токены нормализуются упрощённо, **не** фактической C# `TrainingVectorBuilder.NormalizeRoots`. Native parity и actual teaching consumer остаются `NOT_RUN`.

## N31-R2: `patternScore` — не структурная модель, а дубль token-Jaccard

### Прямая доказательная цепь

1. [`ReplayFusionRanker` L23–37](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/18.CapabilityTraining/ReplayFusionRanker.cs#L23-L37): `vector = ComputePredicateOverlap(compiledText, manifest.TrainingText);` затем `pattern = vector;` затем `fused = (vector * 0.45) + (lexical * 0.35) + (pattern * 0.20)`.
2. [Тот же файл L40–50 и L81–87](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/18.CapabilityTraining/ReplayFusionRanker.cs#L40-L50): `ComputePredicateOverlap` считает пересечение **множеств** нормализованных корней/токенов к их объединению; порядок/роли не участвуют. `Tokenize` строит словарь частот через `TrainingVectorBuilder.NormalizeRoots`.
3. Сокращённая фактическая формула: **`fused = 0.65 * token_set_Jaccard + 0.35 * BM25_like`**, а не `0.45 vector + 0.35 lexical + 0.20 independent AST/pattern`. Это строгий алгебраический вывод для этого конкретного класса.
4. [`TransformerEmbeddingKnnLogicalUnit.ScoreCandidates` L217–244](https://github.com/lubochka/llm_mvp_core/blob/79082340f01b744cdf08185616f2bd818dbd06b8/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L217-L244) — **иной** scorer: там реально вызывается `ClassifyPattern`. Нельзя ссылаться на него как на наличие syntax feature **в ReplayFusionRanker**: одинаковые обозначения E34/E25 не гарантируют одинаковый путь.

### Минимальный контрпример и домены

Запрос `agent transfers asset`; старый правильный урок `agent transfers asset`; новый неверный по роли `asset transfers agent`. Оба содержат один и тот же набор токенов с одинаковыми частотами. В shadow scorer текущего replay они получают равные similarity и BM25; `CreatedAtUtc DESC` выбирает новый **неверный** пример. Замена **только** ложного `pattern` на простой order-aware proxy меняет winner на старый правильный урок. Это доказательство *возможности неразличимости по имеющимся признакам*, не доказательство production-ошибки: слово `correct` присвоено автором fixture, реальные AST/семантические labels и роль уже в `PredicateIds/TrainingText` требуют проверки.

| Где полезна настоящая структура | Где НЕ навязывать её |
| --- | --- |
| C#: AST nodes, receiver/callee, nullability, тип и порядок вызова; SQL: оператор `NOT`, `ON` vs `WHERE`, положение предиката; перевод: субъект/объект, отрицание, согласование, синтаксическое дерево | Не все домены требуют AST. FAQ/простая тематика без отношений: обычный lexical/vector scorer может быть быстрее и достаточно точен; структуру включать только при измеренном выигрыше и корректном parser |
| Временная последовательность / причинно-следственные планы: порядок событий и зависимостей | Не считать простой порядок слов синонимом AST, когда перестановка безопасна/стилистическая |

## Уникальность относительно соседних работ (не удваивать баги)

| Уже написано в possibleDifs | Наш независимый проверяемый delta |
| --- | --- |
| [N27 replay-aperture](27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md) | Измеряет **max3 и последующее recency drop**, а здесь — **условный пропуск ранжирования именно при count <= k**, даже когда flag/query присутствуют |
| [N20 нелокальный BM25](20-LESSON-ADDITION-RERANKER-NONLOCAL-QUALITY-GAPS-2026-10-16.md) | Другая ветка fusion и нормировки в `TransformerEmbeddingKnnLogicalUnit`; наш `ReplayFusionRanker` без independent pattern и имеет ранговую discontinuity на n=k |
| [N27 sequence/negation](27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md) | Потери порядка в исторических train split/KNN stopwords; здесь — **псевдоструктурный вес в replay-манифестах**, до новой тренировки |
| [N29 rejected annotation](29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md) | Несовпадение source/train memory; не совпадает с replay-selection score |

Новизна относится к **узкому code-path и фальсификатору**, не заявлению об отсутствии любых упоминаний в 200+ параллельных документах. Это **не два новых независимых наблюдённых падения качества**.

## Проверка качества кода/архитектуры и решение

- **Architect:** сначала установить `request flags→SelectAndValidateReplaySnapshot→BuildPreferences→new Unit→ordinary consumer`. Не чинить этот selector ради direct RP01 0/4.
- **Plan:** выполнить [N31-MICRO](31-MICRO-REPLAY-RANKING-NATIVE-CAUSAL-PROTOCOL-2026-10-16.md) с двумя независимыми one-factor arms; не менять одновременно DPO, LoRA, tokenizer, размеры памяти и метрику.
- **Code/Review:** читать полный вызывающий метод, признавать безопасность replay-attestation и limit256; proposed patch не вправе обходить auth, provenance и canonical manifest validation.
- **Prompt/human:** заранее фиксировать `source language, domain, law id, created/order, expected response, forbidden response`; хранить реальные отрицательные/положительные ошибки, не выдавать proxy за native.
- **Verdict:** `RESEARCH_ONLY / NATIVE_CONFIRMATION_REQUIRED`. Автоматическая правка продуктового кода или CI/Windows matrix здесь **не санкционирована**.
