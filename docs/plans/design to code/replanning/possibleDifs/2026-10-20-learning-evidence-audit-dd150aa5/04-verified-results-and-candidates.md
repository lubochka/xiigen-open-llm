# Подтверждённые результаты и кандидаты в исправления

Срез core: `dd150aa5216e42b8a8bba14955337398ec5f8397`. Только чтение raw JSON/исходников. Первые семь опытов завершены: повтор не назначать. Полезный измеренный эффект сохраняет статус успеха своей метрики; причинная атрибуция оценивается отдельно.

## Первые семь: полезные эффекты вместе с ограничениями

| Опыт | Проверенный полезный результат | Ухудшения и границы |
|---|---|---|
| N-KU-01, обучение советам по C# | EXAM 1/4→3/4; ложные ADD_LOOP 3→0 | Нужный ADD_LOOP 1/1→0/1. В B все четыре ответа ADD_METHOD. При обратном порядке B→A качество то же, знак времени подготовки меняется. Это обучение таблицы памяти. |
| NEURAL-FROZEN-TRANSPOSE-01, SFT/LoRA | Allocations 13 647 840→12 140 448 B (−1 507 392, −11,045%); loss, logits и state hashes A/B совпали | Кэш +249 856 B; wall84,3133→84,57мс; CPU62,5→78,125мс. Ответы 0/2→0/2, target NLL снизился. Четыре SFT updates. |
| BPE-NO-MATCH-COPY-01 | Code/doc allocations3 904 664→1 126 744 B (−71,144%); EN/RU2 892 824→893 656 (−69,108%). В каждом плече выучено64 merges; состояние/токены A/B одинаковы | CPU code/doc31,25→46,875мс; EN/RU46,875→46,875. Измерено обучение BPE, downstream neural/kNN accuracy не измерена. |
| N62-R1, сравнение замороженной базы | Allocations−90 504 B; applyOptimizer1873,27→378,60мс — полезное локальное наблюдение | GC[188,12,9]→[189,13,10]. Fixed A→B; менялись также нетронутые фазы. Ускорение именно RequireValid не доказано. |
| N62-R2, scratch softmax | Allocations−312 328 B | Peak+30 474 240 B; GC по+1. Source manifest B hash=APPLIED_AFTER_FACTOR_EDIT, study A/B hashes одинаковы. B binary не привязан. |
| E-OPT-STEP, чтение шага AdamW | Allocations−467 944 B; GC−4/−5/−3 | Peak+204 546 048 B. Raw factor/measured_path ошибочно унаследованы отN62-R1. Три placeholders B source hashes. Измеренный эффект сохраняется; его атрибуция заявленному фактору ограничена. |
| LF-03, копирование начального градиента | Allocations−646 816 B; peak−43 495 424 B; GC−3/−3/−3 | Wall+1908,1644мс; CPU+609,375мс. First-token NLL6,814549566→6,814264389, rank221→221, exactfalse→false. Не full-response. B hash placeholder и несовпадение bridge hash. |

У №04–07 действительно меняются16 адаптерных массивов, база/образец сохранены, конечные A/B hashes совпадают. У №04–06 TRAIN-target before/after отсутствует; loss0,91264373/0,9189987 относится к двум разным урокам. Эти числа не доказывают улучшение одного target. Начальное состояние указано как InitialBaselineNoUpdate. У №07 появился только первый токен; нельзя приписать ему полный ответ/retention. Числа wall/CPU/peak одного fixed-order запуска — наблюдения, не изолированный причинный эффект. Отсутствие точной binary provenance не стирает измеренное уменьшение allocations.

