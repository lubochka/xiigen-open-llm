# XIIGEN: аудит переноса экспериментов в код и достоверности опытов

**Дата задания:** 11 ноября 2026. **Вывод:** существенная часть прежних ремонтов действительно реализована. Однако приёмка ещё пропускает ошибки на границах выбора и допуска, смысла метрик, происхождения ответа и завершения запуска. Общий вывод «переносы полностью приняты» имеющимися доказательствами не поддерживается. Это аудит названной ниже области, а не аттестация всего XIIGEN.

Проверяемые срезы: core **`009bf226174441fa0fbf0336e74ad2f2b7174d92`**, planning **`04ae31e9232d7c8763bd393a36fc163ac2fd0149`**. Повторная сверка core с **`96e043cc54b231336dac27e203b92a99e44f409f`** обнаружила четыре новых commit только в журналах чтения правил; проверяемые code/tests/cards/skills не изменились. Затем по прямому указанию владельца опубликованы промежуточные commit **`0dd69c4a95ed97ac55afd47e24d5cbf012d94202`** в core и **`f7a05c7dc13a335006caa4cd8ef6695adce27bf0`** в planning. Перед итоговой публикацией проверен новый core **`55cfd9d00bbebdeae04da7c505b84d0b1cb9b747`**: добавлены доска и копии переданных checkpoint-пакетов, удалены два журнала чтения; проверяемые код, тесты и карточки не изменены. Пересечений с 23 подготовленными документами нет. Итоговая публикация фиксируется отдельной квитанцией, чтобы не приписывать её ещё не существовавшему срезу.

## 1. Что проверено и с какой силой доказательства

Инвентаризация current registry дала **124 уникальные work-строки**, current execution index — **131 уникальную строку Q001–Q131**. Это знаменатели каталога, не число полноценно перепроверенных опытов. Отдельная авторская очередь из53 исполнимых работ использует другое основание отбора; различие124/131/53 само по себе не является ошибкой.

Углублённая проверка охватила P48/P49/P50/P52/P58; Wiki feature reuse; P54 phrase coverage; Method KNN через facade; жизненный цикл DPO reference cache; активную цель prompt-flow; коррекционный адаптер перевода; loss neural probe; sink/runner и batch inventory. Дополнительно проверены связанные planning-постановки и перенос выявленных ошибок в другие карточки.

| Уровень доказательства | Что выполнено | Чего из этого не следует |
|---|---|---|
| Закреплённый исходник | Сверка Git blob, чтение методов, callers, tests, contracts, поздних поправок | Самостоятельное выполнение этого кода текущей DLL |
| Сохранённый native результат | Разбор доступных TRX/JSON/логов с проверкой identity и чисел | Новый native PASS на движущемся HEAD |
| Свежий независимый расчёт | Gold histogram, overlap, суммы расхода, XML/console recount, exhaustive P54 oracle, WC01 replay, scalar loss counterexample | Выполнение tokenizer/facade/полного runtime на C# |
| Свежая нативная приёмка | **Не выполнена:** dotnet и pwsh не найдены в текущей среде | Ни сборка, ни discovery, ни новые C# mutations не обозначены зелёными |

Отсутствие SDK не использовано как причина остановить аудит: исходники, сохранённые данные, независимые пересчёты, конкретные описания ремонта и изменения карточек выполнены. Продуктовые ремонты ниже ещё не реализованы. Исторические raw не переписываются.

## 2. Сводка актуальных находок

Приоритет обозначает порядок ремонта, а не доказанную частоту ошибки у пользователей. `Source` — подтверждение текущим исходником; `replay` — отдельно помеченный Python/математический расчёт. Полная нативная достижимость нового P54 случая остаётся обязательной проверкой реализации.

