# 20 — Новая независимая гипотеза: KNN может менять старые ответы после добавления постороннего урока

<!-- POST117-CURRENT-2026-11-03:START -->
## После P117: P115 подтвердил стоимость, source-ремонт cache уже опубликован

[текущая сверка после P117](codex-2026-11-03-POST117-TRANSFER-AND-EXPERIMENT-AUDIT.md) подтверждает паритет 4 запросов/16 кандидатов, включая полные ответы, при 156→4 токенизациях; cold build считается отдельно. Product snapshot на core 879a07b уже строится на версию корпуса. Это относится к N20-02; нелокальная нормировка N20-01 от candidate pool остаётся отдельным фактором. Положительный cache result не даёт разрешения одновременно менять fused weights или знаменатель.

Следующая приёмка требует однозначного кодирования полей корпуса перед hash, доверенной ревизии/идентичности словаря, конечности чисел, точного набора document IDs и полного output parity. Плохой пример: те же ID и count, но другой target, лишний ghost-document либо NaN среднего — проверка всё ещё принимает. Хороший пример: каждый такой контроль отклоняется, а честный снимок принимает. Строки с разделителем внутри Text/Output тоже должны давать разные fingerprints для разного содержимого.

Независимость экзамена проверяется для всех preprocessing-потребителей: в P1154 вопроса попали в BPE как TrainingAlias. Это ограничивает holdout claim, сохраняя общий A/B cost result. Общий расход 24 чтения включает 6 retention сверх 18; отдельная категория не обнуляет цену. IDF общего слова 0,10536051565782635 даёт одинаковый ненулевой вклад, а не нуль. Ремонт отчёта/проверяющего выполняется сначала на saved evidence. Ниже историческая постановка 16.10, не новое поручение повторить P115.
<!-- POST117-CURRENT-2026-11-03:END -->


**16.10.2026 · DRAFT / source-confirmed mechanism, causal impact on real quality NOT_MEASURED.** Base: `llm_mvp_core@83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`, planning branch `llm_plan3`. Авторская документарная проверка плюс выполненный локальный JavaScript-прокси. Это **не** новый DPO run, **не** запуск .NET, **не** независимая оценка CODEX/Astra и **не** доказанная причина RP01 C#500/ENRU500.

## Коротко для человека: где может теряться уже выученное знание

Предположим, KNN правильно выбирал урок A, хотя рядом был менее правильный B. Мы добавили третий урок C, который сам не должен побеждать. Ожидаемая интуиция: пока A и B не изменились, ответ останется A. Но нынешняя формула гибридного поиска пересчитывает вес **каждого** старого кандидата относительно максимального лексического совпадения **среди всех доступных соседей**. Если C имеет высокий BM25, даже не побеждая сам, он способен поменять местами A и B. Снаружи это выглядит как «после обучения ещё на одном примере модель забыла старый закон», хотя ни LoRA, ни правильная метка старого примера не изменились.

Это отличается от уже зарегистрированных top-1, хеш-коллизий, отсутствующей law-aware rerank и drift encoder/index: новый фактор — **состав candidate set меняет шкалу сравнения старых кандидатов**.

## N20-01 · P1 · Нелокальная нормализация lexical score в transformer+KNN

**Прямой факт исходника (C).** [`TransformerEmbeddingKnnLogicalUnit.ExecuteAsync`, строки 184–210](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L184-L210) запрашивает HNSW кандидатов с `topK=Math.Max(1,_examples.Count)` и вызывает `ScoreCandidates`; это **не** простой `KnnLogicalUnit` из проекта 21. [`ScoreCandidates`, строки 216–243](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L216-L243):

- `maxLexical = max(1e-9, max(candidate.lexicalScore))` — зависит от того, кто ещё попал в пул;
- `normalizedLexical = clamp(candidate.lexicalScore/maxLexical,0,1)`;
- `fused = 0.45*vectorScore + 0.35*normalizedLexical + 0.20*patternScore`;
- победитель — максимальный `fused`, его `Output` возвращается без дополнительного предметного сравнения.

**Выполненный локальный shadow-контрпример (M-proxy, НЕ native).** Замороженные сырые признаки: A: cosine=0.4, lexical=2, pattern=0, законный ответ; B: cosine=0.7, lexical=0.5, pattern=0, ошибочный ответ. C добавляется без изменения A/B: cosine=-1, lexical=12, pattern=0; C тоже ошибочный. Получилось:

| Измерение | Winner | A fused | B fused | C fused |
| --- | --- | ---: | ---: | ---: |
| A/B, current dynamic max | **A** | 0.6650 | 0.4700 | — |
| A/B/C, current dynamic max | **B** | 0.3733 | 0.3971 | 0.3500 |
| A/B/C, frozen lexical scale=2 (контрфакт) | **A** | 0.6650 | 0.4700 | 0.3500 |
| A/B/C, C.lexical=0.01 (placebo) | **A** | 0.6650 | 0.4700 | 0.0018 |

Все четыре результата получены локальным Node.js v22 через [скрипт формулы](20-MICRO-FUSION-CANDIDATE-SET-PROXY.mjs) и [сырые результаты](20-MICRO-FUSION-CANDIDATE-SET-PROXY-RESULT.json). Использованы **заранее заданные synthetic lexical/cosine значения**, а не реальные embedding или вычисленный native BM25. Логическое свойство формулы подтверждено, наличие такой конфигурации в RP01 и пользовательский ущерб **не установлены**.

**Фальсификатор качества (H).** На одном frozen embedding/tokenizer/index candidate snapshot, реальных 12 source-disjoint queries сравнить current dynamic normalization против заранее выбранного масштаба lexical-score на **тех же exact raw features и том же candidate set**. Поведение должно измениться у тех случаев, где действительно появлялся high-lexical distractor. Если native scores/answer не меняются на изученных корпусах, H20 не подтверждена для них; если нормализатор меняет rank, но корректность ответа не меняется — обнаружена только rank instability, не деградация обучения.

**Branch archaeology:** тот же участок scoring-формулы обнаружен в `language_translate@f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` [старый код, строки 176–202](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L176-L202). При прямом сравнении текста функции формулы совпадают. Поэтому это **не доказанная новая регрессия** при RP01: seam унаследован и мог быть невидим на малом наборе или на ином consumer. Также не доказано, что ранние E09/E34 использовали этот конкретный scorer.

## N20-02 · P1 производительности / P2 качества · Повторный расчёт всего BM25-корпуса внутри каждого соседа

**Прямой факт исходника (C).** В [`ComputeBm25LikeScore`, строки 246–281](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L246-L281) для **каждого** кандидата повторно выполняются: `_examples.Values.Select(example=>Tokenize(example.Text)...).Average()` и, для каждого совпавшего query-root, `_examples.Values.Count(example=>Tokenize(example.Text).ContainsKey(token))`. Вызывающий `ScoreCandidates` делает это по всему найденному пулу; при возврате N соседей и q совпавших query roots в худшем случае число повторных токенизаций порядка **N²(1+q)**. Для N=500, q=8 это до **2 250 000** повторных токенизаций документов на один запрос, **теоретический предел**, не измеренная нагрузка и не обещание, что поиск вернёт все 500.

**Отличие от N20-01:** здесь не обязательно меняется лучший ответ; риск — затраты CPU/latency/timeout и «обучение не дошло до пользователя» при росте числа примеров. Эффект в обычном consumer **пока H**. Отдельный фальсификатор: на одном frozen индексе/корпусе размера 16/32/64/128 сопоставить текущий scorer с test-only scorer, заранее кэширующим токены, document frequency и average length **при том же corpus revision**. Требовать точной parity всех raw/fused ranks/output и сравнить wall/CPU/tokenization calls. Нельзя менять формулу, список соседей или learner одновременно. Если latency и timeout не улучшаются существенно — не приписывать этому обучающую деградацию.

## Что это НЕ дублирует

- [09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md) уже установил, что историческая E34 формула и нынешняя фиксированная 0.45/0.35/0.20 **разные**, а E34 baseline и enhanced сравнивались на разных выборках. Мы **не** открыли E34 повторно: N20-01 — зависимость ранга прежних кандидатов от появления третьего кандидата *внутри нынешней формулы*.
- [16-LIVE-ENCODER-INDEX-VERSION-DRIFT](16-LIVE-ENCODER-INDEX-VERSION-DRIFT.md) проверяет **изменение embedding encoder/LoRA или 64-token truncation**; N20-01 сохраняет encoder и все старые векторы.
- [14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS](14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md) уже содержит duplicate IDs и HNSW RNG. Здесь нет нового claim об этих дефектах.
- [18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md) доказал различие Unit consumer и `GenerateAsync`. N20 вывод о RP01 **строго условный**: пока нет трассы `actual_rp01_user_call→this_TransformerEmbeddingKnnLogicalUnit.ExecuteAsync`, связь с RP01 main500 ENRU500 отсутствует.

