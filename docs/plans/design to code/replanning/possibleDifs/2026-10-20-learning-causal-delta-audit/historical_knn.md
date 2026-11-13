# Историческое обучение, KNN и использование обученного состояния: проверка первичных доказательств

Дата проверки: 2026-10-20. Это независимый разбор сохранённых исходников и результатов, без нового обучения и без изменения core или protected experiment-папки. Статус выводов: **CORROBORATION / REFINEMENT** существующих гипотез; новые гипотезы из этого ограниченного исторического блока не заявлены. В частности, E07 tier/alignment/timing, E09 target codec / unmatched panel, P55.3 morphology, P25.3/4 cache и P51 normalization уже описаны другими агентами.

## 1. Вывод для сводного отчёта

Успешное обучение в первичных артефактах есть. Самое сильное проверенное свидетельство — обновление памяти настоящего Executor и последующее изменение ответов обычного Flow: **0/24 → 19/24** в P125, с контрольным отсутствием обучения и с испорченными учебными targets. Отдельное исправление доставки `phrase_mappings` в настоящий учебный вход даёт **11/24 → 19/24** после равных восьми уроков. Это два разных опыта; их нельзя объединять в одно A/B или описывать как улучшение нейронного DPO. В обоих опытах neural updates = 0, но реально меняется обучаемая память, которую читает обычный потребитель.

Архив E25 подтверждает улучшение классификации порядка POS последовательностей **0.3438 → 0.8750**, вместе с замедлением prediction **0.4 → 15.4 ms**. Архив E43 подтверждает улучшение удержания старых переходов в frequency-memory **0.3226 → 0.3978**. Эти доменные выигрыши нельзя обесценивать тем, что механизмы не являются градиентным обучением.

E07 и E09 действительно используют структурную информацию по-разному. Отрицательные E07 не проверяли некоторые объявленные поля и содержат несовместимое сопоставление BPE ordinal с символьными позициями. E09 хранит реальные обучающие пары с признаками непосредственно из Roslyn AST и даёт положительные сохранённые показатели. Однако **точная историческая линия «этот же алгоритм из неудачной матрицы был перенесён в KNN и исправил тот же экзамен» пока не установлена**. Для такой линии нужны конкретный donor/commit/caller и сопоставимые учебные данные, labels и exam. Это граница имеющихся доказательств, а не отрицание пользовательского свидетельства о других успешных ветках.

## 2. Воспроизводимая привязка к исходникам

| Источник | Зафиксированная версия | Что прочитано |
|---|---|---|
| `llm_mvp_core/DesignToCode` | `8f20ceb8a9bfbadab2ad309226c47dd76e714548` | Полные E07, E09, E25, E43, StructuralKnn, сохранённые May1 и May7 JSON |
| `llm_mvp_core/language_translate` | `f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` | Полный `12.SemanticCompiler/Translation/PatternClassifier.cs` |
| Текущий `llm_mvp_core/codex/dpo-implementation` | `ab583032562d6656de9dd5ca6fa0a325fd4b1f30` | TranslationMachine, Executor, contracts, Tools reader/builder; retained P125, ingress и cold-restore reports |
| Новизна относительно planning | `f73d229c2810219e6b8e7b9cb4d4ec7565a6f0c9` | Поиск в локально загруженных `possibleDifs`, включая audit12 и новые 2026-10-20 пакеты; 805 файлов на момент заключительного поиска, поэтому окончательный exhaustive dedup выполняет root по полному manifest |

Git tree `35.ExperimentsV2` в обеих исторических ветках один: `aa5c7d5d6f3a1ecb1f221755dae61f03141d62da`. Это подтверждает сохранение данного корпуса code/results в соседних ветках. Совпадение subtree не доказывает использование этих классов текущим RP01.

Метод: прочитаны native writer/state/reader и соответствующие сохранённые JSON; для P125 и ingress независимо пересчитаны raw outputs по опубликованным gold и правилам scorer. Результат пересчёта сохранён рядом как `historical-recount.json`. Исходный `score.cjs` не запускался, файлы исходных результатов не переписывались. Core skills code-examination, claim-verification, architect-behavior-classifier прочитаны из приложенных исходников.

## 3. E07 → E09: поля, обучение, потребитель, фактические метрики

### 3.1. Сравнение реальных вычислений