| ID | Приоритет | Актуальная проблема | Основание и граница |
|---|---|---|---|
| F01 | P1 | P54 выбирает недопустимый coverage/span winner, потеряв допустимую альтернативу | Новый конфликт source + два независимых exhaustive oracle; facade UNRUN |
| F02 | P1 | Карточка109 делает вывод «ниже5%, запас1,7п.п.» из несопоставимой доли3,3% | Current card и сохранённые receipt; общий product A/B отсутствует |
| F03 | P0 | Неопубликованная/прерванная попытка не имеет исключительного владельца; есть обходные writers | Актуальный остаток прежней A01; sink уже существует |
| F04 | P0 | Test writer удостоверяет весь runner преждевременно; P58 invalid/no-contrast не всегда достигает terminal-файла | Source control flow; правильный validator/serializer не отменяется |
| F05 | P1 | P50 guardProblems не запрещает VALID/SUPPORTED и return0 | Новый source predicate counterexample; healthy saved run сохраняется |
| F06 | P1 | P52 cold0/4 повторно назван constant ceiling; настоящий ceiling2/4 | Gold histogram, writer, fresh author recheck |
| F07 | P2 | P49 новая target при знакомом input названа незнакомым input | Current helper/test/report и row-level overlap |
| F08 | P1 | Старая цель навсегда мешает накоплению новой; выбор тела не ограничен active revision | Актуальный остаток A06; узкий FIX-RET уже установлен |
| F09 | P1 | Неравенство rejected/source выдаётся за происхождение фактического ответа | Актуальный остаток A05; существующий adapter доставляет урок, но не удостоверяет actual |
| F10 | P2 | Отчётная clipped loss не соответствует используемой производной CE на насыщении | Current formula + независимый scalar counterexample; gradient p−y сохраняется |
| F11 | P2 | Семь обновлений Wiki ошибочно превращены в семь активных эпох | Новый точный replay:4active/14empty,3975 bit-exact параметров |
| F12 | P1 |119классов остатка guard не имеют явного положительного маршрута в текущем batches.tsv | Raw исторические ошибки + статическое сравнение текущей таблицы; discovery UNRUN |
| F13 | P2 | Сводная цена P58 не показывает отдельный setup-probe и все Unit readouts |105 Unit lesson calls и56 прямых Unit chains основного Fact; не whole-class/lifetime |

## 3. Исправления, которые действительно сделаны

Эти выводы проверялись по текущему срезу; старые обвинения об отсутствии реализации не перенесены автоматически.

**P48 и P49:** прежний finite-verdict дефект закрыт в реализованной области. P48 связывает validity с конечностью пяти блоков обученного состояния и loss каждого шага; P49 — с конечностью проверяемого состояния, logits и loss контрольной панели. В P49 сохранены отдельные NaN-контроли embedding/output-bias; в P48 добавлены отрицательные проверки. Это не свежая native приёмка и не полный охват всех промежуточных gradients/activations. Остатки F04 и F07 относятся к другим границам.

**P58:** current source уже считает constant ceiling1/8, отдельно от cold0/8, и использует TRAIN-control0/8→8/8. `DecideReceipt`/`ComposeStatusPayload` вычисляют результат. Старое «везде литеральный успех» больше неверно. Открыт путь записи этого результата при раннем отказе.

**Wiki feature reuse:** `Featurize` вынесен перед циклом, порядок и multiplicity строк сохранены. Архивные тесты показывают baseline4/4, после восстановления4/4, named regressions3/3. Есть внутреннее приобретение и cold restore, публичный gate остаётся закрытым законно. Мутация, возвращающая extraction внутрь эпох, остаётся зелёной4/4: эквивалентность охраняется, ресурсный выигрыш этими тестами не охраняется. Это ограничение уже честно объявлено автором. [S3]

**Method KNN:** обычный `SemanticCompilerFacade.Compile` действительно задействован. Архив после ремонта8/8; удаление distance tie-break даёт2/3 в facade-семье и2/5 в predictor-семье. Положительный случай ломается, majority control сохраняется. Это адресная чувствительность. Граница top-k при равных расстояниях и перенос вывода на произвольные нормализованные inputs не закрываются этими числами. [S14]

