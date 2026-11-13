# Последняя адресная дельта: опубликованы ответы N-KU, конкретизирован N60-E2

Финальный просмотренный core: **`ce890a48efda7f00d0ee040f2383cc2d06428ea6`**. После `7c5718` изменены только пять документов: три существующих Markdown и две новые таблицы ответов. Нативный код не менялся. Реестры62bcc8f и дельта7c5718 сохраняются как закреплённые срезы; новый полный обход библиотеки не выполнялся. Этот документ уточняет только перечисленные изменения.

## 1. N-KU-01: таблицы результатов появились, но подтверждают узкий показатель

В [основном answers.md][answers] и [order-check-BA/answers.md][answers-ba] опубликованы одинаковые таблицы:

| Задача | Gold узкой задачи | A0 | A1 | B0 | B1 |
|---|---|---|---|---|---|
| KQ1: цикл уже есть | непустой ответ, отличный от ADD_LOOP | null | ADD_LOOP | null | ADD_METHOD |
| KQ2: цикл уже есть | непустой ответ, отличный от ADD_LOOP | null | ADD_LOOP | null | ADD_METHOD |
| KQ3: цикл уже есть | непустой ответ, отличный от ADD_LOOP | null | ADD_LOOP | null | ADD_METHOD |
| KQ4: цикл нужен | ADD_LOOP | null | ADD_LOOP | null | ADD_METHOD |

По опубликованным строкам **A0=B0=0/4, A1=1/4, B1=3/4**. Итоговая разница равна **+2 правильных случая из4**, а не итоговому качеству2/4. Ложные советы ADD_LOOP уменьшились **3→0**, но правильное распознавание отсутствующего цикла KQ4 ухудшилось **1→0**. Это успех по устранению лишнего совета на трёх случаях с уже существующим циклом и регрессия на KQ4; обе стороны должны оставаться в отчёте.

Таблицы имеют одинаковый Git blob `8dd063be91e5f1d0a30b181ef803bcd22212c771`. Их нельзя считать двумя независимыми подтверждениями устойчивого качества. Приk3 и четырёх учебных строках majority остаётся одинаковым для любой тройки соседей в своём плече: опубликованная панель согласуется с ранее описанным эффектом меток и большинства. **Структурного обобщения KNN, качества расстояния, универсальной полезности ADD_METHOD или готовности production она не доказывает.**

Теперь неверно писать, что в repository вообще нет материалов результата N-KU. Точная формулировка: **есть опубликованные таблицы before/after ответов; в этом адресном просмотре не получено достаточного raw/exit/counter/provenance материала для независимого подтверждения полного native исполнения и его бюджета**. Обе добавленные страницы содержат только шесть строк Markdown, без времени, памяти, issued/completed, hashes исполняемой сборки и actual call counts. Имя каталога `order-check-BA` само по себе не доказывает отдельный корректный запуск, его разрешение либо независимый порядок процессов. Отдельные runtime или source-equivalence проверки ради этого аудита не назначались.

Материалы находятся в прежнем `results/.../N-KU-01/`, тогда как актуальная полная карточка описывает отдельный ещё не начатый вариант `N-KU-01-do-loop-label-01/`. Поэтому таблицы не доказывают, что именно новый описанный runner уже был собран и исполнен. Сохранить версию фактически использованного паспорта; не мигрировать и не повторять опыт автоматически.

## 2. N60-E2: прежний OPEN_STATE_AND_READER больше не описывает текущий документ

В [актуальной N60-карточке][n60] теперь указан конкретный контракт из уже существовавшего helper-overlay:

