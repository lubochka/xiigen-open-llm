# Независимая проверка пакета восстановления и neural различий

18 октября 2026. Проверен подготовленный root-пакет в исходной папке 40-PREPARETOGPL-RESTORATION-2026-10-18; root планирует публикацию под именем 43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18. Проверка не является native learning/load экспериментом.

**Вердикт: содержание пригодно к публикации после перечисленных ниже точечных исправлений.** Центральный source/data вывод legacy4→6 и арифметика retained neural scores независимо воспроизведены. Core, protected docs, модели и учебные данные не изменялись. Проверяющий не публиковал документы удалённо.


## Статус исправлений в публикуемой версии — проверка root

Все четыре замечания независимого review применены к пакету43:

- **REV-01 выполнено:** legacy4→6 назван независимым подтверждением N41-T1; добавлены ссылка, publication HEAD и delta новых материалов. Novelty N-NEURAL-01/02 проверена отдельно.
- **REV-02 выполнено:** early E07/E09 ссылки исправлены на корневой experiments-v2 и независимо открыты root через GitHub.
- **REV-03 выполнено:** равенство final output не считается доказательством непрочитанного predictor field; требуется trace следующего решения.
- **REV-04 выполнено:** ссылка на modulo codec включает фактическую строку101.

Root отдельно повторил neural script с проверкой исходных Git blobs, просмотрел обе ориентации DPO derivative и текущий checkpoint fingerprint/descriptor; source/data legacy comparison совпал с независимым повтором. Проверены относительные ссылки, существование файлов источников по загруженному содержимому/деревьям и диапазоны строк доступных primary sources. Это проверка артефакта и арифметики, без новых native training/load/generation calls.

Ниже сохранён собственно отчёт независимого проверяющего с замечаниями к прежней просмотренной версии. Формулировки «нужно исправить» относятся к тому этапу, а не означают оставленную ошибку в пакете43.

## 1. Что независимо выполнено

Прочитаны README, 01, 03, 04, 05, 06; исходная нейронная записка 02 подготовлена этим же агентом и не выдаётся за независимый повторный review собственного авторства. Её пересчёт повторён из публикуемого скрипта/данных; root отдельно проверяет интерпретацию 02.

Повторены оба offline скрипта:

1. evidence/legacy_checkpoint_probe.py на точных baseline/current roots из audit-work/independent/exact. Новая копия результата: audit-work/package-review-legacy-result.json. JSON структурно полностью совпал с evidence/legacy-checkpoint-result.json.
2. evidence/neural/recompute_likelihood_components.py непосредственно из подготовленного publish пакета. Результат полностью совпал с исходным likelihood-components.json; все шесть original Git blob checks прошли.

Legacy проверка подтвердила Git blobs двух TranslationMachine.cs и checkpoint; точный checkpoint одинаков в обеих ветках: 2550 bytes, SHA256 95b5d1c6de1d70a6bba4adbafdf57ffafa0735b7c52d193e79f64acad7e84887. Старый runtime и checkpoint имеют EN_HE/EN_RU/HE_EN/RU_EN; новый runtime дополнительно EN_ES/ES_EN. Source-проверка predicts EXECUTOR_DIRECTIONS_INCOMPLETE, но C# loader не запускался и migration не исполнена.

Neural повтор подтвердил ENRU chosen decrease 20/20, rejected decrease 19/20, margin increase при chosen decrease 11/20. Средняя Δchosen −0.0010038137435913086, Δrejected −0.0011138200759887695, Δmargin +0.00011000633239746093. Это teacher-forced retained scores с нулём новых model/optimizer/teacher/evaluator calls.

Дополнительно: все 125 карточек в 05 перечислены ровно по одному разу, ID 1–125 без пропусков, указанные filenames существуют в frozen protected catalog. Тела E26ContextualDictionary class из current и language_translate совпали после исключения header: SHA256 нормализованного тела 2a1f601e30dd35ec4e812c6921df7bebe0f2c071a4d28f40bebf521985c4a07a.