**DPO reference cache:** публичный constructor создаёт cache0; остальные проверенные семьи также инициализированы. Accepted restore очищает cache после успешного импорта; rejected precondition происходит до мутации; failure-after-begin проходит rollback. Тесты прогревают того же владельца, наблюдают hit, проверяют восстановление optimizer/RNG и следующий miss. Нового дефекта в проверенной цепочке не установлено; свежего native прогона не было. [S15]

**Batch guard:** исправление чужих вложенных copies подтверждено точным составом трёх исторических TRX. Это реальная починка21 исхода, а не арифметическая интерпретация заголовка commit.

## 4. Новые и уточнённые ошибки переноса

### 4.1. P54: алгоритм соответствует своему score, но конфликтует с caller

`TranslationPhraseCompositionPlanner.Plan` сохраняет единственный лучший suffix: максимум покрытых слов, затем минимум spans. `TranslationTrainedUnits` допускает полученный результат, если coverage≥0,85 **либо** spans≥2 и coverage≥0,50. Эти критерии не монотонны друг относительно друга. [S1]

| Нормализованный пример | Допустимая альтернатива / прежний greedy | Новый winner | Coverage/span predicate |
|---|---|---|---|
|8слов; keys`a1 a2`,`a5 a6`,`a2 a3 a4 a5 a6 a7`|4/8,2spans|6/8,1span|true→false|
|6слов; keys`a1 a2`,`a3 a4`,`a2 a3 a4 a5`|4/6,2spans|4/6,1span|true→false|

Каждый независимый перебор рассматривает все5 непересекающихся планов, включая пустой. EN_RU и отсутствие accepted word mappings — явные предусловия. Не проверены native normalization, target compatibility после этого predicate и конечный fallback. Поэтому точная находка — **потеря допустимой по coverage/span альтернативы до caller gate**, а не уже измеренная ошибка русского перевода.

Ремонт: сохранить достаточные альтернативы и выбирать лучший из допустимых. Конкретная ограниченная DP-форма описана в плане. Пороги и gold остаются прежними. Проверка должна дойти до настоящего phrase reader и показать selected keys, spans, coverage, predicate, target compatibility и final route.

Предлагаемая DP дополнительно сопоставлена с полным перебором на **33 866 графах** всех подмножеств непрерывных интервалов для длин 1–5: расхождений 0. Сравнивались coverage, точное число spans и последовательность синтетических canonical keys. Скрипт и receipt сохранены как `check_proposed_phrase_dp.py` / `proposed_phrase_dp_receipt.json`; статус — `PASS_ANALYTICAL_MODEL`. Эта конечная проверка дополняет доказательство сохранения доминирования, не проверяет все порядки ключей, C#, target compatibility, память или время.

### 4.2. P54:3,3% — правильная дробь с неверным смыслом допуска

Сохранённый компонент измерен как9248 против1344 байт (+588,10%); dense-case4656 против224 (+1978,57%). Процессное окно шести чтений дало239488 байт. `(9248−1344)/239488=3,3003741%` вычислено верно. Но числитель и знаменатель описывают разные окна; знаменатель содержит оснастку, а числитель включает девятисловный planner-вызов, исключённый из продуктовой области≤8. Это не `(ProductB−ProductA)/ProductA`. [S2]

Карточка109:101–110 сначала называет число нижней оценкой, затем делает вывод о допуске ниже5% и запасе1,7п.п. Из lower/mixed estimate нельзя получить такой upper bound. Актуальный `claim-boundaries.md` уже признаёт отсутствие полного A/B — нужно согласовать карточку с ним. Полезный session-result и принятый restore остаются; общий resource nonregression не доказан.

### 4.3. Wiki: точное число пустых эпох

На фиксированной WC01 fixture source-equivalent Python-повтор дал updates_by_epoch=`[2,2,2,1,0,…,0]`:7обновлений,4активные эпохи,14пустых из18. Все3975параметров совпали с архивным snapshot по double bit patterns, включая знаки нулей. Это существенно сильнее вывода только по сумме обновлений, но остаётся аналитическим повтором, не C# timing run. [S3]

