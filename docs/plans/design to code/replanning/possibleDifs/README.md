# Аудиты исполнения и подготовка переносов экспериментов

<!-- TRANSFER-PLANNING-20261112 -->
**Очередь переносов на 13 ноября.** Пункты из файла Любы R28 → 124/Q06 → Q35 → Q34 → Q32 приняты. Следующее продуктовое поручение архитектора — подключить исправление к выдавшей ответ единице Q12, затем к переводчику Q22. R11 остаётся у исполнителя приёмки BPE. [Очередь и все 124 работы](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#executor-next) · [Коммиты и результаты](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-13/transfer-execution.md).

[Сверка реестра](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#all-work-status): **124 уникальные строки с пятью колонками в одной таблице, 114 исходных и 10 поздних**. Историческое разбиение 114 и сокращённые 102 нельзя выдавать за нынешнее число завершённых переносов. KDP сохраняет принятую вспомогательную часть; обычный продуктовый вызов остаётся открытым. [Карта реальных правок карточек](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/audits/2026-11-12/executor-context-and-index-repair.md) содержит R01/R02/R03/R05/R09, 03/90/120/45/46, 11/48/100/107/109 и P48/P49/P50/P52/P58 с конкретными проверками. Остальные карточки не объявлены повторно аттестованными. Новые правки core пока локальны: публикация блокируется `index.lock`; опубликованная версия `370fd451db26a26c166e43576d4c7ca89bd8605f` сохраняет прежний срез. Этот planning-указатель не доказывает публикацию core.

**12 ноября — [исправленный контекст исполнителя и индексы после F03](codex-2026-11-12-executor-context-and-index-repair.md).** [Очередь на 12 ноября](https://github.com/lubochka/llm_mvp_core/blob/370fd451db26a26c166e43576d4c7ca89bd8605f/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#executor-next): ранний захват каталога, явное завершение попытки и остальные пункты R01/B05; после них R02/B06 и R03/B14. Дополнительная выборка уже опубликована в `6b52307dc26`: [51/51](https://github.com/lubochka/llm_mvp_core/blob/6b52307dc26f982dde161ee1d6f470eec1c84de4/artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0004/results/run-01-eight-unverified-callers.trx) и [3/3](https://github.com/lubochka/llm_mvp_core/blob/6b52307dc26f982dde161ee1d6f470eec1c84de4/artifacts/fbe/AUDIT-F03-ATTEMPT-OWNERSHIP/attempt-0004/results/run-02-supplementary-control-class.trx) контрольного класса. Три замечания второй рецензии исправлены в `ce245ae2d3a`. Карточки теперь требуют точную связь файла, вызывающего метода и полного класса; имя файла не заменяет фильтр. Весь F03 этими проверками не закрыт. Старые NEXT ниже исторические; новых научных запусков эта запись не назначает.

**4 ноября — [исправление контекста после Q01/POSTQ011](codex-2026-11-04-POSTQ011-EXECUTOR-CONTEXT-REPAIR.md).** [План 161](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-161-postq011-executor-context-repair-2026-11-04.md) согласует Q02 RULE, тестовые ожидания, собственное состояние MC, точный смысл метрики генерации и контекст входа TR07. Очереди и допуски сохранены; Q01 не переоткрыт, новых опытов нет. Инструкции наборов и индексы обновлены вместе с карточками.

**4 ноября, после приёмки Q01:** назначенные остатки переноса проверены свежими29/29; [отчёт и границы](C:/Projects/llm_mvp_core/qa-results/q01-concurrency-2026-11-04-20261104091631764/codex-report.md). Следующий перенос — [Q02/PAIR](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#executor-next). Это не меняет отдельные11/131 разборов опытов и не объявляет подготовку новых наборов принятой. Датированные блоки ниже описывают свои source pins.

Краткое объяснение новой поставки: [наборы, измеренная польза и порядок работы](codex-2026-11-04-LEARNING-SETS-TRANSFER-COVERAGE.md). Здесь нет нового запуска опыта; документы связывают готовые исходные партии с будущими проверками уже обоснованных переносов.

**4 ноября — [наборы для проверки переносов: план 158](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-158-learning-sets-transfer-coverage-2026-11-04.md).** Реальные партии связаны с существующими владельцами, ожидаемыми проверками и ограничениями. Все 5729draft-строк покаTRAIN; отдельный экзамен предстоит подготовить. Улучшения наборов — только вsimulation-каталоге по [инструкции 31](C:/Users/lubao/Documents/xiigen/llm%20mvp/learning-sets/preparation/31-TRANSFER-VALIDATION-SIMULATIONS.md), исходные партии сохраняются. Продуктовая очередьQ01 и отдельный разбор опытовQ011 не объединяются; новых PRIMARY и новых FIX здесь нет.

<!-- POSTQ011-2026-11-04:START -->
## 4 ноября — аудит после Q011 и адресный план

[Новый разбор](codex-2026-11-04-POSTQ011-TRANSFER-AND-EXPERIMENT-AUDIT.md) синхронизирован с core45bd4e3 и planning9cd5351. Проверены переносы, опубликованные Q002–Q011, saved P127/P131 и 3502-path inventory; усилены 22 экспериментальные карточки и 4 тематические planning-карточки.

Основной новый product gap — numerical import Q007: внешний reader отвергает NaN/неполный Step401, сам importer принимает. План сохраняет lawful fresh/sparse states. Старые CAS/Evaluate/owner/lineage/P115/P127/P131 остатки остаются у своих владельцев; принятые части не повторяются. Native этой сессии UNRUN; новых model calls0. **QA11/131, осталось 120, Q012 не назначен; scientific recorded131/131.** Git identities:92 original,3358 прежних изменённых,19 иных,33 additions; это не массовая загрузка LFS. Порядок переносов Q01→Q02(PAIR)→Q03→Q06, Q04 paused/Q05 dependent. Нижние датированные счётчики отражают историю.
<!-- POSTQ011-2026-11-04:END -->


**4 ноября — [исправлены карточки и индексы после POSTQ003](codex-2026-11-04-POSTQ003-CONTEXT-REPAIR.md).** Переносы: первым остаётсяQ01; R42 теперь явно требует восстановления S1, принятый warmup-код сохраняется. Q011/P10-PRIMARY-01 принят root как ROOT_ACCEPTED_SAVED_EVIDENCE_AND_BYTE_REPAIR; принято 11/131, осталось 120. Следующий Q012 не назначен и не начат до нового полного reload. Один P1001 checkpoint восстановлен exact original320620байт;6saved сравнений и8строк панели сверены, checker exit0/2. Исторически1DPO,2/4→2/4 без gain/loss, NLL T2/E2 ухудшился; новых model/native/build/Train0, nativeUNRUN/transferHOLD. [Отчёт Q011](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P1001/results/codex-review-2026-11-04/codex-report.md). Личная приёмка root получена. Основной результат Q011 опубликован root: core `be8db77baa7d62b78fc7deda64eabe5df7a93e9f`, planning `5b92f0d270c1a48a966a7efb39c2780f1d8468fe` (PUBLISHED_MAIN_RESULT); add/commit/push и отдельные ls-remote завершились exit0. Оба дерева после публикации чистые/upstream 0/0; адресный LFS refetch подтвердил exact checkpoint, checkout не выполнялся. SHA нынешней metadata-правки неизвестен. Q010 принят root как ROOT_ACCEPTED_SAVED_EVIDENCE_AND_BYTE_REPAIR: один P0901 S1.weights восстановлен exact original bytes, остальные21 прежний файл сохранены;21 504 пары Float32/84позиции совпали, ответы2/4 и gains/losses0. Уточнён отрыв верного знака от противоположного (+/−); saved checker exit0/2, новых model/native/build/tests0, native acceptance UNRUN, transfer HOLD. [Отчёт Q010](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0901/results/codex-review-2026-11-04/codex-report.md); основной результат Q010 опубликован root: core `7fd19eee3dbbe0f2b85756cdd420a75f754ad163`, planning `0cf7a4890fae3779c817163a3c73ed9fa4e9fd72`; push и отдельные ls-remote exit0. Root подтвердил адресный LFS refetch и равенство original/published/cache/live S1; checkout не выполнялся, native Load UNRUN. Исторически Q009 принят root как ROOT_ACCEPTED_SAVED_EVIDENCE_AND_BYTE_REPAIR: один S1.weights восстановлен exact original bytes, остальные53 прежних файла сохранены; сохранённые ответы2/4 A/B, gains/losses0, учтены все четыре строки ухудшения затрат. Saved checker exit0/2; новых build/Train/Forward/Generate/Sample/native Load0, fresh native UNRUN, transfer HOLD. [Отчёт Q009](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0801/results/codex-review-2026-11-04/codex-report.md); основной результат Q009 опубликован root в core `e2c455dd56bd8ce8f0ad0eef706572107d6934db` и planning `5a720971fcd3bcd3b41be2f1fe5a2702fcd97e31`: push и отдельные ls-remote exit0. Исторически Q008 принят root как saved ASCII evidence repair: один исторический BPE Train сохранил разницу чистых целей0/1, checker исходных данных exit0 и single-position mutant exit2; все18 прежних файлов неизменны. Новых build/Train/Load/Encode/CreateBatch/Forward/Generate0; восстановление не требовалось, fresh native acceptance и план83 UNRUN. [Отчёт Q008](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0701/results/codex-review-2026-11-04/codex-report.md) опубликован root в core `6d0b3cf1cb0b8b137e74e7810b42222b8a63e6dd` (push exit0, отдельный ls-remote совпал). Q007 принят root: восстановлены два original checkpoint P0601, технический native reader проверил состояния A/B и 9 случаев (19 calls с сохранённым первым отказом); новых Train/Forward/Generate 0, обычные ответы после загрузки UNRUN, product empty/NaN gaps OPEN и transfer HOLD. [Отчёт Q007](C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/P0601/results/codex-review-2026-11-04/codex-report.md); Q007 уже опубликован root: основной core `e9bbd9176cf4cc2c2ab1515a2ca84e91f9267ec8`, metadata `99d1fa1d979218c12af6185e804f81523c309482`, planning `55434ca6d3fbf0137c19104255bb4ec2771a3d92`. Предыдущее planning-уточнение Q008 опубликовано в2a3d528734e4d37132dd550571e810ebfafd60db; основная синхронизация Q009 опубликована в planning `5a720971fcd3bcd3b41be2f1fe5a2702fcd97e31`; основная синхронизация Q010 опубликована в planning `0cf7a4890fae3779c817163a3c73ed9fa4e9fd72`; текущая metadata-запись фиксирует уже полученную квитанцию, её собственный SHA ещё неизвестен. Q006 принят root: 30 original файлов P0501/P05Q01 восстановлены исполнителем /root/experiment_q002 и опубликованы вместе с отчётом в core `784e056457230d86a199e936f6d08057af7f0da4`; native Load UNRUN, учебная цель OPEN. Все 131 результата записаны, это отдельный исторический счёт. В Q005 показаны прежняя точность2/4 и пять малых ухудшений NLL. Старые PRIMARY-промпты помечены историческими; для readback исключено скрытое обучение в fixture. Исторический Git-счёт на core67af после Q005: 57 original и 3393 прежних изменённых, не 57 native Load. Прежние числа в датированных блоках ниже исторические.

<!-- POSTQ003-2026-11-03:START -->
## После восстановления Q002/Q003 — сохранность и переносы

[Новый аудит и адресный план](codex-2026-11-03-POSTQ003-ARTIFACT-AND-TRANSFER-AUDIT.md): полная RP01-дельта3502 пути,49 восстановленных pointers,19 путей с конфликтным содержимым и два shared S1, расходящихся с manifest. Уточнены authority P127, InputRoot P131, завершённый recovery Q002 и предел final state P03. Усилены существующие карточки checkpoint/LoRA/DPO/audit. Параллельные документы POST131/P115/P127 сохранены; новые Train и очередь не назначены.
<!-- POSTQ003-2026-11-03:END -->


**Подготовка успешных переносов, 3 ноября:** [P115/P127 — показатели и пропущенные границы](2026-11-03-P115-P127-TRANSFER-BOUNDARIES.md). P115: обычный producer теряет null-output до нового hash, поэтому добавлена адресная приёмка существующего R117-04. P127: исправлено чтение ресурсов двух попыток и TRAIN/EXAM; положительный результат сохранён, обычный вызывающий измеренного механизма ещё не назначен. Новых PRIMARY и параллельной очереди нет.

**После разбора POST131:** [исправлен контекст исполнителя](2026-11-03-POST131-EXECUTOR-CONTEXT-REPAIR.md): разрешён технический ремонт фильтра в назначенном QA, отдельно задана сборка checker P131, уточнены полнота P127 и синтетические контроли. Это документная правка; очередь переносов и исторические результаты сохранены. На проверенном срезе местного merge-конфликта уже нет.

<!-- CODEX-POST131-NAV:START -->
## Действующий срез после P131 — 3 ноября 2026

[Аудит переносов и опытов P126–P131](codex-2026-11-03-POST131-TRANSFER-AND-EXPERIMENT-AUDIT.md) закрепляет core `b27783593745`: **131 из 131 экспериментальных строк имеют RESULT_RECORDED; незаписанного NEXT нет.** Это не означает приёмку 131 продуктового переноса. [Очередь переносов](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#executor-next) остаётся отдельной, первая работа Q01 /135-R01-CONCURRENCY; [индекс экспериментов](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#experiment-next) — её единственный экспериментальный вход. Planning не назначает параллельную очередь.

Новый пакет связывает слабые проверки P127/P131, отставший текущий prompt Q130, точный состав native test acceptance и обоснованный ремонт неверных tests. Полезные результаты и исторические raw сохранены; native приёмка новой правки остаётся UNRUN. [Аудит 149](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-149-post131-transfer-and-experiment-audit-2026-11-03.md) · [план 150](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-150-post131-addressed-repair-plan-2026-11-03.md) · [покрытие 151](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-151-post131-card-coverage-2026-11-03.md) · [договор 49](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/codex-49-post131-output-binding-and-test-coverage-2026-11-03.md).

Нижние POST128 и другие датированные срезы сохранены как история. Их 129/131, future Q130/Q131 и старые NEXT не отменяют текущий учёт и не разрешают повтор PRIMARY. Отрицательный научный результат при целых свидетельствах допустим; несогласованность reported actual с его реальным источником требует отказа проверяющего.
<!-- CODEX-POST131-NAV:END -->


**3 ноября — конфликт разрешён, действующие задания согласованы с новыми публикациями:** [что исправлено после аудита P125](2026-11-03-POST125-LIVE-CARD-CONTEXT-REPAIR.md). Устранены повторные назначения уже исправленного кода, старый NEXT и запрет обоснованно исправлять неверный тест. Соседние P125/P131 получили собственные проверки и QA-вход. Следующее действие брать из двух живых индексов ниже, не из чисел исторического аудита.

<!-- CODEX-POST128-NAV:START -->
## Сверка исполнения после P128 — 3 ноября 2026

[Адресный аудит](codex-2026-11-03-POST128-TRANSFER-AND-EXPERIMENT-AUDIT.md) связывает проверенный core `f8784df0`, результаты P126–P128, остатки продуктовых переносов и десять уточнённых planning-документов. Его 128 из 131 результатов — исторический срез. На опубликованном core `abf83ee797b` уже записан Q129: 129 из 131, следующая незаписанная строка Q130, затем Q131. Первая работа переносов остаётся Q01 / 135-R01-CONCURRENCY; записанный опыт не означает принятый перенос. [Согласование карточек и проверок после получения origin](2026-11-03-POST128-LIVE-CARD-REPAIR.md) сохраняет обе стороны слияния и уточняет проверки без повторного PRIMARY. Отсутствие dotnet/pwsh относилось к среде автора аудита, а не к разрешениям следующего исполнителя.
<!-- CODEX-POST128-NAV:END -->

**3 ноября — QA включает реальные проверки и исправление тестов:** [что исправлено в контексте исполнителя](2026-11-03-QA-TEST-EXECUTION-CONTEXT-REPAIR.md). Убрано наследование общего исследовательского запрета при назначении переноса/аудита кода/ремонта. В живых навыках и договоре46 заданы применение QA/testing/review/debug, свежесть результатов и правила обоснованной адаптации тестов.

**3 ноября — отрицание после P125 и обязательная приёмка:** [дополнительный опыт и исправленные правила](2026-11-03-P125-NEGATION-AND-TEST-TRIAGE.md). P131 описан, но этой документной работой не запускался. Архитектор обязан различать неверную учебную предпосылку и дефект продукта; операционный исполнитель — действительно собирать код, запускать назначенные тесты и передавать отказы архитектору. Точный остаток Q01 сверять в живом реестре: b4af9eef70f уже исправил подтверждение байтов, остальные границы не закрыты его целевым прогоном.

**Исполнителю: единственная рабочая очередь переносов находится [в начале реестра core](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md#executor-next).** Там указаны конкретный следующий остаток, пропуски зависимых/приостановленных строк и принятые части, которые повторять не нужно. Для продолжения самих опытов использовать [другой индекс](C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md#experiment-next). Старые NEXT и числа в датированных аудитах ниже — исторические; они не конкурируют с этими двумя входами.

**3 ноября — исправлен контекст исполнителя после аудита P114–P117:** [что изменено в карточках](2026-11-03-POST117-EXECUTOR-CARD-REPAIR.md). В core обновлены35 экспериментальных карточек и8 переносных документов: текущий вход, собственные хорошие/плохие примеры, навыки и разбор падающих проверок. Исторические опыты и очередь сохранены; новые runtime-проверки не запускались.
<!-- CODEX-POST125-NAV:START -->
## Опубликованный аудит среза после P125

[Сверка переносов, опытов и исправленных карточек](codex-2026-11-03-POST125-TRANSFER-AND-EXPERIMENT-AUDIT.md) привязана к core `9d4d79d27f18` и planning `bd37b3a653ee`. Подтверждённые остатки: BOM/hash candidate file, owner→actual recipient, поздняя отмена оценки, неполные finalizers/comparers и неверный счёт вложенных вызовов. Полезные измеренные результаты сохранены; продукт и raw этим документным этапом не менялись.

Уточнены14 связанных проектных карточек и22 текущих core-входа, включая профилактикуQ126–Q130. Одна очередь переноса остаётся в core01-registry; Q01 — ремонт существующего candidate store, не повтор реализации. Основной аудит заканчиваетсяP125 на9d4d79d; публикационная дельта `3eed42a80a4f` уже содержитQ126. После добавления отдельной будущейP131 на5a885d6c актуальная очередь имеет126/131 записанных результатов; Q127 — следующий незаписанный, без назначения запуска. [Полный аудит142](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-142-post125-transfer-and-experiment-audit-2026-11-03.md) · [план143](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-143-post125-addressed-repair-plan-2026-11-03.md) · [дополнение codex45 о доказательствах](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/codex-45-post125-evidence-consumer-contract-2026-11-03.md).

Ниже — исторические срезы со своими числами и пределами. Они не переопределяют нынешний NEXT и не превращают source-only правку в native PASS.
<!-- CODEX-POST125-NAV:END -->

<!-- POST117-CURRENT-2026-11-03:START -->
## Историческая сверка после P117

**Следующая публикационная дельта 918765f:** [проверенный коммит](https://github.com/lubochka/llm_mvp_core/commit/918765f8ad4e486fab9629b9abb43b0e4c1f6651) уже содержит trusted routing thresholds, finite observations и общий gate .75 до обоих orchestration import. Это source-исполнение части S03/S06; девять новых тестов UNRUN, диапазоны/принадлежность routing evidence и оставшаяся приёмка открыты. В P118 записаны terminal receipt и status COMPLETED; report.md отсутствовал именно на этом pin. На последующем [84fcd8e](https://github.com/lubochka/llm_mvp_core/commit/84fcd8e43550d4fc23ed83c41f304c4d4b7a761d) отчёт уже опубликован и Q118 учтена. Нынешний аудит оставляет научную приёмку P118 UNREVIEWED: 118 записанных и 12 будущих из 130, Q119 — только навигация. Q118 не запускать повторно: текущий этап — сверить существующие report/raw без Train. Объявленные validity и 8/10 не аттестованы научным аудитом. Подробности — в текущем отчёте ниже.

**Поздняя публикационная дельта:** core [c257523](https://github.com/lubochka/llm_mvp_core/commit/c25752314f4c02512236c5c51284bc714d2e3f6c) добавил отрицательную связь actual DPO/text inputs с frozen heldout и чтение receipt через trace. Это частичное source-исполнение R04, с отдельными остатками KNN/положительного покрытия/durable receipt и native UNRUN; неоднозначный consumed hash и вложенный mutable vector уточнены в новом отчёте. Основные научные данные и числа ниже остаются привязаны к879a07b, planning base66c53bb.

[текущая сверка после P117](codex-2026-11-03-POST117-TRANSFER-AND-EXPERIMENT-AUDIT.md) отделяет опубликованный механизм от его приёмки и проверяет точность новых опытов. P115 сохраняет паритет ответа при 156→4 лексических токенизациях; P116 убирает 3 межфайловых перехода; P117 открывает 1 из 5 строк доступ к памяти RU_EN. Предметная точность на соответствующих панелях не выросла. Обнаружены неполные итоговые проверки, подмена фактического KNN-экспорта ожидаемым журналом, exposure экзамена при обучении BPE и неполный учёт диагностического обучения.

Материалы уточнения: [аудит 138](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-138-post117-transfer-and-experiment-audit-2026-11-03.md), [ремонт 139](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-139-post117-addressed-repair-plan-2026-11-03.md), [покрытие 140](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-140-post117-card-coverage-2026-11-03.md), [контракт 44 для копируемых поручений](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/codex-44-post117-saved-evidence-and-finalizer-contract-2026-11-03.md). R117 — адресные уточнения существующих владельцев [единого registry](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/01-registry.md), а не самостоятельная очередь. Продуктовые zero-weight, key/ValueKind, CAS и cache исправления уже есть в исходнике; новые нативные проверки здесь не запускались.

Срез core 879a07b, planning 66c53bb. По Q-таблице на этом срезе 130 строк:117 RESULT_RECORDED и 13 DESIGNED_NOT_RUN, первая Q118. Это навигация, не разрешение запуска и не 117 принятых переносов. Дальнейшие счётчики 114/16/NEXTQ115 и последующие старые заголовки относятся к их историческим срезам; текущий порядок берётся из живого индекса core. Ошибка документа, JSON или проверяющего не назначает повтор PRIMARY. Сначала используются сохранённые raw/состояния/ledger; новый consumer-run требует отдельного назначенного объёма. Исторические результаты, gold и бюджеты сохранены.
<!-- POST117-CURRENT-2026-11-03:END -->


## Исправление контекста после POST113

[Журнал текущей правки карточек](2026-11-02-EXECUTOR-CONTEXT-REPAIR-AFTER-POST113.md) отделяет исполненные PRIMARY от следующих ремонтов и уточняет новый продуктовый перенос P112: проверенный provider ledger должен быть связан с материалом, который действительно получает обучатель. Срез core `80e8983` уже содержит результат P115; в индексе 115 записанных результатов/15 незапущенных, первая Q116. Эти числа не означают 115 успешных переносов и не являются разрешением повторить опыт.

На момент проверки Git прежний merge уже завершён, блокировки отсутствуют, local/origin совпадали. Я не выполнял merge/commit/push и не удалял блокировку; последующие параллельные коммиты учитываются отдельно. Нижние записи отражают предыдущие аудиты; новые условия и пределы проверки приведены в журнале.

<!-- POST113-CURRENT-2026-11-02:START -->
## Сверка после P113 — действующие исполнение и карточки

Свежий [аудит](codex-2026-11-02-POST113-TRANSFER-AND-EXPERIMENT-AUDIT.md) подтверждает выполненные M02a/M03c/основной M04/S02 и выделяет 17 адресных разрывов приёмки/публикации. В 130 Q-строках 114 результатов, включая частичный Q002,16 не запускались; следующая Q115. Записанный результат не равен принятому переносу.

Проверены первичные материалы P109–P113 и текущие исправления P104/P107/P108. [План 135](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md) задаёт R01–R09 и открытые границы T110–T113. [Покрытие 136](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-136-post113-card-coverage-2026-11-02.md) перечисляет 25 экспериментальных и 12 документов переноса; ошибочный oracleQ129 исправлен под уже реализованный staging. Новые code fixes здесь не объявляются выполненными.

Основание: [сводка этого аудита](codex-2026-11-02-POST113-TRANSFER-AND-EXPERIMENT-AUDIT.md), [core 134](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-134-post113-transfer-and-experiment-audit-2026-11-02.md), [план 135](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-135-post113-addressed-repair-plan-2026-11-02.md). Core `21007e25ec3f6f6634dd8733fc8631ae267e518c`, planning `5fc34f3c19a12d46b7e48707aea89c28b6ec32a0`, 2 ноября 2026. Нижний текст сохраняет свой исторический срез; он не получает новую нативную приёмку и не переопределяет нынешний контракт. Новые Train/inference/PowerShell/.NET этим аудитом не исполнялись; данные, дозы и очередь сохранены.


**Последующая публикационная сверка core 1aaa3bc:** основной canonical writer и ряд readers/imports M03 уже реализованы, но шесть нативных проверок UNRUN. Основной builder-case N15 и прежняя source-причина N17 больше не назначаются как отсутствующие. Открыты version/default, candidate N16, RULE/restore и новая N18: TryGetProperty на non-object JSON выходит исключением до typed trainer. Всего пакет содержит 18 находок на трёх срезах с раздельным статусом, а не 18 одинаково открытых ошибок последней вершины. Подробности: [публикационная сверка](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/experiments-v2/_audits/2026-11-02-post113-audit/publication-reconciliation.md); актуальный ремонт — 135/R09. Q129 и условный прогноз 39 AddPair сохранены.
<!-- POST113-CURRENT-2026-11-02:END -->

## Подготовка P110–P113

[Новый разбор и планы 130–133](2026-11-02-P110-P113-POSITIVE-TRANSFER-DELTA.md) основаны на core `21007e25ec3f6f6634dd8733fc8631ae267e518c`: адресность исправляющего урока, фактический учебный материал и независимость семейств, доменная нормировка каналов, численная точность проекции. Полезные эффекты сохранены вместе с регрессиями и условиями переноса. Планы записаны в живой каталог core; продуктовый код и runtime этой задачей не менялись.

После начала разбора общий HEAD продвинулся до `36866e5bcf0321aa830e771493853b92637aeb8d` и содержит P114. Его результаты здесь не аттестованы. Счётчики и NEXT в нижних исторических разделах относятся к указанным там SHA; действующая очередь остаётся в индексе core. Статус локальных редакций и пределы проверки описаны в новом разборе, публикация всего пакета не заявляется.

<!-- POSTMERGE-AUDIT-2026-11-02:START -->
## Историческая сверка после local merge — 2 ноября 2026

Новый проверенный срез: core `3cd2f26ced57da3acc4cd8a860c716a6bdac8a1e`, planning base `568758eaa5f86c4df5a4fb787c221b96e0eaa7ca`. [POSTMERGE-аудит](codex-2026-11-02-POSTMERGE-TRANSFER-AND-EXPERIMENT-AUDIT.md) уточняет предыдущий POST107: опубликованные конфликт-маркеры, тестовые требования без переноса, UTF-8 целой последовательности, чтение импортированного target и полнота расхода P66 проверяются раздельно.

Оба договора 120 теперь доступны в опубликованном core; две прежние ссылки C:/ ниже заменены repository URLs. Прежние сообщения об их отсутствии описывают старый срез. Исторические raw и результаты не заменяются. Восемь карточек получили адресные требования; это не новая научная аттестация всей библиотеки. План исправлений: [core 127](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-127-postmerge-addressed-repair-plan-2026-11-02.md).

Основание: [Аудит нового среза](codex-2026-11-02-POSTMERGE-TRANSFER-AND-EXPERIMENT-AUDIT.md); неизменные исходные результаты и назначенные бюджеты сохраняются.
<!-- POSTMERGE-AUDIT-2026-11-02:END -->

**Исторический срез после P108:** [сохранность признаков и адресный план переноса](2026-11-02-P108-POSITIVE-TRANSFER-DELTA.md). Измерены 4/12→12/12 доставок при 12/12→12/12 ответов; превышение 40/38 и 2 Train сохранено. На этом срезе core-карточка и индекс были согласованы с уже существующим результатом: 108 recorded / 22 designed, следующим был Q109. Эти счётчики и NEXT не задают текущую очередь: её источник — [индекс core](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md). Это планирование узкого ремонта, без нового runtime; POST107 ниже — прежний срез.

<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

Актуальный связанный срез — core e235f0971d6172f020822d42c61fe34a2819f9f8; planning base ee34c1a3cefac92c0b4b7b4c8001586dfa5da208. [POST107-аудит и соответствие планов](codex-2026-11-02-POST107-TRANSFER-AND-EXPERIMENT-AUDIT.md) уточняет прежний POST102. Очередь core: 107 RESULT_RECORDED /23 DESIGNED_NOT_RUN,следующий Q108.

Сохранены положительные результаты и опубликованные ремонты terminal/finite gates. Отдельно открыты: saved-gate +Infinity, четыре искусственных false P104, P107 control/exit и budget, PAIR semantic authority, token roles P106, import threshold. Усилены 9 исходныхкарточек planning; дозы и научные raw не изменяются. Полный набор назначений и конкретные файлы — в [аудите core](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/codex-123-post107-transfer-and-experiment-audit-2026-11-02.md).

При согласовании сохранён concurrent commit 266f15da (`local audit`). Его локальные ссылки C:/ на два договора120 не имеют целей в опубликованном core проверенного SHA; эта граница доступности описана в POST107 и S16. Их содержание не угадывалось.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->


<!-- POST102-AUDIT-2026-11-02:START -->
## Действующее дополнение после P102 — 2 ноября 2026

[Аудит переносов, исполнения и контекста](2026-11-02-POST102-TRANSFER-AND-EXECUTOR-AUDIT.md) фиксирует свежие ошибки P102, реальную границу переноса P85 и адресный план. Усилены7исходных planning-карточек,44экспериментальные и12transfer карточек core. Scientific raw и дозы сохранены; новых Train/native запусков нет. Текущая очередь core130Q:103recorded/27designed, следующийQ104. Прежние summaries POST89/POST101 относятся к своим SHA, четыре Q127–Q130 теперь опубликованы.
<!-- POST102-AUDIT-2026-11-02:END -->

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход перед новым аудитом или заданием

Обязателен [договор120](https://github.com/lubochka/llm_mvp_core/blob/3cd2f26ced57da3acc4cd8a860c716a6bdac8a1e/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

До действий прочитать выбранную карточку целиком и её локальную секцию120, source/raw и фактический маршрут producer→ordinary caller→learner→consumer; local HEAD, published SHA и dirty bytes различаются. Списки навыков ниже — дополнительные, не освобождение от полного свежего стека120. Исторические Tier3/vocab/split/UI/Git/model правила не расширяют нынешний паспорт и авторизацию.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Missing API/filter/данные вернуть автору как точный HOLD, не выбирать замену. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.

**Сохранение постановки:** exact recorded result не повторять, old NOT_RUN не выше текущей Q-строки. Полный outer/nested бюджет и независимый gold остаются у карточки; пересчёт raw/структурный guard сTrain0 не заменяет обязательный учебный цикл и не получает фиктивный Train. Команды, API и фильтры не додумывать исполнителю. Вход обязателен также в передаваемом промпте выбранного ID.
<!-- EXECUTOR-CONTEXT-120:END -->

**Связь с прежним аудитом:** опубликованный119 уже усилил42 карточки. [Матрица120](https://github.com/lubochka/llm_mvp_core/blob/3cd2f26ced57da3acc4cd8a860c716a6bdac8a1e/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-executor-context-coverage-2026-11-02.md) фиксирует новый охват живых инструкций, не научную аттестацию и не новый результат исполнения.


<!-- POST101-AUDIT-2026-11-02:START -->
## Текущее дополнение — 2 ноября 2026

[2026-11-02-POST101-TRANSFER-AND-EXPERIMENT-AUDIT.md](2026-11-02-POST101-TRANSFER-AND-EXPERIMENT-AUDIT.md) связывает planning `6ed289b26ce198e16890fe4c7dd6b0ae55f885d9` и core `3194310941c428ef9a5e643c4dc9d7ad7d15447e`. Подтверждены уже внесённые узкие переносы; открытые decoder/Final-metric/atomicity/eligibility/consumption задачи имеют конкретный [план118](https://github.com/lubochka/llm_mvp_core/blob/ecd0642d20f726adbc50606680621662df5dc93a/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/118-post101-addressed-repair-plan-2026-11-02.md).42 карточки усилены адресно, из них33 вне P93–P101. Новых native runs/Train в аудите0.

**Текущий опубликованный учёт результатов:101 recorded/25 designed из126 Q, следующийQ102/P102.** Это наличие результатов с оговорками, не101PASS. Единственный источник NEXT — [индекс core](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md); исторические NEXT/OPEN/DONE ниже относятся только к своим срезам. P99 revised scope и P101 copied/native границы не скрыты.
<!-- POST101-AUDIT-2026-11-02:END -->


<!-- POST89-EXECUTION-AUDIT-2026-11-01:START -->
## Актуальная проверка после P89

[Новый аудит и границы публикации](2026-11-01-POST89-EXECUTION-BOUNDARY-AUDIT.md) / [core116](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/116-post89-execution-boundary-audit-2026-11-01.md) уточняют прежнюю сводку. Исправлены 45 строк исполнения и три входа продолжения; актуально 92 recorded/34 design из 126 Q на проверенной опубликованной ветке. Дополнительно усилены 20 карточек; исходные 53 относятся к прошлому пакету. Данные опыта и состояние продукта не менялись; протоколы новых маленьких audit probes указаны отдельно. Четыре локальных заметки теперь явно ограничены своими source snapshots и не подтверждают отсутствующие remoteQ127–Q130.
<!-- POST89-EXECUTION-AUDIT-2026-11-01:END -->


<!-- POST88-AUDIT-2026-11-01:START -->
## Действующее дополнение после опыта88

[Аудит исполнения, новые ошибки и план ремонта](2026-11-01-POST88-TRANSFER-AND-EXPERIMENT-AUDIT.md) закреплён на core `dbf4c99c519b76485f618f4a2043d7df024db88e`. В нём53 адресные карточки, независимый пересчёт P82–P88 и свежие native/component проверки без нового обучения. Утренний срез ниже сохраняется как история; его статусы нельзя читать без этой актуализации.
<!-- POST88-AUDIT-2026-11-01:END -->


<!-- EFFECTIVE-AUDIT-2026-11-01:START -->
## Действующая сверка — 1 ноября 2026

[2026-11-01-TRANSFER-EXECUTION-AND-EXPERIMENT-AUDIT.md](2026-11-01-TRANSFER-EXECUTION-AND-EXPERIMENT-AUDIT.md) связывает planning `1bec1b8907a87dc2f768bb950a488732ff925a00` с core `790228c256168a336b01abae4387e28391048c0c`. Очередь102: **6 DONE / 2 PARTIAL / 1 OPEN / 1 CONDITIONAL**;89 карточек усилены. R6/R7 нельзя закрыть только наличием реализации. Точный остаток и признанные уже работающими переносы — в новом audit/repair package.

Все старые NEXT, PR-статусы и численные шкалы ниже относятся к своим историческим срезам. Единственная действующая научная очередь остаётся у core execution index; этот audit её не меняет и нового обучения не назначает.
<!-- EFFECTIVE-AUDIT-2026-11-01:END -->

**Публикационная сверка:** сохранены новые материалы этой ветки до `9b58737da5183da37af5685e86f9e89d3eabe895`. Core дополнительно проверен на `aab5e5143f2516b92b42ce82606759bb6b4ef0b2`: R8 завершён в назначенном объёме, R7 частичен из-за tie-case; текущая очередь102 —5/3/1/1. Исходный срез аудита3b7537c/cbf3b1d остаётся историческим.

## Актуализация переноса — 30 октября 2026

[Новая сверка исполнения, точности и карточек](2026-10-30-TRANSFER-EXECUTION-AND-EXPERIMENT-VALIDITY-AUDIT.md) связывает этот срез с core `cbf3b1d834cbf28baa2bb13baeb1bddc17e21019`. В текущем ремонте102:5 завершено,3 частично,1 открыто,1 условно. Коммит, исправленный native API, публичный consumer, полезный эффект и восстановимый state — разные уровни подтверждения. Изменения описаний не означают новый научный запуск.

**Статус: документные изменения в review.** Core-пакет опубликован в [черновике PR100](https://github.com/lubochka/llm_mvp_core/pull/100), commit `2fad6d60cb0037504a453489325b4341e4094d35`. Принятие в рабочую ветку и runtime-приёмка учитываются отдельно. Закреплённые основания: core `7d0817d4693c713789b25c75798a2971c9add14a`, planning `3b7537c267e45002ebab4f44845fe4ce92f204f9`.

[Мост текущего аудита29.10](2026-10-29-CURRENT-TRANSFER-AUDIT.md) разделяет уже подтверждённые исправления95 и десять оставшихся находок A29-01…A29-10: constructor/cache, disk fixtures, привязка запуска, причинные выводы P23, предоставленные признаки P28, точные координаты P26, исполнимость P30, распространение требований, владение metadata plain KNN и неполный persisted S1 P24.

- [97 — доказательства и границы проверки](https://github.com/lubochka/llm_mvp_core/blob/2fad6d60cb0037504a453489325b4341e4094d35/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/97-current-transfer-audit-2026-10-29.md).
- [98 — адресный ремонт, зависимости и бюджеты](https://github.com/lubochka/llm_mvp_core/blob/2fad6d60cb0037504a453489325b4341e4094d35/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/98-current-transfer-repair-plan-2026-10-29.md).
- [99 — приёмка и полная карта усиленных владельцев](https://github.com/lubochka/llm_mvp_core/blob/2fad6d60cb0037504a453489325b4341e4094d35/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/99-card-acceptance-contract-2026-10-29.md).

В пакете усилены **56 owner-файлов /62 stable ID**, четыре общих договора **02/03/04/90**, семь паспортов **P23/P24/P26/P27/P28/P29/P30** и общий экспериментальный договор34. Это документное покрытие; все558 элементов possibleDifs не объявлены глубоко проверенными, новое обучение не выполнялось. Ссылки97/98/99 закреплены на опубликованный commit core.

**Единственная экспериментальная очередь — [канонический индекс core](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md). Исторический срез29.10: тогда следующий PRIMARY был Q030/P30; текущий следующий указан в дополнении02.11 выше.** Этот README не создаёт отдельную очередь; более новый действующий статус core имеет приоритет. Полный прежний README ниже сохранён побайтно как история: его прежние NEXT и даты относятся к прежним срезам.

---

# Дополнение 20.10.2026: доказательства обучения и объединённая очередь

**Срез core `6e3e8864`, planning `d7c41ad3`: 42 различных ID, 7 завершённых опытов, 35 будущих; следующий — №08 `N63-INGRESS-LENGTH`.** LF-03 уже завершён по A/B receipts; прежние заголовки NEXT LF-03 ниже относятся к более ранним снимкам и не назначают повтор.

Новый [доказательный пакет](2026-10-20-learning-evidence-827a1acd/README.md) добавляет два отдельных фактора — `ATTN-TRANSPOSE-STRIDES-01` и `JSON-OPTIONS-REUSE-01`, сохраняет четыре новые гипотезы других агентов без дублирования, сохраняет семь успешных кандидатов с регрессиями и уточняет неполную TRAIN-панель и происхождение LF-03. EOPT erratum и исправленный KN observer учтены; второй observer не нужен.

- [Объединённый порядок и буквальные NEXT](2026-10-20-learning-evidence-827a1acd/04-execution-registry.md)
- [Измеренные результаты и кандидаты](2026-10-20-learning-evidence-827a1acd/01-results-and-candidates.md)
- [Последняя сверка и ограничения доказательств](2026-10-20-learning-evidence-827a1acd/12-concurrent-planning-d7.md)
- [Историческое обучение, KNN и границы доменов](2026-10-20-learning-evidence-827a1acd/06-historical-evidence.md)

Фаза — документация. Protected каталог, product, tests, веса и старые эксперименты этой работой не менялись. Полный прежний README сохранён ниже.

---

## 2026-10-20 — Проверка обучения, новые факторы и согласование очереди

**Документированный срез: core `6e3e8864933e35fccdb4f21369c9912fa195aaf6`, planning `eaed843ad2b063fbb003f073deb69da19f0cb135`. Следующий ещё не выполненный ID — №08 `N63-INGRESS-LENGTH`.** LF-03 уже завершён; прежние NEXT на него, E-OPT-STEP и N62-R2 ниже относятся к более ранним срезам и не назначают повтор.

- [Вход в полный аудит](2026-10-20-training-delta-669477/README.md): первичные результаты RP01, историческое обучение и карта различий DPO/LoRA/BPE/kNN/Units/restore.
- [Семь измеренных кандидатов с регрессиями и ограничениями доказательств](2026-10-20-training-delta-669477/02-measured-candidates.md). Полезное улучшение одной оси сохраняется как успех по этой оси и предметной теме; рост точности не обязателен.
- [Единый реестр:40 ID,7 completed,33 future](2026-10-20-training-delta-669477/04-execution-registry.md) · [Точные пути и команды](2026-10-20-training-delta-669477/05-output-contract.md). GRAD→FROZEN соседнего пакета сохранены, затем собственные VT→RMS; зависимые readers последними.
- Новые отдельные проекты этого аудита: [VT-APOSTROPHE-01](2026-10-20-training-delta-669477/cards/VT-APOSTROPHE-01.md) и [RMS-ROW-REDUCE-01](2026-10-20-training-delta-669477/cards/RMS-ROW-REDUCE-01.md). Их эффект ещё не измерен.
- [Проверка экспериментов и семь незакрытых recipe dependencies P54](2026-10-20-training-delta-669477/03-experiment-coverage.md) · [Последнее согласование core6e и planning eaed](2026-10-20-training-delta-669477/10-final-reconciliation.md) · [Источники, hashes и независимый review](2026-10-20-training-delta-669477/07-evidence-and-review.md).

Сейчас выполнено только документирование. Runtime/build/обучение и product fixes не запускались; защищённая папка core не изменялась. Немедленный STOP/NEXT выше дооформления прежнего опыта. Старые эксперименты, результаты и весь прежний README сохранены ниже без изменения bytes. Эта запись не отслеживает будущие запуски других сессий автоматически.

---

<!-- RP01-PRIMARY-READBACK-BPE-REALIZED-2026-10-20-BEGIN -->
## Дополнение после пакета63: исходный RP01, DPO→Units и новая BPE-гипотеза

**[Открыть доказательный пакет](2026-10-20-learning-delta-audit-883496/README.md)** · [Первичные результаты RP01](2026-10-20-learning-delta-audit-883496/16-RP01-primary-readback.md) · [Текущий DPO→Units](2026-10-20-learning-delta-audit-883496/17-current-dpo-unit-transfer.md) · [Новая BPE-карточка](2026-10-20-learning-delta-audit-883496/08-BPE-REALIZED-MERGE-COUNT-01.md) · [Единая очередь](2026-10-20-learning-delta-audit-883496/05-execution-registry.md).

Дополнение согласовано с planning9d311 и core883496. Прочитанные первичные C#/ENRU scientific-result JSON фиксируют настоящее изменение LoRA, небольшой положительный preference margin и отсутствие прибавки собственных правильных ответов0/3 и0/4. Полезный margin сохранён вместе с ухудшениями по отдельным случаям; это не переименовано в отсутствие обучения. Текущие memory-Units и neural DPO routes сопоставлены по конкретным producer/consumer, без подмены одного другим.

**Единственный новый отдельный фактор — BPE-REALIZED-MERGE-COUNT-01.** Проверяется альтернативное правило выбора merge во время настоящего BPE Train; ожидаемые выигрыши и проигрыши профилей заранее показаны. Это DESIGN/NOT_RUN. Наш независимо подготовленный SKNN признан тем же фактором, что уже опубликованный AST-SIB-CACHE-01: сохранён только как reference, без второго запуска. ROPE, AST, NG01 и прежние результаты не удалены и не переименованы в новые находки.

**№01–04 завершены; NEXT — №05 N62-R2.** Согласованная программа содержит36 различных пунктов и32 будущих: **05–15 →17–31 →ROPE →AST →NG01 →BPE-REALIZED →исходный16 IDENTITY →исходный32 KN-ROUTER →END**. Исходные номера служат ссылками на прежние карточки. Порядок и точные переходы определены новым [реестром](2026-10-20-learning-delta-audit-883496/05-execution-registry.md), а все28 прежних result-root/entry из63/14 сохранены в [одной карте выходных путей](2026-10-20-learning-delta-audit-883496/15-output-path-contract.md); добавлен только BPE root. Это документный порядок, не запуск очереди.

Структурно проверены153 карточки; полный protected Markdown-корпус получен и проверен по Git blob SHA334/334. Учтены747 прежних и20 свежих planning Markdown. Это не аттестация470 исполнимых опытов. Защищённая core-папка, продуктовый код, веса и прошлые эксперименты не менялись; новых build/train/inference нет. Четыре сохранённых полезных результата остаются кандидатами для решения пользователя, с регрессиями и точными границами доказательств. [Происхождение и дедупликация](2026-10-20-learning-delta-audit-883496/18-concurrent-reconciliation.md).
<!-- RP01-PRIMARY-READBACK-BPE-REALIZED-2026-10-20-END -->

<!-- N63-LEARNING-RESULTS-READBACK-2026-10-20-BEGIN -->
## Результаты обучения, проверка рецептов и новые гипотезы — срез core 883496

**[Пакет 63 — результаты и статусы](63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/README.md)** · [Реестр и порядок](63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/04-REGISTRY.md) · [Четвёртый результат N62-R1](63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/13-LATEST-N62-R1-READBACK.md) · [Актуальные команды, TRAIN-панели и пути](63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/14-CORE883-PROTOCOL-DELTA.md).

Пересчитаны сохранённые raw A/B: BPE allocations −71,14%/−69,11%; frozen transpose −11,04% при retained cache +249 856 байт; N-KU убирает ложный ADD_LOOP 3/3→0/3, но теряет нужный совет 1/1→0/1. В N62-R1 allocations потока учебного цикла уменьшились на 90 504 байта (0,002404%), wall-фаза applyOptimizer — на 79,7891%; GC Gen0/1/2 каждый вырос на 1. Финальное качество N62-R1 не измерялось, а одна A→B пара не доказывает устойчивого причинного ускорения. Четыре кандидата уже зарегистрированы; повтор не назначен. Исторический E09 tier1 с FAILED имеет полезный +1,04 п.п.; поздний tier2 и унаследованные результаты соседних веток разобраны с их границами.

Census: 153 карточки / 125 P-семейств / 470 сопоставленных подслучаев / 32 исходные строки очереди. Это не 470 готовых hosts. **№01–04 завершены; следующий — №05 N62-R2.** Для 28 оставшихся исходных строк в документе 14 заданы актуальные entry-команды, 26 разрешённых замен protected output roots и две прежние допустимые позиции. Новые TRAIN diagnostics для 15 будущих нейронных факторов отделены от updates и стоимости обучения; wrapper не запускается дополнительно к старой цепочке.

Два новых точных проекта: **ROPE-TRAIN-TABLE-01** и **AST-SIB-CACHE-01**, оба NOT_RUN, с native sources, промптами и бюджетами. Формулы DPO sign/mask и exact ordered KNN corpus restore уже присутствуют в нынешнем коде; прежние описания отсутствия не перенесены на него автоматически.

[Согласование внешнего пакета 6bc4](63-LEARNING-RESULTS-AND-EXPERIMENT-READBACK-2026-10-20/12-CONCURRENT-RECONCILIATION.md): сохранены NG01, P54 и аудит передачи 133 промптов. Общий маршрут по актуальному реестру: **05–15 → 17–31 → ROPE → AST → NG01 → исходный16 IDENTITY → исходный32 KN-router → END**; всего 35 строк, четыре завершены. P54 допускается как проверка чтения выученного состояния, но его семь вариантов ещё не имеют всех точных входов для исполнения.

Пакет содержит документы и сверку сохранённых данных. В рамках этого аудита новых обучений, сборки, продуктовых изменений и записей в защищённую core-папку нет. Старые исследования ниже сохранены. Правила успеха по любому полезному измеренному показателю, видимых регрессий, default модели и немедленных STOP/NEXT включены в новые карточки и промпты.
<!-- N63-LEARNING-RESULTS-READBACK-2026-10-20-END -->

## 20 октября 2026: результаты обучения и новые проверяемые пробелы

Добавлен [доказательный пакет аудита RP01](2026-10-20-learning-results-and-new-gaps/README.md): текущий код, RAG и родительская архитектура, сохранённые успешные опыты и соседние ветки, сверка карточек и промптов. Защищённая папка `llm_mvp_core/docs/plans/replanning-possible-diffs` не изменена; новые запуски и изменения production не выполнялись.

- [Порядок исполнения](2026-10-20-learning-results-and-new-gaps/08-execution-registry.md): первые три опыта завершены, следующий — **№04 N62-R1**. Новый NG01 расположен перед заключительным reader-опытом.
- [Проверка измерений](2026-10-20-learning-results-and-new-gaps/02-learning-measurement-audit.md) и [три измеренных кандидата на исправления](2026-10-20-learning-results-and-new-gaps/CANDIDATES.md): польза по каждой метрике и домену, регрессии рядом.
- [Семь дополнений P54](2026-10-20-learning-results-and-new-gaps/06-P54-learned-reader-addendum.md): доказанная связь reader обученного состояния с обратной связью обучения, точные недостающие входы.
- [Новая карточка NG01](2026-10-20-learning-results-and-new-gaps/cards/NG01-null-guard-label.md): узкий контрпример внутри известного семейства null-guard, точный малый native-эксперимент; результат пока не измерен.
- [133 встроенных промпта и полная оболочка передачи](2026-10-20-learning-results-and-new-gaps/09-prompt-handoff-audit.md): STOP/NEXT, default-модель, назначенная очередь и русский отчёт.

Ниже полностью сохранён прежний README. Это добавочное уточнение; старые эксперименты и результаты не удалены.

---

<!-- N61-CAUSAL-LEARNING-AUDIT-2026-10-20-BEGIN -->
## N61: различия обучения, проверка измерений и новые точные гипотезы

[Начать с доказательного пакета](61-LEARNING-UNBLOCK-AND-EXPERIMENT-AUDIT-2026-10-20/README.md) · [Покрытие экспериментов](61-LEARNING-UNBLOCK-AND-EXPERIMENT-AUDIT-2026-10-20/01-PROTECTED-EXPERIMENT-COVERAGE.md) · [Сверка последних параллельных отчётов](61-LEARNING-UNBLOCK-AND-EXPERIMENT-AUDIT-2026-10-20/12-CONCURRENT-UPDATES-AND-NOVELTY.md)

Новые адресные находки: выбранные строки LM-head после полного Transformer и конкретный owner guard, смешивающий нулевую дельту LoRA с ошибкой выполнения. Исправлены постановка Verifier E1 и категории измерения E2, конкретизированы PAD и training-write T. Исторические полезные выигрыши показаны рядом с регрессиями; неподтверждённые причины не названы измеренными результатами.

Проверенный основной реестр: 125 P-карточек / 470 подслучаев / 133 промпта; 555/555 относится к прежним источникам, а не ко всем новым пакетам. Срезы core a822 и planning fb9aadf, а также поздняя дельта c38 учтены отдельно. Защищённые документы не изменены; отсутствующие точные эксперименты перечислены явно. В каждый новый паспорт и промпт включены правило успеха по любой полезной метрике и порядок проверки на точной экспериментальной копии. Нового runtime этот аудит не заявляет.
<!-- N61-CAUSAL-LEARNING-AUDIT-2026-10-20-END -->

<!-- CURRENT-LEARNING-AUDIT-2026-10-20-BEGIN -->
## Аудит 20 октября: новые гипотезы, актуальные эксперименты и порядок обучения

[Начать с нового пакета](2026-10-20-current-learning-audit/README.md) · [Прямое обучение → обязательные обновления → зависимые readers](2026-10-20-current-learning-audit/00-LEARNING-FIRST-ACTION-MAP.md) · [Проверка текущего каталога](2026-10-20-current-learning-audit/coverage/LATE-HEAD-RECONCILIATION.md) · [Последняя дельта f5dd](2026-10-20-current-learning-audit/coverage/FINAL-DELTA-f5dd.md).

Добавлены два source-confirmed проекта без вымышленных результатов: повторное использование frozen reference scores внутри настоящих DPO updates и согласование выученных C#/.NET keyword-признаков с их reader. Приложены небольшие входы, точные original-code изменения, метрики и промпты. Уточнены уже существующие N60-E1/E2; они не объявляются отсутствующими. Обновление phrase_mappings уже внесено: 11/24→19/24 при wrong-accepted 1→3.

Проверены 125 карточек / 470 подслучаев / 133 промпта и ссылочное покрытие всех 555 исходных Markdown. Это не сертификат семантической готовности каждого подслучая. Снимки и пределы чтения сохранены; последняя проверенная core-дельта — f5dd532045716495a2c7668573250e5d7cc46810. Исторические запреты и требования нового личного допуска каждого ID/SHA ниже не заменяют действующее уточнение 20.10. Любой полезный измеренный выигрыш — успех по своей метрике и домену; ухудшения показываются рядом.

Новых запусков обучения, изменений core и защищённой папки этим аудитом нет. Старые исследования ниже сохранены; новая навигация не назначает повторный запуск всей библиотеки.
<!-- CURRENT-LEARNING-AUDIT-2026-10-20-END -->

---

<!-- N61-LEARNING-FIRST-AUDIT-2026-10-20-BEGIN -->
## Актуальный аудит20 октября: учебная цепь, новые kNN-гипотезы и проверка экспериментов

**[Начать с пакета61 — результаты, доказательства и конкретные малые опыты](61-LEARNING-FIRST-AUDIT-2026-10-20/README.md).** В нём: текущий DPO/LoRA против legacy, положительные первичные результаты соседних веток, независимый пересчёт ingress11/24→19/24 с wrong-accepted1→3, две новые Mode1-гипотезы, проект ограниченного нейронного caller и карта125семейств/470подслучаев. Новое native обучение этим аудитом не выполнялось; причинность новых исправлений пока не измерена.

**Обновление core учтено.** На `de4ef0f` уже отменены общая блокировка карточек и повторное одобрение каждого SHA; loader/restore/consumer допускаются при доказанной зависимости обучения и точечном source repair. [Сверка свежих изменений и оставшихся вопросов](61-LEARNING-FIRST-AUDIT-2026-10-20/06-LIVE-CORE-DELTA.md). Прежние `PENDING`, `GOLD GATE` и обязательные персональные согласования ниже — исторические формулировки; они не отменяют актуальное поручение пользователя. Разрешённый scope и техническая готовность проверяются отдельно.

[Порядок и все направления запроса](61-LEARNING-FIRST-AUDIT-2026-10-20/00-LEARNING-FIRST-ORDER.md) · [125-карточный индекс](61-LEARNING-FIRST-AUDIT-2026-10-20/04-CARD-INDEX.md) · [Новые опыты и промпты](61-LEARNING-FIRST-AUDIT-2026-10-20/02-HISTORY-AND-KNN-HYPOTHESES.md) · [DPO micro-caller](61-LEARNING-FIRST-AUDIT-2026-10-20/03-NEURAL-LEARNING-AND-MICRO-CALLER.md).

Защищённую core-папку этот аудит не редактирует. Семантическое покрытие не приравнено к инвентаризации555planningMD или извлечению470подслучаев; точные границы и неизменность248локальных protected-копий указаны в [источниках](61-LEARNING-FIRST-AUDIT-2026-10-20/05-SOURCES-AND-REVIEW.md). Остальные материалы ниже сохранены.
<!-- N61-LEARNING-FIRST-AUDIT-2026-10-20-END -->

<!-- N58-REVIEW-GATED-SOURCE-LAW-AUDIT-2026-10-19-BEGIN -->
## Дополнение N58: успешная memory ветка, честное покрытие экспериментов и отдельная новая source-гипотеза

[Исследовательский пакет N58 (7 документов)](58-KNN-CONTAINMENT-AND-APPROVAL-2026-10-19/README.md) содержит исходный code path, историческое отличие реального улучшения Executor 0/24→19/24 от neural RP01 0/4, проверку protected P01–P125/470, паспорта коротких экспериментов с model/skills/inputs/prompts/baselines/expected results, прямой human gate. Защищённую core-папку `docs/plans/replanning-possible-diffs/**` не изменяли. **Никаких новых native экспериментов/обучения; все K1–K3 PENDING персонального разрешения.**

**Коррекция after duplicate-screen:** [K1/K2](58-KNN-CONTAINMENT-AND-APPROVAL-2026-10-19/02-KNN-CONTAINMENT-AND-NEGATION-HYPOTHESES.md) про source containment/not/no уже содержались в [N27](27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md) и [N37](37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md): теперь это только уточнённые репликационные micro-protocols, НЕ новые открытия. **[Новая кандидат-гипотеза K3](58-KNN-CONTAINMENT-AND-APPROVAL-2026-10-19/06-NEW-PREDICATE-COMPLEMENT-LAW-AND-SMALL-EXPERIMENT.md)**: уже распознаваемые SourceLaw состояния safe/unsafe, valid/invalid, enabled/disabled при разных `predicate` возвращают UNKNOWN вместо CONTRADICTION (при иных отрицаниях могут быть и эквивалентны). Источник и формула проверены; обычный Flow/предметный прирост NOT_RUN. Новизна среди всех исследований не гарантируется без исчерпывающего дубликатного прохода.

<!-- N58-REVIEW-GATED-SOURCE-LAW-AUDIT-2026-10-19-END -->

## N58 · 19 октября: независимый аудит RP01 и паспорта на утверждение человеком

- **[N58 — доказательная карта деградации RP01](58-SOURCE-EXPERIMENT-CROSSWALK-2026-10-19.md)**: реальные положительные доноры vs RP01 LoRA, source-confirmed kNN/Units/DPO, сопоставление с защищёнными P01–P125, доказательные границы, новая составная гипотеза H58-01.
- **[N58-B — доказанные межветочные различия DPO/LoRA](58-SIBLING-DPO-LORA-SOURCE-DELTAS-2026-10-19.md)**: противоположные выражения локального логит-градиента в PrepareToGPL/current, разные optimizer bindings и training-flag LoRA; H58-02/H58-03 — отдельные утверждаемые мини-проверки, без native runs.
- **[N58 — пять экспериментальных паспортов с оригинальными новыми промптами](58-HUMAN-APPROVAL-EXPERIMENT-PASSPORTS-2026-10-19.md)**: pinned источник модели и данных (или честный `BLOCKED_INPUT`), точные скилы, A0/A1/B0/B1, домены, наблюдаемые ответы, метрики, ожидаемый различающий результат. **Все новые native опыты NOT_RUN; до `APPROVED <ID>` запускать нельзя.**
- Архив `llm_mvp_core/docs/plans/replanning-possible-diffs/**` — **READ-ONLY / ZERO MODIFICATIONS**. Существующие N56/N57 сохраняются как авторы прежних находок; coverage всех 125 P-card остаётся **PARTIAL**, не FULL PASS.

---

<!-- N57-ARGUMENT-VERIFIER-LEARNING-GAPS-2026-10-19-BEGIN -->
## Дополнение 19 октября: где теряется учебный сигнал и что проверяют малые опыты

**[Новый пакет: две точные гипотезы, доказательства и аудит экспериментов](57-ARGUMENT-AND-VERIFIER-LEARNING-GAPS-2026-10-19/README.md).** Структурный KNN теряет номер аргумента за ArgumentSyntax; переводческий verifier не сохраняет и не читает отдельные правила с нормальными кириллическими маркерами. Для каждого — отдельный опыт на двух уроках, четыре проверочных случая, фактический получатель и условия опровержения. Обе особенности унаследованы; причина нейронного ухудшения RP01 ими пока не доказана.

[Проверка 18 активных mini и карты 125/470](57-ARGUMENT-AND-VERIFIER-LEARNING-GAPS-2026-10-19/03-experiment-coverage-review.md) · [исторические успехи и все направления](57-ARGUMENT-AND-VERIFIER-LEARNING-GAPS-2026-10-19/04-history-and-mechanism-crosswalk.md) · [одно короткое поручение исполнителю](57-ARGUMENT-AND-VERIFIER-LEARNING-GAPS-2026-10-19/05-one-experiment-work-order.md) · [источники, независимые рецензии и воспроизводимость](57-ARGUMENT-AND-VERIFIER-LEARNING-GAPS-2026-10-19/06-source-and-review-record.md).

В двух прежних C# панелях уточняется различие между новыми текстами и новыми признаками. Все 470 старых подслучаев адресуются; это не 470 подтверждённых native опытов. Сохранены положительные результаты Executor 0/24→19/24, cold restore 19/24 и доставки подуроков 11/24→19/24; заново пересчитаны 144 архивных ответа. Нового native обучения в этом аудите нет. Защищённый core-каталог не меняется; новые протоколы находятся в этом дополнении.

<!-- N57-ARGUMENT-VERIFIER-LEARNING-GAPS-2026-10-19-END -->

<!-- N55-PREPARETOGPL-GOLD-FIRST-2026-10-19 -->
## Глава первого приоритета — PrepareToGPL: вернуть доказанное обучение

**[Список первой очереди: GOLD PrepareToGPL → CURRENT, с доказательствами по каждому механизму](55-PREPARETOGPL-RESTORATION-PRIORITY-AND-DIFFERENCE-MATRIX-2026-10-19.md).** Начало: сохранённый learned Unit/модель, честная загрузка старого checkpoint, kNN/структурный контекст в реальном получателе, обычный ответ и независимый смысловой экзамен — раньше следующего крупного RP01 DPO/LoRA. Разделять исторический measured gain и ещё не доказанную реставрацию.

**[Новая условная дельта: policy-version ↔ checkpoint identity ↔ actual consumer](55-PREPARETOGPL-POLICY-IDENTITY-AND-CONSUMER-BOUNDARY-2026-10-19.md)**; **[короткие G55-00…06 эксперименты](55-PREPARETOGPL-GOLD-PARITY-SMALL-DOMAIN-EXPERIMENTS-2026-10-19.md)** по переводу, C# AST, LoRA Student, Design-to-Code и оркестрации. Ранее установленные N35/N41/N54 не переименовывать в новые; native качество восстановления **NOT_RUN / NOT_PROVEN**. Каталог `llm_mvp_core/docs/plans/replanning-possible-diffs/**` не изменять.


<!-- N55-INDEPENDENT-EVALUATOR-ROUTE-AUDIT-2026-10-19 -->
**Дополнительный прямой аудит PrepareToGPL:** [gold-first восстановление и контроль старого learner](00-55-PREPARETOGPL-GOLD-FIRST-2026-10-19.md) · [все группы DPO/LoRA/tokenizer/kNN/Unit/restore с источниками](55-CROSSBRANCH-GAP-REGISTER-2026-10-19.md) · [узкая source-гипотеза N55: 6 языковых направлений Evaluator оценивает как RU→EN](55-NOVEL-DIRECTION-EVAL-AND-METRIC-STATE-2026-10-19.md) · [шесть маленьких доменных причинных A/B](55-CAUSAL-DOMAIN-MICROEXPERIMENTS-2026-10-19.md) · [CODEX и независимая проверка](55-CODEX-EXPERIMENT-AND-INDEPENDENT-REVIEW-2026-10-19.md). [Синтетический oracle](55-MIXED-DIRECTION-ROUTING-PROXY.mjs)/[receipt](55-MIXED-DIRECTION-ROUTING-PROXY-RESULT.json) 2/12 forced vs 12/12 typed — **НЕ native quality**. Унаследованный eval seam не выдавать за новое branch regression; native A/B NOT_RUN.

<!-- DESIGNTOCODE-CLEAN-LEARNING-AUDIT-2026-10-19 -->
## Глава 0 — сначала восстановить успешное обучение DesignToCode-clean

**[Начать с подтверждённых архивов clean и порядка восстановления](2026-10-19-design-to-code-clean-learning-audit/00-DESIGNTOCODE-CLEAN-RESTORATION-FIRST.md).** CodeTrace019: 49/50 **relative DPO preferences**, продолжение batch018; actual LFS payload/cold restore ещё не доказаны. E25/E26/E09/E43 сохранены со своими предметами и ограничениями; CodeTrace064/QueryPlanning134/ContextRouting075 не приписаны clean.

**[Новый пакет с доказательствами, малыми доменными опытами и исполнимыми формульными пробами](2026-10-19-design-to-code-clean-learning-audit/README.md).** Особый восстановительный приоритет — **[DC-01b: canonical generic принимается clean, но current может отвергнуть весь checkpoint](2026-10-19-design-to-code-clean-learning-audit/01-NEW-DESIGN-CORRECTION-SEMANTICS.md#dc-01b-r-неправильное-имя-может-блокировать-весь-старый-checkpoint)**. Подтверждена source-цепь; общее следствие отказа целого импорта уже описано в [CLEAN-CP01](54-DESIGNTOCODE-CLEAN-RECOVERY-2026-10-19/02-NEW-CHECKPOINT-AND-CONTEXT-GAPS.md). Наше уточнение относится к canonical generic/underscore и миграции двух alias; native отказ конкретного архива и качество после миграции пока не измерены.

После сверки параллельных пакетов54 остаются **четыре семьи с ограниченной новизной**: DC-01 (phantom/canonical category и уточнение restore) и KU-01…03 (suffix→concept→KNN gate, ложный PAST, punctuation→gender). **DC-02/03 уже известны** и приведены как независимое подтверждение, с кредитом предыдущему автору. После чтения ещё 12 версий на `33bf0f449d0a440a6ef6f0a32799ce5e1b4af1de` базовая сверка507 текстов расширена до **563 уникальных Markdown-путей: 471 planning + 92 protected**; 182 protected core Markdown остались непрочитанными из-за automatic approval запрета, поэтому глобальная уникальность не заявляется.

[Карта DPO/LoRA/vector/KNN/Unit/restore](2026-10-19-design-to-code-clean-learning-audit/03-MECHANISM-COVERAGE-AND-DPO-LORA.md) · [малые domain experiments P/C0/T](2026-10-19-design-to-code-clean-learning-audit/04-SMALL-DOMAIN-EXPERIMENTS.md) · [источники, новизна и независимый review](2026-10-19-design-to-code-clean-learning-audit/05-SOURCES-NOVELTY-AND-REVIEW.md) · [конкретное поручение CODEX](2026-10-19-design-to-code-clean-learning-audit/06-CODEX-EXPERIMENT-WORK-ORDER.md).

Выполнены source review, пересчёт архивов и Python formula probes; native обучение, model calls, реальный restore и visual/semantic quality измерения не запускались. Подтверждённые нынешние neural fixes сохраняются. Все прежние документы и следующий текст README сохранены.

<!-- /DESIGNTOCODE-CLEAN-LEARNING-AUDIT-2026-10-19 -->

<!-- CLEAN-CP01-CTX01-RECOVERY-PACKET-2026-10-19 -->
## Сначала восстановить DesignToCode-clean: проверенные механизмы и новые разрывы

**[Приоритетная глава восстановления](00-DESIGNTOCODE-CLEAN-RECOVERY-FIRST-2026-10-19.md)** · **[Полный пакет с доказательствами и короткими доменными опытами](54-DESIGNTOCODE-CLEAN-RECOVERY-2026-10-19/README.md)**.

В начале — точное обученное состояние и его обычный получатель, восстановление E09/E25/E43 с корректными метриками и отдельный порядок для UI/DesignConversion. Дополнительные цепочки: **CLEAN-CP01** — один retired feature блокирует импорт всего checkpoint; **CLEAN-CTX01** — source condition обучения заменяется runtime identities. Удаление feature уже имеет peer owner; новым здесь является whole-checkpoint consequence. Предполагаемый replay gap отмечен уточнением AUT-REPLAY-02. Качество этими исправлениями пока не измерено; все новые native A/B — **NOT_RUN**. [Новизна, источники и review](54-DESIGNTOCODE-CLEAN-RECOVERY-2026-10-19/05-EVIDENCE-NOVELTY-AND-REVIEW.md).

<!-- /CLEAN-CP01-CTX01-RECOVERY-PACKET-2026-10-19 -->

---

## Глава 0 — DesignToCode-clean: причинные проверки и восстановление обучения (19.10.2026)

**[Читать первой: восстановительные эксперименты DesignToCode-clean](2026-10-19-designtocode-clean-causal-audit/00-RESTORE-DESIGNTOCODE-CLEAN-FIRST.md).** [Полный пакет](2026-10-19-designtocode-clean-causal-audit/README.md): шесть новых точечных проверок и две подтверждающие/уточняющие проверки N54; DPO/LoRA, KNN, Units и продолжение состояния сведены в [общую карту](2026-10-19-designtocode-clean-causal-audit/04-ALL-MECHANISMS-CROSSWALK.md). Для каждой проверки — предметная область, исходники и [маленький опыт](2026-10-19-designtocode-clean-causal-audit/05-SMALL-EXPERIMENTS-AND-PROMPTS.md).

Главный донор — `DesignToCode-clean@e346b88a`, current — `358af351`. Старые положительные receipts, расчёты исходных формул и ещё не выполненные предметные A/B разделены. Из шести новых проверок scalar→SIMD backward — реальный branch diff; остальные уточняют унаследованные условия/ограничения. Сверены обе прежние коллекции и свежие параллельные публикации; [источники, окончательная новизна и независимые reviews](2026-10-19-designtocode-clean-causal-audit/06-EVIDENCE-NOVELTY-AND-REVIEW.md). Чужие материалы ниже сохранены.

---

<!-- N54-DESIGNTOCODE-CLEAN-LOCAL-PREFERENCE-2026-10-19 -->
## Глава 0 · DesignToCode-clean: восстановление DPO → обучаемый Unit (новое прямое сравнение)

**[Начать здесь — эталонная DesignToCode-clean, приоритет P0, что действительно переносить](00-DESIGNTOCODE-CLEAN-GOLDEN-FIRST-2026-10-19.md).** Обнаружено прямое изменение gold → current: `ToCanonicalTrainingItem` утратил `visual_score`, при этом `DesignConversionMachine.BuildLearnedWeightDeltas` вместо прежнего отказа без оценки теперь допускает разные `html/css` как фиксированную `preferenceMargin=1.0` и строит source-only локальные weights. В current canonical responses также нет категорий, которые ожидает обучаемый Improver. **Это подтверждено кодом, но причинное влияние на RP01 нейронный Student не измерено**.

**[Source crosswalk + 6 проверяемых узких гипотез](54-DESIGNTOCODE-CLEAN-SOURCE-PARITY-AND-NEW-GAPS-2026-10-19.md)** · **[8 коротких однофакторных опытов по темам](54-DESIGNTOCODE-CLEAN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md)** · **[primary source refs, review, caveats](54-DESIGNTOCODE-CLEAN-EVIDENCE-REVIEW-2026-10-19.md)** · **[toy Python script](54-DESIGNTOCODE-CLEAN-PROXY.py)** / **[его JSON-проба](54-DESIGNTOCODE-CLEAN-PROXY-RESULT.json)**. Новая гипотеза: в одинаковом для двух веток Improver положительная обученная память переводит `all matching defects` в `top-1 rule` и потенциально ухудшает multi-defect HTML/CSS. SOURCE-EQUIVALENT proxy выполнен, native .NET/Student/semantic heldout **NOT_RUN**.

Уже опубликованные другими агентами главы Gold [(E25/E26/E09/E43/оркестрация)](00-54-DESIGNTOCODE-CLEAN-GOLD-FIRST-2026-10-19.md), [(загрузка моделей и архив)](00-DESIGNTOCODE-CLEAN-GOLDEN-RECOVERY-FIRST-2026-10-19.md) и [(прямые branch paths)](54-DESIGNTOCODE-CLEAN-DIRECT-DIFFS-AND-NOVEL-BOUNDARIES-2026-10-19.md) **сохранены и дополнены, не заменены**. Protected `llm_mvp_core/docs/plans/replanning-possible-diffs/**` — READ ONLY.

<!-- /N54-DESIGNTOCODE-CLEAN-LOCAL-PREFERENCE-2026-10-19 -->

---

<!-- N54-DESIGNTOCODE-CLEAN-GOLD-FIRST-2026-10-19 -->
## ГЛАВА 0 · DesignToCode-clean — восстановить проверенные исторические механизмы прежде новых рефакторингов

**[GOLD-FIRST: E25 POS/order, E26 context, E09 AST-kNN, E43 replay, E40V2 trust + честное различие с RP01](00-54-DESIGNTOCODE-CLEAN-GOLD-FIRST-2026-10-19.md).** Прямые источники из `DesignToCode-clean`, отдельные ограничения метрик, tiny SFT→DPO vs trained Unit, требование `training → cold restore → ordinary consumer → unseen semantic quality`. Унаследованные `KnnLogicalUnit/HnswIndex/Trainer` совпадают с GOLD побайтово — не считать неизменившийся код виновником branch regression.

**[Три точные дополнительные гипотезы N54](54-NEW-LOGICAL-KNN-METADATA-ID-HASH-SEAMS-2026-10-19.md):** потеря predicate tier при не-`string[]` metadata в прямом Unit; aliasing output при повторных примерных IDs (в index остаются оба вектора, а map берёт последний output); гарантированное переполнение signed hash на искусственном predicate `ZSJBINM`. **Все три source-confirmed, унаследованы GOLD/current, их причинный вклад в RP01 НЕ ДОКАЗАН.**

[Короткие доменные causal-проверки и Gold пилоты](54-MICRO-DOMAIN-CAUSAL-EXPERIMENTS-2026-10-19.md) · [сквозная карта 25 семейств / источники RAG / reviewer](54-MECHANISM-COVERAGE-AND-REVIEW-2026-10-19.md) · [исполненный source-equivalent Python proxy](54-KNN-SOURCE-EQUIVALENT-PROXY-2026-10-19.py) · [его JSON-квитанция](54-KNN-SOURCE-EQUIVALENT-PROXY-RESULT-2026-10-19.json). **Native .NET / новое обучение / semantic gain: NOT_RUN.** Защищённый `llm_mvp_core/docs/plans/replanning-possible-diffs` не меняли.

<!-- /N54-DESIGNTOCODE-CLEAN-GOLD-FIRST-2026-10-19 -->

---

## 19.10.2026 — сначала восстановить подтверждённое обучение external_domains

**[Learning Semantics: приоритетная глава восстановления — state/consumer, structural KNN, порядок и контекст перевода, neural recipient, полный resume](53-EXTERNAL-DOMAINS-LEARNING-SEMANTICS-AUDIT-2026-10-19/00-EXTERNAL-DOMAINS-RECOVERY-FIRST.md).** Малые протоколы P0–P5 опираются на повторно прочитанные первичные результаты external_domains и положительный ProjectWiki DPO из соседней ветки. Matrix FAIL, KNN success, точная память и качество свободного ответа различаются явно; современные исправления DPO/backward сохраняются.

**[Learning Semantics — пять новых гипотез и одна локализация](53-EXTERNAL-DOMAINS-LEARNING-SEMANTICS-AUDIT-2026-10-19/README.md):** source-ID конфликт learned aggregation; global cutoff до edge eligibility; положительный duplicate → margin0 в PromptToTask; ложный missing_gates из substring gate; single-domain → cap provider weight1; replay admission до quality verdict. У каждого — consumer, donor ancestry, ограничения, доказательства и короткий доменный A/B. AUT-пункты P2 conditional не выдаются за причину direct RP01.

[Карта всех классов различий](53-EXTERNAL-DOMAINS-LEARNING-SEMANTICS-AUDIT-2026-10-19/01-MECHANISM-CROSSWALK.md) · [Малые эксперименты и готовое задание](53-EXTERNAL-DOMAINS-LEARNING-SEMANTICS-AUDIT-2026-10-19/05-SMALL-EXPERIMENTS-AND-WORK-ORDER.md) · [Новизна, источники и review](53-EXTERNAL-DOMAINS-LEARNING-SEMANTICS-AUDIT-2026-10-19/06-SOURCES-NOVELTY-AND-REVIEW.md).

Сверены основной корпус и поздние дельты: итог **689 уникальных Markdown**. PT-01 локализует прежний открытый duplicate/margin lead; общая идея не заявляется как новая. [Финальное уточнение](53-EXTERNAL-DOMAINS-LEARNING-SEMANTICS-AUDIT-2026-10-19/08-FINAL-CORE-DOCS-NOVELTY-CLARIFICATION.md). Выполнены source/formula Python probes; нового native обучения и измеренного прироста качества этим пакетом не заявлено. Существующие материалы ниже сохранены.

---

<!-- EXTERNAL-DOMAINS-LEARNING-AUDIT-2026-10-19 -->
## Глава 0 / первый приоритет — восстановить успешное обучение `external_domains`

**[Начать с главы восстановления: E09, обученная память E25, контекст E26, replay E43 и обычный потребитель](2026-10-19-external-domains-lexical-state-audit/00-EXTERNAL-DOMAINS-RESTORATION-FIRST.md).** Сопоставлены исходники `external_domains@3036a01` и current `44507823`, первичные результаты и состояние RP01. Определены отдельные маршруты восстановления C# KNN, переводческих Units и raw Student; полезные исправления current LoRA/autograd сохраняются.

**[Три новые исходниковые гипотезы](2026-10-19-external-domains-lexical-state-audit/02-NEW-CSHARP-INPUT-AND-CALL-LOSS.md):** комментарий внутри C# строки стирает код; интерполяция теряет вычисляемый AST-признак между TRAIN и query; условные вызовы отсутствуют в completion observations и знаменателе E02. Эти ограничения унаследованы от успешной ветки, поэтому их вклад в ухудшение конкретного RP01 проверяется отдельно. Новизна сверена по **678 Markdown-путям / 685 версиям** обоих исследовательских каталогов.

[Весь пакет](2026-10-19-external-domains-lexical-state-audit/README.md) · [DPO, LoRA, токенизация и проекция в Units](2026-10-19-external-domains-lexical-state-audit/01-LEARNING-INTERNALS-AND-EXISTING-GAPS.md) · [малые опыты по доменам](2026-10-19-external-domains-lexical-state-audit/03-MICRO-EXPERIMENTS-BY-DOMAIN.md) · [доказательства и независимая проверка](2026-10-19-external-domains-lexical-state-audit/04-SOURCES-NOVELTY-AND-REVIEW.md) · [первое поручение CODEX](2026-10-19-external-domains-lexical-state-audit/05-CODEX-WORK-ORDER.md).

Выполнены source review, две Python-диагностики и сравнение шести пар файлов. Native C# probe подготовлен; новое обучение и semantic quality gain — **NOT_RUN / NOT_MEASURED**.

<!-- /EXTERNAL-DOMAINS-LEARNING-AUDIT-2026-10-19 -->

---

<!-- EXTERNAL-RECOVERY-53-2026-10-19 -->
## Первая очередь — восстановить успешные механизмы `external_domains`

**[Глава восстановления: R0–R5, исторические результаты и маленькие опыты](53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/00-EXTERNAL-DOMAINS-RECOVERY-FIRST.md).** Сначала установить реальное обученное состояние и ordinary consumer; затем проверить память порядка E25, двусторонний контекст E26, структурный KNN E09 при сопоставимом сравнении E07, обучаемую память E40V2 и удержание E43. Сохранены положительные, отрицательные и нейтральные свидетельства: E40-v1 0% → 0% не отменяет E40V2 66.67% → 100% на его небольшой панели. Текущие исправления graph-backward, exact DPO и attached LoRA сохраняются.

**[Три новых конкретных условия и доказательства](53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/01-NEW-HYPOTHESES.md):** возраст поздно активированного параметра при sparse AdamW state; знаковая сумма scores в confidence KNN при допустимых IDs; насыщение используемого PositionInParent после 255. Одна новая условная механистическая гипотеза и два уточнения известных семейств; они не объявлены доказанными причинами провала RP01.

**[Предметные A/B и задание исполнителю](53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/02-SMALL-DOMAIN-EXPERIMENTS.md)** · **[Карта 19 групп и дедупликация 671 Markdown-пути / 125 карточек](53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/03-MECHANISM-COVERAGE-AND-DEDUP.md)** · **[Источники, навыки, review и выполненные проверки](53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/04-SOURCES-AND-REVIEW.md)** · **[Весь пакет](53-EXTERNAL-DOMAINS-RECOVERY-AND-SPARSE-KNN-REFINEMENTS-2026-10-19/README.md)**.

Выполнены source audit и воспроизводимые Python-пробы с отрицательными контролями; новые native/.NET/Teacher/DPO/quality опыты — `NOT_RUN`. Защищённый `llm_mvp_core/docs/plans/replanning-possible-diffs` использован только для чтения. Предыдущие исследования ниже сохранены полностью.

<!-- /EXTERNAL-RECOVERY-53-2026-10-19 -->

---

<!-- OE52-GOLDEN-FIRST-2026-10-19 -->
## ГЛАВА 0 / P0 — восстановление успешного обучения `orchestrator-extensions`

**[Главная глава: gold donor → trained recipient → checkpoint → cold restore → обычный consumer](00-ORCHESTRATOR-EXTENSIONS-GOLDEN-FIRST-2026-10-19.md)**. Сохранённый исторический `orchestration-v1` показывает **unseen-style 0.8** и три раздельных режима обучения. Сам checkpoint и отчёт не изменились в current, а четыре исходника OrchestrationPlan совпали после удаления лицензионного заголовка. Порядок действий: доказать использование сохранённой машины обычным consumer, затем проверить генерацию/выбор learned Unit и перенос прошлой KNN-памяти.

**[Новые проверяемые cross-branch differences](52-ORCHESTRATOR-EXTENSIONS-DIRECT-DIFFS-2026-10-19.md)**: generated Unit `isLearned:true→false`, default обучение без authority, восстановление `allManifests` против signed replay subset, новый schema-v2 contract. **[7 коротких доменных опытов](52-ORCHESTRATOR-EXTENSIONS-CAUSAL-MICRO-EXPERIMENTS-2026-10-19.md)** · **[доказательства, SHA, границы выводов](52-ORCHESTRATOR-EXTENSIONS-EVIDENCE-AND-REVIEW-2026-10-19.md)**.

Исторический успех **не равен** улучшению RP01; все новые native проверки `NOT_RUN`. Защищённый каталог `llm_mvp_core/docs/plans/replanning-possible-diffs` не изменялся.

<!-- /OE52-GOLDEN-FIRST-2026-10-19 -->

---

<!-- LW-FEEDBACK-BPE-RECOVERY-50-2026-10-19 -->
## Сначала восстановить `local_wiki_planning`: дополнение с двумя новыми механизмами

**[Глава 0 пакета 50 — приоритетный список восстановления обучения](50-LOCAL-WIKI-FEEDBACK-AND-BPE-BUDGET-2026-10-19/00-LOCAL-WIKI-RECOVERY-FIRST.md)**: исходные DPO до получателя, точные policy/reference при продолжении, обычный learned consumer, полный client/server сценарий, обратная связь следующего урока и cold restore. Ссылки на прежних владельцев сохранены; полезные исправления текущего LoRA/autograd ядра выделены отдельно.

**Новая разница с успешной веткой — [LW-FB-01](50-LOCAL-WIKI-FEEDBACK-AND-BPE-BUDGET-2026-10-19/01-NEW-CURRICULUM-FEEDBACK-GAP.md):** нынешний selector игнорирует accepted checkpoints и суммирует всю слабую историю. На одном реестре 421 source-equivalent формулы рекомендуют разные Units;153 балла Router не уменьшаются после accepted. Доказана рекомендация в plan-only Flow; качественный опыт —6 уроков при равном бюджете и 12 независимых ответов.

**Новая унаследованная гипотеза — [LW-BPE-02](50-LOCAL-WIKI-FEEDBACK-AND-BPE-BUDGET-2026-10-19/02-NEW-BPE-SEED-BUDGET.md):**512 искусственных числовых seed-строк расходуют ограниченный BPE merge budget. Приложены Python-проверка токенов, отрицательный контроль и предметный опыт KNN. Этот одинаковый old/current helper не объясняет сам по себе регрессию RP01 с сохранённым tokenizer.

[Весь пакет и результаты](50-LOCAL-WIKI-FEEDBACK-AND-BPE-BUDGET-2026-10-19/README.md) · [все механизмы и прежние гипотезы](50-LOCAL-WIKI-FEEDBACK-AND-BPE-BUDGET-2026-10-19/03-MECHANISM-COVERAGE-AND-KEPT-FIXES.md) · [малые эксперименты](50-LOCAL-WIKI-FEEDBACK-AND-BPE-BUDGET-2026-10-19/04-SMALL-EXPERIMENTS-AND-CODEX-WORK-ORDER.md) · [источники, новизна и независимая проверка](50-LOCAL-WIKI-FEEDBACK-AND-BPE-BUDGET-2026-10-19/05-SOURCES-NOVELTY-AND-REVIEW.md).

Новых нативных запусков обучения и доказанного качественного выигрыша в этом пакете нет. Исторические CodeTrace 49/50 и 46/50 пересчитаны и отделены от обычных ответов. Ниже полностью сохранён прежний указатель исследований.

---


<!-- WIKI51-RECOVERY-FIRST-2026-10-19 -->
## Сначала local_wiki_planning: восстановление алгоритма, новые SQL-доказательства и короткие опыты

**[Первая глава — восстановить рабочий local_wiki: state/consumer, исходную DPO-тройку, SQL-сигнал и место адаптации](51-LOCAL-WIKI-SQL-KNN-TENSOR-RECOVERY-2026-10-19/00-LOCAL-WIKI-RECOVERY-FIRST.md).** Новый [пакет51](51-LOCAL-WIKI-SQL-KNN-TENSOR-RECOVERY-2026-10-19/README.md) добавляет адресный W→C SQL diff и поздние подтверждённые receipts110/111/112 с47/50. Успех .94 был **до** read-only correction111;106 имел failed loop summary, хотя structural verdict был PASS. Эта история сохранена без причинной подмены.

**[Первый новый опыт AC-SQL01: одинаковый EXAM, меняется только TRAIN,10 updates на плечо](51-LOCAL-WIKI-SQL-KNN-TENSOR-RECOVERY-2026-10-19/05-SMALL-DOMAIN-EXPERIMENTS.md)**. Отдельно проверяется потеря evidence на входе neural AnswerComposer. Три KNN/context и два tensor пункта обозначены как условные, унаследованные от рабочей ветки; они не названы причинами RP01 без actual caller. **[Точные внутренности модели и LoRA](51-LOCAL-WIKI-SQL-KNN-TENSOR-RECOVERY-2026-10-19/01-LEARNING-INTERNALS-BEFORE-AFTER.md)** · **[Все запрошенные механизмы и дедупликация](51-LOCAL-WIKI-SQL-KNN-TENSOR-RECOVERY-2026-10-19/06-COVERAGE-AND-NOVELTY.md)** · **[Независимый review](51-LOCAL-WIKI-SQL-KNN-TENSOR-RECOVERY-2026-10-19/07-REVIEW-AND-METHOD.md)**.

Предыдущие восстановления N49/G-W, E09/E26/E43 и соседние отчёты ниже сохранены. Здесь выполнены source/receipt и Python-проверки; новых native training/free-answer результатов не заявлено. Core-каталог использован только для чтения.
<!-- /WIKI51-RECOVERY-FIRST-2026-10-19 -->

<!-- N50-EXTERNAL-DOMAINS-GOLDEN-FIRST-2026-10-19 -->
# ПРИОРИТЕТ ВОССТАНОВЛЕНИЯ — `external_domains` и проверенный исторический алгоритм

**[Глава 0: вернуть измеренные способности `external_domains`](00-EXTERNAL-DOMAINS-POSITIVE-RESTORE-P0-2026-10-19.md)** — сначала восстановить C# AST/kNN E09, contextual dictionary E26 и удержание знаний replay E43 **на конкретных прежних получателях**, затем проверить настоящее применение после сохранения/восстановления. Исторические положительные отчёты — `VERIFIED_HISTORICAL_RECEIPTS`, не доказательство прироста RP01. Ранний E22 имел `passed=false`, его поздний положительный сигнал был в другом сценарии и с изменёнными метками.

**[Новый исходно измеренный дефект: 3 000 DPO-пар из пяти тем](50-NEW-EXTERNAL-DOMAINS-PAIR-IDENTITY-AND-INVERTED-PREFERENCES-2026-10-19.md)** — **55 уникальных Prompt**, **270 (9%) `Chosen == Rejected`**, **219 (7,3%) потенциально обратно размеченных по собственной шкале curriculum**, все `compiled_prompt_in_training=false`; сохранённый `overlap_ratio` получен случайным числом, а не сравнением токенов. Это аудит **15 архивных JSONL (runs 001–003)**, не входного файла RP01. Потенциальная семантическая инверсия не объявляется установленной без независимого judge; обучение и причинный gain **NOT_RUN**.

**[Независимая cross-branch коррекция: E09/E26/E43 уже существуют в current без изменений алгоритма; E26 был заранее заполненным lookup, не обучением](51-GOLD-BRANCH-EXPERIMENT-CODE-PARITY-AND-E26-DETERMINISTIC-LOOKUP-2026-10-19.md)** — 5 файлов побайтно одинаковы за вычетом лицензионных заголовков. Это понижает гипотезу «переписали успешный E26» и поднимает проверку `learning state → consumer`, а также необходимость нового обучения переводу вне заранее прописанных значений.

**[Малые предметные A/B, негативные контроли и критерии](50-EXTERNAL-DOMAINS-CAUSAL-EXPERIMENTS-2026-10-19.md)** · **[воспроизводимый read-only Python-аудит JSONL](50-EXTERNAL-DOMAINS-CORPUS-PROBE.py)**. Публикация разрешена только здесь. `llm_mvp_core/docs/plans/replanning-possible-diffs/**` оставлен **без изменений**; не обходить legal DPO data admission.

<!-- /N50-EXTERNAL-DOMAINS-GOLDEN-FIRST-2026-10-19 -->

<!-- G-WIKI-GOLDEN-FIRST-2026-10-19 -->
# ГЛАВА 0 — СНАЧАЛА ВОССТАНОВИТЬ `local_wiki_planning`

**[00-00 — приоритет golden `local_wiki_planning`: зафиксированные отличия + восстановительные опыты](00-00-LOCAL-WIKI-GOLDEN-FIRST-2026-10-19.md)** — direct audit 19.10.2026. **P0-W0 — первым проверить исчезновение Prompt/Chosen/Rejected в neural DPO**, подтверждённое [peer source-equivalent 50→4 projection](49-LOCAL-WIKI-DONOR-DELTA-2026-10-19/01-DPO-PAIR-LOSS-AND-HISTORICAL-PROJECTION.md) (NOT native quality result). **[Пакет G-W01..G-W08](49-LOCAL-WIKI-GOLDEN-RECOVERY-2026-10-19/README.md)**: новое source-confirmed отсутствие `ClientServerBusinessFlow` и её 15-шаговой цепи; предложены независимые проверки модели маршрутизации, checkpoint/label mapping, nonfunctional confidence gate и gold-negation false positive. **19 source-predicted вопросов** и **8 коротких опытов**; все `NATIVE_NOT_RUN`, `QUALITY_GAIN_NOT_ESTABLISHED`. Эталон pinned `219d7ba89dc3ef43e03e17f1461ad96154d3dd7a` vs current pinned `4945b8e85c17b5f150eb5b840d16bbdc4b506242`.

**Дедупликация:** [49-DONOR-DELTA](49-LOCAL-WIKI-DONOR-DELTA-2026-10-19/README.md) уже владеет находками потери исходных DPO prompt/chosen/rejected и publication/serving gate; [49-DONOR-CAUSAL](49-LOCAL-WIKI-DONOR-CAUSAL-RECOVERY-2026-10-19/README.md) уже исследовал DPO scorer, маршрутизатор и holders. Наш G-W02 — *независимое подтверждение*, не заявляется новым. Историческая `language_translate` (E09/E26/E43) сохраняется как соседний профиль, а не заменяет проектный эталон.

<!-- /G-WIKI-GOLDEN-FIRST-2026-10-19 -->

<!-- LT19-RESTORATION-AUDIT-2026-10-19 -->
## 19 октября: восстановление language_translate — проверенный пакет и новая LoRA-гипотеза

**[Начать с восстановительных направлений: E25, E26, E09, E34V2, E51A и E43](2026-10-19-language-restore/README.md)** — точные классы и потребители, исторические raw, малые A/B по предметным темам и ограничения переноса. Уже исправленная потеря phrase mappings (11/24→19/24), положительный memory effect и cold restore отделены от нейронных результатов.

**[Новая узкая гипотеза: масштаб факторов LoRA при одинаковом начальном forward](2026-10-19-language-restore/04a-lora-factor-experiment.md)** — fresh A₀ против 2A₀ при B=0 и optimizer=0; проверка effective update и собственного качества по EN→RU и C# документации. Не выполнена и не объявлена доказанным дефектом. Подфазы U-N1 и согласованный encoder replay P92.2 признаны прежними работами; конкретный grammar caller совпал с параллельным LT-N03 пакета47 и отнесён к известному P79.

**[Карта всех рассматриваемых механизмов](2026-10-19-language-restore/05-mechanism-crosswalk.md)** · **[реестр источников и новизны](2026-10-19-language-restore/06-source-and-novelty-ledger.md)** · **[независимый review и исправленные замечания](2026-10-19-language-restore/08-review-and-publication.md)**. Проверен исходный корпус 1278 Git blobs (1274 полных текста и 4 LFS pointers); новые параллельные документы ниже сохранены и сверены отдельно. Core и защищённый каталог использованы только для чтения.

<!-- /LT19-RESTORATION-AUDIT-2026-10-19 -->

<!-- N48-LT-RECOVERY-SOURCE-AUDIT-2026-10-19 -->
## N48 — Сначала восстановление language_translate: точные механизмы, новые гипотезы и короткие опыты

**[Первая глава: вернуть положительные E25/E26/E09/E43 и проверить нынешний Unit/Flow](00-LANGUAGE-TRANSLATE-RECOVERY-FIRST-2026-10-19.md)** — 19 октября 2026. Исходники закреплены за language_translate f6032aff и current a392b6be; положительные архивы памяти, контекстной функции и replay сохранены со своими границами. Нынешние исправления LoRA forward/backward сохраняются.

Новые конкретные цепочки **N48-01** (guard невызванного callable портит E14 label), **N48-02** (finite gradient → сохранённый AdamW Inf-v при конечных весах), **N48-03** (recipe TeachingVector current→replay; уточнение P06/N26/N38/P89) — в [документе с доказательствами](48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/02-NEW-HYPOTHESES.md). Причинный эффект на RP01 не объявлен установленным; native/learning опыты не запускались. N48-02 имеет только отдельно обозначенное NumPy FP32 наблюдение.

- [Что было / что стало: все запрошенные механизмы](48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/01-MECHANISMS-AND-EVIDENCE.md).
- [Малые доменные эксперименты, recovery первым](48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/03-SMALL-DOMAIN-EXPERIMENTS.md).
- [Покрытие, 691 файлов корпуса, pinned источники и независимое review](48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/04-SOURCES-COVERAGE-AND-REVIEW.md).
- [Готовое задание CODEX на первый опыт](48-LANGUAGE-TRANSLATE-AUDIT-2026-10-19/05-CODEX-EXPERIMENT-WORK-ORDER.md).

Материалы N44–N47, добавленные параллельно другими агентами, сверены и сохранены; их E26/routing/evaluator находки не переобъявлены нашими. Защищённый core/docs/plans/replanning-possible-diffs использован исключительно для чтения.

<!-- /N48-LT-RECOVERY-SOURCE-AUDIT-2026-10-19 -->

<!-- LT-INTERNALS-RESTORATION-2026-10-19 -->
## 47 — language_translate: восстановление механизмов и шесть новых внутренних гипотез

**[НАЧАТЬ С R00–R04: восстановительные проверки language_translate](47-LANGUAGE-TRANSLATE-INTERNALS-AND-NEW-HYPOTHESES-2026-10-19/01-LANGUAGE-TRANSLATE-RESTORATION-FIRST.md)** — точный donor/state/обычный ответ, контекстный выбор значения, AST-distance в KNN, сохранение старой памяти и C# action representation. Исторические положительные результаты E26/E09/E43/E14 сохранены с их ограничениями.

**[Новый пакет: что происходит внутри модели и Units](47-LANGUAGE-TRANSLATE-INTERNALS-AND-NEW-HYPOTHESES-2026-10-19/README.md)**: точные q/v-LoRA shapes/53 248 scalars, DPO objective/masks, токены и учебные вектора, части урока, восстановление. **[LT-N01–03](47-LANGUAGE-TRANSLATE-INTERNALS-AND-NEW-HYPOTHESES-2026-10-19/03-NEW-KNN-HYPOTHESES.md)** — неразличимые признаки List/Task, пары между независимыми классами и повторное обучение при Compile. **[LT-N04](47-LANGUAGE-TRANSLATE-INTERNALS-AND-NEW-HYPOTHESES-2026-10-19/04-STATE-AND-RESTORE.md)** — export до проверки старого index. **[LT-N05–06](47-LANGUAGE-TRANSLATE-INTERNALS-AND-NEW-HYPOTHESES-2026-10-19/05-ADDITIONAL-MECHANISM-HYPOTHESES.md)** — nonlocal morphology normalization и повторный рост supervised LR за горизонтом; приложен выполненный формульный Python-контроль, native quality NOT_RUN.

**[Дедупликация и свежесть](47-LANGUAGE-TRANSLATE-INTERNALS-AND-NEW-HYPOTHESES-2026-10-19/06-COVERAGE-AND-EXECUTION.md)**: обе прежние коллекции и дополнительно 15 параллельных файлов до `3215267c`. Чужие N44/N45/N46 связаны ссылками и не присваиваются; общий ProductMatch gap уже описан прежним N28. Исправленные 18 октября ingress/embedding/bias/mask не объявляются открытыми заново. Новые шесть — конкретные гипотезы с разной применимостью, не шесть доказанных причин неудачи RP01.
<!-- /LT-INTERNALS-RESTORATION-2026-10-19 -->

<!-- N44-LANGUAGE-TRANSLATE-RESTORE-AND-NEW-E26-2026-10-19 -->
## N44 — language_translate: в первую очередь восстановить прежние рабочие механизмы

**[Новая Глава 0: порядок восстановления, источники old→current, реальные цифры и ограничения](44-RESTORE-LANGUAGE-TRANSLATE-FIRST-2026-10-19.md)** — 19 октября 2026. `language_translate` — исторические E26/E09/E43, `PrepareToGPL` — прежняя Translation Unit memory, `codex/dpo-implementation` — нынешний neural Student и отдельный translator; их результаты нельзя объединять. Приоритет **legacy4→6 checkpoint**, существующий путь **11/24→19/24** и сохранение **19/24** после cold restore; последние neural SFT8 changes пока дали **0/24**, это другой получатель и это уже измерено в core.

**[Два новых узких кандидата в исходном E26](44-E26-UNKNOWN-CONTEXT-AND-ORDER-2026-10-19.md):** (1) отсутствующий query POS не штрафует запись, которая его требует; (2) Any overlap не учитывает порядок/кратность POS. Проверены **5 случаев source-equivalent proxy**, [скрипт](44-E26-SOURCE-EQUIVALENT-PROXY.py) и [квитанция](44-E26-SOURCE-EQUIVALENT-PROXY-RESULT.json). **Не** выдаём это за native .NET или доказанное новое качество Flow. Общий E26 POS context→Unit gap **уже** найден агентами в главе 23/25 и не назван новым.

**[Сводная карта 25 механизмов ухудшения, с доказательствами, уже известными агентам результатами и необходимыми проверками](44-REQUESTED-MECHANISMS-COVERAGE-2026-10-19.md)** — DPO-пары, random base, LoRA, masks, vector/tokenizer, kNN/AST, перевод DPO→Unit, сохранение checkpoint, forgetting, маршрутизация и ложные quality gates. Только K44-01/02 обозначены здесь нашими новыми узкими кандидатами; остальные известные работы не присваиваются.

**Параллельные находки N44/N45, подтверждённые чтением кода:** [old R1/R2 language/topic routing не эквивалентен новому обязательному входному контракту](44-NEW-LANGUAGE-ROUTING-CONTRACT-DIFF-2026-10-19.md) (upstream может компенсировать, ещё не доказан продуктовый дефект) и [ProductHeldOut ProductMatch может принимать перевод с изменённым смыслом](45-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-2026-10-19.md). Исторические E22 late-PASS и E51A также учтены в [обновлённой главе 0](44-RESTORE-LANGUAGE-TRANSLATE-FIRST-2026-10-19.md#интеграция-с-параллельными-n44n46-источник-проверен-отдельно). Эти находки сделаны **другими агентами**, не объявляются нашими новыми K44-01/02. Их [собственные микроэксперименты](46-MICRO-LANGUAGE-TRANSLATE-RESTORATION-AND-EVAL-2026-10-19.md) уже опубликованы; повторная постановка не нужна.

**[Короткие A/B по предметным темам и реальным потребителям](44-FAST-DOMAIN-EXPERIMENTS-2026-10-19.md)** · **[архитектор, code examination, планировщик, independent reviewer и человеческая коммуникация](44-CODEX-RESEARCH-AND-REVIEW-PACKET-2026-10-19.md)**. В этом пакете original core и защищённый каталог `llm_mvp_core/docs/plans/replanning-possible-diffs` **только читались**, изменений там **0**. Новизна K44-01/02 требует независимой полной дедупликации; влияние на ordinary consumer — **NOT_RUN**.

<!-- /N44-LANGUAGE-TRANSLATE-RESTORE-AND-NEW-E26-2026-10-19 -->

# XIIGEN possibleDifs — ПЕРВАЯ ГЛАВА: восстановление language_translate (19.10.2026)

**Главный приоритет перед новыми DPO500/Windows CI:** восстановить и проверить в ordinary consumer полезные механизмы из действительно успешной ветки language_translate, не смешивая KNN/memory и neural LoRA. Архивные receipts имеют ограничения, перечисленные в первой главе.

- **НАЧАТЬ ЗДЕСЬ:** [00 — language_translate restoration first](00-LANGUAGE-TRANSLATE-FIRST-PRIORITY-2026-10-19.md): E09 structural KNN; E26 contextual meaning; E43 replay; E22 early FAIL versus later PASS; RP01 owner/consumer split.
- **Архивные доказательства с pin SHA:** [44 — verified successes and losses](44-LANGUAGE_TRANSLATE-SUCCESS-FIRST-RECOVERY-2026-10-19.md). E09 0.2413→0.3507 (unmatched denominator), E26 .40→1.00 (15 cases, static dictionary), E43 .3226→.3978 (bigram replay); не выдавать их за доказанный neural gain.
- **Новые отдельные гипотезы:** [45 — ProductHeldOut semantic/emoji false-positive risks](45-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-2026-10-19.md). Это source-level gaps с аналитическими контрпримерами; native quality improvement **NOT_RUN**, причина RP01 не доказана.
- **Готовые малые проверки по доменам:** [46 — paired micro-experiments](46-MICRO-LANGUAGE-TRANSLATE-RESTORATION-AND-EVAL-2026-10-19.md): C# AST, RU/EN ambiguity, replay retention, multi-label routing, ordinary Flow, independent semantic scoring.
- **Дедупликация:** Parser enum-order в Verifier, E09 label drift, DPO prompt truncation, RuleTag loss, legacy checkpoint и другие известные пробелы уже описаны в planning и защищённом **read-only** core catalog; повторно новыми находками не считаются.

Ни один новый .NET train/Student evaluation не запускался при подготовке этих документов. Кодовые репозитории, frozen исторические результаты и core/docs/plans/replanning-possible-diffs не изменялись.

---

<!-- N44-2026-10-19-SOURCE-CONTRACT-DELTA -->
## N44 · 19.10.2026 — восстановление успешного обучения + новый R1/R2 source diff

**Глава успешного восстановления**: [N44 historical primary receipts / приоритет P0](44-RECOVERY-FIRST-EVIDENCE-AND-PRIORITY-2026-10-19.md); соседний независимо опубликованный [language_translate success-first baseline](44-LANGUAGE_TRANSLATE-SUCCESS-FIRST-RECOVERY-2026-10-19.md). Оба используют положительные E09 structural kNN, E26 contextual dictionary, E43 replay и отделяют learned memory от neural LoRA; прежде всего вернуть прежний **ordinary consumer** и отдельно измерить качество. Нынешний cold restore сохранил **24/24 наблюдаемых output** при **19/24 правильных переводах** (P25).

**Новая адресная находка**: [R1/R2 raw-text routing и поле RejectedAdapters существовали в `language_translate`, а current Flow требует SourceLanguage и получает TopicDomain от caller](44-NEW-LANGUAGE-ROUTING-CONTRACT-DIFF-2026-10-19.md). Есть [четыре тестируемые H44-R1..R4, доменные A/B/C эксперименты](44-SMALL-CAUSAL-EXPERIMENTS-BY-DOMAIN-2026-10-19.md) и [18 synthetic eval-only RU/HE/EN примеров](44-ROUTING-CAUSAL-FIXTURE-2026-10-19.json). Это source-confirmed **несовпадение контрактов**, а не доказанная причина падения semantic quality. Новая запись по уже исполненному 18.10 DPO mask-fix: finite difference 162/162, но **0/24→0/24 ответов**; исправление градиента само по себе качество не восстановило. Никакие native опыты N44 не запускались; защищённый core-путь read-only.

<!-- /N44-2026-10-19-SOURCE-CONTRACT-DELTA -->

<!-- N43-RESTORATION-AND-NEURAL-DIFFS-2026-10-18 -->
## N43 · сначала восстановление PrepareToGPL, затем новые причинные проверки

**[Пакет: приоритет восстановления, доказательства и шесть небольших опытов](43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18/README.md)** — 18 октября 2026. Учтены параллельные главы 40–42, пакет 2026-10-18-preparetogpl-recovery и новая публикация 412d3364; прежние документы ниже сохранены.

- **Сначала восстановление:** [конкретный archived checkpoint и опыт без новых уроков](43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18/01-LEGACY-CHECKPOINT-GAP.md). Это независимое подтверждение N41-T1/U-R1 и параллельных legacy4→6 проверок; приложены точные bytes/SHA старого состояния и два retained probes.
- **Новое наблюдение:** [EN→RU chosen likelihood снизилась на 20/20 при выросшем среднем relative margin](43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18/02-NEURAL-SCORES-AND-STATE.md). Приложен воспроизводимый пересчёт старых scores; это не новая semantic grading.
- **Новое однофакторное уточнение DLT-K1:** [сбросить только первый момент AdamW m50 на границе C#→ENRU](43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18/04-SMALL-EXPERIMENTS.md#r06--первый-момент-adamw-после-смены-предметной-программы), оставив v50, step50, C50/reference, RNG и материал прежними. Общая гипотеза переноса moments уже опубликована другим автором; actual conflict пока не измерен.
- **Охват и проверка:** [исторические положительные E09/E26/E22 и их реальные consumers](43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18/03-HISTORICAL-LINEAGE.md) · [125 прежних карточек, 25 областей риска и новые статусы](43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18/05-COVERAGE-AND-EXISTING-CARDS.md) · [новизна с учётом параллельных публикаций](43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18/06-EVIDENCE-AND-NOVELTY.md) · [независимый review и выполненные исправления](43-PREPARETOGPL-RESTORATION-AND-NEURAL-DIFFS-2026-10-18/07-INDEPENDENT-REVIEW.md).

Новых native training/load/generation/teacher/evaluator calls в этом аудите — 0. Уже исполненные memory-ingress **11/24→19/24** и current cold restore **19/24→19/24** учтены как существующие результаты. Core и защищённый каталог не изменялись.

<!-- /N43-RESTORATION-AND-NEURAL-DIFFS-2026-10-18 -->

<!-- PREPARETOGPL-RECOVERY-FIRST-2026-10-18 -->
## 18.10.2026 — сначала вернуть поведение PrepareToGPL; новые конкретные различия

**[Главный приоритетный список: восстановление → работающий Unit → новые гипотезы](00-PREPARETOGPL-RECOVERY-FIRST-2026-10-18.md).** Сопоставлены PrepareToGPL 31346d8 и current a392b6b; перед публикацией учтены свежие N40–N42.

- **В начале восстановления:** старый translation checkpoint4→6 ([независимое подтверждение N41-T1 с tracked asset](PREPARETOGPL-KNN-HISTORY-2026-10-18.md)) и отдельный orchestration guide schema1→2 ([G40 коллег](40-PREPARETOGPL-RECOVERY-PRIORITY-CHAPTER-2026-10-18.md)). Guards и происхождение данных сохраняются.
- **Уже работающий путь:** память переводческого Unit0/24→19/24; исправленная доставка phrase mappings11/24→19/24; современный cold restore24/24 совпадений поведения. Эти результаты уже получены другими исполнителями, не числятся новыми pending repairs.
- **Пять дополнительных конкретных механизмов:** [subphase вне KNN, branch→registration grouping, готовый pattern](PREPARETOGPL-UNITS-RESTORE-2026-10-18.md); [cold constructors→RNG и FP32 totals→потеря margin](PREPARETOGPL-DPO-LORA-TOKENS-2026-10-18.md). One-token Q/K — отдельное уточнение применимости, не общий дефект LoRA.
- **[Короткие доменные эксперименты PG-01…PG-11 и PG-01G](PREPARETOGPL-EXPERIMENTS-2026-10-18.md)**: один фактор, ordinary consumer, явный фальсификатор, смысловые ответы и ресурсы. **[Доказательства, новизна и независимые проверки](PREPARETOGPL-EVIDENCE-AND-NOVELTY-2026-10-18.md)**.

Новые native обучение/inference/CI в этом аудите не запускались; приложены только source-contract и численный proxy с явными ограничениями. Запрошенные роли будущих DPO: CODEX/astra6 ultracode выбирает и независимо оценивает; внешняя открытая китайская модель создаёт обе стороны. Защищённый core-каталог остаётся read-only.
<!-- /PREPARETOGPL-RECOVERY-FIRST-2026-10-18 -->

<!-- PREPARETOGPL-RECOVERY-2026-10-18-AUDIT -->
## 18 октября 2026 — PrepareToGPL: восстановление прежнего поведения и дополнительные стыки обучения

**[Начать с главы восстановления PrepareToGPL](2026-10-18-preparetogpl-recovery/README.md).** Шесть документов с точными SHA, before/after и короткими опытами по предметам обучения. В начале списка — прежний checkpoint, обученный POS/AST KNN в реальном ответе и место нейронной адаптации.

- **Дополнительные конкретные находки:** [U-R2 — обязательное evidence старого manifest; U-N3 — raw PairId против производных Unit ExampleIds](2026-10-18-preparetogpl-recovery/01-translation-restore-and-unit-lineage.md); [N-L1 — намеренный legacy base-import и ожидание полного LoRA restore](2026-10-18-preparetogpl-recovery/02-neural-and-lora.md).
- **U-R1 — независимое подтверждение [N41-T1](41-FOUR-VS-SIX-TRANSLATION-CHECKPOINT-MIGRATION-2026-10-18.md):** добавлены exact old artifacts, сохранённые blob SHA и опыт восстановления без нового обучения; повторной новизной не считается.
- **[Исторические KNN и доменные опыты](2026-10-18-preparetogpl-recovery/03-historical-knn-and-domain-experiments.md):** E07/E09 port, E25 POS learner против rules, E26 context, E43 replay; учтён уже выполненный native repair 8 уроков с11/24→19/24 и wrong-accepted1→3.
- **[Карта всей цепочки DPO→Unit/LoRA→state→ответ](2026-10-18-preparetogpl-recovery/04-learning-path-crosswalk.md)** · **[источники, 555 текстовых путей, новизна и review](2026-10-18-preparetogpl-recovery/05-sources-novelty-review.md)**. Все предложенные здесь опыты NOT_RUN; новые restore-стыки не названы доказанной причиной neural RP01.

<!-- /PREPARETOGPL-RECOVERY-2026-10-18-AUDIT -->

<!-- N40-PREPARETOGPL-GOLD-FIRST-2026-10-18 -->


<!-- N41-PREPARETOGPL-INDEPENDENT-2026-10-18 -->
## N41 · четыре дополнительных расхождения и отдельные опыты восстановления PrepareToGPL (18.10.2026)

**[Индекс N41 — новые проверки + исторический baseline](41-ADDITIONAL-INDEPENDENT-GAPS-PREPARETOGPL-RECOVERY-INDEX-2026-10-18.md)**. Пишем только в possibleDifs; защищённый core/docs/plans/replanning-possible-diffs не трогаем. Сопоставлены PrepareToGPL/current источники; новые .NET, learning и blind-eval runs **NOT_RUN**.

- **P0 · [old 4 directions → current strict 6-dir translation checkpoint import](41-FOUR-VS-SIX-TRANSLATION-CHECKPOINT-MIGRATION-2026-10-18.md):** проверить восстановление прежней памяти переводчика после cold restart, не смешивать с orchestration guide checkpoint v1→v2.
- **P0 · [rejected diagnostic → positive phrase memory](41-REJECTED-SIDE-ANNOTATION-CONTROLS-POSITIVE-EXECUTOR-MEMORY-2026-10-18.md):** три одинаковых chosen, разные [PHRASE] markers; обычная unseen композиция, precision + wrong-accepted.
- **P1 · [topic_domain passed then ignored by Executor](41-TRANSLATION-TOPICDOMAIN-INPUT-ERASURE-AND-MAP-COLLISION-2026-10-18.md):** scoped legal/technical translation only, same source / two meanings.
- **P1 · [old external LoRA vs current attached projections](41-LORA-BRANCH-PARITY-AND-REAL-NEURAL-QUALITY-BOUNDARY-2026-10-18.md):** native logits/gradient sanity, не приписывать успешную память нейронному DPO.

**Приоритетная глава уже существующих проверок восстановления:** [good orchestration guide checkpoint и допустимость старых данных](40-PREPARETOGPL-RECOVERY-PRIORITY-CHAPTER-2026-10-18.md) · [исторические E09/E26/E43 и доменные отрицательные контроли](40-PREPARETOGPL-RESTORE-P0-INDEX-2026-10-18.md). Для translator memory есть выполненный native результат 8 lessons: **11/24→19/24**, но wrong-accepted **1→3** — см. [N41 индекс](41-ADDITIONAL-INDEPENDENT-GAPS-PREPARETOGPL-RECOVERY-INDEX-2026-10-18.md). Нельзя приравнивать к RP01 neural free 0/3 и 0/4.

## 18 октября 2026 — ГЛАВА 0: восстановить успешное обучение PrepareToGPL прежде чем оптимизировать RP01

**Новый приоритет:** исторически успешная ветка [PrepareToGPL](https://github.com/lubochka/llm_mvp_core/tree/PrepareToGPL) — исходный baseline для переноса *реально полезных learned mechanisms*. **Не считать рост DPO margin или изменение LoRA tensors равным улучшению свободных ответов.** RP01 зафиксировал free C# **0/3→0/3**, EN→RU **0/4→0/4**; исторические kNN / contextual memory / replay результаты решали иные учебные задачи и требуют новой paired проверяемой интеграции с ordinary consumer.

**Сначала — три новых документа, затем уже существующий архив DIF/RAG:**

1. **[GOLD-RESTORE-FIRST: PrepareToGPL vs RP01 — доказанные исторические успехи, SHA сравнение веток и очередность восстановления](40-PREPARETOGPL-RESTORE-P0-INDEX-2026-10-18.md).** Источники: E09 C# structural kNN **0.2413→0.3507** (01.05), E26 contextual dictionary **0.40→1.00** на 15 случаях, E43 replay **0.3226→0.3978**, поздний E22 predicate router **macro-F1 0.4709→0.5646** (07.05; другой corpus/labels). **GOLD-EX01/02** (code/translation learned consumer) первые, **GOLD-EX03** (retention), **GOLD-EX05** (отдельно neural warm-start), E22 по делу маршрутизации. Не переносить AST features на все домены.
2. **[N40 — две дополнительные source-backed гипотезы: незамеченное изменение corpus после checkpoint resume и отсутствие внутрибатчевого novelty comparison](40-NEW-CORPUS-IDENTITY-AND-INTRABATCH-ADMISSION-2026-10-18.md).** Это обнаруженные в коде *возможности*, **унаследованные также PrepareToGPL**, а не доказанная причина падения RP01. Отличать от ранее исследованных mutable-cache alias и operator-blind Jaccard.
3. **[Быстрые минимальные A/B по доменам с фальсификаторами, learned-recipient, cold ordinary consumer, временем/памятью и independent quality](40-MICRO-CAUSAL-EXPERIMENTS-BY-DOMAIN-2026-10-18.md)** · [исполненный JS source-only shadow](40-SOURCE-ONLY-NOVELTY-AND-RESUME-PROXY.mjs) · [сырой JSON результат](40-SOURCE-ONLY-NOVELTY-AND-RESUME-RESULT.json). **Native .NET / Student-learning quality / blind judge = NOT_RUN.** Нельзя выдавать proxy или старые PASS за подтверждённый новый quality gain.

**Read-only защищённая область:** \`lubochka/llm_mvp_core@codex/dpo-implementation/docs/plans/replanning-possible-diffs/**\` **НЕ ИЗМЕНЯЛАСЬ**. Более ранние N01–N39 и RAG первоисточники ниже сохранены без удаления и изменения их исторических статусов.

<!-- /N40-PREPARETOGPL-GOLD-FIRST-2026-10-18 -->

# XIIGEN RP01 — каталог возможных причин потери качества обучения

## 18.10.2026 — PrepareToGPL recovery P0: повторная проверка **старых успешных моделей** и новых причинных различий

**Новый независимый аудит двух branch heads (SOURCE+ARTIFACT REVIEW; native runs NOT_RUN):** [приоритетная глава 40 — три реальных расхождения восстановленного OrchestrationReasoning](40-PREPARETOGPL-RECOVERY-PRIORITY-CHAPTER-2026-10-18.md): старый checkpoint schema v1 → current loader only v2, old train8 vs 5 retained train rows after R17 / ledger has zero admitted и stale hashes, model registry removed while real API attempts manifest direct. Речь о конкретном guide-state learner, а НЕ доказанной первопричине RP01 q/v LoRA C#/ENRU 0/3 и 0/4. Не восстанавливать denied source rows обходом R17.

[Глава 41 — PrepareToGPL parity, метрики и domain-specific AST/POS/KNN](41-PREPARETOGPL-PARITY-METRIC-AND-DOMAIN-SCOPE-2026-10-18.md): old translation reward margin (0.03 heuristic) vs current 0; bootstrap-positive confidence давал PASS в старом flow, но current исключает bootstrap; при этом исходники `TranslationTrainedUnits.cs`, `TranslationSemanticTokenizer.cs`, `HnswIndex.cs` в двух проверенных heads идентичны. **Изменение отчётной метрики — не свидетельство изменения качества перевода.**

[Глава 42 — новые проверяемые гипотезы, существующий evidence crosswalk и семь небольших опытов E42-A…G](42-NEW-CAUSAL-HYPOTHESES-AND-DOMAIN-MICROEXPERIMENTS-2026-10-18.md). Начинать с source-level checkpoint-reload, затем normal consumer и только потом R17-admitted retrain; отдельно C#/translation neutral blind outcomes. Sibling `language_translate` исторически имеет другую структуру, его нельзя сравнивать по несуществующим путям позднего `27.TranslationDomain`.

**Claim limits:** GitHub source, JSON and previous receipts inspected; new native .NET experiment / training / blind judge **NOT_RUN**; not all 367 previous files read in full. Existing [N38/N39 findings](39-RP01-CROSSWALK-NOVELTY-PRIORITIES-AND-MICRO-EXPERIMENTS-2026-10-16.md) and protected core research P81–P91 preserved, not mislabeled new. **Protected `llm_mvp_core/docs/plans/replanning-possible-diffs` READ_ONLY.**

---


## N38 · CODEX — дополнительные source-confirmed расхождения и доменные быстрые проверки (16.10.2026)

- [N38 новые конкретные пробелы: generic KNN ExampleId→старый ANN node/новый output, teaching-vector density→DPO loss→early-stop, псевдо-fusion replay без независимых AST/ANN features; редкий predicate hash overflow](38-CODEX-NEW-CAUSAL-GAPS-GENERIC-KNN-DPO-LOSS-REPLAY-FUSION-2026-10-16.md). **Это три разных learned recipients, а не доказанные причины RP01 0/3 и 0/4.**
- [N38 MASTER: исходные и соседние ветки, положительные E22/E40/translation persisted-memory receipts, карта осей DPO–LoRA–BPE–KNN–Units–restore и применимость по доменам](38-CODEX-EVIDENCE-CROSSWALK-RP01-HISTORICAL-BRANCHES-2026-10-16.md). Не обнулять исторические положительные результаты и не считать proxy качеством free generation.
- [N38 MICRO: минимальные однофакторные опыты по C#, EN→RU, SQL, replay и нейронному RP01](38-CODEX-MICRO-EXPERIMENTS-BY-DOMAIN-2026-10-16.md) · [исполненный JS source-logic receipt](38-CODEX-SOURCE-LOGIC-PROXY-2026-10-16.json). **Native .NET / real Student learning / blind CODEX/Astra review NOT_RUN**. CODEX (astra 6 ultracode) выбирает и оценивает; внешняя разрешённая открытая китайская модель создаёт обе стороны DPO.

**Изменена только плановая папка possibleDifs.** Защищённый `llm_mvp_core/docs/plans/replanning-possible-diffs` остаётся READ_ONLY.

<!-- N37-INDEPENDENT-NEW-GAPS-2026-10-16 -->
## N37 · три новых отдельных механизма, которые нельзя приписывать RP01 без consumer trace (16.10.2026)

**Дополнительное независимое чтение current source + sibling branches; native .NET experiments NOT_RUN.** Эти записи **не повторяют** прежние классы 1–26 и N35–N36: их отдельное условие и фальсификатор указаны в собственных карточках.

- [N37-01: mutable learned KNN Output и PredicateIds](37-NEW-KNN-UNIT-MUTABLE-OUTPUT-ALIAS-2026-10-16.md) — у generic typed Unit исходный `TOutput` / массивы не deep-owned; ответ может измениться **без Train**; только если actual consumer передает mutable payload. Сверен старый `language_translate`/`DesignToCode`/`external_domains`: **унаследованный**, не доказанный RP01 regression.
- [N37-02: float32 sampler residual-tail выбирает masked last-token](37-NEW-FLOAT32-SAMPLER-MASK-ESCAPE-2026-10-16.md) — условная ветка stochastic decoder; [repro JS source-logic](37-MICRO-SAMPLER-FLOAT32-TAIL-PROXY.mjs) и [численный receipt](37-MICRO-SAMPLER-FLOAT32-TAIL-PROXY-RESULT.json), **не native model outcome**. При RP01 greedy не объясняет 0/3→0/3, 0/4→0/4.
- [N37-03: точная модель автора DPO chosen/rejected теряется в trainer admission schema](37-NEW-PER-SIDE-DPO-TEACHER-MODEL-ID-LOSS-2026-10-16.md) — `OpenTeacher` есть как класс, но нет `chosen.modelId/rejected.modelId`; возможный конфликт с проектным требованием о разных моделях только после проверки области действия и upstream manifests. Не доказано снижение learning quality.
- [Короткие независимые native опыты N37 для C# и SQL + отдельный decoder](37-MICRO-TYPED-UNIT-AND-SAMPLER-CAUSAL-PROTOCOL-2026-10-16.md), [16 проверочных domain-law cases](37-MICRO-TYPED-UNIT-DOMAIN-CASES-2026-10-16.json). Не запускались; фиксируют paired arms, фальсификаторы, blind law metric и запрет на Windows CI / DPO500.

**Общий источник реальных исторических успехов, а не новая сенсация:** [core/65 E22](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/65-e22-positive-late-run-and-label-transfer.md) — macro-F1 0.4709→0.5646 на позднем результате при изменённом корпусе и label function; [core/38 E09](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/38-e09-transfer-labels-and-opt-in-consumer.md) — перенос structural KNN, но default opt-in off/не доказан ordinary consumer. Соседний `core/docs/plans/replanning-possible-diffs` для N37 **не редактировался**.


## 16.10.2026 — N37: новые KNN learning-качество seam и быстрый причинный falsifier

- [N37 MASTER — точная карта RP01 Student DPO/LoRA против отдельных KNN/Unit/Graph consumers, исторические успешные E07/E09/E14/E22/E26/E43 и научные приоритеты](37-RP01-EVIDENCE-CROSSWALK-NEW-HYPOTHESES-AND-CAUSAL-PRIORITIES-2026-10-16.md).
- [N37 A/B/C — новая связка: потеря UNKNOWN-условий и true/false при построении полного semantic key; каскад KNN прекращается до проверки target; SourceSimilarity асимметрична](37-NEW-KNN-UNKNOWN-TAIL-AND-POSTFILTER-CASCADE-2026-10-16.md).
- [N37 tiny native A/B/C и доменные критерии фальсификации](37-MICRO-THREE-FACTOR-KNN-CAUSAL-PROTOCOL-2026-10-16.md); [исполненный узкий JS source-logic proxy](37-MICRO-CASCADE-UNKNOWN-PROXY.mjs), [его JSON-результат](37-MICRO-CASCADE-UNKNOWN-PROXY-RESULT.json). **Source-logic только; native .NET, реальный Student learning и independent blind exam NOT_RUN.**
- Сравниваем только одинаковый learned recipient→ordinary consumer. Этот Translator KNN **не объявляется причиной** RP01 C#500/ENRU500 neural free score 0/3 и 0/4. Protected llm_mvp_core/docs/plans/replanning-possible-diffs/** только READ-ONLY.

## 16.10.2026 — N35: RAG contract, все получатели обучения и loss-only stop

- [Общая карта RP01 / historical E07, E09, E14, E26, E43 и независимые ограничения по доменам](35-RP01-ALL-RECIPIENTS-CAUSAL-PRIORITY-MAP-2026-10-16.md) — существующие 16 осей и новая N35, первичные code/source links; **не репликация learning runs**.
- [Новый N35-CVG: generated-capability DPO может остановиться по двум малым изменениям train-loss на step3 при лимите5](35-NEW-GENERATED-CAPABILITY-DPO-LOSS-PLATEAU-STOP-2026-10-16.md), [5-update однофакторный A/B по доменам](35-MICRO-DPO-CONVERGENCE-VS-DOMAIN-QUALITY-2026-10-16.md). **Реальный RP01 Student идёт по другому пути; native quality NOT_RUN.**
- [RAG source: applied-once/accepted-checkpoint/independent semantic judge/rollback vs RP01 bulk simulation](35-RAG-CANONICAL-LEARNING-LOOP-VS-RP01-2026-10-16.md). **Несовпадение доказательных границ, не установленный causal root RP01.**
- Core `llm_mvp_core/docs/plans/replanning-possible-diffs/**` **READ ONLY**; original RP01 scores and historical experiment results не изменены; новый native/judge эксперимент пока **NOT_RUN**.


## N34-E43 — positive replay: зависимость исторического успеха от file boundaries (16.10.2026)

- [Новый адресный аудит положительного исторического E43](34-E43-POSITIVE-REPLAY-FILE-BOUNDARY-CAUSAL-GAP-2026-10-16.md): при склеивании файлов `ExtractPairs` создаёт boundary bigrams, в отчёте старый `oldVal` получен не file-disjoint; влияние на `0.3226→0.3978` **пока гипотеза, не доказанная причина RP01**. Это уточнение уже известного N32-E09 механизма применительно к *позитивному* E43.
- [Малый парный 4-arm эксперимент](34-MICRO-E43-REPLAY-CAUSAL-PROTOCOL-2026-10-16.md); [исполненный SOURCE-ONLY скрипт](34-E43-FILE-BOUNDARY-SOURCE-PROXY.mjs) и [сырой результат](34-E43-FILE-BOUNDARY-SOURCE-PROXY-RESULT.json). 7 исторических соединений vs 4 настоящие пары в toy fixture, из первых 3 межфайловые; native обучение и quality **NOT_RUN**.
- [Свежая 22-осевая crosswalk-карта](34-RP01-FRESH-TRIAGE-AND-DOMAIN-CAUSAL-ORDER-2026-10-16.md): DPO, LoRA, tokenizer, KNN, Unit/restore, RAG, feedback, replay, доменные эксперименты и приоритеты научной проверки. Не выдавать уже найденные другими агентами GrammaticalRole, MaxAnswerChars и E09 Zip за свои новые открытия.


## 16.10.2026 — N35: первичные новые гипотезы и полный RP01 crosswalk

- [N35 Master — RP01, DPO/LoRA/KNN/Unit/Graph/checkpoint и исторические branches: источники и приоритеты](35-MASTER-RP01-HISTORICAL-LEARNING-CROSSWALK-NOVELTY-AND-EXPERIMENT-ORDER-2026-10-16.md)
- [N35-A C# AST KNN: unused SiblingCount](35-NEW-E09-SIBLING-FEATURE-INVISIBLE-IN-STRUCTURAL-KNN-2026-10-16.md); [N35-B редкий класс без validation](35-NEW-RARE-CLASS-VALIDATION-BLINDNESS-IN-STRATIFIED-SPLIT-2026-10-16.md); [N35-C stop-gradient Pool/Normalize/EmbedBatch](35-NEW-EMBEDDING-POOL-NORMALIZE-TRAINABILITY-CUT-2026-10-16.md).
- [JS synthetic source-shadow receipt](35-MICRO-SOURCE-SHADOW-RECEIPT-2026-10-16.json). **Native .NET и independent CODEX/Astra learning-quality evaluation ещё не проведены.** Protected core/docs/plans/replanning-possible-diffs не тронут.


## N34 — новый source-confirmed аудит pretraining data path (16.10.2026)

- [N34: разные EOF/EOS, сильно перекрывающиеся окна pretraining, silent clamp uint16, разрывы shard/document boundaries и file-order drift](34-NEW-PRETRAIN-SOURCE-AND-BATCHING-GAPS-2026-10-16.md). Код подтверждён. **Не объявлять эти находки root cause DPO RP01 C#500/ENRU500** — этот конкретный pipeline в замороженной кампании не использовался. Механизмы унаследованы также `DesignToCode/language_translate/external_domains`, а не доказанная потеря при переносе.
- [Короткий доменный однофакторный A/B план](34-MICRO-PRETRAIN-DOMAIN-AB-EXPERIMENTS-2026-10-16.md): C# документные границы, RU/EN sliding windows, SQL/DSL vocab >65535 (условно), manifest/cold restore. Прежде чем мерить качество обучения — [проверить N33 gradient/warmup](33-NEW-AUTOGRAD-EMBEDDING-BIAS-AND-SUPERVISED-WARMUP-GAPS-2026-10-16.md).
- [Исполненный source-only Node proxy](34-SOURCE-ONLY-DATA-PIPELINE-PROXY.mjs) → [JSON receipt](34-SOURCE-ONLY-DATA-PIPELINE-PROXY-RESULT.json): seq4/batch3 => offsets 0/1/2, 6 уникальных input IDs vs 12 stride4; EOF1 != EOS628; 65536/70000→65535. **Это не native .NET run и не проверка свободных ответов**.
- [Связный индекс всех основных классов RP01/Unit/KNN/DPO/исторических experiments и N34](34-RP01-GAP-AND-EVIDENCE-CROSSWALK-2026-10-16.md): владельцы прежних доказательств и разделение learner/consumer.



## 16.10.2026 — N33-LEX и N34-ALIAS: два дополнительных независимых source-confirmed механизма

- [Полная RP01→исторические E07/E09/E14/E26/E43/DesignToCode→Unit/KNN crosswalk и приоритет коротких экспериментов](33-INDEPENDENT-RP01-GAP-AND-HISTORICAL-SUCCESS-CROSSWALK-2026-10-16.md), включая параллельные N33-autograd/SFT и E44/E45 target confounding из других опубликованных работ.
- **N33-LEX:** [filtered token ordinal drift в phrase→word-map](33-NEW-PHRASE-FILTERED-ORDINAL-INDEX-DRIFT-2026-10-16.md), [4-case executed source proxy](33-MICRO-PHRASE-INDEX-SOURCE-PROXY-RESULT.json), [быстрый native paired A/B](33-MICRO-PAIRED-PUNCTUATION-INDEX-CAUSAL-EXPERIMENT-2026-10-16.md). Кодовый дефект условен; `native .NET NOT_RUN`; phrase-map 0.86 может пройти Verifier FAIL (известный N16-06).
- **N34-ALIAS:** [one lesson twice under equal full/prefix key can fill fixed KNN top-k](34-NEW-ONE-LESSON-DOUBLE-INSERT-UNDER-ALIAS-SIGNATURE-2026-10-16.md), [conditional source-only proxy](34-MICRO-KNN-ALIAS-TOPK-SOURCE-PROXY-RESULT.json) и микро-протокол в том же документе. Native/product semantic gain не измерен.
- **Уровень доказательств:** source logic + JS proxies ≠ обучившаяся модель; фактический RP01 Student `0/3→0/3`, `0/4→0/4` не объяснён эти двумя не-нейронными seam. Core protected `docs/plans/replanning-possible-diffs` НЕ менялся.

## N33 — NEW: разрыв autograd в Embedding, Linear.bias и нулевой supervised warmup (16.10.2026)

- [Новые source-confirmed дефекты и ограничения области применимости](33-NEW-AUTOGRAD-EMBEDDING-BIAS-AND-SUPERVISED-WARMUP-GAPS-2026-10-16.md): `Embedding.Forward` не даёт градиент таблице токенов; `Linear.ExpandBias` отсоединяет bias (но обычная RP01 Transformer использует bias=false); первый `TrainSupervisedNextTokenStep` с `WarmupSteps=100` получает LR=0 и выбрасывает исключение; первый pretraining warmup может засчитывать нулевой weight update; cold supervised LR counter — условная гипотеза. **НЕ подтверждена причинность уже состоявшегося RP01 DPO free 0/3 и 0/4, native NOT_RUN.**
- [Быстрые native finite-difference и SFT A/B эксперименты](33-MICRO-AUTOGRAD-WARMUP-AND-LEARNING-IMPACT-2026-10-16.md): повторённый ID Embedding с аналитическими градиентами 2/1; zero-LR first step; bias gradient; one-factor training→heldout→cold restore. **Все запуски ещё NOT_RUN.**
- [Мастер-дополнение к 26 известным классам причин, исторические positive/FAILED matrix→KNN и порядок исполнения](33-MASTER-DELTA-RP01-HISTORY-RECIPIENTS-AND-FAST-CAUSAL-ORDER-2026-10-16.md): P0 сначала невидимые блокеры neural warm-start, отдельно реальный KNN/Unit consumer и независимый blind domain-law evaluation.
- **Роли:** CODEX/Astra 6 UltraCode выбирает учебный пример (human role) и отдельно оценивает обе DPO стороны + learned answers; внешняя допущенная открытая китайская модель создаёт **обе** финальные `chosen`/`rejected`. **Core protected research — READ ONLY**.



## N33 — новые подтверждённые source-пробелы в E44v2/E45v2 (16.10.2026)

- [Primary code + sibling branches: baseline/enhanced E44v2 и E45v2 обучают разные пары токенов, D-09/D-08 интерпретация конфундирована](33-NEW-E44-E45-PAIR-TARGET-CONFOUND-2026-10-16.md). **Подтверждён механизм формирования labels, НЕ доказана причина RP01 0/3 или продукта.**
- [Малый однофакторный native A/B: фиксируем genuine source-pairs, сравниваем только ×10/dynamic веса и порядок easy→hard](33-MICRO-E44-E45-FACTORIAL-QUALITY-PILOT-2026-10-16.md); [исполненный toy proxy JS](33-N33-PAIR-TARGET-SYNTHETIC-PROXY.mjs) → [его JSON receipt](33-N33-PAIR-TARGET-SYNTHETIC-PROXY-RESULT.json). **Только synthetic proxy исполнен; native обучение/проверка качества NOT_RUN.**
- [Общая crosswalk-карта N33: 18 осей DPO/LoRA/BPE/KNN/Units/restore/domain/evaluation + история positives и границы источников](33-MASTER-CROSSWALK-QUALITY-LOSS-AND-NOVEL-N33-2026-10-16.md). Не переписывает более ранние агентские отчёты и protected core notes.


## N33 — независимая сверка real learning с оценкой и новые falsifiers (16.10.2026)

- [Сводная карта N33: RP01 Student против trainable Units, реальные исторические результаты, RAG provenance, матрица→KNN и доменные границы](33-RP01-INDEPENDENT-DELTA-AND-RECIPIENT-MAP-2026-10-16.md). Используется существующий каталог доказательств; **не заявляется повторный native training run и не переобозначаются старые E09/E26/E43 результаты**.
- [Два N33 source-confirmed новых узких расхождения](33-NOVEL-UNIT-SCORE-AND-STUDENT-LENGTH-PARITY-2026-10-16.md): **Unit heldout macro-mean** по группам вместо доли верных конечных запросов; **разная enforcement политика MaxAnswerChars** в simulation raw generation и saved-model assessment. Их влияние на quality — **HYPOTHESIS**, не установленная причина RP01 0/3 или ENRU 0/4.
- [MICRO-N33: минимальный парный native-план с обычным user Flow, новым законным уроком, source-disjoint holdout, cold restore, M0/M1/M2, numerator/denominator и output-policy probe](33-MICRO-RECIPIENT-AND-METRIC-QUALITY-EXPERIMENT-2026-10-16.md). **PLAN ONLY / NATIVE NOT_RUN / INDEPENDENT JUDGE NOT_CALLED**. Core `docs/plans/replanning-possible-diffs` не изменять.


## N30 — Source-independent новые различия и полная карта существующих гипотез (16.10.2026)

- [Мастер-карта 26 классов возможной потери качества, первичные успешные E26/E14/E43/E09 и sibling DesignToCode, порядок causal-проверок](30-MASTER-QUALITY-LOSS-COVERAGE-AND-SOURCE-LINEAGE-2026-10-16.md). **Это индекс доказательств, не выполненный native learning run.**
- [Три N30 находки по реальному C#: жадная leftmost phrase-композиция при 100% coverage, \`can't/can\` collision в KNN source guard, operator \`->\` erasure с ложным phrase match](30-INDEPENDENT-NOVEL-COMPOSITION-CONTRACTION-AND-OPERATOR-GAPS-2026-10-16.md). Условная applicability: EN→RU и DSL/Graph; **не заявлены прямой причиной LoRA Student RP01**.
- [Короткие one-factor native experiments и поручение CODEX](30-MICRO-NATIVE-CAUSAL-PILOTS-AND-CODEX-EXECUTOR-WORK-ORDER-2026-10-16.md) · [source-only JS proxy](30-SOURCE-LOGIC-PROXY.mjs) · [сохранённый JSON результат](30-SOURCE-LOGIC-PROXY-RESULT.json). Аналитический proxy показывает два competing outputs и две source normalizer collisions, **native run + blind CODEX/Astra 6 UltraCode quality NOT_RUN**. Проверка также **опровергла** первоначальное объяснение через перестановку порядка двух фраз.
- Непереопределяемые роли: **CODEX/Astra 6 UltraCode выбирает учебный случай (human role) и независимо оценивает качество обеих DPO sides и итоговые ответы; обе финальные \`chosen/rejected\` создаёт разрешённая внешняя открытая китайская модель.** Указанные внешние операции здесь не выполнялись. Core protected planning docs **READ ONLY**.



## N28-U/S — новые Unicode и mixed-script seams обученного Executor (16.10.2026)

- [Новые гипотезы: Unicode NFC/NFD `sí` ↔ `si`, ложное распознавание законной `Ñ` как mojibake](28-NEW-UNICODE-CANONICAL-KEY-AND-SPANISH-N-FALSE-MOJIBAKE-2026-10-16.md); [несовместимый фильтр латинских API-идентификаторов в EN→RU KNN и PhraseMap](28-NEW-ENRU-MIXED-SCRIPT-PROTECTED-IDENTIFIER-REJECTION-2026-10-16.md).
- [Три коротких однофакторных native A/B, law-contrast и проверка после cold restore](28-MICRO-UNICODE-AND-MIXED-SCRIPT-LEARNING-EXPERIMENTS-2026-10-16.md), [source-only JS proxy](28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY.mjs) и [его receipt](28-MICRO-UNICODE-AND-SCRIPT-SOURCE-PROXY-RESULT.json).
- [Дельта-карта: новые N28 относительно старых DPO/LoRA, AST/KNN, Units, replay/restore и исторических положительных receipts](28-DELTA-CROSSWALK-NOVEL-UNICODE-AND-SCRIPT-2026-10-16.md). **Кодовые ветви / JS proxy подтверждены; native learning quality, отдельная оценка CODEX/Astra 6 UltraCode NOT_RUN.** Human choice — CODEX/Astra 6 UltraCode, обе DPO стороны — разрешённая внешняя китайская open model, их independent quality — CODEX/Astra 6 UltraCode. Core protected planning docs READ ONLY.


## N28 — новый source-confirmed разрыв оценки обученного перевода (16.10.2026)

- [Новый независимый аудит реального TranslationProductHeldOutRunner: marker-only ProductMatch, неверная оценка отрицания/порядка ролей, недекларированные emoji, метрики без учёта порядка и ложный отказ на потолке](28-NEW-PRODUCT-HELDOUT-SEMANTIC-GATE-DIVERGENCE-2026-10-16.md). **Кодовый observer-gap доказан, качество живой модели НЕ измерено; direct RP01 Student cause НЕ доказана.** Это не тот же scorer, что N23 Tatoeba, и не повтор N27 negative-only Verifier.
- [Короткий CODEX/Astra 6 UltraCode work order: 4-case proxy → native scorer A/B → лишь условно cold-restored TranslationMachine Unit](28-MICRO-PRODUCT-HELDOUT-SEMANTIC-GATE-2026-10-16.md). [Исполняемый Python proxy](28-PRODUCT-HELDOUT-SCORER-PROXY.py) и [его результат](28-PRODUCT-HELDOUT-SCORER-PROXY-RESULT.json): **4/4 формальных ложных accept на специально подобранных случаях; только PYTHON_LOGIC_PROXY**, не нативная C# проверка и не реальная accuracy.
- Роли не менять: **CODEX/Astra 6 UltraCode** выбирает учебный случай и независимо проверяет качество окончательных DPO-пар; внешняя открытая китайская модель генерирует **обе** стороны **chosen/rejected**. Никакие DPO вызовы здесь не выполнялись. Каталог `llm_mvp_core/docs/plans/replanning-possible-diffs` read-only.

## N27 — дополнительный source-аудит сохранения ранее выученных законов (16.10.2026)

- [Новые расхождения: top3 replay aperture, различия DPO/KNN recipients, непрозрачная вложенная история и исторический `DesignToCode`](27-NEW-REPLAY-APERTURE-DPO-KNN-DIVERGENCE-2026-10-16.md). Подтверждено **по коду**, ухудшение настоящего ответа **не измерялось**.
- [Небольшой проверочный эксперимент: frozen-policy KNN memory A/B, затем только при различении tiny DPO replay A/B](27-MICRO-EXPERIMENT-REPLAY-APERTURE-AND-RECIPIENT-PARITY-2026-10-16.md). `.NET native` / независимый `CODEX/Astra 6 UltraCode` judge **не запускались**.
- [Source-only JS microproxy](27-MICRO-REPLAY-APERTURE-SELECTION-PROXY.mjs) и [сырые результаты](27-MICRO-REPLAY-APERTURE-SELECTION-PROXY-RESULT.json): на шести синтетических manifest выбран максимум 3 для нового KNN и 1 для DPO; это не verified learning gain.
- Граница: историческая `DesignToCode` брала 10 самых новых перед top3, но **нынешний основной** `TrainAndRegister` проверяет full pool и выбирает top3. Текущий `FindReplayManifestsResult` с top10 в прочитанном классе не найден как активный caller. Не смешивать ветки.
- Роли: **CODEX на Astra 6 UltraCode выбирает** предметный учебный случай, **разрешённая внешняя открытая китайская модель создаёт обе стороны DPO**, **CODEX на Astra 6 UltraCode независимо проверяет качество пары и реального Student ответа**. Source-only proxy не выдавать за выполнение этих ролей.



## 23 · Новый независимый кодовый аудит языковых признаков и быстрый проверочный опыт (16.10.2026)

- [Новые source-confirmed различия](23-INDEPENDENT-LANGUAGE-ROUTING-POS-AND-CROSS-CLAUSE-GAPS-2026-10-16.md): language case `ru/RU` после нормализации в роутере, но не в обучаемом tokenizer; грубые EN→RU POS-адреса `NOUN/SVO`; перенос `prevConcept` через точку; отдельно условный `topK/topP` decoder. **Это разные conditional code seams, native quality НЕ измерено.**
- [Причинный микроэксперимент](23-MICRO-TOKENIZER-LEARNING-QUALITY-PILOT.md): **одна** обученная память Unit, одинаковый source, только `source_language="RU"` против `"ru"`, exact-control/no-training/shuffled-label и blind source-disjoint correctness. Не смешивать с RP01 LoRA, не запускать 500 DPO пар и Windows CI.
- [Четыре training rows и 12 предложенных heldout запросов](23-LANGUAGE-CASING-PAIRED-PILOT-FIXTURE.json) — machine-readable draft, **gold требует независимого approval; эксперимент NOT_RUN**.
- [Реестр всех классов различий](23-MASTER-GAP-TRIAGE-AND-EXPERIMENT-PRIORITY.md) — B0/DPO/LoRA/BPE/KNN/Unit/restore/metric/ordinary consumer; links к существующим 44 DIF и найденным другими агентами фактам. [Зеркало вывода в core](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/13-independent-translation-language-parity-and-structure.md).

**Дедупликация:** [параллельный E26 POS-контекст](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md) исследует отсутствие **left/right POS** при переносе старого словаря в Unit; текущий аудит `prevConcept` — другое расхождение, а `ru/RU` — отдельная языковая граница. Ни один не объясняет сам по себе `RP01 Student free 0/3, 0/4` без связи с actual consumer.



## N23 — new independent verifier and semantic-prefix causal hypotheses (16.10.2026)

**Новые проверенные по коду механизмы, НЕ новый native learning run:**
- [N23: накопление требований как AND и общие правила QualityVerifier между 6 направлениями](23-VERIFIER-ACCUMULATED-RULE-CONFLICT-AND-DIRECTION-SCOPE-2026-10-16.md) — проверяющий может отклонить ранее правильный ответ после следующего законного урока; направление в правиле отсутствует. **Source-confirmed seam, impact H.**
- [EXP23: малый native контраст A→A+B на том же ответе + source-disjoint законный экзамен](23-MICRO-VERIFIER-CAUSAL-LEARNING-EXPERIMENT-2026-10-16.md) — исполнимый протокол, **не выполненная .NET-оценка**.
- [N23: Transformer-KNN semantic alias при обрезании до 64 BPE токенов](23-TRANSFORMER-KNN-64TOKEN-PREFIX-ALIAS-2026-10-16.md) — downstream BM25 может компенсировать, поэтому quality loss надо доказать paired native contrast.
- [Зеркало источников и границы результатов в core docs](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/15-verifier-accumulated-law-conflicts-and-64token-semantic-alias.md).

Сверка с архивами: прежние N14/N16/N20/N21/N22 и 44 DIF остаются действующими, новая группа их не заменяет. Кодовые механизмы присутствуют также в части старых веток; **нельзя считать их вновь внесённой регрессией RP01 без доказательства lineage**. Сохранённый RP01 Student-only free output C#0/3, ENRU0/4 этим экспериментом пока не объяснён.



## 23–25 · Независимые новые различия: E26 POS-контекст и E36 feedback KNN

**Дополнительное чтение первичных C# и двух архивных JSON 16.10.2026; независимый blind/native quality RUN не выполнен.** Это не повторение DIF-01…44 и более поздних N14/N16/N20/N21/N22, а два конкретных gap в переносе исторических функций.

1. [NEW-23 — E26 context dictionary → обучаемый ExecutorUnit](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md). Архивный E26: **40%→100% на 15 заранее заданных случаях**, но BuildDefault заранее содержит словарь, поле _k не используется, результат не доказывает тренировку. В нынешнем SemanticTokenizer контекст обрезан до нескольких правил **previous concept** (для «поток» оба → THREAD_REF), тогда как отдельные PosBigrams уже вычисляются; ExecutorUnit word-map ключуется (lemma,concept)/surface без **left/right POS**. Старый числовой POS enum и новый TranslationPos также несовместимы без явного маппинга. **Кодовый разрыв подтверждён, влияние на ordinary answer — H/NOT_RUN.**
2. [NEW-24 — E36 feedback correction kNN](24-NEW-E36-FEEDBACK-KNN-SPLIT-AND-ROUTING-2026-10-16.md). 15 повторно случайно выбираемых шаблонов разбиваются **по строкам** на train/val; train и val могут иметь одинаковый текст. Исторические tier1 **2/8** (PASSED) и tier2 **0/16** (FAILED), оба оценивают только 2 из 5 классов в val; это **не доказательство** положительной/отрицательной способности направлять *новые* пользовательские исправления к правильному Unit.
3. **[MICRO-25 — быстрый один-факторный causal опыт](25-MICRO-E26-POS-CONTEXT-CAUSAL-EXPERIMENT-2026-10-16.md).** 6–12 утверждённых учебных различений, 12 независимых новых формулировок, A=current Unit против B=same learner + POS-context memory, shuffled-label и rule-only controls; trace до обычного TrainedTranslationAdapter, blind correctness/retention, после холодного restore. **Готовый work order, NOT_RUN.** Отдельный E36-SPLIT-01 описан в NEW-24; не смешивать в одном A/B.

**Дедупликация:** E26 не переоткрывает общий top1 KNN из DIF-07; E36 не является повторением несопоставимых validation subsets E09/E34. Общая причинная гипотеза остаётся **несовпадением recipients: старые структурные функции/KNN/словари ≠ RP01 q/v-LoRA Student**, поэтому даже успех микро Unit не будет доказательством ремонта RP01 генерации без отдельного matched consumer contrast.


## Дополнительный независимый механизм 20 — не-локальная регрессия обученной KNN-памяти (16.10.2026)

**Новые проверенные code seams, которых нет в исходной матрице 44 DIF:** в отдельном `TransformerEmbeddingKnnLogicalUnit` кандидатная lexical-нормализация `score/maxLexical(current pool)` делает рейтинг **ранее выученных** ответов зависимым от вновь добавленного *нерелевантного* урока, даже когда сам новый урок не выбран. `UNKNOWN==UNKNOWN` дополнительно даёт полный structural bonus **+0.20** для чисто не-ASCII текстов, у которых regex root-канала `[^a-z]+` не извлекает корни. Это анализ **отдельного обучаемого KNN Unit**, а не доказанная причина RP01 Student free-generation.

- [20 — точный source audit, контрпример, уровни доказательства и дедупликация](20-LESSON-ADDITION-RERANKER-NONLOCAL-QUALITY-GAPS-2026-10-16.md)
- [MICRO-20.01 — matched causal native pilot: 12 TRAIN + 4 DEV + 16 HELDOUT, один denominator diff](20-MICRO-RERANKER-LESSON-ADDITION-CAUSAL-EXPERIMENT.md)
- [Запускаемый аналитический Node proxy](20-RERANKER-NONLOCAL-PROXY.mjs) и [зафиксированный вывод](20-RERANKER-NONLOCAL-PROXY-RESULT.json): в модельном примере correct A→wrong B, хотя новый C не выигрывает. **Численная возможность подтверждена, native C# / blinded quality NOT_RUN.**

Дата: 2026-10-16. Источник планирования: `xiigen-planning@bfcc610bbaf3cffa54b921b2f435dc616955e2cb` / `llm_plan3`. Исходный код: `llm_mvp_core@83590a462b2f8aedc9d3ab4cb775193eb9c25ce5` / `codex/dpo-implementation`.

## EXP01 — минимальный причинный опыт на обучаемой памяти

[Протокол: четырёхarmовая проверка с C до/после обучения и отдельным restore](11-EXP01-UNIT-MEMORY-IMPACT.md) · [4 учебные пары и 14 пробных задач](11-EXP01-CASES.json) · [точное поручение CODEX](11-EXP01-EXECUTOR-WORK-ORDER.md) · [малые one-factor пробы других DIF](11-OTHER-CAUSAL-MICROPROBES.md).

**Пока это только материалы эксперимента — runner и модель здесь не запускались.** Он покажет, улучшает ли сохранённая память ExecutorUnit пользовательские переводы, не смешивая этот эффект с DPO/LoRA. Нельзя объявлять улучшение памяти доказанной причиной неудачи самого RP01 Student без отдельного matched Student+memory contrast.

## Что именно обнаружено
**Итог проверки — документарный причинный аудит, а не новый запуск модели.** Существующие эксперименты RP01 действительно меняют LoRA, но их сохранённые научные результаты **не показывают прироста смыслового качества**. Критический вопрос: почему некоторые механизмы помогали в старых опытах и какие из этих механизмов отсутствуют, трансформированы или не вызваны на нынешнем Student→ordinary consumer path.

| Уровень доказательства | Значение |
| --- | --- |
| **C — подтверждено исходным кодом** | Точное поведение названного метода в указанной версии, но не обязательно причина ухудшения качества. |
| **M — измерено сохранённым артефактом** | Реальный результат завершённого исторического прогона, но не повторное измерение здесь. |
| **S — подтверждено документом/архитектурным контрактом** | Требование или ранее выявленный пробел; текущая реализация может иметь другую ветку. |
| **H — гипотеза** | Правдоподобное объяснение без причинной проверки, не объявлять дефектом. |
| **U — не проверено** | Не обследован полный маршрут или не проведён требуемый эксперимент. |

## Непереопределяемые роли при создании и оценке DPO
**Человеческую роль в выборе учебной задачи выполняет CODEX с моделью Astra 6 UltraCode.** CODEX выбирает и утверждает конкретный законный урок/ошибку Student, контекст и ожидаемый учебный recipient. Это НЕ поручается внешней китайской модели.

**Обе финальные стороны DPO (chosen и rejected) создаёт разрешённая внешняя открытая китайская модель.** Она выступает автором обоих ответов на одном и том же prompt/context, а не решающим судьёй и не Student. Права на использование конкретной модели, провайдера, версии и результатов требуют отдельного фактического подтверждения.

**Качество полученной DPO-пары отдельно оценивает CODEX, также с моделью Astra 6 UltraCode.** Это независимое от генерации назначение: проверить доменный закон, корректность chosen, содержательную ошибку rejected, точные байты и происхождение; допустимые исходы включают VALID, TIE, BOTH_BAD и UNJUDGEABLE. Проверка полезности **обученного Student** после optimizer update — ещё одна самостоятельная оценка, не заменяемая приёмкой пары.

[Прямой контракт ролей WorkLeft-02, строки 6–18](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/09.09.26WorkLeft-02-teaching-chinese-model.md#L6-L18). В отличие от требуемого внешнего режима, существующий [RP01 offline simulation importer](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L99-L125) маркирует стороны как `DeterministicGenerator`; **нельзя переименовать исторические simulation-пары в реально созданные китайской моделью**. В этой документарной проверке внешняя модель и CODEX/Astra не запускались; внешний независимый verdict НЕ получен.

## Четыре важных вывода
1. **Обучаемость исходной модели — первейший вопрос.** RP01 стартовал со случайной необученной модели на 629 токенов; DPO-предпочтения и q/v-LoRA не гарантируют возникновения грамматики и генеративной компетенции. Это **гипотеза причинности**, а не доказанная единственная причина. [generation-v1.json](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/config/code-documentation/generation-v1.json#L7-L49); [RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md#L21-L35).
2. **Признаки превращаются в почти несодержательный вес.** Для RP01 DPO-preparation при отсутствии вектора строит его заново из prompt, а DPO берет лишь число ненулевых слотов: теряется семантическая координата как обучающий target. Это **C**. [LlmIngredientLogicalUnits.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/18.CapabilityTraining/LlmIngredientLogicalUnits.cs#L1711-L1745); [TrainingVectorBuilder.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/23.SemanticCompilerAdapter/TrainingVectorBuilder.cs#L14-L61); [DpoTrainer.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/11.DPO/DpoTrainer.cs#L1159-L1166).
3. **Старый гибридный поиск не равен текущему RP01 inference.** Старые structural-kNN, BM25+kNN, arbiter-сигналы и replay измерялись в других runtime/моделях; RP01 experiment вызывает Transformer/LoRA, а не этот составной retrieval+law→typed Unit→Flow→Graph pipeline. Нужна проверка трассы обычного запроса, не копирование «прошлого PASS». [CodeDocumentationSimulationDpoExperiment.cs](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/14.ContinuousLearning/Training/CodeDocumentationSimulationDpoExperiment.cs#L103-L139); [DECISIONS-LOCKED.md](https://github.com/lubochka/xiigen-planning/blob/bfcc610bbaf3cffa54b921b2f435dc616955e2cb/llm%20mvp/llm%20dynamic%20flow/DECISIONS-LOCKED.md#L9-L21).
4. **Ошибка checkpoint не подтверждена.** Два опубликованных прогона сообщают о полном reopen весов/optimizer/RNG и неизменной базе; дальнейшие исследования восстановления — boundary, identity, ordinary serving и продолжение через домены, а не повторное объявление restore дефектом. [scientific-result.json](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/.kiro/xiigen-slices/work/results/RP01-simulation-main500-execution-20261015/scientific-result.json#L8-L17); [scientific-result.json](https://github.com/lubochka/llm_mvp_core/blob/83590a462b2f8aedc9d3ab4cb775193eb9c25ce5/.kiro/xiigen-slices/work/results/RP01-enru-continuation-20261016/runtime-20261016-01/analysis-20261016-01/scientific-result.json#L11-L28).

## Навигация
- [00 — общая причинная оценка и перекрёстные проверки](00-MASTER-SYNTHESIS-AND-CONFLICTS.md)
- [01 — матрица всех возможных пробелов](01-PRIORITIZED-DIFFERENCES.md)
- [02 — фактические старые эксперименты и зафиксированные решения](02-HISTORICAL-EXPERIMENTS.md)
- [03 — пары DPO, objective, masks, авторство](03-DPO-PAIR-AND-GRADIENT.md)
- [04 — base/LoRA/tokenizer/restore/inference](04-BASE-LORA-CHECKPOINT.md)
- [05 — векторы, семантика, kNN и arbiter](05-SEMANTIC-VECTOR-KNN.md)
- [06 — Unit / Flow / Orchestrator / Graph и настоящий consumer](06-UNITS-FLOWS-GRAPH.md)
- [07 — retention, обучение между доменами, обратная связь](07-CONTINUAL-REPLAY.md)
- [08 — различающие малозатратные эксперименты](08-CAUSAL-DIAGNOSTICS.md)
- [09 — источники, ограничения и handoff исполнителю](09-SOURCE-AND-REVIEW-LEDGER.md)

**Границы:** не проводились модельные вызовы, training, fresh blind examination, Windows CI, самостоятельный аудит всех многомегабайтных RAG, полная end-to-end instrumentation, независимый CODEX/Astra review. Ссылки на R01–R25 обозначают исторические/плановые материалы, а не автоматически подтверждённую реализацию. Не менять исходные C#500/ENRU500, grades, их frozen tests/thresholds и checkpoint. Следующий шаг — только согласованные небольшие причинные пробы из раздела 08.

## Быстрые причинные опыты (добавление 16.10)

- [11 — матрица мини-экспериментов ко всем 44 гипотезам](11-FAST-EXPERIMENTS-BY-DIF.md): один фактор, frozen контроль, измерение реального ответа, условия остановки и owner.
- **[11A — приоритетный быстрый причинный эксперимент KNN: 16 TRAIN / 16 HELDOUT](11A-MICRO-KNN-ORDER-CAUSAL-PILOT.md)** и [полностью заданный JSON fixture](11A-MICRO-KNN-ORDER-FIXTURE.json). Исправлен экспериментальный confound: **один общий BPE320, один набор TRAIN и exact cosine KNN для обоих плеч**; A = текущие 64D bag-of-tokens + нули, B = те же 64D + 128D универсальных признаков позиции. Единственная меняемая информация — порядок токенов. Замеряются `correct/16`, зеркальные инверсии, false SAFE, контроль с перевёрнутыми train labels и реальный `KnnLogicalUnit` shadow отдельно. **SPEC+FIXTURE, RUNNER NOT_CREATED, NOT_RUN, независимый CODEX/Astra review NOT_PERFORMED.** Даже положительный локальный опыт не докажет ухудшение RP01 без ordinary-consumer trace.

## Дополнительные адресные исследования (в этом же каталоге)
- [09a — честность сравнения E34V2/E09, baseline на разных validation subsets и несовпадение коэффициентов hybrid](09-HISTORICAL-EXPERIMENT-VALIDITY-AND-FUSION-DRIFT.md): воспроизводимые расхождения code→historical claims; **не** доказанный runtime cause.
- [10 — EOS, stopping, prompt framing и разрыв между обучением Student и дискретными переводческими Unit](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md): точные методы декодера, словарных коррекций, E26/E27; гипотеза причинности требует однофакторного опыта.
- [10b — дополнительные прямые проверки Unit, metadata-only checkpoint, stale-cache и E09 локальных меток](10-TRANSLATION-UNIT-AND-RESTORE-EDGE-GAPS.md): 11 адресных ограничений, точные code anchors и критерии проверки.

Есть также [второй, более детализированный реестр source→specific code](01-EXPERIMENT-BASELINE-RECONCILIATION.md) с тематическими [DPO](02-DPO-DATA-OBJECTIVE-GRADIENT.md), [LoRA/BPE](03-LORA-TOKENIZATION-VECTOR-SPACE.md), [KNN](04-KNN-HYBRID-RETRIEVAL.md), [Unit/Flow/Graph](05-UNITS-FLOW-GRAPH-DETERMINISM.md), [checkpoint](06-CHECKPOINT-RESTORE-ADOPTION.md), [triage-планом](07-CAUSAL-TRIAGE-AND-REPAIR-PLAN.md) и [evidence ledger](08-EVIDENCE-CLAIM-REGISTER.md). Это перекрывающиеся аналитические проходы, **не независимые подтверждающие эксперименты**.

## Быстрый причинный эксперимент: готовый протокол

- [MINI-01 — проверка cold-start B0: **DPO-only vs SFT-only vs SFT→DPO**, 12 новых TRAIN-пар, 12 heldout задач, максимум 8 SFT + 2 DPO шагов на arm](12-RP01-MINI01-COLD-START-CAUSAL-EXPERIMENT.md). Имеются контроль B0, одинаковые settings, заранее определённая семантическая оценка, условия остановки, источник SFT/DPO API и минимальное поручение для реализации без длинного Windows CI. **План опубликован, запуск/новый Student gain NOT_RUN.** Владелец — существующие RP01.REC02/REC03, не новая независимая очередь. Не использовать несуществующий в текущем CLI `train 12`.

## Независимая сверка current code (добавление 16.10)

- [10c — независимая актуальная сверка **exact-v2 vs legacy** и матрица причинных проверок](10-INDEPENDENT-CODE-PATH-RECONCILIATION-2026-10-16.md): первичные строки DpoTrainer, DpoExactPairMaterial, RP01 simulation, LoRA, TeachingVector, BPE/EOS, kNN, Translation Units, checkpoint и Graph; отдельно исключены ложно приписанные современному RP01 старые Compact/full-sequence defects.

## Архивные результаты, матрица→KNN и протокол причинной сверки — 16.10.2026

Эти дополнения не являются дублями исходного R01–R25. Они восстанавливают **реальные DPO-run/checkpoint и обучаемые функциональные Unit** из соседних веток, выделяют неверно прочитанный исторический FAILED и задают проверяемый мост к RP01.

- [11a — прошлые **реальные DPO** прогоны V191/V194/V210, repeat10 и non-exact KNN transfer](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md): old optimizer, persisted checkpoint, разные tokenizer/target-mask и риск role-label shortcut; ни train margin, ни safe abstention не равны blind competence.
- [11b — архивные E51A/E51B/E52/E53/P3, утраченные точные пути в `language_translate` и cross-branch semantic parity](11-BRANCH-ARCHAEOLOGY-AND-LEARNING-PROOFS.md): настоящие сохранённые functional cycles с source limitations, не выдуманный neural training.
- [12a — multi-tier KNN и источники provenance старой матрицы](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md): positive/negative Tier E09/E14 и обоснование same-panel проверок, а не перенос статусной метки FAILED.
- [12b — **кодовый дефект E07**: неактивные измерения tier и BPE ordinal↔character sample alignment; E12 aggregate PASS с ухудшением двух подопераций](12-MATRIX-TO-KNN-EXPERIMENT-MISREADS.md): точные методы, сохранённые значения, отдельные невыполненные falsifiers.
- [13 — **протокол причинной сверки старого DPO, LoRA, KNN, обученных Unit/Flow/Graph и обычного consumer**](13-CAUSAL-LEARNING-PARITY-PROTOCOL.md): work orders, 4-arm ablation, CODEX/Astra 6 UltraCode as human selector + quality assessor, внешняя разрешённая китайская модель автор обеих DPO-сторон.

**Ограничение статуса:** файловый аудит и GitHub публикация НЕ означают выполнение fresh training, independent CODEX/Astra review, matrix→KNN commit-level provenance или доказанную причинность падения RP01. Не переписывать замороженные старые результаты или оценку под желаемое изменение.

## Дополнение 16.10.2026 — реальные соседние ветки и быстрый проверочный эксперимент

- [11 — исторические реальные DPO runs](11-CROSS-BRANCH-REAL-LEARNING-RESULTS.md): сохранённые V191/V194/V210, base-wiki150/200, repeat10→train accuracy 1.0, но без доказанного unseen semantic gain; новый seam старого hashed-token + side-marker вводится как отдельная гипотеза.
- [12 — matrix→KNN lineage и успешные/неуспешные Tier](12-MATRIX-KNN-TRANSFER-PROVENANCE-AND-EVALUATION.md): E09 Tier1 `FAILED` с малым положительным приростом и Tier2 `PASSED` с несопоставимой baseline; E14 Tier1 успешен, Tier2 хуже majority, E12 измерял скорость матриц, а не качество; связь конкретной matrix implementation с KNN пока не установлена.
- **[13 — MICRO-KNN-01: маленький причинный эксперимент](13-MICRO-KNN-CAUSAL-LEARNING-EXPERIMENT.md)**: одна train12/dev6/holdout12, один обученный KNN index, same ranked hits; A=current `hits[0]`, B=top3 + lawful candidate filtering; C=shuffled learned outputs, D=rules-only; paired correctness/abstain/law breaches, без 500 DPO и полной Windows CI. `NOT_EXECUTED` — это готовый проверочный runbook, **не результат**.

Главная новая граница: **старое реальное обучение действительно запускалось**, но числа old training accuracy нельзя напрямую сравнивать с RP01 free-generation 0/3 и 0/4. Нужны точные recipient/consumer и one-factor heldout. Документы 11–13 не заменяют более ранние DIF и не заявляют независимый code review или successful reproduction.

## Дополнительная проверка влияния обучаемого Unit и покрытие всех DIF

- **[PD-FAST-01 — controlled learnable Unit](11-QUICK-CAUSAL-PILOT-UNIT-VS-STUDENT.md)**: 3 состояния EN→RU переводческого Unit (с нуля, 8 правильных уроков, контроль с испорченными метками), отдельная проверка cold restore; 8 новых композиций и 4 контрольных случая. [Исходные JSON-входы](11-QUICK-CAUSAL-PILOT-FIXTURE.json). Это проверяет **эффект обучения памяти Unit на ответ пользователя**, но не делает ложный вывод, что isolated LoRA/DPO Student стал компетентен.
- **[DIF-01…DIF-44 — адресные короткие проверочные опыты](12-MINIPROBES-DIF01-DIF44.md)**: для каждого расхождения одна изменяемая переменная, проверяемый эффект, владелец, блокирующее условие. Это карта гипотез, **не 44 выполненных запуска**. Существующий [MICRO-KNN-01](13-MICRO-KNN-CAUSAL-LEARNING-EXPERIMENT.md) — альтернативный локальный опыт для retrieval; разные механизмы не смешивать в одном A/B.


## Быстрый проверочный эксперимент: измерить изменение качества, а не число PASS

- **[MX-01 — 6 новых EN→RU пар, одна настоящая мутация ExecutorUnit, 8 source-disjoint heldout ДО/ПОСЛЕ; MX-02 — B0 warmup + tiny exact-v2 DPO](12-QUICK-CAUSAL-MICROEXPERIMENTS-2026-10-16.md)**. Добавлен компактный readout реального semantic quality и таблица по другим DIF: что меняется, какой контроль, какую метрику измерять и какой результат опровергает гипотезу. Никакого нового 500-батча, повторения старых DPO или обязательного Windows CI; `SPEC_ONLY / NOT_RUN`. **MX-01 не дублирует** [MICRO-KNN-01](13-MICRO-KNN-CAUSAL-LEARNING-EXPERIMENT.md): там при фиксированном обученном KNN меняется использование соседей, здесь проверяется прирост от одного обучения translation Unit.

## Быстрые причинные мини-эксперименты — практическое дополнение

- [11 — первый конкретный A/B: random B0+DPO-only против short SFT→DPO](11-MICRO-EXPERIMENT-COLD-START-VS-DPO.md). Содержит гипотезу, одинаковые controls, реальные C# методы, scoped small-run admission, число шагов, независимые новые family-случаи, stop и форматы научного результата. **План, не выполненное обучение.**
- [12 — все 44 расхождения → по одному недорогому опровергающему опыту](12-DIF-01-44-CHEAP-CAUSAL-PROBES.md). У каждого DIF указаны A/B, измеримый эффект, предельный бюджет/владелец и условия, при которых нельзя объявлять выигрыш качества.

**Не делать 44 запуска:** сначала проверить градиенты/маски/восстановление без большого обучения, потом провести один разрешённый tiny A/B на новых примерах. Утверждение об улучшении должно опираться на `correct_valid` по новым законам, а не на `train_loss`, `DPO margin`, изменение hash или green CI. Текущий CLI **не поддерживает** произвольный `train 16`; диагностический вход из документа 11 ещё предстоит реализовать и допустить.


## E51A — проверка нового исходного кода после короткого обучения (16.10)

- **[E51A-SOURCE-DISJOINT-LEARNING-01: три режима A/B/C и заранее заданный результат](15-MICRO-E51A-SOURCE-DISJOINT-LEARNING-GAIN.md)** — воспроизводимый микроэксперимент для подтверждённого повторения `CodeContext` в старых T/V; [fixture: 8 TRAIN + 8 новых HELDOUT](15-E51A-SOURCE-DISJOINT-LEARNING-FIXTURE.json). A — старые 27 источников; B — обучение ещё на 8 корректных примерах; C — те же новые примеры с неверно переставленными метками. Оценивает прирост точности смысловых предикатов на новых C# фрагментах, использует отрицательный контроль и source provenance. **SPEC+FIXTURE / NOT_RUN / независимо не оценено**. Это **не** проверка обучения весов RP01 Student; для последней есть отдельные [cold-start SFT→DPO](11-MICRO-EXPERIMENT-COLD-START-VS-DPO.md) и [Unit-vs-Student](11-QUICK-CAUSAL-PILOT-UNIT-VS-STUDENT.md).

## Новый независимый аудит 16.10.2026 — поздняя KNN-коррекция не видна потребителю

- **[16 — NEW-16.01…08: фактическая инверсия приоритета KNN, shared memory разных направлений, no-op Unit receipts, сброс флага trained, неправильное направление evaluator, macro-vs-micro, first32 order, HNSW directed edge](16-INDEPENDENT-NEW-GAPS-KNN-CORRECTIONS-AND-UNIT-TRUTH-2026-10-16.md).** Дополняет прежние DIF-01…44 и DIF-14.*, не дублирует их: прямые pinned source citations, границы причинности и source→consumer пути. Центральное противоречие: `AddPair` append «поздние лучше» против `Take(k)` earliest-first; новое исправление может не попасть в `ExecutorUnit`.
- **[MICRO-16 — один очень короткий причинный опыт](16-MICRO-CAUSAL-EXPERIMENT-KNN-PRIORITY-AND-UNIT-STATE.md).** C# reproducer: 5 старых+1 свежая коррекция, same signature/source, `k=5`; A/B изменение только selection order; затем 12 frozen unseen translation cases и два no-op/false-regression контроля. **NOT_RUN:** создание документов не означает запуска эксперимента или независимого CODEX/Astra review.

**Точные роли:** CODEX с GPT-6 Astra 6 UltraCode выполняет выбор учебного случая (роль человека) и отдельно оценку качества конечной пары; разрешённая внешняя открытая китайская модель генерирует ОБЕ окончательные стороны DPO `chosen/rejected`; отдельный independent Student evaluator проверяет quality после использования trained state. Synthetic KNN fixtures сами по себе не являются DPO.

## Независимое дополнение — encoder/index, E13 split и доказательство вызова learned Unit (16.10.2026)

Это **другие механизмы**, не замена каталогам DIF-01…44, DIF-14.* и дополнению NEW-16.01…08. Все code-reference привязаны к `llm_mvp_core@83590a462b2f8aedc9d3ab4cb775193eb9c25ce5`; **новых замеров обучения в этой серии нет**.

- **[16 — live encoder vs frozen embedding index](16-LIVE-ENCODER-INDEX-VERSION-DRIFT.md):** если LoRA изменяется *после* индексации примеров и до запроса, query encoding может не соответствовать corpus vectors. Два отличающихся микро-контраста — W1/stale-W0-index vs W1/refreshed-W1-index (12 blind cases) и усечение до первых 64 токенов. Существование такого жизненного цикла в RP01 **не доказано**; не чинить без actual call trace.
- **[17 — E13 train-boundary mismatch](17-HISTORICAL-E13-TRAIN-VALIDATION-BOUNDARY.md):** архивный Tier2 содержит baseline 2/7 vs KNN 5/7, но baseline train по `trainPairs`, KNN по `allMethods.Take(80%)`. Новый one-factor probe и count фактических пересечений train/val отделяет полезный structural memory от нарушения split; точный overlap **пока неизвестен**.
- **[18 — ProductHost GenerateAsync не вызывает Unit.ExecuteAsync](18-PRODUCTHOST-GENERATION-RETRIEVAL-BYPASS.md):** конкретная цепочка `GetServingRuntime → TextGenerator.Generate` показывает, почему высокое качество обученной KNN-памяти не обязано повысить чистую free-generation оценку. Два маршрута могут быть намеренно раздельными — без подтверждения intended consumer это **contract gap**, не production bug.
- **[19 — MICRO-CONSUMER-01: минимальный готовый work order](19-MICRO-CONSUMER-01-QUICK-LEARNING-QUALITY-PILOT.md):** повторно использует **имеющуюся** 8 train / 8 source-disjoint heldout fixture, сравнивает learned labels с shuffled labels при одном и том же ranked hit pool, затем trace final consumer. `SPEC_ONLY; RUNNER_NOT_CREATED; NOT_RUN; INDEPENDENT_REVIEW_NOT_PERFORMED`.

**Приоритет:** сначала `19` (реально измерить, влияет ли learned Unit на heldout и final output), затем `17` (исторический KNN gain source split), `16` только при доказанном обновлении encoder после index-build. Не запускать 500 DPO, Windows GH workflow или 44 независимых исправления в рамках этого аудита.


## N16 — энергетический масштаб KNN и два новых seam проверки (16.10.2026)

- [16 — новый независимый code/evidence-аудит](16-NEW-CAUSAL-GAPS-KNN-TIER-DPO-ZERO-WEIGHT-KV-2026-10-16.md): **N16-01** общий L2 в KNN при длинном тексте подавляет правильно переданный predicate-tier; **N16-02** даже `Weight=0` в `DpoTrainer.StepVectorCore` получает нижний вес `0.01` (**не причина RP01: simulation задаёт Weight=1**); **N16-03** логитный parity full Forward и KV-cached generation ещё не проверен. Есть pinned методы, математические контрпримеры, границы причинности и адресные фальсификаторы.
- [N16-01 — короткий однофакторный native эксперимент](16-KNN-TIER-BALANCE-CAUSAL-MICROEXPERIMENT.md): same BPE320 + same train/predicates/labels + exact cosine; A=current global L2, B=separate tier L2, C=corrupt-label negative control, настоящий source-disjoint semantic heldout отдельно от числа test passes.
- [`16-KNN-TIER-BALANCE-PROBE.mjs` — воспроизводимая арифметика Node](16-KNN-TIER-BALANCE-PROBE.mjs): synthetic 8 TRAIN/16 numeric HELDOUT: **A 8/16 vs B 16/16; corrupted-label C 0/16** (сконструированная numeric-панель; **фактически запущен Node v22.16.0, без .NET/Student**). [Результат и SHA исполненного скрипта](16-KNN-TIER-BALANCE-PROXY-RESULT.json).

**Ответственность за вывод:** свойство метода ≠ ухудшение `RP01` ≠ полноценное восстановленное обучение. Исследовательская `RP01 CodeDocumentationSimulationDpoExperiment` учит отдельный Transformer/LoRA, не этот `KnnLogicalUnit`; положительный kNN-пилот локализует механизм RP02/ordinary consumer, но не объясняет сам по себе free-generation 0/3→0/3 и 0/4→0/4. Для **DPO** человеческую роль выбора случая выполняет **CODEX с Astra 6 UltraCode**, внешняя разрешённая открытая китайская модель генерирует **обе** стороны `chosen/rejected`, а качество exact пары независимо оценивает **CODEX с Astra 6 UltraCode**; оценка эффекта Student — отдельная. В этих новых материалах внешняя модель и CODEX/Astra **не запускались**; независимый review и native causal proof **NOT_PERFORMED**.

## Независимые дополнительные source seams: Translation Units, KNN и эксперимент MICRO-16 (16.10.2026)

- [16 — новые различия N16-01…08 в реальном Translation Unit](16-INDEPENDENT-TRANSLATION-UNIT-AND-KNN-LEARNING-GAPS-2026-10-16.md): потерянный TopicDomain, перезапись exact source, стирание операторов, отсутствие отрицания в ordinary KNN similarity, межъязыковые corrections, обход Verifier на learned phrase, поздняя duplicate проверка и fixed-confidence вместо blind quality. Это **source-verified отдельный Unit route**, не автоматическая причина neural RP01 0/3 и 0/4.
- [MICRO-16 — причинная проверка потери operator identity](16-MICRO-OPERATOR-IDENTITY-CAUSAL-EXPERIMENT-2026-10-16.md): одна переменная — адрес exact learned memory; 2 синтетических учебных правила, 4 matched probes, negative control, обратный порядок обучения, A/B на обычном consumer. Проверяет качество применения ранее изученного правила, **не** новый нейронный skill.
- [Fixture MICRO-16 JSON](16-MICRO-OPERATOR-IDENTITY-FIXTURE.json): входные строки, ожидаемые смыслы, контролируемые переменные и явный статус `NOT_RUN`. Это **не** новые DPO-пары и не китайский teacher output.

**Роли:** разрешённая внешняя открытая китайская модель создаёт *обе* стороны финального DPO (`chosen` / `rejected`); CODEX с GPT-6 Astra 6 UltraCode выполняет делегированный human selection и отдельную оценку exact DPO-pair. Ревью этого дополнения независимо моделью CODEX/Astra **ещё не выполнялось**; все новые A/B-результаты **NOT_RUN**. Frozen receipts не изменять.

## Сверка пересечений и выполненный shadow probe E16 — 16.10.2026

- **[16 — модельно-невидимый DPO-контраст при hash-коллизии и truncation](16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md)**: точные пары `accept/invalid`→289, `true/unauthorized`→421, собственный диагностический контроль. **Overlap** с [16 translation bridge token erasure](16-TRANSLATION-DPO-BRIDGE-TOKEN-ERASURE-2026-10-16.md); механизм считать однократно.
- **[17 — accounted-but-not-trained unknown Unit recipient](17-STRUCTURED-UNIT-TRAINING-SILENT-DISPATCH.md)**: unknown UnitId пропускается, RowsTrained учитывает запись; **overlap** с `NEW-16.03` в [16 independent new gaps](16-INDEPENDENT-NEW-GAPS-KNN-CORRECTIONS-AND-UNIT-TRUTH-2026-10-16.md). Не считать повторным новым дефектом.
- **[E16 — точный малый causal plan](18-E16-PAIR-CONTRAST-SURVIVAL-MICROEXPERIMENT.md)** и [4-case JSON fixture](18-E16-PAIR-CONTRAST-FIXTURE.json): native L0, затем только при наличии допущенных данных bounded L1, без Windows CI и без подгонки frozen RP01.
- **[E16 — фактически выполненный Python shadow source-reproducer](18-E16-SHADOW-PROBE-RESULT-2026-10-16.md)**: прогноз равенства model-visible ID подтверждён **4/4**, в том числе отрицательный контроль. **Это не native .NET, не запуск нейросетевого обучения и не доказательство улучшения RP01**.

Основной смысл дополнительной работы: не увеличивать число уникальных дефектов за счёт параллельных формулировок. По одному владельцу механизма, одному model-visible контрпримеру, одной blind-evaluation панели и одному фактическому consumer trace.

## Дополнение: исходный текст Translation DPO исчезает до модели — new handoff-specific gap (16.10.2026)

В дополнение к уже опубликованным parallel-code аудитам найдено конкретное место, где **исчезает не разница между chosen/rejected, а сам source input учебной задачи**, и создан отдельный контроль, который можно проверить без большого обучающего прогона:

- **[Новые найденные source seams: TranslationDpoTrainingHandoff → TextFeaturizer](16-TRANSLATION-DPO-BRIDGE-TOKEN-ERASURE-2026-10-16.md)** — `BuildTrainerPrompt` добавляет `Input: source` **в конец**; `BuildPromptPrefix` помещает длинные метаданные перед ним; при дефолтном `ContextLength=64` `prefixTokens.Take(30)` обрезает весь source текста типовой инструкции `Translate from en to ru.\nInput: bank`. Самая значимая **дополнительная новая причинная гипотеза**: модель не видит условие перевода. Здесь же конкретный no-op после `StripReservedDiagnosticsForModel` (отличие raw `rejected` стирается) и отдельные пары для hash/response-tail collision. **Source-confirmed + offline tokenization; native C# NOT_RUN.**
- **[Independent specific KNN/Unit findings + отрицательный контроль](16-KNN-CORRECTION-VISIBILITY-AND-TOPIC-LOSS-2026-10-16.md)** — `TranslationKnnStore` оставляет старые записи перед поздней правкой и `ExecutorUnit` теряет `TopicDomain`; **обнаруженные механизмы перекрываются** с [параллельным KNN аудитом](16-INDEPENDENT-NEW-GAPS-KNN-CORRECTIONS-AND-UNIT-TRUTH-2026-10-16.md) и [параллельным Unit/Domain аудитом](16-INDEPENDENT-TRANSLATION-UNIT-AND-KNN-LEARNING-GAPS-2026-10-16.md). Не считать совпадения несколькими независимыми *продуктовыми дефектами*. Отличие этого дополнения — source-pinned contract для 5 старых + 1 corrected и отдельный negative-newest пример, который запрещает бездумный LIFO.
- **[Малый experimental work order MICRO-16](16-MICRO-KNN-CORRECTION-AND-DPO-NOOP-PROTOCOL.md)** и **[готовый synthetic fixture с отрицательными контролями](16-MICRO-KNN-CORRECTION-DPO-FIXTURE.json)**. Три независимых коротких L0/L1 пилота: (1) correction visibility A=current FIFO/B=read-only recency/C=newest-wrong; (2) topic-context overwrite A/B reverse order; (3) **две разные реальные source инструкции через handoff→encoder → identical BOS..SEP**, и raw chosen/rejected после stripping. Любое изменение DPO objective — отдельная переменная, не совмещать с tokenizer patch.

**Дедупликация против других агентов:** [16 — DPO pair collapse](16-TRANSLATION-DPO-MODEL-VISIBLE-PAIR-COLLAPSE.md) уже покрывает *hash-collision, response-tail cut, legacy whole-sequence objective*. [16 — KNN correction](16-INDEPENDENT-NEW-GAPS-KNN-CORRECTIONS-AND-UNIT-TRUTH-2026-10-16.md) уже покрывает FIFO. [18 — E16 исходный DPO contrast pilot](18-E16-PAIR-CONTRAST-SURVIVAL-MICROEXPERIMENT.md) уже проверяет отдельную гипотезу исчезновения chosen/rejected. Эти находки **не переименовывать в новые**, сохранить как cross-review evidence. **Новый приоритетный seam** — exact producer `BuildTrainerPrompt` и полная потеря source: он отличается от урезания конца ответа и может сделать весь DPO conditioning бессмысленным.

**Граница причинного вывода:** `TranslationDpoTextFeaturizer` и структурная `TranslationKnnStore` являются другими caller/consumer, а direct RP01 использует `segmented-byte-BPE-v2` и отдельный LoRA Student. Пока нет traced invocation from RP01 ordinary consumer, source-seams `CAUSE_IN_RP01=NOT_PROVED`. Сначала один дешевый native contract check на **реально используемом** bridge/config, затем one-factor small quality A/B на общем blind holdout; не лечить RP01 500 строками новых DPO или многодневным Windows CI. **CODEX/Astra independent review: NOT_PERFORMED; обучающие прогоны: NOT_RUN.**


## N20 · Дополнительный независимый аудит гибридного KNN: размер памяти меняет ранжирование (16.10.2026)

- [20 — новые source-backed гипотезы N20.01/N20.02](20-NEW-FINDINGS-FUSION-MEMORY-GROWTH-2026-10-16.md): нормировка BM25 по текущему candidate pool может переворачивать выбранный ответ при добавлении нерелевантного урока; пересчёт BM25-statistics для каждого кандидата многократно токенизирует всю память (условная O(N²·(1+Q))). Прямо подтверждены вычисления и состав вызовов, **не доказан downstream/RP01 learning impact**. Та же формула есть в старой ветке DesignToCode — не считать её автоматически новым регрессионным изменением.
- [20 — короткий исполнимый микроэксперимент](20-MICRO-FUSION-POOL-INVARIANCE-EXPERIMENT.md): 12 train, 12 source-disjoint heldout, 4 новых законных distractors; одинаковые модель, tokenizer и критерии, парные B0/B1/N0 плюс shadow с фиксированными raw features; отдельно latency при N=8/16/32/64; 0 DPO steps и без Windows CI.
- [Node MJS: воспроизводимая арифметическая проба](20-FUSION-CANDIDATE-NORMALIZATION-PROXY.mjs) и [фактически посчитанный JSON](20-FUSION-CANDIDATE-NORMALIZATION-PROXY-RESULT.json). Показано B(correct)→A(wrong) при добавлении C с большим BM25; контроль low-BM25 и фиксированного lexical scale сохраняет B. **PROXY_EXECUTED / C# UNIT NOT_RUN / independent CODEX Astra verdict NOT_PERFORMED**. Не выдавать proxy за качество обученного Student.

В новых DPO CODEX (GPT-6 Astra 6 UltraCode) выполняет человеческий выбор урока и отдельно оценивает качество обеих сторон; разрешённая внешняя открытая китайская модель автор ОБЕИХ chosen/rejected. N20-проба DPO вообще не создаёт.


## Новые независимые N21: самообучение метками, недостижимые ключи и отсечённые важные уроки

**16.10.2026 — документарный audit, C#/Student/teacher native experiments не выполнялись.** После сверки уже опубликованных 44 DIF и N16–N20 добавлены исходниково подтверждённые дополнительные риски, не объявленные причиной RP01 neural quality без соответствующего вызова маршрута:

- [21 — новые source gaps](21-INDEPENDENT-STRUCTURED-LEARNING-GAPS-2026-10-16.md): `BuildGeneralBatch` выдаёт SemanticTokenizer собственный `sequence.Pattern` вместо независимо исправленной метки; `|PAIR:id`/rule-tag key из corpus недостижим через другой source; `QualityVerifierUnit` сохраняет пару `(src,ctx)`, но проверяет только `src`; substring-верификатор; collapsed rejected в некоторых типах ошибок; Jaccard 0.70 отсекает recipient-changing corrective lesson. Рассматриваемый `Batch029Runner` использует ExactPair только для corpus, Structural для нового/supplemental — область действия доказательства уточнена.
- [MICRO-N21 — быстрый причинный пилот](21-MICRO-CAUSAL-LEARNING-QUALITY-PILOT.md): L0 native фильтр, затем A=current-filter / B=independently-approved lawful correction admitted / C=same new source with wrong label / D=frozen base, 8 source-disjoint heldout through ordinary learned Unit consumer. Primary result — lawful correct final answers, не RowsTrained и не DPO margin; `NOT_RUN`.
- [Fixture MICRO-N21](21-MICRO-CAUSAL-LAW-DELTA-FIXTURE.json): synthetic DEV_ONLY input, три algebraic collision cases и восемь candidate heldouts; без самостоятельно объявленного final external Chinese model авторства/независимого gold.

**Proxy ONLY:** `пожалуйста отправьте отчёт Марии сегодня утром` → `... Ольге ...` даёт token Jaccard 5/7=0.7143, поэтому при пороге 0.70 новый пример прогнозируемо отфильтровывается. Небольшая JavaScript-проверка этой арифметики **не** равна .NET model/Unit execution или measured learning gain.

**Дедупликация:** N21 не повторяет FIFO correction/uncalled-KNN/hashed-token collisions в N16–N20; структурная ошибка `chosen/rejected` возникает *до* vectorization; вывод о neural RP01 StepVector отсутствует до causal cross-path proof. Основная научная граница прежняя: исторические положительные learned-unit результаты нельзя переименовать в подтверждение LoRA-качества или наоборот.

- [N21: точный one-run prompt для Kiro/CODEX](21-MICRO-N21-EXECUTOR-PROMPT.md): L0 native Jaccard; L1 A/B/C/D на actual Unit+Flow; 8 per-case receipts; отдельный reviewed SHA; ни одного придуманного quality PASS.


## N21 · Дополнительный независимый failure mode: учебные противоположности исключаются ещё до обучения (16.10.2026)

- [N21 — source audit фильтра учебных примеров и маршрута frequency quote → Unit](21-NEW-GAPS-SYMBOL-BLIND-LESSON-ADMISSION-AND-DIRECTION-MISROUTING-2026-10-16.md): настоящий Batch029 runner применяет novelty Jaccard по символо-слепым fingerprints; противоположные условия C#/SQL, отличающиеся оператором ==/!=, >=/<=, !, считаются дублями (1.00 similarity), если в корпусе уже есть первая сторона. Тот же loss-of-operator есть в контроле train/heldout. Дополнительно FrequencyQuoteMapBatchBuilder всегда ставит executor-ru_en, даже при SourceLang="en"; actual en-caller пока не подтверждён.
- [MICRO-21 — минимальный native C# эксперимент и работа CODEX](21-MICRO-OPERATOR-AWARE-ADMISSION-AND-UNIT-DIRECTION-EXPERIMENT.md): вызвать исходный public C# FilterByTrainingHistoryUniqueness, затем one-factor operator-aware admission, 8 локальных Unit training lessons и 12 sealed heldout **только после native подтверждения**. [JS shadow runner](21-MICRO-LAW-ANTIPAIRS-NOVELTY-PROXY.mjs), [raw JSON](21-MICRO-LAW-ANTIPAIRS-NOVELTY-PROXY-RESULT.json): 4/4 opposite-law отброшены текущим фильтром, 4/4 приняты operator-aware; дубликат и unrelated verb controls корректны. **JS EXECUTED, NATIVE NOT_RUN, REAL LEARNING GAIN NOT_MEASURED, INDEPENDENT REVIEW NOT_PERFORMED**.

Это не повторение [N16 loss of operator exact-memory key](16-MICRO-OPERATOR-IDENTITY-CAUSAL-EXPERIMENT-2026-10-16.md): там теряются различия после обучения при записи/поиске, здесь **урок не проходит до Unit**. Не выводить автоматический root cause RP01 DPO Student, который использует отдельный exact-v2 путь. DPO роли сохраняются: human selector CODEX/Astra 6 UltraCode, обе chosen/rejected генерирует разрешённая внешняя китайская модель, pair-quality judge — отдельно CODEX/Astra 6 UltraCode.


## N23 / MICRO-N23 — неверное определение качества обученного перевода (16.10.2026)

Независимый [N23 source audit](23-INDEPENDENT-TATOEBA-QUALITY-SCORER-LEXICON-CACHE.md) добавляет ранее не зарегистрированные **source-specific** seam: `TatoebaPilotLoopRunner.ProductMatch` с one-sided content-recall принимает противоположное отрицание, замену получателя, числа и действия; `TargetScriptOk` отвергает правильный Hebrew-only ответ; `InduceLexicon/InducePhraseMappings` извлекают потенциально ложные суб-Unit mappings из co-occurrence, без word alignment; cache identity не проверяется SHA (последнее только P2 conditional, обычная запись инвалидирует cache). **Это относится к Tatoeba/TranslationMachine, НЕ доказывает причину RP01 Student C#0/3 ENRU0/4.**

[Малый план](24-MICRO-TATOEBA-QUALITY-GATE-EXPERIMENT.md) требует сначала 12 native C# cases на настоящем `BuildRowResult`, затем только если подтвердится — frozen двухцикловый scorer→promotion→trained state→ordinary consumer A/B с source-family-disjoint heldout. [Исполняемый JS shadow](23-MICRO-SCORER-LAW-PROXY.mjs) / [raw JSON](23-MICRO-SCORER-LAW-PROXY-RESULT.json): 12 *намеренно adversarial* synthetic cases, 8 false accepts / 1 false reject по авторскому условному oracle. **JS proxy executed, native not run, real quality gain not measured, separate CODEX/Astra review not run**. Отрицательные/положительные human-role DPO полномочия не менялись: CODEX/Astra 6 UltraCode выбирает задачу и оценивает пары; разрешённая внешняя открытая китайская модель автор обеих `chosen` и `rejected`.


## UCA-01 / UCA-02 — отдельное N23-UNIT исследование: case-sensitive memory и отказавший Unit batch (16.10.2026)

**Не путать с независимым N23-Tatoeba** (scoring/lexicon) в соседних документах. Здесь два других механизма: [N23-UNIT: source-level case-key collision + partial mutation](23-NEW-UNIT-CASE-IDENTITY-AND-NONATOMIC-TRAINING-2026-10-16.md) и [MICRO-UCA: native C# falsifiers + blind paired quality protocol](23-MICRO-UNIT-IDENTITY-AND-ATOMICITY-2026-10-16.md). UCA-01: `SemanticTokenizerUnit._signatureCorrections` имеет `OrdinalIgnoreCase`, поэтому два различимых по регистру входа могут перезаписать learned correction друг друга даже **при одном SourceLang** (не дублирует N16 language-scope). UCA-02: `TranslationMachine.TrainStructuredBatchAsync` мутирует Unit построчно и при позднем `JsonException` может оставить изменённую correction, не опубликовав успешную запись batch (не дублирует RP01 transactional DPO optimizer). Core mirror: [12-unit-state](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/12-translation-unit-case-identity-and-batch-atomicity.md). **Native test НЕ запускался, реальное ухудшение/улучшение качества не измерено, RP01 Student route не связан причинно.** Для DPO: внешняя открытая китайская модель — автор обоих chosen/rejected; CODEX Astra 6 UltraCode — человеческий selector и независимый quality evaluator.


## NEW-26 — E30: условный признак предиката потерян до KNN (16.10.2026)

В дополнение к ранним 44 DIF и новым N14–N25 прочитан старый эксперимент **E30 PredicateCache** вместе с ранним/поздним вариантами и сохранёнными run receipts; проверены содержательные тела трёх файлов в ветках language_translate, DesignToCode, external_domains, orchestrator-extensions, main и codex/dpo-implementation. **NEW26.01**: все (Current,Next) переходы одного C# файла используют один full-file 22D vector, а cache.Add/Lookup игнорируют Current; **NEW26.02**: равный cosine сохраняет первого кандидата; **NEW26.03**: E30V2 изменяет target с next-predicate на восьмиклассовый NL task, поэтому не является доказанным ремонтом исходного E30. Исторический E30 имел cacheAcc **2,3%** на 128 попаданиях и overall **16,67%** против bigram **32,25%**. Всё это **source-confirmed seam**, а не доказанная причина RP01 Student 0/3 или 0/4. Не объявлять E30 пользовательским доказанным «матрица→успешная функция KNN» без lineage.

- [NEW-26 — первичный код, SHA, архивные результаты, проверка веток, четыре фальсифицируемых объяснения](26-NEW-E30-PREDICATE-CACHE-STATE-ALIAS-AND-MISREAD-2026-10-16.md)
- [MICRO-26 — быстрый native .NET A/B эксперимент на новых C# семействах; одно изменение Current в адресе KNN](26-MICRO-E30-CONDITIONED-KNN-CAUSAL-EXPERIMENT-2026-10-16.md)
- [Исполняемый Python-контрпример](26-MICRO-E30-CONTEXT-IDENTITY-PROXY.py) и [сохранённый результат](26-MICRO-E30-CONTEXT-IDENTITY-PROXY-RESULT.json): synthetic 6/12→12/12, corrupted-label 6/12, **не .NET и не продуктовая accuracy**
- [Consolidated causal gap map — связь REC01–REC05, всех основных mechanism families и приоритетных независимых мини-экспериментов](26-RP01-CONSOLIDATED-CAUSAL-GAPS-AND-NEXT-EXPERIMENTS-2026-10-16.md)

Человеческий выбор обучающей задачи и независимый judge выполняет **CODEX / GPT-6 Astra 6 UltraCode**; **внешняя открытая китайская модель** генерирует **chosen и rejected DPO**; CODEX/Astra 6 UltraCode также отдельно оценивает качество обученного Student/Unit на blind queries. В этом NEW-26 DPO/нативного обучения/judging не исполнялось. Каталог core/docs/plans/replanning-possible-diffs не редактировался.


## NEW-26B — commit-level E09 transfer и drift train targets / tie policy

[NEW-26B E09 history + two bounded falsifiers](26-NEW-E09-GIT-PRODUCTION-TARGET-AND-TIE-DRIFT-2026-10-16.md): доказан конкретный перенос экспериментального E09 AST/KNN в 12.SemanticCompiler в commit [27ae03266](https://github.com/lubochka/llm_mvp_core/commit/27ae03266ae2f7300d63e915795c0ff62369fa67); обнаружены два изменения механизма — **per-file ordinal NextToken** исторического E09 против **SyntaxKind modulo 200** на production train path и неявный tie MaxBy(count) против явного smallest-ID tie. Эти изменения потенциально меняют labels/решения без обновления learnable memory. Корректность native old/new, частота ties, фактический product caller и причинная деградация **не измерены**. Это отдельный доказанный **E09→production transfer**, не доказанное владельцем точное **FAILED matrix→successful KNN** происхождение. Короткие раздельные read-only/native фальсификаторы указаны в документе; не смешивать с [MICRO-26 E30](26-MICRO-E30-CONDITIONED-KNN-CAUSAL-EXPERIMENT-2026-10-16.md).


## N27 — Отдельный новый разрыв: роль слова → обучаемая память → ложная морфология (16.10.2026)

**Новые source-backed механизмы, не дубликат предыдущих N16/N21:** GrammaticalRole присутствует в FrequencyQuoteEntry, но не попадает в Unit training material; DeriveGenderAlternateForm по одному русскому окончанию «-л/-ла» может порождать неверные WordMappings даже для существительных. Правильный noun-урок «пила→saw» способен перезаписать старую learned verb-map «пил→drank» значением «saw». Дополнительно TargetPrefix.last-word без подтверждённого word alignment может обучить ложное соответствие, а BuildRejectedCandidate — создать textually identical chosen/rejected.

- [Исходники, file→line доказательства, контрфактический пример, историческая ветка и scientific verdict](27-NEW-ROLE-BLIND-MORPHOLOGY-AND-QUOTE-NEGATIVE-COLLAPSE-2026-10-16.md).
- [Малый native .NET causal эксперимент: same train, POS gate — единственный фактор, 12 blind heldout, ordinary Flow и cold restore](27-MICRO-ROLE-GENDER-RETENTION-CAUSAL-EXPERIMENT.md).
- [Исполняемый Node.js source-equivalent proxy](27-ROLE-GENDER-QUOTE-MICRO-PROXY.mjs) и [сырой фактический результат](27-ROLE-GENDER-QUOTE-MICRO-PROXY-RESULT.json): в формульном контрпримере «пил» меняется drank→saw, POS gate сохраняет drank; marker «a» неверно выбран для noun; возможен chosen/rejected text-tie. **Только JS FORMULA PROXY, native .NET NOT_RUN, реальное повышение качества НЕ измерено.**
- [Архитектурный crosswalk со старыми успешными KNN/матрица экспериментами и точный Kiro/CODEX work order с skills](27-DELTA-CROSSWALK-SKILL-ROUTES-AND-NATIVE-WORK-ORDER-2026-10-16.md).

**Жёсткое ограничение:** не объявлять эту структурную ошибку причиной neural RP01 C#0/3, EN→RU0/4 без доказанного вызова FrequencyQuoteMapBuilder → actual Unit → ordinary consumer. Положительные эксперименты E09/E14/E26/E43 — отдельные узкие механизмы, не DPO Student gain. Соседний core/docs/plans/replanning-possible-diffs **не изменялся**.

---

## 29 — дополнительная независимая сверка: teacher annotation vs kNN key и positional word alignment (16.10.2026)

**Новые документы N29 (только в planning; core research read-only):**

- [29-CONSOLIDATED-EVIDENCE-ROUTING-AND-NOVELTY-REGISTER-2026-10-16.md](29-CONSOLIDATED-EVIDENCE-ROUTING-AND-NOVELTY-REGISTER-2026-10-16.md) — карта 15 классов возможных причин, границы между DPO/LoRA Student, историческим learned KNN/Units, ложными quality gates, позитивными/отрицательными экспериментами и доменами; прочитанные RAG-источники и приоритеты.
- [29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md](29-NEW-REJECTED-ANNOTATION-IN-TRAIN-ONLY-KNN-KEY-2026-10-16.md) — **узкий новый code seam**: `[RULE:]` из отклонённого teacher-ответа попадает в позитивный semantic/KNN key, который для unseen source не обязательно воспроизводим. Source-confirmed, end-to-end quality not run.
- [29-NEW-FIRST-TOKEN-PSEUDOALIGNMENT-ORDER-CONTAMINATION-2026-10-16.md](29-NEW-FIRST-TOKEN-PSEUDOALIGNMENT-ORDER-CONTAMINATION-2026-10-16.md) — **новая причинная проверка известной эвристики**: source-first token при переставленном word order записывается как target-first word, возможна перезапись learned dictionary. Узкая новизна не выдаётся за обнаружение самой строки, уже упомянутой core исследованием.
- [29-MICRO-REJECTED-KEY-AND-WORD-ORDER-CAUSAL-EXPERIMENT-2026-10-16.md](29-MICRO-REJECTED-KEY-AND-WORD-ORDER-CAUSAL-EXPERIMENT-2026-10-16.md) — два **независимых** быстро реализуемых one-factor native C# A/B: state/key trace → fresh cold checkpoint → ordinary Flow ответы → независимый semantic gold 8–12 unseen случаев. Без Windows Actions и без повторного применения прежних DPO пар; **NATIVE/CODEX REAL RUN NOT_PERFORMED**.

**Ограничение причинности:** эти N29 findings относятся к structural Translation Units и не объявляются причиной прямого neural RP01 0/3/0/4 без caller trace. Исторические ветки и RAG перечислены с их реальными границами; чужие положительные и отрицательные измерения не переписывались.


## 16.10.2026 — Independent N28 role-parser / ProjectWiki learned-content delta

**Отдельный новый source→learner→consumer аудит** (quality native `NOT_RUN`, `.NET` не запускался; не заменяет исторические позитивные эксперименты):

- [N28-A: enum-order issue parser переставляет `src`/`ctx` и может загрязнить trained Verifier](28-NEW-VERIFIER-ISSUE-ROLE-PARSER-AND-CONTEXT-LEAK-2026-10-16.md); [5-case JS proxy](28-ROLE-PARSER-MECHANISM-PROXY.mjs) и [машинный результат](28-ROLE-PARSER-MECHANISM-PROXY-RESULT.json), **не native ML**. Исправление только parser не гарантирует качество: необходимо отдельное context-scope сравнение.
- [N28-B/C: ProjectWiki Mode2/Mode3 реально не читают `Chosen.PlanText`; same-PairId исправления пропускаются](28-NEW-PROJECTWIKI-MEMORY-CONSUMPTION-AND-CORRECTION-ID-2026-10-16.md); это исторический механизм `DesignToCode` и current, а не доказанная новая ошибка RP01.
- [Исполнимые V28/O28/C28 мини-опыты с falsifier и source-disjoint heldout](28-FAST-CAUSAL-PILOTS-VERIFIER-AND-ORCHESTRATION-2026-10-16.md): 1 изменяемый фактор, new-instance per arm, cold restore, ordinary consumer, семантическая correctness, без GitHub Windows runs.
- [Общий подробный N28 master crosswalk](28-DELTA-MASTER-AUDIT-ALL-AXES-HISTORY-AND-NOVEL-CAUSAL-GATES-2026-10-16.md) остаётся владельцем широкой 25-осевой матрицы, старых E09/E14/E26/E43 successes и нескольких иных N28 hypotheses. Этот независимый delta не подменяет его и не заявляет полный обход всех RAG.

**Отдельная поправка к атрибуции:** известный E38V2 `expectedCategory` oracle и нестрогий TopK уже описаны в [core historical function transfer, section Ф-02](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/10-historical-function-transfer-and-learning.md). Наше чтение E38 не является новой находкой.


## 16.10.2026 — N38: learning-first причинные контрасты и новые узкие расхождения

**Начинать здесь:** [N38 MASTER: приоритеты, положительная история, документы и domain/recipient matrix](38-MASTER-PRIORITIES-POSITIVE-HISTORY-NEW-HYPOTHESES-AND-MICRO-DECISIONS-2026-10-16.md). Собраны прямые neural RP01, исторические E09/E14/E26/E51–E53/P3, сохранённый TranslationMemory continuation и риски Unit/KNN/LoRA/DPO/decoder/restore; специально указано, какие результаты **не сопоставимы**.

- [N38-A: новое проверяемое объяснение 0/3 C# — preference-prior без чувствительности к самому входному закону](38-NEW-RP01-INPUT-INSENSITIVITY-COUNTERFACTUAL-DPO-2026-10-16.md). Отличается от ранее исследованного общего начала response: сравниваются **противоположные источники при одних и тех же кандидатах**. SOURCE EVIDENCE / QUALITY NOT_RUN.
- [MICRO-38-A: короткий read-only native A/B на B0/C50](38-MICRO-COUNTERFACTUAL-CONDITIONING-EXPERIMENT-2026-10-16.md) с [6 frozen OPEN контрфактическими C# парами JSON](38-MICRO-COUNTERFACTUAL-CODE-PAIRS-OPEN-v1.json): canonical response-masked scores, 2×2 input-response preference, ordinary free answer, cold restore. Ноль новых DPO updates и Windows CI; **RUN NOT_PERFORMED**, ответы требуют independent C# stdout oracle.
- [Готовый Kiro/Codex prompt на единичный native read-only прогон и независимый review](38-KIRO-CODEX-FAST-CAUSAL-REPLICATION-PROMPT-2026-10-16.md): skills архитектора/планирования/code audit/эксперимента/проверки утверждений и человеческая коммуникация; строгие stop-lines, никаких Windows CI; **execution NOT_RUN**.
- [N38-B: условная file leakage в историческом E09](38-NEW-E09-WITHIN-FILE-HOLDOUT-LEAKAGE-2026-10-16.md). При разрезании конкатенированных AST узлов на 80%/20% один файл **может** попасть в оба набора; отдельно от N32 cross-file false transitions, N26 label/tie change и known val-denominator mismatch. File split/quality ещё НЕ реконструированы.

**Не подменять историю:** E09 Tier2 .2413→.3507 был сохранён как PASSED, но сопоставимость знаменателей не подтверждена; E14 имел малый позитивный и большой негативный run; E26 .4→1.0 на фиксированных контекстных примерах не является нейронным обучением; P3/E51 были функциональными успешными циклами; restored TranslationMemory проверил KNN non-exact hit старого урока, но не gain после новой пары. Источники и границы все приведены в N38 MASTER. Папка core/docs/plans/replanning-possible-diffs не изменена.

---

## N38 — replay fusion теряет самостоятельный semantic/vector и pattern сигнал (16.10.2026)

**Новая source-дельта, не повтор N09/N20/N27:** [N38-A/B/C: ReplayFusionRanker на деле считает lexical Jaccard под именем vector, дублирует его в pattern и не ранжирует ≤3 кандидатов](38-NEW-REPLAY-FUSION-LEXICAL-SHADOW-PATTERN-ALIAS-AND-SHORTLIST-GATE-2026-10-16.md). В старых DesignToCode/language_translate этот код уже был; регрессия именно RP01 не доказана. Отдельный embedding-HNSW Unit существует и не должен ошибочно называться отсутствующим.

- [Доменный micro A/B: C#, SQL, EN↔RU; actual caller/replay DPO/KNN → ordinary Flow, cold restore, falsifiers](38-MICRO-REPLAY-FUSION-DOMAIN-CAUSAL-EXPERIMENT-2026-10-16.md).
- [Синтезатор/карта RP01 vs E09/E14/E26/E43 positive + E14 Tier3/E22 negative, 16 причин, домены/приоритеты](38-MASTER-RP01-LEARNING-QUALITY-EVIDENCE-ROUTES-HISTORICAL-POSITIVES-AND-PRIORITY-2026-10-16.md).
- [CODEX Astra 6 UltraCode — выбор и независимая оценка; внешняя открытая китайская модель — действительный автор обеих DPO-сторон; независимый review и критерии](38-CODEX-ASTRA6-ULTRACODE-EXPERIMENT-AND-INDEPENDENT-REVIEW-WORK-ORDER-2026-10-16.md).
- [Source-logic proxy MJS](38-MICRO-REPLAY-LEXICAL-SHADOW-SOURCE-PROXY.mjs) и [исполненный числовой receipt](38-MICRO-REPLAY-LEXICAL-SHADOW-SOURCE-PROXY-RESULT.json): подтверждение достижимости counterexample **только в математическом shadow**, `native .NET / semantic quality / real DPO NOT_RUN`.

**Защищённый core/docs/plans/replanning-possible-diffs не редактировался этим аудитом.** Новая потеря качества в самом RP01 от N38 не доказана: его direct neural flow не вызывает рассматриваемый replay selector без отдельной трассы.

---

## N39 — exact DPO pair tokenizer SHA: валидность пары ещё не связывает её с моделью (16.10.2026)

[Новая source-confirmed условная брешь в контракте](39-NEW-DPO-EXACT-MATERIAL-SELF-HASH-VS-MODEL-TOKENIZER-IDENTITY-GAP-2026-10-16.md): `DpoExactPairMaterial` хранит SHA своего BPE, но `DpoTrainer.CaptureBatch` не передаёт `expectedTokenizerSha256`; `DpoExecutionProfile` также не содержит tokenizer identity. Два словаря одинакового размера с разными merges могут давать разные ID одной строки. Это **не доказательство несовпадения в живом RP01** — его producer может проверять identity извне.

[Минимальный нативный тест из двух BPE одного размера, без обучения](39-MICRO-TOKENIZER-IDENTITY-DPO-PAIR-TO-STUDENT-EXPERIMENT-2026-10-16.md) сравнивает приём exact pair с ожидаемым SHA и без него, затем требует actual caller trace прежде чем делать semantic causal claim. Интегрировано в [мастер-карту причин](38-MASTER-RP01-LEARNING-QUALITY-EVIDENCE-ROUTES-HISTORICAL-POSITIVES-AND-PRIORITY-2026-10-16.md) и [CODEX work order](38-CODEX-ASTRA6-ULTRACODE-EXPERIMENT-AND-INDEPENDENT-REVIEW-WORK-ORDER-2026-10-16.md). Состояние: **SOURCE CONFIRMED / .NET NOT_RUN / QUALITY NOT_RUN / CODEX Astra and external Chinese teacher NOT_CALLED**. Protected core research **не редактировался**.


<!-- N59-RP01-LEARNING-ONLY-AUDIT-2026-10-19 -->
## N59 · Source audit RP01, protected-experiment validity and learning-only mini-protocols (19.10.2026)

- [Причины RP01 и два новых узких source-candidates](59-INDEPENDENT-SOURCE-READBACK-AND-RP01-CAUSE-BOUNDARY-2026-10-19.md) — историческое learned-memory 0/24→19/24 отдельно от neural 0/3 и 0/4; DPO/LoRA, Unit, kNN, tokenizer, restore, nested RAG prior art и подтверждённые границы; H59-01 inconsistent task-type ANY/ALL, H59-02 optional supplied learned_pattern. **Нативная причинность не доказана.**
- [READ-ONLY аудит protected cards и новые риски evaluator](59-PROTECTED-EXPERIMENT-VALIDITY-AND-NONLEARNING-SCORER-GATES-2026-10-19.md) — проверенные примеры P10/P51/P52/P56/P81/P92/P107/P125; H59-M1 panel-wide Any(structureRefs) и H59-M2 union-of-all-guides вместо выбранного plan. **Оба measurement-only, не засчитывать как эксперименты обучения.**
- [Три паспорта / исполнительский prompt / BLOCKED-гейты](59-CAUSAL-LEARNING-PASSPORTS-AND-EXECUTOR-PROMPTS-2026-10-19.md) — реальные unit-learning E59-L1/L2 (P52.2/N57 с credit prior authors), новые conditional E59-O1/O2 запрещены к обучающему запуску до доказанного TRAIN recipient. **A0/A1/B0/B1, own outputs, одна переменная, полезный рост любой метрики — успех этой оси даже при flat accuracy; все ухудшения рядом.** Никаких новых runs.

**Immutable constraint:** llm_mvp_core/docs/plans/replanning-possible-diffs/** не менялось. Global semantic certification P01–P125 / 470 subcases НЕ проводилась этим проходом.