## 2. Обязательные исправления перед публикацией

| Пункт | Место в просмотренной версии | Проблема | Требуемое исправление |
|---|---|---|---|
| REV-01: обновить новизну | README/01/05/06: формулировки «новое» и «ранее неописанное» для legacy4→6 | Пока шёл аудит, planning HEAD сменился на a4a3e709f8fb5310baab6daa16501b5d4394bd72, где другой агент уже опубликовал N41-T1 с той же source-границей. | Назвать N-RESTORE-01 независимым подтверждением N41-T1 и точным дополнением: существующий архивный checkpoint, его original bytes/SHA, два retained probes и опыт 0 lessons. Обновить corpus/HEAD оговорку и включить ссылку на параллельную карточку. Не объявлять несуществующим её миграционный опыт. |
| REV-02: две первичные ссылки | 03-HISTORICAL-LINEAGE.md, строки 21–22 | Пути ошибочно содержат 35.ExperimentsV2/experiments-v2. Оба точных URL дали GitHub isError; исходные ранние серии расположены в experiments-v2 от корня. | Удалить только лишний prefix 35.ExperimentsV2/ в этих двух ссылках. Поздние 35.ExperimentsV2/results/... ссылки не менять. Правильные адреса ниже. |
| REV-03: граница причинного вывода | 04-SMALL-EXPERIMENTS.md, R03, строка 52 | «Raw prediction меняется, а ответ нет — это подтверждает разрыв потребления» слишком сильно. Поле могло быть прочитано; downstream confidence/arbiter либо неизменный победивший вариант законно сохраняет ответ. | Написать, что это отсутствие наблюдаемого эффекта на ответ. Caller trace отдельно различает непрочитанное поле и прочитанный прогноз, чьё влияние подавлено дальнейшим решением. Только непрочитанное поле подтверждает конкретный consumption gap. |
| REV-04: точный line anchor | 03-HISTORICAL-LINEAGE.md, строка 32 | StructuralKnn.cs#L99-L100 содержит сигнатуру/скобку, а NodeTypeId = SyntaxKind%200 расположен на L101. | Использовать #L98-L102 либо #L99-L102. |

Правильные pinned primary URLs:

- [E07 early](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/experiments-v2/E07-STRUCTURAL-CONTEXT-TOKEN/results/run-2026-04-30-005.json) — Git blob 253a355ca1124c7a20d5a07f7cc25d101b148d56.
- [E09 early](https://github.com/lubochka/llm_mvp_core/blob/31346d8928be4469b01aaf313780e8f950ff91e2/experiments-v2/E09-STRUCTURAL-KNN/results/run-2026-04-30-001.json) — Git blob 6683a5e08c59bbd5c3d6b46012320abb184b01a7.
- [Modulo label code](https://github.com/lubochka/llm_mvp_core/blob/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/12.SemanticCompiler/Prediction/StructuralKnn.cs#L98-L102)

Root получил эти замечания в ходе проверки и подтвердил намерение исправить causal/newness формулировки. Данный отчёт фиксирует review, а не утверждает, что непрочитанная будущая удалённая версия уже содержит исправления.

## 3. Что проверено и не требует усиления диагноза

### Legacy state и ordinary caller

Прямо сверены current source ranges:

- constructor L31–42 создаёт CreateRuntime;
- Import L308–337 проверяет identity и вызывает preflight;
- ValidateCheckpointStructure L384–430 проверяет обязательные поля, неизвестные/дублированные directions и затем missing directions;
- LoadCheckpointAsync L611–650 десериализует, создаёт new TranslationMachine и возвращает ошибку Import;
- CreateRuntime L714–780 создаёт ровно шесть executors;
- Tools continue L4073–4099 вызывает этот loader после corpus hash check.

Направления в legacy JSON являются строками; checkpoint contracts используют JsonStringEnumConverter. Показанные значения identity, schema и required top-level material согласуются с достижением missing-direction проверки. Скрипт и документ корректно сохраняют статус SOURCE_AND_RETAINED_DATA_COMPARISON.

R01 сохраняет полезную нынешнюю preflight/atomic validation и не рекомендует принимать произвольно повреждённые partial snapshots. В эксперименте B/C меняются только два явно добавленных пустых executor records, learned maps/KNN/corrections/history input сохраняются. Оговорка о нынешней нормализации legacy reward0.03→0 предотвращает неверное требование byte equality всего export. Native A/B/C и восстановление предметного поведения остаются NOT_RUN.

Исторический runner-report действительно связывает тот же checkpoint SHA с exact-source и nonexact KNN ответами. Английский исходник в RU_EN profile явно назван узкой технической памятью, а не широкой RU→EN компетенцией. Это корректное ограничение.

### E07/E09/E22/E26

Все приведённые raw numerical values совпадают с локальными первичными JSON: early E09 +3.08%/passed=false, поздние E09 +45.33% и +39.14%/passed=true, E22 macro-F1 .4709→.5646 с TypeScript/Tests0, E26 .4→1.0 на15 cases. При этом старые split/labels/leakage не объявлены исправленными.

R03 указывает UseStructuralKnn=false, TrainingSize≥10 и CSharp gate; field PredictedNextTokens действительно заполняется. Отсутствие доказанного ordinary RP01 answer consumer сохранено как CONDITIONAL. После REV-03 глава не будет выводить consumption gap из одного unchanged output.

R04 правильно разделяет parity статического BuildDefault lookup и отдельный предлагаемый learner experiment. _k сохраняется, но TranslateWithContext выбирает First после distance/Priority; 3-neighbour vote здесь нет. Старые15 cases не названы новым обучением Executor. Context schema/pipeline producer и route-not-reached указаны как зависимости.

### Уже исполненные 18 октября результаты

R02 и 05 не повторяют 11/24→19/24 как новую незапущенную гипотезу. Current reader L4442–4470 и builder L4682–4695 передают phrase_mappings в ExecutorOutput; отчёт сохраняет смысловые ошибки и неверные edge cases. P25 current6→6 не подставлен вместо historical4→6. Mask/SFT gradient improvements не названы предметным neural успехом. Neural и memory recipients сохраняются раздельно.

### Neural условные гипотезы

N-NEURAL-02/R06 меняет только первый момент; v50, step50, C50 reference, tokenizer/RNG/material/LR и topology удерживаются. Не полученные actual slots из LFS не выданы за measured gradient conflict. Alpha mismatch и Alpha0 явно исключены как фактическое объяснение RP01, где descriptor/alpha16 защищают эти условия. Эти пределы важнее расширения количества новых гипотез.

## 4. Delta novelty во время review

Независимо прочитан новый N41-T1 legacy4→6; он действительно содержит ту же source-границу и опыт со свежим 2–8 lesson fixture. Поэтому наш source mismatch больше не первый относительно publication HEAD. Новое полезное дополнение — existing 2550-byte archived file и его воспроизводимая привязка к старым двум probes: для первого восстановления не нужно заново получать эту память обучением.

В latest-planning прочитаны ближайшие neural части 41-LORA... и 42-NEW-CAUSAL...; выполнен поиск m50/v50/first moment/20/20/19/20/11/20 и характерных численных chosen deltas. Та же фактическая score decomposition и тот же m50-only contrast не найдены. Наличие старого warm-SFT-versus-DPO и LoRA-parity опыта не тождественно этим двум пунктам. Новизна N-NEURAL-01/02 остаётся bounded выводом относительно проверенного corpus плюс данного delta, без заявления о недоступных разговорах других агентов.

## 5. Понятность и публикационные границы

Before/after для checkpoint описан понятно: таблица показывает старые4, новые6, тот же schema/file и изменённое условие допуска. Исторические experiments отделены от новых проектов и текущих результатов. Добавить пробелы в слитых narrative фрагментах вроде «все 532 Markdown», «4 model blocks», «по 2запроса» полезно для читателя; не применять широкую замену внутри кода, URLs, hashes и IDs.

До публикации создать обещанный 07-INDEPENDENT-REVIEW.md и отразить фактически внесённые REV-01…04. Не приписывать этому review запуск native restore/migration/Student/optimizer; таких вызовов ноль. Exact neural raw receipts следует публиковать без newline normalization: повторный скрипт проверяет original Git bytes.

После исправления REV-01…04 иных существенных factual/causal препятствий публикации в просмотренном объёме не обнаружено. Это ограниченная независимая проверка конкретного пакета, не доказательство исчерпывающего аудита всех historical branches.

## Финальная независимая проверка параллельного пакета

Перед записью ветки проверяющий отдельно сопоставил шесть новых документов из `2026-10-18-preparetogpl-recovery` на planning commit `004c4a79f3ec0c1dba387ba713abad108b7568fe`. N-NEURAL-01 сохраняет новизну как фактическая декомпозиция retained scores, N-NEURAL-02 — как отдельное вмешательство m50→0 при неизменных v50/step50/C50/reference. Их E-HEAD/E-BASE являются другими опытами. N-NEURAL-03 отличается от нового N-L1: полный keyed payload с другой live Alpha против намеренного legacy base-only import с сохранением прежних A/B. RP01 guards обоих случаев остаются явными контрдоказательствами.

Новый U-R1 уже содержит тот же конкретный legacy checkpoint, retained probes и опыт без обучения. Поэтому mismatch, обнаружение файла и идея zero-training не заявляются исключительным вкладом нашего пакета. Наш source/data скрипт и независимый повтор сохранены как дополнительное проверяемое свидетельство общего результата. Все шесть чужих документов сохраняются при публикации.

Этот последний review ограничен delta-новизной и сопоставлением механизмов; новых source/native learning/load/model calls не было. Root внес эти оговорки в README, 01, 05, 06 и publication-delta.json до обновления ref.

## Финальная поправка новизны после параллельного commit 412d3364

Root прочитал DLT-K1, соответствующие новую карту evidence/experiments и сверил их с N-NEURAL-01/02/03. Общая old-moment interference и cosine-диагностика уже описаны другим автором. Формулировки в README, 02, 04, 05, 06 и общем индексе сужены: N-NEURAL-02 — отдельный m50-only contrast, сохраняющий v50/step50, а не новая общая тема переноса optimizer. Предыдущие review-параграфы относятся к указанным в них ранним срезам. Legacy fixture и zero-learning идея — общий независимо подтверждённый результат.

Последний сопоставленный corpus — 555 Markdown-путей; source и native выводы этим изменением не расширены. Фактический пересчёт 20/20 chosen↓, 19/20 rejected↓, 11/20 margin↑при chosen↓ остаётся отдельным новым результатом относительно проверенных материалов. Все новые native training/load/generation вызовы по-прежнему NOT_RUN.

Независимый проверяющий отдельно сопоставил этот третий delta и подтвердил сужение: DLT-K1 уже содержит general moment interference, cosine и долю конфликтующих координат. Наша отдельная переменная — только m50→0 при прежних v50/step50; это уточнение известной гипотезы, без увеличения числа самостоятельно найденных причин деградации. Новый optimizer из DLT-K1 меняет также второй момент и позицию bias correction.

Проверяющий подтвердил, что фактическая score decomposition N-NEURAL-01 не найдена в новых шести Markdown, включая EXPERIMENTS/EVIDENCE, а N-NEURAL-03 отличается от legacy base-only import и одноэлементного attention. Пределы неизменны: offline scores, отсутствующий actual optimizer payload, RP01 descriptor защищает Alpha, новых native/model/optimizer calls нет.