Гипотеза бесполезных проходов сохраняется. Нельзя зашить4или7 эпох: онлайн-критерию полной эпохи без изменений на этой панели требуется пройти пятую эпоху. После текущего hoist пустые эпохи повторяют scoring/prediction, а Featurize уже не повторяют. Новый процент экономии не измерен.

### 4.4. Активная цель и происхождение исправления

В `PromptFlowGroundingModel.Train` новый урок сравнивается со всей историей FlowRef. Последовательность A→B1→B2 оставляет B2 без признаков B1, потому что A всё ещё несовместима с B. `FindLearnedPair` фильтрует FlowRef, но не редакцию тела. Выбранный ремонт — типизированная active revision, отдельная от истории, и совместное сохранение редакции/весов/пар. Нельзя выбирать «последнюю» цель по PairId. [S4]

В `TranslationCorrectionLearningAdapter` равенство rejected=source отвергается как подмена. Настоящий source-echo при этом тоже будет отвергнут, а выдуманный третий текст одни неравенства не разоблачат. Выбран producer receipt в `TranslationMachine.InlineTranslationOrchestrator.RunAsync`, который уже захватывает одну published version и возвращает фактический ответ. Новые квитанции должны связывать request, actual, direction, topic, receiver и state; stale admission использует существующий expectedStateVersion/CAS. Повтор уже принятой квитанции возвращает committed outcome до проверки stale новой доставки. Это предлагаемый ремонт, не найденный готовый API. [S5]

## 5. Точность и исполнение пяти опытов

| Опыт и попытка | Проверенный результат | Что поправить в выводе/исполнении |
|---|---|---|
| P48 /0003 | Equal-weight и repeated-row control по12/16; EXAM3/4; constant ceiling6/16; input-only ceiling12/16 | Это другой фактор, чем weighting A/B попытки2. В attempt3 ответы различаются6/16, в attempt2 —0/16. Равный score не равен равным ответам |
| P49 /0003 | Cold6/16,warm11/16; новый материал6/8→7/8; прежний TRAIN6/6→3/6; цена60 против600+60updates | Exact pair overlap cold2/4,warm4/4; input overlap cold3/4. Утверждение о двух незнакомых входах неверно. Не называть это равнодозовым ускорением |
| P50 /0002 | JournalsA2/4,B4/4,constant2/4; sweep2,4,4,4,4;8main+46extra=54lessons | Нарушение MainLessons8→9 меняет только execution label, оставляя VALID/SUPPORTED/exit0. Сохранённый guard list пуст: healthy outcome не обнуляется |
| P52 /0002 | Cold0/4; ответыA4/4,B4/4; tense address1/4→3/4; настоящий constant2/4 | Writer и свежий recheck сравнивают ceiling с cold. Все trained answers идут exact-source; улучшение адреса не является приростом качества перевода |
| P58 /0002 | TRAIN0/8→8/8; A/B по1/8, состояния/ответы одинаковы; C-state различается; rejectedB=actualwrong e1 | Current ceiling1/8 исправлен. No-gain/unequal-start/instrument failure asserts ещё перекрывают terminal; no-errors должен дать NO_CONTRAST и0коррекций |

Источники и JSON pointers приведены в отдельном `experiment_validity/findings.md`; пересчёт выполнен `recompute_saved.py`. [S6–S9]

**P52 показывает, почему одного сверочного скрипта недостаточно.** Проверка10.11 подтвердила совпадение записанного ceiling с cold из другого файла. Оба носителя повторяли одну ошибку. Независимый gold histogram даёт два ответа с частотой2, значит потолок2/4. Поправка не меняет raw и не требует повторного Train.