Первичные пакеты: [сохранённые результаты на этом SHA](https://github.com/lubochka/llm_mvp_core/tree/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20). В каждой строке таблицы указан одноимённый каталог; прочитаны report.md и raw A/B JSON, для E-OPT-STEP-01/LF-03-01 также measurement-erratum.md. Source-manifest: placeholders N62-R2:724; EOPT:834/839/844; LF03:939.

## Отдельная 400-шаговая проверка

Raw LEARN-SANITY-PROBE показывает TRAIN/EXAM0/2→1/2; TRAIN NLL7,9608855→1,8879446. На400-м шаге TRAIN предсказывает +,+; EXAM−,−. Это ограниченная обучаемость; не освоение знака и не A/B кэша. Program.cs:77 обучает до CreateNew:135. Отчёт №02 описывает два terminal attempts; общее число updates неизвестно. Это уже описанное ограничение.
Источник: [raw](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/experiments-v2/RP01-MICRO/LEARN-SANITY-PROBE/probe-result.json); Program.cs рядом.

## Историческое обучение и применимость

- E25: POS-последовательности, k=3/Jaccard по биграммам:0,3438→0,875; 160 примеров/32validation; время0,4→9,7мс. Run:22–28 вызывает Train. Применимость — порядок слов/POS. Это не доказательство успеха одноимённого fixed-rule consumer. GlobalUsings.cs:11 связывает SentencePatternClassifier с SemanticCompiler.PatternClassifier.
- E09: AST/kNN результат0,2413→0,3507 полезен как сохранённое наблюдение, но runner:51–58 оценивает baseline на1123 переходах, :70–77 kNN только на499; сравнение не устанавливает причинный выигрыш. :113–120 remap kind IDs выполняется отдельно для каждого файла; это не реальные BPE IDs. E07 использует BPE+структурный n-gram. Поэтому доказанного одинакового опыта «тот же matrix-механизм переехал в kNN» эти два отчёта не дают.
- E22: поздний macroF1 0,4709→0,5646 (360train/90val) — успех метрики; время0,1→100,5мс. Router.Train:47 находится до обоих таймеров:53/80; это не время обучения. Нулевые F1 TypeScript/Tests сохранены; нельзя объявлять одинаковую полезность для всех доменов.
- E43: retention0,3226→0,3978; время0,1→2,4мс. :43/:53 заново строят bigram tables из новых данных/смеси replay. Это доказательство полезности replay для данного счётного learner, не neural continual fine-tuning.
[E25 raw](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-04-30-001.json) [E09 raw](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-05-01-001.json) [E22 raw](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E22-DATASET-ROUTER/results/run-2026-05-07-002.json) [E43 raw](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-04-30-001.json) 

E25 raw дополнительно прочитан в соседних ветках `language_translate`@`f6032aff81f1f83de7c09cf9b9efe6b058dd12b9` и `DesignToCode-clean`@`e346b88a3cc1fcce48aefa626bfc7b00509eefd6`; тот же путь и результат0,3438→0,875.

## Переводческая память

P125: после8 настоящих уроков0/24→19/24, но принятые ошибки0→3. Corrupted control даёт13/24 и9 ошибок: сигнал зависит от материала. Receipt accuracyAfter=0 не отражает независимый score19/24. P25 cold restore воспроизводит24/24 ответов/состояние, в том числе те же3ошибки. Ingress phrase_mappings даёт11/24→19/24, composition0/8→8/8, ошибки1→3. Нельзя обесценивать обучение памяти, но нельзя приписывать его neural DPO.
[P125 независимый score](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P125-enru-memory-01/score-summary.json) [P25 восстановление](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/P25-enru-cold-01/comparison-summary.json) [Ingress before](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/before/score-summary.json) [Ingress after](https://github.com/lubochka/llm_mvp_core/blob/dd150aa5216e42b8a8bba14955337398ec5f8397/docs/plans/replanning-possible-diffs/experimets/results/2026-10-18/translation-memory-ingress-01/after/score-summary.json) 

Этот раздел исторических результатов не заявляет новых гипотез. Raw не переписывать; сохранить результаты кандидатами.


## Кандидаты для решения пользователя

Эти кандидаты добавлены документально; перенос в production не произведён. Улучшение полезной метрики достаточно для успеха этой метрики и домена. Успех не стирает ухудшение соседней метрики и не означает автоматическое принятие всей реализации.

| Кандидат | Основание сохранить | Что необходимо учесть при решении о переносе |
|---|---|---|
| N-KU-01: учебная метка do-while | 1/4→3/4 и меньше ложных ADD_LOOP | Потеря единственного нужного ADD_LOOP; не принимать как безусловное улучшение всех C# случаев. Адрес изменения: 12.SemanticCompiler/Prediction/FileActionKnn.cs, ActionLabelGenerator.Label. |
| NEURAL-FROZEN-TRANSPOSE-01: кэш transpose замороженной базы | −1 507 392 B allocations при совпавших measured loss/logits/state | Кэш занимает +249 856 B; измеренное CPU хуже. Качество полного ответа не улучшилось. Адрес: 00.TensorCore/Layers/Linear.cs, Forward; не кэшировать изменяемые LoRA A/B как frozen. |
| BPE-NO-MATCH-COPY-01: не копировать неизменённый chunk | −71,144% и −69,108% allocations при том же выученном tokenizer | Успех именно обучения BPE на двух корпусах. Downstream качество не измерено. Адрес: 01.Tokenizer/BpeTokenizer.cs, MergeChunk; вернуть существующий chunk только при отсутствии replacement и при сохранении владения массивом. |
| N62-R1/R2, E-OPT-STEP, LF-03 | Полезные измеренные reductions allocations сохраняются | Предлагаемые source изменения уже описаны в карточке N62. Для R2/EOPT/LF03 неполна привязка B binary к источникам; кандидат имеет этот предел, он не становится доказанным product patch. Не назначать повтор ради нового отчёта. |
| Переводческая память и её ingress | 19/24 после обучения; ingress 11/24→19/24; cold restore воспроизводит 24/24 outputs | Три принятых ошибки требуют отдельного решения о допустимом trade-off. Обучение памяти действительно произошло; neural DPO этим не проверен. Сохранять независимый score рядом с ошибочным/неполным receipt accuracy. |
| Исторические E25, E22, E43 | Есть положительные measured quality/retention в конкретном learner | Переносить закон и функцию после сопоставления actual caller/representation/state; одинаковое название компонента не означает одинаковый алгоритм. |

**NEXT/STOP:** команды пользователя о переходе и остановке немедленно прекращают дальнейшее оформление предыдущего опыта. Не добавлять новые запуски, hashes, исправления тестов или публикационную цепочку ради этой таблицы. Старые raw и неудачные попытки сохраняются.

## Поздний положительный кандидат RESTORE-MINIMAL-SET

Проверены новые report/source/raw на core81141d57: NLL−0,055098, rank−1,775, response tokens3/40→4/40 после продолжения восстановленной H16модели. CONTROL ухудшается, полные TRAIN ответы0/2→0/2. Кандидат сохраняет все полезные сдвиги и точное восстановление model tensor hash; требует исправить интерпретацию «2строки с совпавшим токеном» как будто это2правильных ответа. [Подробная таблица и документный кандидат](11-latest-core-delta-and-restore.md). Повтор не назначается.

## Позднее подтверждение восьмого завершённого опыта

Первичный N63I raw уже есть наdd150aa: A отказал до обучения, B выполнил2updates/70targets, отложив один длинный урок. TRAIN response NLL6,595570459→6,592119856; правильных response tokens0/40→0/40 и полных ответов0/2→0/2. Полезные admission/NLL эффекты сохранены кандидатом, ограничение learning panel указано; повтор не назначен. CPU62,5→93,75ms и allocations81 992→3 330 408B сравнивают отказ с настоящим обучением, не одинаковый объём работы. [Raw, полный разбор и текущий NEXT09](12-current-queue-and-n63-evidence.md).