| Элемент | E07 StructuralContextToken | E09 StructuralKnn |
|---|---|---|
| Учебный вход | BPE vocab 512, обученный на combined source; затем BPE sequence | Roslyn `DescendantNodes` отдельно каждого файла |
| Target | Следующий BPE id | Локальный ordinal `SyntaxKind` следующего AST node |
| Обученное состояние | Dictionary transitions с ключом `(BpeId, AstNodeType, AstDepth)` | Список `(StructuralVector, NextToken)` |
| Получение структуры | `posMap[i]`, где i — номер BPE token, а posMap построен по sampled char positions | `StructuralVectorExtractor.Extract(node, fileId)` непосредственно из текущего AST node |
| Решение | Точное совпадение composite key и fallback | Взвешенная structural distance, k=5, majority vote |
| Признаки, реально читаемые решением | BPE id, node type, AST depth | SyntacticRole, Parent1/2/3Type, AstNodeType, PositionInParent, ScopeDepth, DepthInMethod |
| Поля, которые нельзя считать проверенными в этом сравнении | AstParentType, FileId, ScopeDepth и прочие поля вне composite key; tier только меняет note | SiblingCount и FileId заполнены vector extractor, но не входят в `DistanceTo` |
| Экзамен baseline/enhanced | Одинаковый val BPE sequence внутри одного Run | baseline весь val; enhanced `val.Take(500)` |