**P49 показывает ошибку отрицательного oracle.** Sensitivity input1→5 назван невиданным, хотя warm TRAIN уже содержит1→0. Контроль должен проверять виденный input с новой target. Input novelty и pair novelty — три disjoint группы, а не один флаг.

**P50 показывает неполную зависимость итогового решения.** Из условий исходника и отдельной source-equivalent функции следует: observed quality может остаться положительным при обязательном guard failure. Один outcome builder должен сначала решить пригодность и только затем научный исход. Проверять serialized status вместе с настоящим process exit.

**P58: точный расход основного Fact.** Setup-probe24 + три setup по24 + три corrective по3 = **105 Unit.TrainOnExample calls**. Семь восьмистрочных чтений дают **56 прямых Unit Tokenizer→Executor chains**. Это не56 полных TranslationMachine/Orchestrator/Verifier Flow и не общий расход всех Facts: другие тесты тоже обучают. Для class/attempt/lifetime105 — нижняя граница до восстановления фактического selector/TRX. Прежние24/3на плечо сохраняются рядом с derived total.

## 6. Сохранность улик и достоверность приёмки

`ExperimentEvidenceSink` уже защищает tracked published directory. Остаток: `git ls-files` с нулём файлов даёт false независимо от существующих untracked bytes; два владельца могут получить один root; запись использует overwrite. Проверка публикации не является захватом владения. Отдельно остаются runner TRX и фиксированные test writers вне общего sink. [S10]

P48/P52 по-прежнему печатают literal «2из2,код0» из test body; P48 теперь содержит7Fact. В P49 `COMPLETED`/report записываются до хвостовых проверок. В P58 correct serializer отдельно протестирован, но no-gain assert:665, unequal-start:708–709 и instrument:748 стоят раньше status:901. Внешний test failure будет виден; обещанный terminal artifact с причиной может отсутствовать. Это нельзя лечить одним изменением подписи JSON. [S6,S7,S8,S9]

Нужны два документа результата: measurement verdict и внешний runner terminal. Каталог получает исключительного владельца до подготовки/Train, все writers входят в этот scope, а ошибки и partial data сохраняются. После terminal сверяются discovery, cases, exit, TRX и полнота стадий. Нормальный отрицательный научный исход не приравнивается к аварии, а abort не становится «нет эффекта».

### Что подтверждает большой исторический прогон

Независимый console recount дал501Failed и94Skipped; все501имя совпали с авторским JSON,167различных методов. Итог:13751passed+501failed+94skipped=14346записанных результатов. В XML-counter-пересказе `notExecuted=0`, тогда как rows/console дают94; авторское `two_methods_agree=false` сохраняется. Лог заканчивается **`Test Run Aborted.`**. Сам автор уже снял тезис о полном охвате:501 — нижняя граница обнаруженных отказов исторического запуска. Это исправленная автором граница, не новая находка. [S11]

Полный22MB main TRX получить не удалось; его собственный XML parse не заявляется. Два доступных raw проекта260/260 и9/9 разобраны отдельно; складывать их с основным числом уникальных тестов нельзя из-за linked source overlap.342из501записанного отказа относятся к одному publication-coordinator классу; это указатель для triage, не установленная причина342ошибок.

### Что подтверждает ремонт guard

| Raw запуск | Passed | Failed | Всего |
|---|---:|---:|---:|
| Reverse mutation |24|36|60|
| Fix |45|15|60|
| Restore |45|15|60|

Все60имён совпали.21Failed→Passed,15Failed→Failed,0Passed→Failed. Current manifest совпадает с отпечатком принятой починки.119классов остатка по-прежнему не получают явного положительного термина в current batches.tsv. Нельзя объявлять480=501−21 текущим числом ошибок: это разные срезы/запуски, один из них прерван. [S12]

## 7. Как усилены описания, чтобы ошибки не повторялись

Подготовлены реальные вставки в **11 существующих документов**: core15/33/106/109 и семь planning-постановок. Дополнительно подготовлены пять адресных core supplements P48/P49/P50/P52/P58, общий planning crosswalk, два product supplements и общий договор проверки. Большие частично прочитанные карточки не выдаются за полностью переписанные; связь с supplement явно указана в карте усилений.

