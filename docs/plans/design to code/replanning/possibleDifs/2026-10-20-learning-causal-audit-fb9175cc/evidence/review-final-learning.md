# Независимый final review: HNSW карточка и сохранённые показатели

Дата: 20.10.2026. Reviewer: learning_source, default/inherited model. Bounded documentation-only review. Ни один Train, inference, build, test, runtime scorer или helper benchmark не запускался. Проверка JSON — чтение артефактов и сравнение сохранённых полей.

## Вердикт

**PASS_WITH_TWO_DOCUMENTATION_CORRECTIONS.** У HNSW карточки есть настоящий обучающий путь, один численный фактор, фиксированные данные и контролируемые A/B. Подмена полного Train изолированным cosine benchmark не обнаружена. Бюджет согласован. Включённые N63R/RESTORE/SANITY данные сверены непосредственно с raw без численных расхождений.

Перед финальной публикацией желательно закрыть два небольших документных пробела ниже: добавить отсутствующий N63I блок в машинную сверку и уточнить семантическую область Manifest parity. Это не требует runtime или изменений core. Статус новой HNSW карточки должен остаться AUTHOR_SETUP_REQUIRED / NOT_RUN.

Объекты:

- cards/HNSW-QUERY-NORM-REUSE-01.md;
- 02-saved-results-and-candidates.md;
- data/saved-metric-reconciliation.json;

в пакете 2026-10-20-learning-causal-audit-fb9175cc.

## R1. Пропущена машинная сверка N63-INGRESS-LENGTH

**Место:** data/saved-metric-reconciliation.json — отсутствует ключ N63I/N63-INGRESS-LENGTH; 02-saved-results-and-candidates.md:86 — есть только краткая строка кандидата. N63R, RESTORE и SANITY представлены полно, но другое последнее consumed исследование N63I здесь не имеет таблицы своих полезных метрик и ухудшений.

Это пробел покрытия, не обнаруженная ошибка существующих чисел. Нужна добавка из уже прочитанных exact raw, без повторного эксперимента:

- core/experiments-v2/RP01-MICRO/N63I/results/arms.json;
- core/experiments-v2/RP01-MICRO/N63I/results/per-row.json;
- caller core/experiments-v2/RP01-MICRO/N63I/Program.cs.

Проверенные значения:

| Метрика | A-full | B-bounded |
|---|---:|---:|
| Actual optimizer updates | 0 | 2 |
| Actual shifted targets | 0 | 70 |
| Native elapsed ms | 279.9091 | 111.3761 |
| CPU ms | 62.5 | 93.75 |
| Allocated bytes | 81992 | 3330408 |
| Состояние | FAILED_NO_STATE_CHANGE / TRAINING_EXAMPLE_CONTEXT_EXCEEDED | COMPLETED / changed |
| Отложенные строки B | — | train-long |

Начальный hash обоих arms одинаковый: f337cb5126342e0744af820227c5b760fcc9c4a96cca748345a5f487cd64e3d7. Это random S0, не готовая выученная модель.

В B mean response NLL меняется:

| Строка | S0-before | S1-after |
|---|---:|---:|
| train-one | 6.554350670659838 | 6.550180174020116 |
| train-two | 6.636790247616372 | 6.634059538608386 |
| heldout-three | 6.699461780801719 | 6.697845993180661 |
| heldout-four | 6.528147523227716 | 6.5275596130608395 |
| control-branch | 6.566935850077769 | 6.565852480334515 |

Correct response tokens всех пяти scored rows остаются0; full teacher-forced exact=false. train-long не scored и не имеет выдуманного нулевого NLL. Общий count20 scoring forwards. Успех — разблокирование настоящих updates/targets и отдельные улучшения NLL. A ничего не обучает, поэтому меньшее elapsed B не означает ускорение одинакового учебного объёма. Рост CPU/allocations показывается рядом. №08 остаётся consumed; NEXT10 не меняется.

## R2. Уточнить Manifest parity в HNSW карточке

**Место:** cards/HNSW-QUERY-NORM-REUSE-01.md:190, строка «index Count4→8, ordered vectors/IDs/graph, manifest4→8; A0↔B0 и A↔B byte/hash parity».

Native LogicalKnnModelManifest содержит TrainingTimeMs и ManagedAllocationBytes (core/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs:23–35). Эти два значения измеряют расходы и по смыслу могут различаться между A/B. Если исполнитель прочитает фразу как требование byte/hash равенства всего manifest, полезный опыт может быть ошибочно помечен INVALID_EQUIVALENCE именно из-за измеренной разницы затрат.

