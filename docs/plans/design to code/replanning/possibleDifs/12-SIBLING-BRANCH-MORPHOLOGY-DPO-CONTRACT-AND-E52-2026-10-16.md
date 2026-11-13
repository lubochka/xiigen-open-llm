# 12 — Новая сверка соседних веток: морфологический KNN, DPO-формат и неподтверждённая learning-метрика

**Дата:** 2026-10-16. **Статус:** NEW-BRANCH-FORENSICS / C+M+H. Исследованы публичные remote-ветки, а не только прошлые RAG и главная ветка.

| Источник | Пин | Роль доказательства |
|---|---|---|
| `codex/dpo-implementation` | `83590a462b2f8aedc9d3ab4cb775193eb9c25ce5` | Действующий DPO/data/vector и исторические ExperimentV2 implementations |
| `language_translate` | `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` | Старые морфологические алгоритмы и сохранённые successful E51 runs |
| `external_domains` | `3036a01acec938f0b83fa4ffbda26202cc35d639` | Длинные старые DPO/teaching guides и специализированные external domain пробы; **полный аудит ещё не выполнен** |
| `orchestrator-extensions` | `b293045bd89b1eb96d3e32fedd341f1a4170adbc` | Другой `DpoDataset` contract, design-conversion handoff и эксперименты; ветка существенно старше, wholesale merge запрещён |

## 1. N12-01 — Морфологический векторный механизм есть в соседней ветке, но не на указанных путях канона