Ключевые новые требования: независимое определение метрик; три вида overlap; сохранение допустимой альтернативы; active target revision; actual-response provenance; conditional training и идемпотентное повторение; source/assembly/attempt identity; полный физический расход; exclusive evidence ownership; внешний terminal; negative executor path; чувствительность ресурсной мутации. Новые типы помечены предлагаемыми, существующие API названы точно.

Независимый review исправил и подготовленный аудит: обратный порядок merge нельзя было объявлять Train0;8verifier calls не равно8Facts; P58 Unit-chain не равно full Flow; duplicate lookup должен предшествовать stale admission; Mode2 fallback не назван доказанно безопасным. Исправления внесены до итоговой публикации. Это одна содержательная перекрёстная проверка с адресным readback, не фиктивные30/50циклов.

## 8. Граница достаточности и следующие работы

Соседний проход проверил confidence/public gates, target/source leakage, restore/cache transitions, revision identity, default facade и no-contrast. Остались за границей глубокого аудита: остальные карточки, не перечисленные в карте проверенного охвата; другие predictors/top-k tie edges; полная матрица translation directions; все publication-coordinator failures; современный full-suite native baseline; весь корпус документов. Наличие файлов в локальном snapshot не приравнивается к их полному прочтению.

Следующий продуктовый порядок: сначала сохранить неизменность улик и честный terminal; затем исправить admissible-plan выбор, обязательные validity guards и смысл baseline/overlap; далее active revision/provenance и loss; затем точные ресурсные измерения и отдельная гипотеза остановки эпох. Остаток batch inventory имеет самостоятельный triage. Детальные файлы, алгоритмы, бюджеты, отрицательные случаи и критерии приёмки — в плане ремонта.

## 9. Связь с доской, появившейся после промежуточного сохранения

В core `55cfd9d...` появилась доска B01–B12 на основании ранее переданных checkpoint-пакетов. Нумерации доски и F01–F13 описывают разные единицы: B07 сохраняет уже исправленную историческую оговорку, а B09/B12 выделяют соседние условные границы. Само появление доски не означает исполнения ремонта или нового native результата. [S16]

| Строка доски | Связь с итоговым аудитом |
|---|---|
| B01 | F01 — выбор допустимого фразового плана |
| B02 | F11 — обновления и эпохи Wiki |
| B03 | F06 — независимый constant ceiling P52 |
| B04 | F10 — согласованность loss и gradient |
| B05 | F03 — исключительное владение попыткой |
| B06 | F04 — измерительный результат и внешний terminal; итог дополнен ранними отказами P58 |
| B07 | Историческая оговорка о прерванном full-suite, §6; отдельной F-строки нет |
| B08 | F12 — 119 классов без явного маршрута |
| B09 | Условная соседняя граница F12: существующее, но непригодное solution; план 4.1 |
| B10 | F09 — происхождение фактического ответа |
| B11 | F08 — активная редакция цели |
| B12 | Соседняя граница F08: пустой/нулевой сосед; план 3.2 |

**Отдельными строками доски ещё не представлены F02, F05, F07 и F13:** ресурсный знаменатель P54, обязательные guards P50, input novelty P49 и полный расход основного Fact P58. В итоговом плане им соответствуют шаги 2.4, 1.3, 2.2 и 2.3. Эти работы нельзя потерять при согласовании очереди.

**Уточнение B03:** доска сужает остаток до прозы, однако current writer P52 на строках 511–515 действительно записывает числовое поле `предел_постоянного_ответа.верных_ответов` через `cold.Count`. Правильные 2/4 в attempt1 не исправляют ошибочное поле attempt2. Поэтому F06 требует исправления измерителя и его проверки, а не только редакторской поправки. [S8,S16]

## Первичные источники