- `ExecutorUnit.TrainOnExample`403–455 записывает `SourceSpanFrames{version=1,spanTokenLengths,gapTokens,supportExampleIds}` из source урока и его mappings.
- `TryComposeFromPhraseMappings`798–857 читает learned frame перед успешным возвратом; неподдержанная форма даёт false и прежний fallback. Специальные ветки под EXAM `do not`/`did you` не добавляются.
- `ExecutorUnitCheckpoint`97–107 и Export/Import484–535 сохраняют version/frames; отсутствие поля в старом state означает unavailable.
- Определены typed TRAIN/EXAM источники, mappings `delete→удали`, `the file→файл`, `save→сохрани`, `the cache→кэш` и конкретный reader/builder gzip→executor-en_ru. `BuildGeneralBatch` с дополнительными Unit-уроками исключён.
- Назначены `experiments-v2/RP01-MICRO/N60-E2/Runner.csproj`, source-copy `35.ExperimentsV2/Experiments/Rp01Micro/N60E2/` и бюджет **2 TrainStructuredBatchAsync, 4 вложенных Executor.TrainOnExample, 16 ordinary RunAsync**.

Следовательно, утверждение «schema, writer и reader ещё совсем не определены» следует заменить на **«контракт и материал конкретизированы; полный сборочный контракт/native host ещё не готовы»**. Это документное покрытие known E2, не выполненное измерение и не новое открытие. Отказ и правильный перевод считаются раздельно; downstream reader способен скрыть эффект. На четырёх заданиях поведение может совпасть с prior P54 full-coverage, поэтому уникальное преимущество нового representation не доказано.

N60-E1 с быстро/тихо этой дельтой не исправлен; наша correction-фикстура и bounded preparation остаются актуальны. Источники нативного кода неизменны.

## 3. Прямое обучение DPO и карточные конфликты

[Documentation-neural][documentation] теперь прямо запрещает менять исходный tools/Program.cs для micro-paired режима: используется экспериментальная копия и одна study с двумя независимыми issued/completed journals. Новый RunId не обходит marker domain+Mode+InputSha256; старые markers не удаляются.

N60 PAD получил буквальное размещение `N61-DPO-PAD/Runner.csproj` и `N61DpoPad/`, ссылку на paired-owner контракт и отдельную ссылку на reference reuse. Host всё ещё не реализован; generic500 не становится его заменой. PAD и reference-cache остаются отдельными факторами. Это дополнительные уже внесённые документные исправления, а не текущие отсутствующие решения размещения.

Для BuildContext разрешена точная source copy с одинаковым узким доступом вA/B, бюджет8вызовов, без подмены полным Retrieve. Принадлежность обучению всё ещё требует установленного происхождения learned state и consumer; сама копия packing это не доказывает.

## 4. Новая N-KU инструкция не разрешает дополнительные проверки

[Актуальная N-KU-карточка][nku] отдельно поясняет: summary/compaction, новый reviewer или «read-only diagnostic» не добавляют бюджет Train/Predict/Label. Упомянутые в прежнем overview neutral warmup, product-A Train, ручной cosine и смена reviewer не становятся разрешёнными от наличия в summary. Существующие следы сохраняются; неисполненные добавления не запускаются.

Для возобновления читаются только конкретные паспорт, stage/exit, issued/completed и сохранённые inputs. Отсутствующий exit code не означает успех, провал или разрешение повторить процесс. Тексты подготовки/запуска назначены в `PrepareInputs.ps1` и `RunExperiment.ps1`, с короткими командами вызова. Эти уточнения уже внесены в protected; текущий аудит их не реализовывал в чужом runtime.

**Граница этого дополнения:** прочитаны все изменённые участки трёх карточек и обе таблицы целиком. Ни нового обучения, ни сборки, ни повторного N-KU, ни проверки чужих процессов нами не выполнялось. Любое измеренное полезное улучшение остаётся успехом по своей метрике и домену без обязательного роста accuracy; остальные изменения и потери показываются рядом. Пользователь решает последующий production перенос.

[answers]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/answers.md
[answers-ba]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/N-KU-01/order-check-BA/answers.md
[n60]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/23-additions-n60-small-learning.md
[documentation]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/2026-10-19-small-learning-probes/04-documentation-neural.md
[nku]: https://github.com/lubochka/llm_mvp_core/blob/ce890a48efda7f00d0ee040f2383cc2d06428ea6/docs/plans/replanning-possible-diffs/experimets/cards/N-KU-01-do-loop-label.md