В `language_translate` имеются [`01.Tokenizer/MorphFamilyIndex.cs`](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/01.Tokenizer/MorphFamilyIndex.cs#L1-L106), [`01.Tokenizer/RussianStemmer.cs`](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/01.Tokenizer/RussianStemmer.cs#L1-L92), [`01.Tokenizer/HebrewRootExtractor.cs`](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/01.Tokenizer/HebrewRootExtractor.cs#L1-L79) и [`06.VectorSearch/MorphDimensionAppender.cs`](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/06.VectorSearch/MorphDimensionAppender.cs#L1-L65).

`MorphFamilyIndex` строит семейства по английскому, русскому или ивритскому stem/root, по умолчанию из frequency source. `MorphDimensionAppender.Append` к прежнему embedding добавляет одну координату: `index.GetNormalizedFamilySize(word)`. По прямому GitHub contents read на `codex/dpo-implementation` **оба одноимённых пути `MorphFamilyIndex.cs` и `MorphDimensionAppender.cs` возвращают 404**. Это подтверждает отсутствие точных файлов в текущем дереве **по этим путям**, но не отсутствие аналогичной функции во всём core, не её применение в прежних успешных runs и не причину RP01 без runtime trace. Полная история конкретных caller-и-merge оставлена **U**.

**Новый риск N12-01 (S+C, causality H):** исторический раздельный morphological feature channel не попал в текущий consumer и при переносе «матрица→kNN» могла исчезнуть прежняя возможность группировать словоформы. Требуется искать фактические вызовы `Append/AppendBatch` и сравнивать старый индекс с текущим на одинаковых словах. Не переносить старый код только по наличию файлов.

## 2. N12-02 — Проблема математического дизайна старого MorphDimensionAppender

[Прямой код `MorphFamilyIndex.GetNormalizedFamilySize`](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/01.Tokenizer/MorphFamilyIndex.cs#L83-L110) возвращает **размер семейства / максимальный размер семейства**, а не идентификатор конкретного корня. [`Append`](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/06.VectorSearch/MorphDimensionAppender.cs#L18-L32) добавляет это число как последний компонент.

**Доказанный алгебраический контрпример:** если семейства A и B не имеют общих слов, но содержат по три словоформы, их morph-coordinate строго равны `3 / maxFamilySize`. Даже если A и B обозначают противоположные смыслы, добавочная размерность у них совпадёт. Аналогично все разрозненные OOV с fallback family size 1. Следовательно, этот **единственный scalar не способен кодировать root identity или подтверждать, что именно слова одного корня стали ближе друг к другу**; это лишь характеристика продуктивности семейства, которая может смещать cosine в сторону крупных семейств.

Статус **C — исходный код плюс математический вывод**; ухудшение качества в прежней ветке или RP01 не измерено (**H**). Правильная альтернативная проба: общий frozen stem-ID projection / signed or multi-hot root feature либо purely reranking by shared stem, с контролем ложных совпадений, отрицания и unseen stems. Не вводить новые признаки сразу в DPO gradient.

## 3. N12-03 — Исторический E51A/ E51B и нынешний RP01 использовали разные объекты «обучения»

[Сохранённый E51A](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/experiments-v2/E51A-CODE-CONTEXT-PIPELINE/results/run-2026-05-11-044429.md) показывает 0.205→1.000 predicate overlap на 13 уже встречавшихся `CodeContext`; [E51B](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/experiments-v2/E51B-DYNAMIC-FLOW-MINI-E2E/results/run-2026-05-04-072046.md) — 6/6 trace success с частью заранее подставленных Flow outputs. [Доказательства на уровне отдельных строк — файл 11](11-E51-KNN-ORACLE-SPLIT-AND-POSITIVE-MECHANISMS-2026-10-16.md).

**E52 [архивный 0%→100%](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/experiments-v2/E52-PROJECTS-21-23-CYCLE-COMPARISON/results/run-2026-05-04-085432.md) нельзя считать нейронным gain**: [`BuildEvidence`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/35.ExperimentsV2/Experiments/E52Projects2123CycleComparison.cs#L43-L95) вычисляет долю четырёх `CycleTrace` boolean gates, `BaselineTimeMs` и `EnhancedTimeMs` выставлены в фиксированные 0.1, нет в этом методе optimizer update. Это свидетельство определённых contract paths, не измеренный runtime timing или language skill. Сохранить исторический pass для соответствующего предмета, **не переносить его как меру Student free-answer quality**.

## 4. N12-04 — Разные ветки и import paths не имеют доказанной общей DPO JSONL-схемы

Текущий [`11.DPO/DpoDataset.cs`](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/11.DPO/DpoDataset.cs#L10-L174) принимает поля с точным регистром `Prompt, Chosen, Rejected`, требует full triple additional admission или полное отсутствие, выбрасывает исключение при другой капитализации (`ValidateStrictFieldNames`). [Ветка `orchestrator-extensions`](https://github.com/lubochka/llm_mvp_core/blob/b293045bd89b1eb96d3e32fedd341f1a4170adbc/11.DPO/DpoDataset.cs#L7-L43) в **комментарии** документирует lowercase JSONL `{"prompt","chosen","rejected"}`, а старый `Load` десериализует его напрямую. Сам комментарий **не доказывает**, что старый десериализатор действительно принимал lowercase (property naming может отличаться). Между тем [RP01 simulation importer](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L99-L128) использует отдельный typed path для материала, а не прямое чтение `DpoDataset.Load`.

**Новый риск N12-04 (C+H):** перенос dataset между ветками/загрузчиками может менять refusal, cardinality, provenance или маски, а одинаковая фраза «500 pairs loaded» не гарантирует одной и той же последовательности exact content. Не заявлять, что текущий RP01 реально пострадал без execution trace.

**Дешёвый контроль:** взять **одну** разрешённую DPO пару, сохранить JSONL в lowercase и PascalCase, поочерёдно подать именно в каждый поддерживаемый loader; сравнить принятие, UTF-8 byte hashes, decoded exact tokens, chosen/rejected, per-side authorship и row count; удостовериться, что failed rows не считаются updates. Закрепить явно версионированный canonical converter, не ослабляя admission policy.

## 5. Как классифицировать старые свидетельства и не потерять пользу

| Что восстановить | Чему доверять | Чего пока не утверждать |
|---|---|---|
| `language_translate` morphological grouping | stem/root helper code, hash конкретных файлов | Измеренный его вклад в RP01 |
| E51A | успешный lookup по **известному** коду на перефразированных задачах | unseen-source adaptation и learned language generation |
| E51B | retrieval of LocalSubject и arbiter routing | самостоятельно выученные WorkStatement/Flow outputs |
| E52 | 4 сценария contract acceptance | optimizer learning/time gain |
| DPO JSONL в соседних ветках | несовпадение документированных форматов и текущих проверок | что текущий RP01 загрузил неправильную пару |
| core checkpoint | опубликованная позитивная проверка reopen | запуск именно этого состояния в обычном consumer |

## 6. Почему это относится к большому плану

[WorkLeft-RESEARCH-01, `4](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-RESEARCH-01-training-quality-speed-accuracy-program.md) требует **доменно-зависимую функцию kNN** для переводов EN↔RU, orchestration, Figma, C#, Angular, SQL и отдельного обучения Unit/Flow/Orchestrator/Graph; единый хэш-вектор или single-rank scalar не заменяет всю архитектуру. Исторические результаты фиксируют различия механизма, а не утверждение «любая успешная ветка должна быть слита». Использовать selective algorithm port + paired ablation, а не wholesale merge.

**Ограничения:** отсутствуют live runtime, exhaustive branch call graph и свежие модельные измерения; текущий исходный код и сохранённые исторические документы действительно прочитаны. Все причины деградации RP01 остаются гипотезами до одинакового blind examination.