- **S1:** [planner](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/27.TranslationDomain/TranslationPhraseCompositionPlanner.cs#L108), [caller](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/27.TranslationDomain/TranslationTrainedUnits.cs#L1043).
- **S2:** [карточка109](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/109-p53-p55-transfer-plan-2026-10-30.md#L101), [ресурсные границы](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/artifacts/fbe/FIX-P54-PHRASE-COVERAGE/attempt-0001/claim-boundaries.md).
- **S3:** [Wiki Train](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L58), [архив FIX-WC](https://github.com/lubochka/llm_mvp_core/tree/009bf226174441fa0fbf0336e74ad2f2b7174d92/artifacts/fbe/FIX-WC/attempt-0001).
- **S4:** [PromptFlowGroundingModel](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/42.ProjectWikiDomain/PromptFlowGroundingModel.cs#L17).
- **S5:** [correction adapter](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/27.TranslationDomain/TranslationCorrectionLearningAdapter.cs#L160), [actual producer](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/27.TranslationDomain/TranslationMachine.cs#L2113).
- **S6:** [P48 test](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/Tests/ExperimentFixes/P4801WeightingCurriculumCeilingTests.cs), [P49 helper](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/experiments-v2/RP01-MICRO/P4901/src/FinitenessGovernedVerdict.cs#L239).
- **S7:** [P50 current host](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/experiments-v2/RP01-MICRO/P5001/attempt-0002/Program.cs#L759).
- **S8:** [P52 writer](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/Tests/ExperimentFixes/P5201RejectedAnnotationAddressArmsTests.cs#L494), [P52 recheck](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/experiments-v2/RP01-MICRO/P5201/results/claude-row52-20261110/02-numeric-recheck.md).
- **S9:** [P58 host](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/Tests/ExperimentFixes/P5801CorrectiveLessonCounterExampleArmsTests.cs#L656).
- **S10:** [EvidenceSink](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/Tests/ExperimentFixes/ExperimentEvidenceSink.cs).
- **S11:** [full-suite evidence](https://github.com/lubochka/llm_mvp_core/tree/009bf226174441fa0fbf0336e74ad2f2b7174d92/artifacts/fbe/FULL-SUITE-20261110/attempt-0001).
- **S12:** [guard evidence](https://github.com/lubochka/llm_mvp_core/tree/009bf226174441fa0fbf0336e74ad2f2b7174d92/artifacts/fbe/FIX-BATCH-INVENTORY-GUARD/attempt-0001), [current batch table](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/tools/test-batches/batches.tsv).
- **S13:** [loss/gradient](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/35.ExperimentsV2/Core/ProbeNeuralNetwork.cs#L100).
- **S14:** [KNN tie-break](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/12.SemanticCompiler/Prediction/MethodKnn.cs#L329), [facade tests](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/Tests/ExperimentFixes/MethodKnnTieReachedThroughFacadeTests.cs), [FIX-R21 evidence](https://github.com/lubochka/llm_mvp_core/tree/009bf226174441fa0fbf0336e74ad2f2b7174d92/artifacts/fbe/FIX-R21).
- **S15:** [DpoTrainer](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/11.DPO/DpoTrainer.cs#L493), [same-owner cache tests](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/Tests/DpoReferenceScoreCacheTests.cs), [constructor lifecycle tests](https://github.com/lubochka/llm_mvp_core/blob/009bf226174441fa0fbf0336e74ad2f2b7174d92/Tests/ExperimentFixes/DpoReferenceCacheConstructorLifecycleTests.cs).
- **S16:** [доска B01–B12 на срезе 55cfd9d](https://github.com/lubochka/llm_mvp_core/blob/55cfd9d00bbebdeae04da7c505b84d0b1cb9b747/docs/plans/replanning-possible-diffs/claude-audit-board-2026-11-11.md).

Полные source manifests, исходные значения, скрипты и review receipts входят в итоговый пакет. Ссылка на источник — основание конкретного вывода; ни один saved результат не объявлен новым native запуском.