Источники: [E07 Run и timer boundaries](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/35.ExperimentsV2/Experiments/E07StructuralContextToken.cs#L31-L126), [E07 token→AST alignment](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/35.ExperimentsV2/Experiments/E07StructuralContextToken.cs#L133-L228), [E07 actual train/predict key](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/35.ExperimentsV2/Experiments/E07StructuralContextToken.cs#L278-L318), [E09 Run, samples, local kindMap](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/35.ExperimentsV2/Experiments/E09StructuralKnn.cs#L15-L120), [StructuralVector distance/extractor](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/12.SemanticCompiler/Prediction/StructuralKnn.cs#L8-L97), [KNN Train/Predict](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/12.SemanticCompiler/Prediction/StructuralKnn.cs#L100-L163).

В E09 имеется дополнительное различие между experiment caller и reusable API: E09 `BuildTokenVectors` использует **per-file kindMap ordinal**, а `StructuralKnnPredictor.TrainFromSource` записывает `NodeTypeId = SyntaxKind % 200`. Поэтому перенос результатов E09 на другой caller требует явного совпадения target codec; одинаковое имя predictor этого не гарантирует. Признаки node/parent тоже кодируются modulo 200. Это конкретизация уже известного P27.4, не новая гипотеза.

### 3.2. Раздельные сохранённые запуски

| Точный run | baseline → enhanced | Время baseline → enhanced | Объём / ограничение |
|---|---:|---:|---|
| [E07 May1-001](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/35.ExperimentsV2/results/E07-STRUCTURAL-CONTEXT-TOKEN/results/run-2026-05-01-001.json) | 0.1531 → 0.1462 | 305.4 → 20.6 ms | tier1; 2158 BPE; 1269 unique contexts |
| [E07 May1-002](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/35.ExperimentsV2/results/E07-STRUCTURAL-CONTEXT-TOKEN/results/run-2026-05-01-002.json) | 0.1502 → 0.1310 | 2187.1 → 26.3 ms | tier2; 17718 BPE; 8357 unique contexts |
| [E07 May7-001](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E07-STRUCTURAL-CONTEXT-TOKEN/results/run-2026-05-07-001.json) | 0.1291 → 0.1221 | 170.8 → 4.2 ms | tier1; 2133 BPE; 1265 contexts |
| [E09 May1-001](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) | 0.2413 → 0.3507 | 14.5 → 763.5 ms | 5617 nodes; 4492 train pairs; k5 |
| [E09 May7-001](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E09-STRUCTURAL-KNN/results/run-2026-05-07-001.json) | 0.2593 → 0.3607 | 5.8 → 635.0 ms | 5806 nodes; 4643 train pairs; k5 |

Все quality числа в этой таблице — сохранённые aggregate receipts, не независимый replay. У E09 положительный наблюдаемый сигнал поиска по AST сохраняется; величину нельзя назвать чистым paired-effect, пока оба алгоритма не оценены на тех же indices/labels. У E07 baseline timer стартует **до** BPE Train/Encode и AST подготовки; enhanced timer — только перед MultidimensionalNGram.Train. Эти времена не измеряют одинаковую работу. Нельзя засчитывать физическое ускорение на их отношении. Это правило сопоставимости не отменяет пользовательского правила признания полезного выигрыша любой метрики.

May1 E07 tier1 и tier2 имеют различный corpus size; их нельзя представлять как добавление одного поля `ast_parent`. Внутри исходника tier не меняет key, следовательно заключение «parent/scope испортили качество» не поддерживается. Нельзя также переносить времена 198.8→7.2 из другой retained серии в строки выше.

### 3.3. Какое уточнение остаётся для существующих карточек

Это вход для существующих P27 / E-MATRIX-KNN, а не новый исполняемый эксперимент: фиксировать dataset, train/val pair IDs и codec; отдельно исправлять один из seams (alignment, реально активный key, label codec, matched exam); времени подготовки, обучения, prediction дать отдельные одинаковые области. Сначала локальная обучающая проба в существующем experiment project, затем документированный кандидат. Никакая запись этого отчёта не разрешает изменять production или protected cards.

Применимость: AST scopes/parents/roles относятся к code domain; POS order — к переводу и языковой структуре; для доменов без синтаксического дерева не обоснована обязательность Roslyn-признаков. Domain-специфический выигрыш остаётся успехом именно своего домена.

## 4. E25 и E43: что действительно было выучено

### E25 POS pattern classification

[`E25PatternClassifier.Run`](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/35.ExperimentsV2/Experiments/E25PatternClassifier.cs#L14-L58) генерирует 160 examples восьми POS patterns, делит 80/20, обучает 128, сравнивает правила и KNN на тех же 32 validation examples. [`PatternClassifier.Train/Predict/SequenceDistance`](https://github.com/lubochka/llm_mvp_core/blob/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/12.SemanticCompiler/Translation/PatternClassifier.cs#L9-L110) сохраняет реальные примеры, использует Jaccard distance между **множествами POS bigrams** и k3 majority vote. Это частичный сигнал порядка, не полное дерево и не полная память произвольной последовательности.

[May1 receipt](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json): accuracy **0.3438 → 0.875**, hard-pattern accuracy **0.8571** на 28 hard cases, prediction time **0.4 → 15.4 ms**. `clf.Train(train)` выполняется до enhanced timer: названия timing нельзя превращать в время обучения. Синтетические templates и optional ADJ insertion поддерживают узкую классификационную способность; этот опыт не измеряет итоговый EN/RU текст и сам по себе не доказывает качество обычного translator. Выигрыш классификации подтверждается на уровне retained receipt и native training/evaluation algorithm, не нового replay.

### E43 replay старых переходов

[`E43ReplayBuffer.Run`](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs#L15-L96) делит файлы на old/new domain, обучает frequency bigrams, сравнивает rebuild(new) и rebuild(new+replayed oldTrain) на одном `oldVal`. Это память частот, а не сохранение нейронных параметров при optimizer step. `replayCount = int(newPairs.Count * .3/.7)` затем ограничен фактической длиной `oldTrain`; в большом new-corpus заявленные 30% replay не гарантированы. Cross-file zip и replay cap уже известны в P116/N63.

[May1 receipt](https://github.com/lubochka/llm_mvp_core/blob/8f20ceb8a9bfbadab2ad309226c47dd76e714548/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-05-01-001.json): retention accuracy **0.3226 → 0.3978**, oldPairs465/newPairs414/replay177, time **0.1 → 5.8 ms**. `retention_rate = 1.1562` — отношение к старой base accuracy, не доля сохранённых нейронных весов. Сохранённый receipt полезен для replay frequency-memory; перенос на DPO требует отдельного измерения реального DPO learner.

## 5. Проверенная текущая цепочка урок → память → обычный ответ

### 5.1. P125-enru-memory-01 — реальное приобретение способности

[Retained report](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/report.md), [raw](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/raw-outputs.json), [gold](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/panel-gold.json), [scorer](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/score.cjs).

| Независимое состояние | До | После | Смысл контроля |
|---|---:|---:|---|
| A, без обновлений | 0/24 correct, 0 answered | 0/24, 0 answered | Нет спонтанного успеха от самого экзамена |
| B, 8 правильных Executor lessons | 0/24, 0 answered | **19/24, 22 answered** | Реальное приобретение exact/near/composition способности |
| C, те же 8 lessons с переставленными validate/delete targets | 0/24, 0 answered | 13/24, 22 answered | Ответы зависят от содержания учебных targets |

Всего 144 ordinary Flow ответа: три состояния × before/after × 24. Два training batch calls, 16 обучающих rows, neural updates0. B_after: exact8/8, near3/3, новые сочетания известных action/object8/8, contrastive/edge0/5. Это узкая композиция изученного словаря, не доказательство всех видов генерализации. Ошибочные принятые ответы B: cc01 отрицание, cc03 два объекта/союз, cc04 вопрос→команда. У C дополнительно испорчены x01,x02,n01,n02,ev01,ev02.

Потребитель видит реальные trained maps: [`TranslationMachine.TrainStructuredBatchAsync`](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/27.TranslationDomain/TranslationMachine.cs#L89-L145) вызывает Unit trainer; [`ExecutorUnit.TrainOnExample`](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/27.TranslationDomain/TranslationTrainedUnits.cs#L403-L455) пишет exact map, KNN, word/phrase maps; [`ExecutorUnit.Execute`](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/27.TranslationDomain/TranslationTrainedUnits.cs#L253-L400) читает их. Internal receipt `AccuracyBefore=0, AccuracyAfter=0` не опровергает 19/24: при отсутствии heldout current trainer вызывает `MeasureUnitCoverage`, а не этот ENRU panel scorer. Нужно хранить названия обеих метрик, не подменять одну другой.

### 5.2. translation-memory-ingress-01 — обучение зависит от loader

[Retained report и полный before/after разбор](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/report.md). Независимый raw пересчёт подтвердил:

| Метрика на равных 8 lessons и 24 exam cases | Старый reader/builder | Reader/builder с доставкой phrase_mappings |
|---|---:|---:|
| Correct / все | 11/24 | **19/24** |
| Новые action/object сочетания | 0/8 | **8/8** |
| Answered | 12 | 22 |
| Accepted wrong | 1 | **3** |
| Correct / answered | 11/12 = 91.7% | 19/22 = 86.4% |
| Exact/phrase обученные entries | 8 / 0 | 8 / 16 |

Оба состояния обучены, control factor — доставка уже существовавшего optional input field. Реальный текущий исправленный участок: [`ReadTranslationCorpusRowsAsync`](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/27.TranslationDomain.Tools/Program.cs#L4442-L4470) читает `phrase_mappings`; [`BuildTranslationContinueExample`](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/27.TranslationDomain.Tools/Program.cs#L4682-L4710) передаёт `row.PhraseMappings` в `ExecutorOutput`; затем настоящий TrainStructuredBatchAsync/Executor.TrainOnExample и GetOrchestratorAsync().RunAsync. На current pin этот transport уже исправлен: нельзя регистрировать его как ещё отсутствующее изменение.

В старом плече cc03 был ошибкой единственного объекта. После исправления cc03 сохраняет оба объекта в «проверь файл кэш», но strict scorer по-прежнему не принимает формулировку; дополнительно cc01 и cc04 начали приниматься с потерей смысла. Это одновременно конкретный успех composition accuracy и ухудшение false accepts/precision. Их следует показывать рядом; добавочные ошибки не отменяют улучшение 8 новых комбинаций.

Отчёт получил before на старой Tools DLL и after на новой при той же Domain DLL. Метрики времени обучения 38.5685→34.5068 ms и 24 ответов291.0535→52.9254 ms — одиночные наблюдения разных процессов. Их можно сохранить с этой границей; нет оснований представлять их как устойчивый коэффициент ускорения. Состояние увеличилось7515→8954bytes. Максимумы наблюдённых сумм RAM66,539,520→82,214,912bytes имеют неодинаковое число samples5vs2. Строго измеренная работа затрагивает общие private reader/builder через reflection; полный CLI с registry/manifest не запускался. Это не RP01 ResearchCheckpointEnRuAdapter и не его нейронный train path.

### 5.3. P25-enru-cold-01 — использование восстановленного результата обучения

[Cold report](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/report.md), [comparison-summary](https://github.com/lubochka/llm_mvp_core/blob/ab583032562d6656de9dd5ca6fa0a325fd4b1f30/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/comparison-summary.json): canonical states равны, 24/24 ответов идентичны, correct19, answered22. Canonical SHA одинаковый для source/loaded/after: `8b03a483d5d32385fb22efdd164f633efc2ae92ca88813ac1f50a88f0c573305`.

Это допустимая проверка зависимого участка уже доказанного обучения: сохранённый learned state после загрузки даёт прежнее поведение. Новое обучение здесь не требуется. Receipt подтверждает удержание и известных ошибок, а не улучшение качества. В этой независимой проверке прочитан retained comparison summary; отдельный пересчёт всех cold raw rows не выполнялся. Не выдавать данную степень проверки за raw replay.

## 6. Новизна и рекомендации для сводного покрытия

| Проверенный кандидат | Статус в существующем корпусе | Что можно добавить без дубля |
|---|---|---|
| E07 tier не действует, token/AST alignment, timer asymmetry | KNOWN: audit12; 2026-10-20-current-learning-audit/routing/HISTORY; 61-LEARNING-UNBLOCK.../reviews/CONCURRENT-KNN-RECONCILIATION | Привязка конкретных May1/May7 receipts, не смешивать годы/объёмы/время |
| E09 kindMap / val.Take(500) / другая TrainFromSource кодировка | KNOWN P27.4 / audit12 | Caller-specific codec refinement и matched panel prerequisite |
| Russian -л/-ла alternate без POS в Executor | KNOWN P55.3 | Не переименовывать в новую morphology гипотезу |
| NormalizeMapKey удаляет meaningful punctuation/operators | KNOWN P51 family | Нужен domain-specific experiment, а не общий string-test |
| Live checkpoint cache alias / mtime-length identity | KNOWN P25.3/P25.4 | Тест только через retained learned state и реальный consumer |
| P125 / phrase ingress / P25 cold | KNOWN P125.1/P25; успешные retained опыты | Независимый raw recount, конкретные gains+regressions, already-fixed transport |

Для будущих работ первый приоритет — разблокировать реальный neural/structured learning path и исправить конфликтующие инструкции в карточках. Этот отчёт не предлагает заново выполнять прошедшие P125/ingress/cold или продолжать их оформление после указания перейти к следующему эксперименту. Новые исследования допускаются отдельным явным поручением. Любая карточка, использующая эти evidence, должна наследовать пользовательские правила: локальный полезный выигрыш признаётся успехом, реальные ухудшения рядом, prod остаётся кандидатом до решения пользователя, модель и команды фиксированы, отчёт по-русски, после записи результата немедленно завершить текущий опыт и перейти только к назначенному следующему; «остановиться» означает остановиться без дополнительных действий.

## 7. Сверка с публикацией planning a868f46 (ограниченный follow-up)

После подготовки отчёта прочитаны три новых документа на planning `a868f46ee3695728406173f8cd945e278ddb0def`: [knn-history.md](https://github.com/lubochka/xiigen-planning/blob/a868f46ee3695728406173f8cd945e278ddb0def/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-candidates-and-causal-gaps/knn-history.md), [knn-novelty-ledger.md](https://github.com/lubochka/xiigen-planning/blob/a868f46ee3695728406173f8cd945e278ddb0def/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-candidates-and-causal-gaps/knn-novelty-ledger.md), [02-translation-and-restoration.md](https://github.com/lubochka/xiigen-planning/blob/a868f46ee3695728406173f8cd945e278ddb0def/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-candidates-and-causal-gaps/02-translation-and-restoration.md). Существенных противоречий с настоящим отчётом не найдено.

- E25 время0.4→6.6ms в новом пакете относится к **7 мая**, а0.4→15.4ms здесь — к **1 мая**. Это разные receipts, исправлять или смешивать числа не требуется. E09 May7 значения совпадают; границы E07→E09 causality совпадают.
- Новый пакет независимо сохраняет P1250→19/24, ingress11→19/24 с false accepts1→3 и cold24/24 retention. Статус ingress у него **SOURCE_ALREADY_PRESENT**, совпадает с проверенным current reader/builder в разделе5. Повторная правка транспорта не нужна.
- `KH-COST-01` (два ParseText при подготовке одной FileAction training row) теперь уже опубликован другим агентом как узкая новая гипотеза. В нашем сводном аудите его следует учитывать как существующий адресный кандидат, не объявлять своей новой находкой. `KH-EXT-01` (sparse replacement очищает ранее выученные MethodKnn pairs) там прямо обозначен уточнением известного LT-N02; actual repeated RP01 caller не доказан.
- В новом историческом документе есть дополнительные прямые ссылки на May7 receipts E13/E14/E18/E22 и на независимые P125/P25 review. В этом коротком follow-up сами новые первичные файлы не перечитывались и их показатели не добавлялись к нашим самостоятельно проверенным данным. Новые source links не доказывают новый запуск или exact matrix→KNN lineage.

Core `65d03a73a76810a0425a41c33d3c7eeaf4cdbc48` сообщён root как последующий docs-only head; данный отчёт сохраняет immutable ссылки на реально прочитанный source pin `ab583032…`. Новый native run не выполнялся.