Минимальная документная правка: полная byte/hash parity относится к сохранённому index, исходным inputs и actual predictions/confidence/evidence. У manifest сравниваются identity/config/count/tokenizer/index-kind/ingredients, а TrainingTimeMs и ManagedAllocationBytes сохраняются и сравниваются как метрики стоимости, исключённые из semantic parity. Остальные правила parity сохраняются. Исходник менять не нужно.

## Что проверено положительно в HNSW контракте

1. Native KnnLogicalUnit.Train действительно выполняет BpeTokenizer.Train и HnswIndex.Add по каждому уроку (KnnLogicalUnit.cs:132–154). Примеры не заменены ready constants; before — Train4, after — новый Train8.
2. API Train и ExecuteAsync, поля LogicalKnnTrainingExample/LogicalUnitRequest соответствуют прочитанным current files. LogicalContracts prefix до marker содержит необходимые flags/request/result/PhaseTransitionSignal/ILogicalUnit; Domain type назначен из конкретного source.
3. GreedySearch содержит2 cosine call sites, SearchLayer2, SelectNeighbours1. Proposed helper сохраняет порядок dot/nb accumulation и denom threshold исходного Distances.CosineSimilarity. Изменение querySquaredNorm касается реального Add пути, включая потенциальный reverse-pruning caller.
4. A/B имеют одинаковые fixtures, common seed7331 и telemetry. Два свежих процесса проходят Train4→Train8, запросы HNSW не расходуют RNG. Fixed seed указан как общее экспериментальное исключение, не product fix.
5. Бюджет верен: два Train на arm, всего4; (4+8)×2=24 presentations/Add; 24×2states×2arms=96 Execute; 4 Save. Отдельный warmup и repeated timing отсутствуют. Warm Train включён в бюджет.
6. Ненулевой gate применяется к actual exact старым четырём урокам после настоящего Train4. Он не рисует output из gold. Это readback маленькой выученной памяти, и карточка не называет его held-out обобщением.
7. Scorer явно отличается от исторического P125 semantic scorer. P12519/24 не присваивается новому generic Unit. Новая карточка не утверждает существование полного checkpoint continuation API.
8. Главная стоимость — весь native Train; Add timing и computed norm-elements отделены. Derived arithmetic counters не объявлены CPU measurement. Single A→B timing не выдаётся за устойчивый причинный speedup.
9. Восьми узлов недостаточно для reverse-pruning coverage; это ограничение сохранено. Mutable-alias/concurrent mutation контракт не объявлен исправленным.
10. Статус AUTHOR_SETUP_REQUIRED честный; будущие host/commands обозначены как будущие, а не существующий CLI. Source-copy location, templates, native data и stop/conflict/report rules назначены. STOP/NEXT запрет есть в карточке и встроенном prompt.

## Что сверено непосредственно с raw для root metric docs

Программная read-only сверка выполнила50 точных сравнений сохранённых объектов/полей; расхождений0:

- N63R schedule полностью; все scalar поля трёх arms; все40 row readouts пяти panels по correct response tokens, знаменателю, teacher-forced exact и response NLL.
- RESTORE весь measurement объект; весь restoreVerification; равенство панелей afterPriorTraining и afterRestore.
- SANITY model_config, checkpoints и все readouts.

Таблица N63R в02 соответствует raw: A07 WA22→A2/B15/C4; EO02 WA15→A1/B15/C3; A01 WA23→A2/B1/C2; N01 A22/B18/C24; N02 A28/B25/C28. Цены и targets62800/117600/94200 верны. Формулировка C=equal updates, но не equal tokens/material, правильна. Заявленного причинного удешевления нет.

RESTORE TRAIN3/40→4/40, NLL4.766696420187372→4.711598363502413, rank51.825→50.05 и CONTROL NLL regression корректны. Сохранён import/step факт, но отдельное побайтовое m/v post-import сравнение не заявлено. Нет подмены двух строк с любым верным токеном двумя полными ответами.

SANITY NLL/exact соответствуют raw. Constant-label baseline1/2 и отсутствие готового H64 checkpoint указаны; положительная ограниченная траектория не стирается и не превращается в условное владение правилом знака.

BPE/Frozen/P125/P25/translation numeric blocks прочитаны на уровне root summary, но повторная полная первичная JSON сверка всех этих блоков не входила в bounded review; новый независимый raw review для них здесь не заявляется. Обнаруженных противоречий с уже прочитанными сохранёнными значениями нет.

## Граница решения

Это review документации и копирования чисел. Он не подтверждает runnable host, сборку, численную parity новой HNSW реализации или выигрыш её native Train. После двух документных уточнений пакет годен к публикации как source-backed audit и очередь кандидатов; новые эксперименты остаются NOT_RUN.