## Приоритетное решение для архитектора

1. **Сначала** [текущий вход MICRO-FUSION-20](20-MICRO-FUSION-CAUSAL-EXPERIMENT-AND-CODEX-WORK-ORDER.md): сверить saved P115 и нынешний product cache, не повторяя PRIMARY. Дальнейшая native арифметика H20 возможна только в отдельно назначенном срезе на фиксированных сырых кандидатах, без500DPO и Windows CI.
2. Только если на native scorer произошло изменение rank **и** correct-vs-wrong, провести 12–16 source-disjoint случаев на реальной эмбеддинг-модели; отрицательный контроль с переставленными training outputs и три разных статуса: `RANK_MECHANISM`, `HELDOUT_QUALITY_EFFECT`, `CONSUMER_EFFECT`. Не выбирать scale по sealed heldout.
3. Если обычный RP01 consumer не использует этот Unit, перенести owner исправления в соответствующий RP02/learned retrieval, а RP01 root-cause оставлять `NOT_PROVED`.
4. Повторный BM25-пересчёт уже вынесен в product snapshot. Следующая работа по N20-02 — приёмка этого владельца: actual corpus/trusted revision/однозначный hash, полные сравнимые ранжирования и ответы, cold/warm cost и invalidation при изменении корпуса. Второй cache implementation не назначается.

## DPO роли (не менять существующую ответственность)

**Выбор учебного случая и роль человека исполняет CODEX на Astra 6 UltraCode.** **Обе финальные стороны `chosen` и `rejected` генерирует разрешённая внешняя открытая китайская модель.** **Отдельную проверку качества каждой точной пары делает CODEX, также на Astra 6 UltraCode.** Независимая проверка способности Student после обучения — ещё одна отдельная задача. В N20 shadow эксперименте DPO **не создаётся**: его заранее заданные A/B/C — только локальные синтетические численные признаки; они не заменяют Teacher/CODEX и не получают статуса готовых DPO.

## Авторское bounded review / статус

- **Architecture:** маршруты `TransformerEmbeddingKnnLogicalUnit`, `KnnLogicalUnit`, DPO Student и `TextGenerator` различены; нет ложного вывода «RP01 сломался именно здесь».
- **Code:** линии исходников сверены, формула не подменена исторической E34, cross-branch parity scoring-подстроки проверена.
- **Planning:** один фактор, контроль отвлекающего примера, sealed quality metric и budget определены; вероятность улучшения пока неизвестна.
- **Independent review:** `NOT_PERFORMED`. Native .NET, heldout quality, actual ordinary consumer: `NOT_RUN`. Только JS formula shadow `EXECUTED`. Публикация документа в GitHub не повышает эти статусы.


## Сверка с параллельным N20 после публикации (дедупликация)

**После публикации сверено с параллельным N20**, уже внесённым в общий README: [20-NEW-FINDINGS-FUSION-MEMORY-GROWTH-2026-10-16](20-NEW-FINDINGS-FUSION-MEMORY-GROWTH-2026-10-16.md), [20-MICRO-FUSION-POOL-INVARIANCE-EXPERIMENT](20-MICRO-FUSION-POOL-INVARIANCE-EXPERIMENT.md), [parallel JS probe](20-FUSION-CANDIDATE-NORMALIZATION-PROXY.mjs). Это **тот же тип source-механизмов** (зависимость maxLexical от кандидатов и дорогой пересчёт BM25), хотя независимо выбранные synthetic числа, контрольные arms и raw logs различаются. **Не считать наш N20 двумя дополнительными уникальными дефектами** и не удваивать доказательства в сводке. Наш отдельный воспроизводимый numeric fixture остаётся независимой контрольной репликацией, не empirical learning result.

Действительно отдельное от существовавших сравнений прямое наблюдение в этой сессии: [N21 — операторы исчезают из допуска учебного случая до Unit и из проверки heldout](21-NEW-GAPS-SYMBOL-BLIND-LESSON-ADMISSION-AND-DIRECTION-MISROUTING-2026-10-16.md). Возможная причина RP01 Student по обеим линиям остаётся NOT_PROVED.
