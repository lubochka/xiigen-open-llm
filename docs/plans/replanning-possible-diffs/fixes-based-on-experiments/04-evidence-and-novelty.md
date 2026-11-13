# Основания, границы доказательств и недублирование

<!-- XIIGEN-AUDIT-FOLLOWUP-20261111-04:START -->
## Уточнение 11 ноября: что подтверждено источником и что остаётся открытым

**Статус: исправление границ доказательства и требований к будущим работам.** [Evidence review](audits/2026-11-11/followup/evidence-review.md) и [адресное продолжение плана](audits/2026-11-11/followup/02-audit-and-repair-addendum.md) закрепляют source pin и конкретных владельцев. Исторические raw сохраняются.

Различать три независимых факта: каталог опубликован, каталог занят, текущий процесс владеет попыткой. Git tracked flag отвечает только на первый. Новый owner/root выделяется до подготовки; untracked чужие и незавершённые записи не становятся свободными. Резервирование, дальнейшие owner-only записи и внешний finalizer описаны в [общем договоре 03](03-test-and-execution-contract.md).

У каждого вывода явно указать уровень: source inspection; независимый разбор raw; пересчёт производного receipt; новое native execution. Подтверждённые здесь **501 failed / 13 751 passed / 94 skipped / 14 346 recorded results** относятся к aborted историческому full-suite запуску. Основной 22-МБ TRX в этом продолжении не получен; подтверждение опирается на console и производный JSON. Три отдельных guard TRX разобраны непосредственно: одинаковые 60 имён, 21 Failed→Passed, 15 Failed→Failed, 0 Passed→Failed. Нельзя объявлять `501−21` текущим full-suite baseline.

ROW124: по опубликованным текстовым таблицам независимо согласованы **6 201 уникальный путь, 2 027 пар объектов, 622 840 снятых байтов**, 16 партий и 255 групп по префиксу хеша. Это повтор счёта receipts, без повторного binary transform, LFS download или восстановления. 39 добавленных путей вне данного доказательства. В 31-файловом proof нет исходного executable transform script, а per-path detail не содержит фактически рассчитанный output SHA256; `PROVEN` — сохранённый verdict автора. Эта граница не опровергает прежнее доказательство класса.

Для отдельной будущей операции над двоичными доказательствами сохранить method source/hash и per-pair pre oid/expected oid/computed oid/sizes/removed bytes. Восстанавливать из подтверждённого исходного объекта, не вставкой CR перед каждым LF: lone CR и исходные LF требуют сохранности. Новая операция получает собственные identity, immutable output и final-state проверку; текущая сохранность/восстановление этих объектов настоящим документом не удостоверяется.

При исправлении метрики хранить датированную correction с исходным path+hash+JSON pointer и независимым определением величины. Согласованность двух одинаково ошибочных носителей не oracle. P52 сохранённое поле attempt2 ceiling=0 и рассчитанный ceiling 2/4 называются раздельно; исходный JSON не переписывается. Закрытие документа не приравнивается к выполнению назначенного продуктового ремонта.

<!-- XIIGEN-AUDIT-FOLLOWUP-20261111-04:END -->

<!-- POSTQ003-2026-11-03:START -->
## Уточнение после Q003 — EXTERNAL-AUTHORITY-AND-METRIC-PHASE

Historical output проверяется по внешнему pinned manifest, не по hash из supplied copy. P127 authority15 files на7ce; root/output разделены. При подмене route+steps+summary+local hashes historical acceptance отказывает. Подлинный ошибочный ответ остаётся научным наблюдением. Для каждой метрики attempt/arm/phase/state/denominator: P03 T1NLL gain и panel regression сообщаются вместе; hash/count-only finalstate не считается восстановленным.

Основание: [154](codex-154-postq003-artifact-and-transfer-audit-2026-11-03.md), действие существующего владельца: [155](codex-155-postq003-addressed-repair-plan-2026-11-03.md), обязательный [договор50](../experimets/codex-50-postq003-binary-and-historical-evidence-contract-2026-11-03.md). В передаваемое поручение включить этот адресный случай, positive/negative и критерий отказа. Сохранить фактические source/bytes/commands/exit, отдельные статусы artifact/native/science/product; не повторять PRIMARY ради файла или отчёта.
<!-- POSTQ003-2026-11-03:END -->


**Уточнение POST131: контроль проверяющего не подменяет опыт.** Полный синтетический пакет с собственной identity может доказать, что validator принимает внутренне согласованный неверный ответ; он не является ответом обученной модели. Изменение historical actual под старым proof — порча свидетельства, даже если исправлены все summaries. Для P127 отдельно проверять 68 ключей чтения и 12 графов на попытку; отдельные native payload не сохранены, поэтому их нельзя восстановить из тех же summaries и назвать независимым источником. Конкретные positive/negative и предел доказательства — [49 §6](../experimets/codex-49-post131-output-binding-and-test-coverage-2026-11-03.md#6-p127-согласованность-ответа-и-точность-изменённого-задания--разные-проверки).

<!-- CODEX-POST131-AUDIT:START -->
## Происхождение actual после P131

[Аудит149](codex-149-post131-transfer-and-experiment-audit-2026-11-03.md) и [договор49](../experimets/codex-49-post131-output-binding-and-test-coverage-2026-11-03.md) добавляют конкретную границу: **исходный native payload → производный raw → отчёт**. В опубликованном P131128/128 связей совпали, однако старый verifier эту связь не проверял. Согласованная порча actual+length у16 writer/reader строк может менять научный score в проекции предикатов; это дефект проверки, не найденная подделка исходного опыта и не выполненный native C# control.

**`POST131-EVIDENCE-ORIGIN`. WRONG:** правильные counts, hash текущего каталога и одинаковые writer/reader summaries удостоверяют actual. **CORRECT:** закрепить run/source и входной inventory, проверить точное множество `(arm,id)`, сверить actual/verdict/flow/полные metrics/warnings с уже сохранённым native результатом и пересчитать score по назначенному gold. Для order/graph связать route с ordered payload и реально существующим trace. Producer consistency не является криптографической защитой от подмены всего пакета; происхождение и независимый gold остаются обязательными.

Отдельно записывать integrity, научную метрику и продуктовую приёмку. Согласованный ошибочный ответ при confidence0,99 остаётся наблюдением ошибки; его нельзя скрыть или считать доказательством качества. Exact C-target follow — научная метрика, не обязательная победа learner для корректной процедуры.

P126 teacher-forced66/66/46/46 не переименовывать в генерацию при generate_calls=0. P130 k5 содержит повторения четырёх семей, не12 независимых семейств. P1292/2 и P131N4/4 измеряют предъявленные фразы; H/X0/4 сохраняются. Стоимость всех попыток и Export включать в существующий расход без нового PRIMARY.

Для опубликованных36/36 писать «по опубликованной сверке TRX», пока исходный XML не прочитан самим проверяющим; SHA/локальный путь из прежней расписки не равны доступному raw. Нельзя вычитать3 из исторических628 для текущего счёта. Собственные rechecks этого пакета находятся [здесь](../../../../experiments-v2/_audits/post131-2026-11-03/README.md).

> Передаваемая дельта: установить источник actual и required IDs до вычисления summary; полный положительный пакет и адресная порча идут через тот же checker. Saved input read-only, новый output, hashes до/после, без Train/Flow/Load. Не объявлять native acceptance по Python-проекции.
**Профилактика совместимости saved reanalysis после P131:** если старое свидетельство закрепляет producer source/DLL/schema, обновлённый checker имеет отдельную verifier identity и версию обязательных проверок. Проверять неизменные original producer bytes по их прежним hashes; не сравнивать старый host hash с новой verifier DLL и не исправлять старые proof/config ради PASS. Явно указать поддержанную historical schema и mapping архивных путей; отсутствие нужных originals оставить NOT_PROVEN полной provenance. Если такой механизм в этой карточке не применяется, пометить N/A с причиной; не создавать новый loader или обучение. Связь результатов и неизменность input проверяются независимо.
<!-- CODEX-POST131-AUDIT:END -->

<!-- POST113-CURRENT-2026-11-02:START -->
## После P113 — польза результата и предел переноса

P109: teacher-forced вероятность и exact answer раздельны. P110: full-window и kernel timings расходятся, universal speedup OPEN. P111: адрес выбранного owner подтверждён в experimental dispatcher. P112: честный split не повысил 2/4. P113: энергия выровнена при прежних 4/4; сравнение с P112 не является A/B. Эти полезные ограниченные результаты сохраняются без требования искусственно получить положительную точность.

Source-level counterexample, Python branch reproduction, сохранённый native log и новый native run — четыре разных доказательства. У каждого поля actual/derived/unknown. Манифесты/ожидания вычисляются до порчи; NoOp или harness exception не дают зачёт. Historical raw и затраченная работа не переписываются companion исправлением.

Основание: [адресный план 135](codex-135-post113-addressed-repair-plan-2026-11-02.md). Срез `21007e25ec3f6f6634dd8733fc8631ae267e518c`, 2 ноября 2026. Этот блок уточняет указанные условия; исторические результаты, собственные данные, единственный фактор, учебный бюджет и очередь сохраняются. Порча копии квитанции не назначает новое обучение или дополнительное чтение модели. Нижние датированные указания применяются в своей исходной области; новые требования не делают старые ненаблюдённые поля измеренными.
<!-- POST113-CURRENT-2026-11-02:END -->

<!-- EXECUTOR-CONTEXT-120:START -->
## Обязательный вход перед новым аудитом или заданием

Обязателен [договор120](120-executor-context-and-test-repair-contract-2026-11-02.md): перед новым аудитом/заданием свежо и целиком прочитать local/canonical AGENTS, CLAUDE, 12 основных навыков и дополнительные experiment-design/test-integrity по роли; восстановить усечения. Модель default без override. Этот вход не разрешает запуск и не меняет данные, budget, команды или очередь.

До действий прочитать выбранную карточку целиком и её локальную секцию120, source/raw и фактический маршрут producer→ordinary caller→learner→consumer; local HEAD, published SHA и dirty bytes различаются. Списки навыков ниже — дополнительные, не освобождение от полного свежего стека120. Исторические Tier3/vocab/split/UI/Git/model правила не расширяют нынешний паспорт и авторизацию.

**Порядок самопроверки:** source SHA + dirty bytes → именованные producer/caller/learner/state/consumer → own positive/negative witnesses → полный outer/nested расход → назначенные ordinary/restore/E2E проверки. При падении сохранить input/expected/actual/stack + source/assembly SHA до restore; fixture/устаревший контракт/регрессия/инфраструктура раздельно, gold/threshold/Skip не лечат ошибку. Назначенный перенос/QA/ремонт включает сборку, discovery, тесты и исправление фильтра, fixture или expected по независимому договору; повторное разрешение на это не нужно. Сверить обязательные имена с discovery и результатами той же сборки, сохранив весь назначенный охват. Автору передать недоступные исходные данные, спорный договор или необходимость нового API/научного опыта; не заменять их удобным аналогом. При текущем задании только на документы запуск не разрешён. Терминал того же handle, русский отчёт, STOP/NEXT немедленны; future checks остаются UNRUN.

**Сохранение постановки:** exact recorded result не повторять, old NOT_RUN не выше текущей Q-строки. Полный outer/nested бюджет и независимый gold остаются у карточки; пересчёт raw/структурный guard сTrain0 не заменяет обязательный учебный цикл и не получает фиктивный Train. Команды, API и фильтры не додумывать исполнителю. Вход обязателен также в передаваемом промпте выбранного ID.
<!-- EXECUTOR-CONTEXT-120:END -->
<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

Сила утверждения ограничена наблюдённым уровнем. Записанные отпечатки, фактически прочитанные байты, загруженное состояние и ответ обычного потребителя — отдельные доказательства. В P104 запись 21/21 булевых значений не доказывает 21 реальную порчу. В P107 отсутствие разногласий на 12 входах не проверяет преимущество большинства. Два правильных Pattern/Signature не доказывают правильные роли токенов; log-odds не равны logP(gold); изменение весов не доказывает ненулевой градиент данных.

Ошибочное историческое утверждение сохраняется, но новый итог должен присоединить адресную квитанцию уточнения. Число файлов и контрольных условий само по себе не показывает глубину ревью.

Основание: [аудит 123](codex-123-post107-transfer-and-experiment-audit-2026-11-02.md), [план 124](codex-124-post107-addressed-repair-plan-2026-11-02.md), [контекст 41](../experimets/codex-41-post107-executor-checks-2026-11-02.md). Это уточнение приёмки будущего переноса; прежние результаты и назначенная доза сохраняются.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->


<!-- POST102-AUDIT-2026-11-02:START -->
## Действующее уточнение после P102 — Определения метрик и причинная скромность

**Договор.** У любого числа именуются формула, единицы, знаменатель, panel/state и provenance. TF position accuracy, free prefix length и semantic correctness не складываются в одну ratio. Changed weights не равны observed gradients; softmax связывает non-target logits. EXAM-local unnamed inventory не означает token never trained.

**Ошибка и ожидаемая проверка.** Wrong case: красивый gain относительно слабого baseline объявлен general ability или hypothesis объявлена единственной причиной. Correct case: перечислены acquired/lost RowId, actual baseline, достаточные и недостающие controls. Историческое explanation исправляется supplemental record без подмены raw.

Источник: [аудит120](120-post102-transfer-and-executor-audit-2026-11-02.md); действующие роли skills, WRONG/CORRECT, testing/recovery и границы полномочий — [40](../experimets/40-effective-executor-context-2026-11-02.md). Дозы, факторы и исторические raw сохраняются. Этот договор не запускает PRIMARY и не объявляет будущие tests выполненными.
<!-- POST102-AUDIT-2026-11-02:END -->


<!-- POST101-AUDIT-2026-11-02:START -->
## Дополнение после P101: Не расширять вывод за границу опыта

Основание — [117](117-post101-transfer-and-experiment-audit-2026-11-02.md) / [118](118-post101-addressed-repair-plan-2026-11-02.md). **Действующий договор.** Результат разделяет scope исполнения, реальность изменения состояния, предметный эффект и допуск переноса. P99 BPE-диагностика по записанному изменённому назначению не равна4SFT исходного паспорта. P93 empirical rate1 определён, а калибровка ошибок не оценена. P94–P97 simple rule с тем же качеством не отменяет реально выполненное обучение, но не доказывает его преимущество. Среднее траектории, last-pre-update и final-state reading называются раздельно.

**Адресная приёмка.** Negative: ASCII-filterP101 переносится на все Unicode tokens; synthetic tie support выдаётся за global generation; copied objective parity закрывает native gradients/updates. Positive: для каждого claim названы corpus/key novelty, состояние, ordinary consumer, шкала, temporal kind, цена и ограничения. Correction bound к target SHA256, raw не переписан.

Это требование к будущему исполнению/приёмке применимой границы. Нового PRIMARY, увеличения бюджета или повторного обучения оно не назначает; исторические evidence и исходные recipe сохраняются.
<!-- POST101-AUDIT-2026-11-02:END -->


**Текущее дополнение 26 октября, диапазон 1–143:** [новые основания и исправления](#delta-through-143) подтверждают перенос измерения 141 в существующие 59/62 через 63, ограничивают выводы 142/143 и восстанавливают три пропущенных ID в очереди. Последующие датированные разделы сохраняют историю своих срезов.

**Дополнение 25 октября 2026:** единственное новое положительное основание — [SEP01/133](#sep133-evidence), исправление чтения усвоенного SFT-ответа. [Проверка роли DPO-партии ce76f597](#dpo-authoring-ce76) фиксирует предел доказательств и не создаёт ещё одного FIX без успешного учебного опыта. Предыдущие разделы сохраняются как история своих срезов.

**Актуальная оговорка23октября2026:** ранние exclusions ниже относятся к своему срезу. Поздние FTR/FBQ, SEXP01, N63IL и частичный SC01 имеют назначенных владельцев54/55/внешних41/55; их текущая оценка —§21 и56/57. Девять внешних механизмов уже документированы, новыми здесь не объявляются.

## Предыдущее дополнение 23 октября: FBC и LFT

Актуальная сверка — [56, measured delta и карта уже описанных планов](56-measured-delta-and-existing-coverage.md). Она дополняет исторические разделы ниже и разрешает их устаревшие исключения:

- `NEURAL-FROZEN-TRANSPOSE-01` cold0/2→0/2 не переаттестован задним числом. Отдельный **FTR01** имеет обученный2/2before, реальные4updates/arm и прежнюю экономию allocations; поэтому появился **FIX-LFT55**.
- Старый R1 из04 не приобретал правильные токены; отдельные **FBQ01/60 и FBQ02/61** квалифицированы33/33 и дают основание **FIX-FBC54**. Поздние62/63 понижают временные проценты, исключают peak gain и перенос удаления RequireValid.
- **57–59** уточняют уже существующий N63C17, не создают3новыхFIX-ID. Нет универсального вывода «один stale урок безопасен» или «три исправленных всегда достаточно».
- Прежняя фраза «SC01 без готовой карточки» ниже историческая: partial boundary FIX-SC уже описан в прежнем каталоге и связан в56. Отрицательный terminal повторSC02 не превращает его в полное освоение ответа. Равным образом SEXP уже имеет описанный положительный ресурсный план, который здесь не дублируется.
- WM02 не добавляет успешного margin-fix: меньше abstention сопровождается большим wrong dispatch. PNL0407 не квалифицирует R2/E-OPT/LF; её9readouts не являются9новыми обучениями.

Новизна проверена против основной очереди и прежнего `fixes-based-on-experimentsperiments`;9уникальных ранее описанных кандидатов приведены ссылками в56. В том дополнении новыми были только **FIX-FBC и FIX-LFT**; поздний отдельный FIX-OSR описан в конце этого документа. Собственные математические выводы54/55/17 привязаны к конкретным кодовым операциям и условиям, не выданы за измерение скорости, статистическую значимость или гарантию другого домена.

Дата исторической проверки: 21 октября 2026. Историческое дополнение на `4e993a33c9396f155a4aaeb42170e0251f34aa77` — раздел 7; опубликованное на `213f0c9b0568384632ab6bb5a67c57d6f17b3df9` — раздел 8, поздний результат на `c8b0d8a029ce721ecd906d3ff747e740b524be23` — раздел 9; review прежних пакетов не считается review новых карточек. Основной срез — [llm_mvp_core@4f35cb71b3db482bfe7ebe611c652c78d89d5db1](https://github.com/lubochka/llm_mvp_core/commit/4f35cb71b3db482bfe7ebe611c652c78d89d5db1). Для двух исходных внешних fixtures — `xiigen-planning@d0722426a90b90486b7b1d446c2478f570190920`. Это чтение исходников и результатов, **не повтор обучения**.

## 1. Что здесь новое

На исходном срезе целевая папка `fixes-based-on-experiments` отсутствовала. При этом отсутствие папки не принято за доказательство новизны содержания: проверены существующий [required-changes](../required-changes.md), [интеграция14308](../2026-10-21-14308-integration.md), [интеграция8346](../2026-10-21-8346-integration.md), [learning-delta-integration](../2026-10-21-learning-delta-integration/README.md), релевантные карточки и контракты29–33.

| Уже задокументировано | Добавлено этим пакетом |
|---|---|
| Названия кандидатов, измеренные выигрыши/регрессии | Конкретные методы продукта, обучающий writer/reader и место применения изменения |
| Исправления исходных карточек, бюджета, wait/STOP, языка | Привязка тех же правил к будущему product-transfer исполнителю, его точным тестам/путям |
| Общие доноры и различие weights/full continuation | Выбор конкретного состояния для каждого нового теста без скрытой подготовки |
| Механические proposals по NKU/N60/BPE/LC/WT/RET/N63 | Судьба существующих assertions, новый независимый oracle, минимальный consumer и recovery |
| Отдельные WT/RET результаты | План learned prompt→topic→реальный native recipient, с явным failed Wiki gate и без keyword-подмены |

Повторный полный аудит всех соседних веток не выполнялся и не заявлен. Исторические E09/E25/E26/E43 и другие корпуса остаются в родительских исследованиях; прошлый успех не превращается автоматически в новый native PASS. Здесь проверяется перенос уже зафиксированных кандидатов на конкретный срез кода.

P125/P25 `phrase_mappings` уже внесён 18.10 по [существующему реестру](../required-changes.md) и [8346](../2026-10-21-8346-integration.md). Новая карточка повторной реализации этой же правки не создана. Старые планы, сырые результаты, source copies и эксперименты не редактируются этой публикацией.

## 2. Измеренные основания восьми кандидатов

Ссылки на файлы продукта и точные будущие тесты находятся в самих карточках. В этой таблице — факты, объясняющие допуск к планированию, и ограничение каждого вывода.

| План / первичное основание | Полезный результат | Обязательная граница |
|---|---|---|
| [FIX-BPE](12-bpe-no-match-copy.md): [code-doc A](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/A/code-doc.json), [B](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/B/code-doc.json), [EN→RU A](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/A/en-ru.json), [B](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/docs/plans/replanning-possible-diffs/experimets/results/2026-10-20/BPE-NO-MATCH-COPY-01/B/en-ru.json) | Allocations 3 904 664→1 126 744 и 2 892 824→893 656 байт; словарь256→320,64 learned merges, одинаковые состояния/IDs | Обучался BPE; не измерен тот же процент ускорения полного Student. Время и allocations не смешиваются |
| [FIX-LC](13-dpo-reference-cache.md): [A](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/A/arm-result.json), [B](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/B/arm-result.json), [preparation](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N61-REF-CACHE/results/common/preparation.json) | Allocations −1 320 537 168 байт, около18%; T1/T2 остаются46/46 и44/44, одинаковые recorded losses/margins/readouts | Историческое равенство всех AdamW/RNG bytes не измерено. B timing policy-start2/completed4/incomplete−2 ошибочен; production timing корректен, его не «чинят» по этому trace |
| [FIX-NKU](10-nku-do-while-label.md): [A](../experimets/results/2026-10-20/N-KU-01/arm-A.json), [B](../experimets/results/2026-10-20/N-KU-01/arm-B.json), [gold](../experimets/results/2026-10-20/N-KU-01/gold.json) | После настоящего4-rowfit false ADD_LOOP3→0, correct1/4→3/4 | KQ4 ухудшился1→0; majority/constant prediction не является структурным обобщением; do-while label, а не новый AST-vector |
| [FIX-N60](11-translation-verifier-admission.md): [A](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/A/arm-result.json), [B](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N60-E1-VERIFIER-ADMISSION/results/B/arm-result.json) | False reject1→0; lessons6→5; Q4 приобретён Pass→Fail в обеих руках | False accept0→1, итог3/4 в обеих. Нельзя называть4verdict результатами свободной генерации перевода |
| [FIX-RET](15-prompt-flow-retention.md): [A](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/A/raw-answers.json), [B](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N61-MODE1/results/RETENTION-M1-01/B/raw-answers.json) | Общее before3/4; после A3/4 с потерей старой строки, B4/4 с её сохранением | Input features8→12; исправления противоречащих планов не проверены. Это модель в project42, не автоматически другой одноимённый learner |
| [FIX-WT](14-wiki-routing-taxonomy.md): [A](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/WT01/results/A/report.json), [B](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/WT01/results/B/report.json) | Correct2/6→4/6, wrong4→1 и один отказ; новые labels стали обучаемыми | Отрицательный переводческий запрос ухудшился; snapshot9224→14885bytes, params570→935. B publishAllowed=false, RouteForEvaluation не равен production selection |
| [FIX-N63C](17-dpo-sft-correction-collision.md): [probe raw](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N63C-PROBE/results/n63c-probe.json) | При before22/23: A20/23 против B21/23; лучше сохранён preference | Потери есть в обоих; whole exactfalse; EXAM0 и B NLL хуже. Source/binary связь historical run не замкнута; donor/full continuation не выдумывается |
| [FIX-N63R](16-replay-domain-retention.md): [probe raw](https://github.com/lubochka/llm_mvp_core/blob/4f35cb71b3db482bfe7ebe611c652c78d89d5db1/experiments-v2/RP01-MICRO/N63R-PROBE/results/n63r-probe.json) | A07before22/49→A2/B15/C4; EO02before15/49→A1/B15/C3 | A01/newN01/N02 хуже, полных ответов нет; B дороже, C уравниваетupdates, но не curriculum/tokens. Правило MinExamples5 в product trainer исходный2-rowprobe не покрывает |

Числа — исторические прочитанные измерения. В этой работе они не переизмерялись. Во всех восьми случаях изменение связано с конкретным обучаемым объектом или материалом, который он реально получает. Детали native masks, frozen weights, подходящего donor и restore не заменяются общим словом «обучение».

## 3. Полезные наблюдения с недостаточной учебной квалификацией

[Реестр](01-registry.md) сохраняет NEURAL-FROZEN-TRANSPOSE-01, N62-R1, N62-R2, E-OPT-STEP и LF-03 с их положительными ресурсными числами. Успех по allocations не отрицается и не требует роста accuracy. Однако это не даёт права добавить уже готовый план product rollout, скрыв `0→0`, отсутствие TRAIN task readout или разрыв historical binary/source.

Для первых двух причин следующий необходимый шаг — назначенный квалифицированный consumer-test с совместимым сохранённым состоянием; для разрыва binary/source — точное восстановление происхождения существующих артефактов. Не новый бесконтрольный запуск. Эти ограничения и исходные proposals уже описаны в [required-changes](../required-changes.md), [training repairs14308](../2026-10-21-14308-training-repairs.md) и [8346](../2026-10-21-8346-integration.md); новая карточка, которая повторила бы только «сначала исправить постановку», не создаётся.

Исходные короткие N63C/N63R, N63-INGRESS-LENGTH и WIKI-MARGIN не превращены в успешные learning fixes. У WIKI-MARGIN постоянная единственная метка объясняет2/4 без различения маршрутов. Невыполненный SFT-EXP-REUSE не получает результатов на основании названия. Механическая ошибка имени RNG-файла сохраняется отдельным известным дефектом; её исправление само по себе не доказывает лучшее обучение.

## 4. Точность происхождения

У `N-KU-01/comparison.json` и `BPE-NO-MATCH-COPY-01/comparison.json` на проверенном срезе — Git LFS pointers. Для чисел использованы полные A/B JSON, NKU inputs/gold; pointer не прочитан как тело результата. То же правило действует для больших weights: путь и размер не доказывают, что bytes восстановлены.

Две внешние fixtures получены по закреплённому planning commit и проверены по SHA-256:

| Fixture | SHA-256 | Что это подтверждает |
|---|---|---|
| [csharp-two-source-rows.jsonl](https://github.com/lubochka/xiigen-planning/blob/d0722426a90b90486b7b1d446c2478f570190920/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-current-learning-audit/dpo/fixtures/csharp-two-source-rows.jsonl) | `75fa380a51e96083332c9609ae69ce368b04e29011fab4af0c0fc161335202c8` | Совпадение фактических двух строк с source digest LC; не новый admission или teacher review |
| [correction-collision.json](https://github.com/lubochka/xiigen-planning/blob/d0722426a90b90486b7b1d446c2478f570190920/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-learning-ingress-and-retention-review/ingress/fixtures/correction-collision.json) | `fcf2e07aa283ad40622b7c05eee738d0c490c5c2448f686fde935775ecff1992` | Совпадение design fixture с raw N63C; её статус NOT_ADMITTED не меняется |

Старые results с абсолютными пользовательскими Windows-путями не требуют от исполнителя искать файл по компьютеру: точные источники и будущие локальные адреса назначены карточками. Historical source copies нужны для сравнения A, а исправленная B-проверка обращается к native продукту. Новые fixture manifests показывают `not_captured` для отсутствующих historical state полей.

## 5. Как проверено качество именно плана

Авторы карточек и независимые рецензенты отдельно сверяли raw, current consumers, существующие test names и общие контракты. После review исправлены реальные противоречия:

- zero-pad15→17 не сохраняет автоматически argmax/margin/confidence: WT обещает только сохранение именованных координат и требует новой квалификации;
- production DPO timing уже корректен; исторический B trace не может диктовать исправление исправного счётчика;
- RET runner и одинаковые LAB-A chosen указаны точно, чтобы compatibility guard не отменил успешный max;
- MinExamples5 отделён от original2-row replay и запрещает скрыто снизить production gate;
- fixture/output пути и class filters согласованы; `NEXT` не разрешает «сначала закончить bounded отчёт».

Это документные выводы и исправления. Fresh build/test failures не заявлены: `build-state.json` за30.04 не заменяет октябрьский baseline. Полный E2E, пригодность бинарных donors после фактической загрузки и готовность rollout проверяются только на следующем назначенном этапе. Фактический охват статической проверки — [90-review-and-publication](90-review-and-publication.md).

## 6. Дополнение перед публикацией: 16-й опыт T-TRAIN-CONTEXT-01

На новом [commit2297566](https://github.com/lubochka/llm_mvp_core/commit/2297566a5c07b564031337c2b5d4187ca10ca729) появился [завершённый результат](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/experiments-v2/RP01-MICRO/T-TRAIN-CONTEXT-01/results/report.md): после двух уроков book ответы A2/4 против B4/4; по трём уникальным входам1/3 против3/3. Q1/Q3 совпадают на входе reader. В B сохраняются четыре POS-значения; allocations чтения +1024байта, managed heap +7968. Наблюдаемое различение значения слова — положительный учебный результат, время не объявляется ускорением.

Обе руки используют неизменный AddEntry. [Продуктовый класс](https://github.com/lubochka/llm_mvp_core/blob/2297566a5c07b564031337c2b5d4187ca10ca729/12.SemanticCompiler/Translation/ContextualDictionary.cs) сохраняет входную запись; его BuildDefault уже записывает POS и намеренные fallback. Ограниченный поиск по57 C# файлам projects12/27 не нашёл внешнего training writer или consumer этого класса. Это не утверждение о всей ветке и не основание строить новую связь с TranslationDomain.

[TEST-TCTX](18-contextual-dictionary-learning-coverage.md) добавлен как **план недостающего native учебного теста**, без изменения AddEntry, расстояния, k или routing. Существующие tests на BuildDefault не покрывают cold EN→RU A/B по сохранённому контексту. Родительский candidate.md уже описывает условный поиск писателя; этот пакет не дублирует его как новый установленный дефект. На этом историческом этапе число product candidates оставалось8; отдельно учитывались TEST-TCTX, добавленный TEST-TMEM и зависимый E2E. Текущий состав после новых результатов указан в разделе7 и реестре01.

## Дополнение после сверки уже опубликованного пакета

На `00f627f1371414f92f43ec2be3c4af56e24d68b2` целевой пакет уже существовал. Повторный набор восьми карточек, второй реестр и конкурирующая E2E-архитектура не добавлены. [TEST-TMEM](19-translation-memory-coverage.md) заполняет другой пробел: постоянный native A/B/C тест восьми P125 executor-уроков и отдельный process cold restore P25 на24ответах. Исправление `phrase_mappings` остаётся уже выполненным; никакая новая product-ошибка этим не объявлена. Исторические19/24,24/24output parity и cc01/cc03/cc04false accepts сохранены. Изменения общего реестра/путей нужны только для связности этой новой test-only карточки.

## 7. Новые результаты после пакета8ea3e78: MC01, PD01 и MS01

Source текущего дополнения — [4e993a33c9396f155a4aaeb42170e0251f34aa77](https://github.com/lubochka/llm_mvp_core/commit/4e993a33c9396f155a4aaeb42170e0251f34aa77), чтение2026-10-21 UTC. Предыдущие17документов пакета [8ea3e78](https://github.com/lubochka/llm_mvp_core/commit/8ea3e78c039971288b12ecf2657c8e9f6804e98a) уже покрывали8product candidates,2test-only плана иE2E. Новые IDs и обе shared families в этих integration plans отсутствовали. [required-changes:202–222](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/docs/plans/replanning-possible-diffs/required-changes.md#L202-L222) уже называет новые кандидаты; этот список не содержал полного product insertion/test/recovery плана.

| Новый план и первичные raw | Измеренный полезный gain | Сохранённые регрессии и предел переноса |
|---|---|---|
| [FIX-MC](21-sft-optimizer-continuity.md), [MC01 pair-summary](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/MC01/results/attempt-0001/pair-summary.json), [A1 EXAM](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/MC01/results/attempt-0001/A/evaluation/A1/evaluation-receipt.json), [B1 EXAM](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/MC01/results/attempt-0001/B/evaluation/B1/evaluation-receipt.json) | D-T1 response NLL A0.00031151573→B0.0001555981; D-T2 A0.00031236818→B0.00015968285. EXAM correct signs2/4→3/4; qualified TRAIN33/33 до/после | D4 B−0.19882107 хуже A−0.15270853; оба неверны. Peak+415318016байт, factorWallMs reset/import+проверки +1.4038мс, portable state426788bytes. Только SFT→SFT без промежуточной DPO mutation, session scheduler отдельно |
| [FIX-PD](22-dpo-valid-length.md), [PD01 pair-summary](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/PD01/results/attempt-0001/pair-summary.json), [A](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/PD01/results/attempt-0001/A/metrics.json), [B](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/PD01/results/attempt-0001/B/metrics.json) | Allocations4455460456→4110381856bytes,−7.745%;60/616side positions устранены. Удержаны33/33 и69/69, objective/active logits/gradient norms в заданной погрешности | Peak+365957120байт, чуть хуже NLL в обоих; финальные weights неbitwise equal. Wall−51% только наблюдение. Narrow exact/batch1/dropout0; fresh DPO optimizer, не SFT continuity |
| [FIX-MS](23-sft-response-mask.md), [MS01 pair-summary](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/pair-summary.json), [A0](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/A/evaluation/A0/evaluation-receipt.json), [A1](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/A/evaluation/A1/evaluation-receipt.json), [B1](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/MS01/results/attempt-0001/B/evaluation/B1/evaluation-receipt.json) | D4 margin BEFORE−0.2006387711→B−0.1951050758, B лучше A на0.003715992. Allocations1163482488→1162231592bytes,−0.1075%; qualified TRAIN33/33 | D4 знак неверен; D1–3 margins/TRAIN NLL хуже, peak+17391616байт, correct signs3/4 unchanged. Cost scope2урока+checkpoint, A/B bridge enumeration различна: не доказана причинная чистая экономия маски. Только explicit opt-in, default прежний |

В MS старый headline «выигрыша нет ни по одной оси» расходится с per-item/resource raw. Пользовательское правило требует сохранить положительные оси и одновременно отрицательный общий баланс. Новый план отражает это различие, не исправляет исторический report, не обещает готовность к rollout и не назначает дополнительные эксперименты ради более красивого результата.

### Проверка реального обучающего участия

- MC [bridge:186–306](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/35.ExperimentsV2/Experiments/Rp01Micro/MC01/Common/ContinuousLearning/MC01ConsumerBridge.cs#L186-L306) восстанавливает trained state1000 и делает native SFT; текущий [session builder:404–407](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/14.ContinuousLearning/Training/IncrementalTrainingSession.cs#L404-L407) создаёт свежий AdamW. План дополнительно замыкает отдельный SupervisedOnly endpoint/coordinator, действительные SFT dataset/admission/transaction, final adapter attachment, typed sidecars, v3 SFT provenance и durable retry. Текущий DPO transaction route требует DPO перед SFT и для измеренного SFT→SFT не подходит; его проверки сохраняются. Два tiny native updates проходят весь новый consumer→accepted→published→fresh resume путь, а H512 A/B отдельно проверяет learning. Одна рекомендация «сохранять моменты» этих связей не закрывала.
- PD [B wrappers:1041–1084](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/experiments-v2/RP01-MICRO/PD01/B/DpoTrainer.cs#L1041-L1084) сокращают actual Forward views после exact validation. Перенос на [native wrappers:1007–1017](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/11.DPO/DpoTrainer.cs#L1007-L1017) сохраняет carrier/digest/response SUM/capacity. Frozen-reference cache LC — другой фактор и другое состояние.
- MS [masked Trainer:455–628](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/35.ExperimentsV2/Experiments/Rp01Micro/MS01/B/Trainer.cs#L455-L628) меняет CE/gradient настоящего SFT шага; [native Trainer:67–145](https://github.com/lubochka/llm_mvp_core/blob/4e993a33c9396f155a4aaeb42170e0251f34aa77/04.TrainingLoop/Trainer.cs#L67-L145) сейчас использует все targets. План фиксирует shifted-mask/124, byte-boundary и opt-in; не требует нулевых gradients всех prompt embeddings.

Новые plans не дублируют FIX-LC, N63C или общий state contract02: MC решает собственный SFT optimizer lifecycle, PD — execution length exact carrier, MS — отдельный loss target mode. LFT01/NEURAL-FROZEN-TRANSPOSE остаётся ранее разобранным ресурсным наблюдением с недостаточной учебной квалификацией, новый план ему не создан. Соседние ветки заново полностью не аудировались; это дополнение по новым измеренным результатам после уже опубликованного пакета.

Состояния из двух source bundles проверены на уровне текстовых manifests/qualification/raw, не native binary load. [02, раздел7](02-learning-state-and-fixtures.md#7-две-уже-квалифицированные-семьи-для-mc01-ms01-и-pd01) задаёт exact bytes/hash и разные optimizer policies. Размер на историческом дополнении MC/PD/MS —20документов,11product candidates+2test-only+1E2E; текущий состав указан в разделе9 и реестре01. Фактический review данного дополнения — [90](90-review-and-publication.md).


## 8. Дельта 22 октября: KH-COST-01 и ранее недооценённый OR01

Срез аудита — [`213f0c9b0568384632ab6bb5a67c57d6f17b3df9`](https://github.com/lubochka/llm_mvp_core/commit/213f0c9b0568384632ab6bb5a67c57d6f17b3df9). До изменения в целевой папке было20 документов/14 пунктов. Проверены их заголовки и содержание, required-changes, новые raw и code; KHC/ORDER integration-test plans в них отсутствовали. Старые кандидаты LFT/N62/E-OPT/LF и исторические ветки не переоткрывались без новых доказательств.

| Дополнение | Прямое доказательство | Положительная ось | Ухудшения и предел |
|---|---|---|---|
| [FIX-KHC](24-file-action-shared-parse.md) | [A raw](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/experiments-v2/RP01-MICRO/KHC/results/arm-A.json), [B raw](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/experiments-v2/RP01-MICRO/KHC/results/arm-B.json), runner/A/B source | Выделения 5 014 352 → 4 975 976 байт; одинаковые подготовленные строки, 0/4→4/4 обеих рук и constant2/4 | CPU +15,625 мс; один запуск; два структурных семейства. 12/6 parse — вывод из source, не инструментальный counter. Native Compile в historical runner не вызывался; в24 он проверяется впервые |
| [TEST-ORDER](25-dpo-sft-order-learning-coverage.md) | [OR01 pair-summary](https://github.com/lubochka/llm_mvp_core/blob/213f0c9b0568384632ab6bb5a67c57d6f17b3df9/experiments-v2/RP01-MICRO/OR01/results/attempt-0001/pair-summary.json), per-item evaluation и TRAIN raw | B−A EXAM margin: D2 +0,00061750412; D3 +0,00004577637; D4 +0,00020742416; сумма +0,000813961. В B меньше NLL трёх TRAIN относительно A и наблюдённый peak −207 491 072 байта; 168/168 сохранены | D1 −0,00005674362; D-T2 NLL хуже A; NLL всех четырёх строк обеих рук хуже BEFORE; allocations +466 136, CPU +7 984,375 мс. Correct signs2/4 прежние, итоговые weights различны. Один A/B и разный native transaction контекст; причинное ресурсное улучшение не установлено |

Старый OR01 report/candidate говорит об отсутствии практически значимой пользы. Новая интерпретация не переписывает его: правило пользователя **любое измеренное улучшение полезного показателя** позволяет сохранить локальные положительные оси даже при неизменной точности. Разница слишком мала и постановка слишком узка для заявления об общем выигрыше; это не основание скрывать сами числа. Разность B−A и изменение относительно общего before показываются в25 раздельно.

KH уже записан кратко в required-changes; новый слой — точные правки двух файлов продукта, совместимость строковых API, минимальная TRAIN/EXAM фикстура, существующие tests, честный учёт соседнего E05 в Compile и связь с E2E. FIX-NKU меняет метки, KHC — подготовку: дубликата нет.

OR01 меняет порядок стадий, MC — перенос moments между SFT сессиями, MS — loss mask, PD — длину DPO исполнения, N63C — состав конфликтующего текста. Эти факторы не объединяются. OR загружает policy S0 из отдельной семьи и оставляет reference B0; current IncrementalTrainer сначала клонирует актуальную policy, затем требует DPO completion перед SFT. Поэтому выбран **TEST-ORDER**, учебное покрытие native primitives и документированный ограниченный кандидат, а не новый product FIX, скрыто разрешающий обход postDpoPermit или смену default.

Бинарные weights/optimizer не восстанавливались в этой работе; manifests/raw прочитаны, числа пересчитаны из текстовых результатов. Нового обучения, фиктивного runtime PASS и повторной квалификации нет. После включения позднего KH-EXT-01 пакет содержит23 документа,13 product candidates +3 test-only +1 E2E. Текущее независимое review и публикация описаны в90.


## 9. Позднее обновление HEAD: KH-EXT-01

Перед публикацией ветка продвинулась с213f0c9 на [`c8b0d8a029ce721ecd906d3ff747e740b524be23`](https://github.com/lubochka/llm_mvp_core/commit/c8b0d8a029ce721ecd906d3ff747e740b524be23). Это прямой дочерний commit: добавлены KHE runner, копии исходников, results и кандидат required-changes, всего13 изменённых путей. Ни продуктовые методы, ни целевая папка планов в нём не менялись. Поэтому новый измеренный кандидат включён сейчас как [FIX-KHE](26-method-knn-training-admission.md); исходные файлы обновления сохраняются при публикации.

Первичные [A raw](https://github.com/lubochka/llm_mvp_core/blob/c8b0d8a029ce721ecd906d3ff747e740b524be23/experiments-v2/RP01-MICRO/KHE/results/arm-A.json) и [B raw](https://github.com/lubochka/llm_mvp_core/blob/c8b0d8a029ce721ecd906d3ff747e740b524be23/experiments-v2/RP01-MICRO/KHE/results/arm-B.json) показывают реальный учебный переход: Q0=0/4 → Train(T1) → Q1=4/4 в обоих плечах. T1 даёт четыре пары соседних методов. Следующий T2 содержит четыре файла с одним методом, то есть ноль пар. Старый guard по числу методов допускает вызов Train, который очищает таблицу: A Q2=0/4. Guard по пригодным парам в B не вызывает Train и сохраняет четыре пары и Q2=4/4. Это новый фактор относительно FIX-NKU и FIX-KHC: admission MethodKnn, не разметка и не подготовка FileActionKnn.

Нативные точки указаны в26: `SemanticCompilerFacade.TrainMethodKnn` считает **точно те же соседние пары**, что принимает `MethodKnnPredictor.Train`; reader требует TrainingSize≥4. Меняется только facade. Прямой low-level Train по трём парам в существующем тесте остаётся допустимым. Текст native MethodKnn и копии KHE имеет один Git blob `e0ecebb6d513259a0930e8706737254f926197cd`; historical runner использует Query-wrapper. Новый регрессионный тест впервые обязан пройти настоящий Compile и его побочное E05-обучение. Бинарная сборка исторического опыта здесь не аттестуется.

Успех ограничен **удержанием**: все gold равны Loop, constant baseline тоже4/4, структурное или классовое обобщение не доказано. Новая политика отвергает также1–3 пары; влияние этих сведений на качество не измерено. Allocations T1+T2 одинаковы, CPU растёт на93,75 мс. Время T1 уменьшается на14,9629 мс, время T2 растёт на0,3582 мс; сумма уменьшается на14,6047 мс, а не на14,96. Подсчёт пар включён в обе исторические руки, поэтому A не является чистым старым product baseline для стоимости. Один A→B не доказывает устойчивое ускорение. Эти ограничения записаны рядом с выигрышем; исторический report не удаляется и не редактируется.

Итого историческая дельта KHC/ORDER/KHE: три новых плана24–26 и шесть согласованных общих документов,23 документа /17 строк реестра. KHE поставлен после FIX-MC и до ресурсных оптимизаций, поскольку сохраняет результат уже завершённого обучения. Это документное включение готового результата, без новых экспериментов и без правки продукта. Текущий review и граница публикации — [90](90-review-and-publication.md).

## 10. Текущая адресная дельта: исполнимость уже выбранных учебных проверок

Источник — [`7eab1172bb3787eed9c1d966931a856df95f4976`](https://github.com/lubochka/llm_mvp_core/commit/7eab1172bb3787eed9c1d966931a856df95f4976). До этого дополнения прочитаны все 23 документа и 17 строк реестра. Новизна этой части — устранение точных конфликтов между существующими планами; она не создаёт нового результата обучения или нового FIX-ID.

| Пробел до исправления | Доказательство | Что добавлено в действующие документы |
|---|---|---|
| Общий документ 03 безусловно запрещал Compile exclusion, а раздел 10 документа 02 предписывал архивный Remove | Прежние раздел 2 документа 03 и раздел 10 документа 02 в опубликованном 7eab; [35 csproj](https://github.com/lubochka/llm_mvp_core/blob/7eab1172bb3787eed9c1d966931a856df95f4976/35.ExperimentsV2/ExperimentsV2.csproj#L1-L29) включает SDK default Compile | Узкое разрешение только архивных Rp01Micro в проекте 35, без удаления источников, штатных E01–E53, тестов или product references; нулевой учебный расход |
| Предпосылка ограничивалась карточками 24–26, хотя более ранние карточки используют тот же Tests проект | [Tests:67](https://github.com/lubochka/llm_mvp_core/blob/7eab1172bb3787eed9c1d966931a856df95f4976/Tests/LlmFromScratch.Tests.csproj#L67) содержит Tests→35 | Общая предпосылка перед первой назначенной Tests-командой любого ID, включая baseline, discovery и E2E. Проверяются direct ProjectReference и точное множество Compile; штатные legacy controls остаются прежними |
| Будущий BPE runner был назначен внутрь проекта 35, где новый Program/baseline попали бы в SDK glob | Прежний документ 12 назначал `35.ExperimentsV2/Experiments/LearningTransfers/...`; та же SDK семантика и [BPE dependency RegexSplitter:53,76](https://github.com/lubochka/llm_mvp_core/blob/7eab1172bb3787eed9c1d966931a856df95f4976/01.Tokenizer/BpeTokenizer.cs#L53-L76) | Будущий `Tests.Support/BpeNoMatchCopy01/` с двумя отдельными проектами и явными Compile items; baseline сохраняет оба исходника, namespace transform имеет свой hash; B — настоящий Tokenizer через ProjectReference. Синхронизированы 4 команды, основной бюджет 12 Train / 48 Encode / 48 Decode прежний |
| 02 называл 12 кандидатов изменения продукта при 13 в реестре | Сопоставление 17 стабильных ID и их типов | Исправлен только счётчик; это не новый эксперимент |
| Карточки 21 и 23 сохраняли устаревшие буквальные Next | [21:5](https://github.com/lubochka/llm_mvp_core/blob/7eab1172bb3787eed9c1d966931a856df95f4976/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/21-sft-optimizer-continuity.md#L5) называл FIX-PD вместо очередного FIX-KHE, [23:5](https://github.com/lubochka/llm_mvp_core/blob/7eab1172bb3787eed9c1d966931a856df95f4976/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/23-sft-response-mask.md#L5) — TEST-TCTX вместо TEST-ORDER | Заменены только две строки на ссылку на актуальный реестр. Остальные факторы, команды и бюджеты этих карточек сохранены; единственный оставшийся literal Next в 22 согласован с FIX-PD→FIX-LC |

Эти изменения нужны до или при запуске **уже назначенного** обучающего теста, потому что иначе он не может собрать настоящий learner либо проверит подменённую сборку. Исходные KHC/KHE/OR/BPE выигрыши и ограничения сохраняются; новый исторический corpus, checkpoint или measured metric не выдуман. Общая предпосылка не разрешает массовое исправление solution, повтор опыта или дополнительное обучение ради build readiness.

Фактическая диагностика компилятора для текущего дерева здесь не получена: конфликт и риск будущего glob выводятся из исходников проекта и подтверждённой семантики SDK. Окончательный будущий evaluated Compile и затем адресный test run — самостоятельные доказательства. Чтение документа не выдаётся за их выполнение.

## 11. TEST-E22: недостающее покрытие существующего обучаемого выбора datasets

Первичное основание — [поздний raw E22-DATASET-ROUTER](https://github.com/lubochka/llm_mvp_core/blob/7eab1172bb3787eed9c1d966931a856df95f4976/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E22-DATASET-ROUTER/results/run-2026-05-07-002.json#L7-L17): macro-F1 `0.4709 → 0.5646`, записанный прирост 19.89%, 450 examples с TRAIN 360 / VAL 90. Ухудшение timing `0.1 → 100.5 ms` и F1=0 для TypeScript/Tests сохраняются. Timing относится к readout/scoring, не ко всему обучению. У раннего прогона тоже есть малый полезный рост, несмотря на `passed=false`; он указан отдельно в [карточке 27](27-predicate-dataset-router-learning-coverage.md), без объединения разных corpus в один результат.

Исследования [65](../65-e22-positive-late-run-and-label-transfer.md) и [66](../66-e22-corpus-lineage-and-positive-run-boundary.md) уже установили положительный raw, изменение разметчика и отсутствие восстановленного corpus из 450 строк. Они задают будущий отдельный контроль corpus/labels с family-disjoint split. Его повторная постановка сюда не переносится. Новизна TEST-E22 — конкретный постоянный native acquisition test: точные 6 TRAIN и 4 EXAM исходника, заранее заданные author labels/gold, реальный producer признаков, один fit, before/after и два comparator, точный filter и конечный бюджет. Это малый regression test приобретения двух близких шаблонов, не исторический replay, не доказательство обобщения на новые семейства программ и не исполнение протокола 66.

Механизм уже находится в продуктовой библиотеке: [`PredicateDatasetRouter.Train/Predict`](https://github.com/lubochka/llm_mvp_core/blob/7eab1172bb3787eed9c1d966931a856df95f4976/22.AgentAlgorithms/Context/PredicateDatasetRouter.cs#L25-L49) записывает таблицу и затем читает её при выборе маски. [Существующий тест после Train](https://github.com/lubochka/llm_mvp_core/blob/7eab1172bb3787eed9c1d966931a856df95f4976/Tests/ExperimentsV2Tests.cs#L1262-L1282) проверяет длину вектора, а [E22 smoke](https://github.com/lubochka/llm_mvp_core/blob/7eab1172bb3787eed9c1d966931a856df95f4976/Tests/ExperimentsV2Tests.cs#L1361-L1370) — ID и диапазон метрики. Они не утверждают содержательное улучшение выбора после fit. Граница поиска дополнительных тестов и точные существующие имена перечислены в карточке; полная ревизия каждого теста репозитория не заявляется.

TEST-E22 не назначает production fix: writer, distance, k=3, labels и старые tests сохраняются. Точное historical trained state не найдено, save/load API у native learner отсутствует; поэтому один маленький fit и reuse того же экземпляра указаны явно в [02](02-learning-state-and-fixtures.md). Новые значения `0/4 → 4/4` и macro-F1 в карточке — аналитические критерии будущего теста, **не новые измерения**. Выход `bool[6]` обозначает datasets; он не становится ID предметного домена или `FlowRef`. Связность prompt→domain и результат настоящего получателя остаются в [FIX-E2E](20-prompt-domain-e2e.md), без нового adapter или неявного допуска E22 к этому пути.

На этапе добавления E22 пакет вырос до 24 Markdown-документов и 18 строк реестра: 13 кандидатов изменения продукта, 4 плана учебного покрытия и 1 E2E. Следующее позднее обновление TS01 описано ниже; E22 остаётся отдельной карточкой тестового покрытия.

## 12. Поздний TS01: один tokenizer snapshot внутри подготовки DPO batch

Перед публикацией HEAD перешёл на [`87284fc734af2c3e57d5486d8f0b13a7aab949cf`](https://github.com/lubochka/llm_mvp_core/commit/87284fc734af2c3e57d5486d8f0b13a7aab949cf). Новые [raw A](https://github.com/lubochka/llm_mvp_core/blob/87284fc734af2c3e57d5486d8f0b13a7aab949cf/experiments-v2/RP01-MICRO/TS01/results/attempt-0001/A/metrics.json) и [raw B](https://github.com/lubochka/llm_mvp_core/blob/87284fc734af2c3e57d5486d8f0b13a7aab949cf/experiments-v2/RP01-MICRO/TS01/results/attempt-0001/B/metrics.json) подтверждают полезный ресурсный результат в пути подготовки реального обучения:

| Показатель | A → B | Вывод и предел |
|---|---:|---|
| Копии tokenizer snapshot | 2 → 1 | Одна закрытая копия переиспользуется в одном batch; это не process-global cache |
| Allocations подготовки | 3 967 792 → 3 496 456 байт | −471 336 байт, −11.88%; полезный показатель подготовки обучения |
| Время подготовки / CPU | 255.8833 → 630.7126 мс / 93.75 → 218.75 мс | Оба хуже; ускорение подготовки не заявляется |
| Peak working set | 996 028 416 → 1 127 182 336 байт | +131 153 920 байт за процесс |
| TRAIN после общего старта и после DPO | D-T1 и D-T2: 33/33 до и после в обеих руках | Усвоенный teacher-forced текст сохранён; EXAM и free generation не измерялись |
| Loss / reward margin / final weights SHA | 0.34322196 / 1.212183 / одинаковый опубликованный SHA | Сохранены carrier digests и учебные endpoints. Чтение JSON с hash не является текущей загрузкой или сравнением binary bytes |

Обе руки сделали по одному DPO update, 8 training forwards, 4 backward и 4 diagnostic forwards; прежние 1000 SFT updates общего producer не повторялись. У исходного отчёта нет отдельного `pair-summary.json`: первичные численные источники — две таблицы metrics выше. Малое отличие training allocations и времени всего шага не превращается в доказательство ускорения этим фактором; подготовка измерена отдельно.

Готовый [candidate.md](https://github.com/lubochka/llm_mvp_core/blob/87284fc734af2c3e57d5486d8f0b13a7aab949cf/experiments-v2/RP01-MICRO/TS01/results/attempt-0001/candidate.md) уже называет `11.DPO/Models/DpoExactPairMaterial.cs` и `18.CapabilityTraining/LlmIngredientLogicalUnits.cs`. [FIX-TS](28-dpo-tokenizer-snapshot.md) добавляет следующий недостающий слой: точный lazy snapshot handoff, совместимость прежнего observer пути, существующие тесты и их ремонт, native A/B на конкретном сохранённом старте и конечный бюджет. Положительный результат не открывается заново и не требует повторить consumed TS01. Планы FIX-LC и FIX-PD меняют другие факторы — frozen-reference forward cache и padded длину исполнения; их невыполнение не подменяется tokenizer snapshot reuse.

Будущий постоянный тест использует настоящий старый per-pair `CreateBatch` в A и полный native `Materializer.Prepare` в B с одинаковыми admitted rows/vectors/signal. Он проверяет copies 2→1, material/readout/weights parity после двух суммарных updates. Эти preparation envelopes различаются, поэтому новое причинное сравнение allocations/time этим тестом **не назначено**. Измеренный ресурсный выигрыш остаётся историческим основанием TS01; будущая проверка не обещает повторить процент −11.88%.

Отдельный donor `h512-dt-segmented/v1` и точный material закреплены в [02](02-learning-state-and-fixtures.md) и карточке 28. Новый старт не создаётся; snapshot tokenization не считается весами, optimizer state или checkpoint всей модели. FIX-TS не создаёт новый prompt selector и не добавляет обучение в FIX-E2E. На прежнем срезе87284 поздний KGB тогда не получил карточку успешного переноса: первичные [A.blocked.json](https://github.com/lubochka/llm_mvp_core/blob/87284fc734af2c3e57d5486d8f0b13a7aab949cf/experiments-v2/RP01-MICRO/KGB/results/A.blocked.json) и [B.blocked.json](https://github.com/lubochka/llm_mvp_core/blob/87284fc734af2c3e57d5486d8f0b13a7aab949cf/experiments-v2/RP01-MICRO/KGB/results/B.blocked.json) фиксируют `SHARED_START_UNAVAILABLE` и 0 Train/forward/update. Появившийся позже shared start не делает прежний отказ измеренным успехом и не назначает повтор KGB.

Исторический итог дополнения TEST-E22/TS01 и адресного ремонта планов — 25 Markdown-документов и19 строк реестра:14 кандидатов изменения продукта,4 плана учебного покрытия и1E2E. Тогда порядок FIX-LC→FIX-TS→FIX-BPE сохранял относительную очередь старых ID; TEST-E22 оставался последним. Следующий раздел фиксирует новую доказанную дельту.


<a id="resource-evidence-20261022"></a>
## 13. Новая дельта: завершённые KGB, RMS01 и BW01

Проверенный source **`7a9fa58d55127ae88e93cd5d71c24a579f730ca3`**. Прочитаны все25 существующих файлов fixes-based-on-experiments, новые completed raw, candidate/Program/diff, текущие native методы и назначенные existing tests. В прежнем пакете уже есть TS01 и no-match BPE, но нет переноса gradB locality, arithmetic broadcast coordinates или weighted original chunks. В [required-changes](../required-changes.md) есть краткие кандидаты; карточки29–31 добавляют недостающий **recipient → exact change → fixed state/data → native learning test → repair/E2E boundary**.

| Новый ID | Первичные измеренные доказательства | Выигрыш и открытые ограничения |
|---|---|---|
| [FIX-GB](29-matmul-right-gradient-locality.md) | [KGB A](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/KGB/results/A/metrics.json), [KGB B](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/KGB/results/B/metrics.json), добавлены commit6f076240 | gradB558,9294→209,0994 и570,4808→224,3966ms; обе TRAIN33/33, actual DPO0→2 и одинаковые final weights. Allocations+32456; kernel savings≈696ms не объясняют весь wall−8,5s. EXAM/Generate нет |
| [FIX-RMS](30-broadcast-coordinate-allocation.md) | [RMS A](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/RMS01/a001/results/A/code-sign.json), [RMS B](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/RMS01/a001/results/B/code-sign.json) | Allocations−1290240/−9,4573%; четыре реальные SFT updates400→404 и одинаковые weights. Wall+16,4572%, CPU+20%, peak+176128. Только T1 acquired; TRAIN/EXAM1/2 не подтверждают различение знаков |
| [FIX-BW](31-bpe-weighted-chunks.md) | [BW A C#](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/BW01/results/attempt-0001/A-code-doc.json), [B C#](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/BW01/results/attempt-0001/B-code-doc.json), [A EN→RU](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/BW01/results/attempt-0001/A-en-ru.json), [B EN→RU](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/experiments-v2/RP01-MICRO/BW01/results/attempt-0001/B-en-ru.json) | Allocations−3158824/−2340144, ≈−80,46/−80,48%; acquired256→320,64merges, exact states и readouts A/B. Corpus5×8 повторный; unique gain и downstream accuracy неизвестны. CPU0 — разрешение счётчика |

Полные остальные показатели/поштучные ответы рядом в новых карточках. **Любое измеренное полезное улучшение признаётся успехом по метрике/профилю без обязательного роста accuracy.** При этом partial skill, constant baselines, regressions и отсутствующий EXAM не скрываются.

### Изменившееся свидетельство KGB и качество raw

Суждение раздела12 о blocked KGB было верным **на старом срезе87284**, когда существовали только отказы `SHARED_START_UNAVAILABLE`. Новый [commit6f076240](https://github.com/lubochka/llm_mvp_core/commit/6f0762405678bd45f8869dd63beb3af526a228e9) добавил завершённые реальные две руки. Поэтому теперь появляется FIX-GB. Старые blocked файлы не удалены, новый успех не выводится из одного факта наличия donor и не получен повтором, запущенным в этой сессии.

KGB raw `budgetActual.backwards=8` — дефект счётчика bridge, а не восьми реально исполненных backward: native StepVector и `timing.Backward.CompletedCalls=2` в каждом из двух update дают4/arm. Новые планы используют actual count, исходный raw сохраняют. Материализация и actual adapter writes связывают выигрыш с обучением.

BW `comparison.json` получен как LFS pointer, не как прочитанная таблица; `status.json`/устаревший fixture status не используются вместо четырёх полных arm receipts. Получение остальных JSON позволило проверить corpus recipe/hashes, before/after state и resource deltas без новых fits. Полный after-tokenizer/binary load в этой документной работе не заявляется.

### Native участие и недублирование

| Фактор | Конкретное участие в обучении | Что не переносится вместе с ним |
|---|---|---|
| gradB locality | [TensorOps.MatMul](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/00.TensorCore/Core/TensorOps.cs#L679-L698) → LoRALinear → DPO backward/optimizer | Telemetry, reference cache LC, valid-length PD, snapshot TS, новая LoRA/authority |
| Arithmetic coordinates | [MultiplyBroadcast](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/00.TensorCore/Core/TensorOps.cs#L148-L214) → [RmsNorm](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/03.TransformerModel/Layers/RmsNorm.cs) → native SFT/AdamW | Runtime flag, удаление mappedBIndices, response-mask MS, новый producer/MC lifecycle |
| Weighted BPE chunks | [BpeTokenizer.Train](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/01.Tokenizer/BpeTokenizer.cs) → [KnnLogicalUnit.Train/Execute](https://github.com/lubochka/llm_mvp_core/blob/7a9fa58d55127ae88e93cd5d71c24a579f730ca3/21.AgentInfrastructure/Logical/KnnLogicalUnit.cs#L103-L169) | No-match MergeChunk FIX-BPE, Regex/normalization, HNSW/k/vector dimension или router |

GB/RMS меняют разные ветви общего файла. BW и no-match copy взаимодействуют по стоимости, поэтому фиксируется одинаковая общая база; экономии не складываются. Native tests используют два source SHA с одинаковыми harness/fixtures, product references и independent state; экспериментальная копия не становится B. Положительные local learning проверки не превращаются в новый measured domain-selection результат.

Состояния и reuse заданы в02§11, конечные native A/B commands —03§9. E2E20 сохраняет прежние три темы, настоящий learned FlowRef, handler invocation и cold restart. Новое дополнение там — изоляция/identity на тех же запросах; не новое обучение, не bypass gate.

Промежуточный итог этой дельты до позднего WK01: **28 Markdown-документов,22ID —17product candidates +4test-only plans +1E2E**. Добавлены29–31; прежние19ID сохраняются. Следующий раздел дополняет этот состав, не пересматривая три уже проверенных фактора. Код продукта, test sources, build, fits, model restore и historical artifacts не изменялись.

<a id="wiki-feature-key-evidence"></a>
## 14. Поздний WK01: полезный выигрыш под слишком общим headline NO_GAIN

Commit чтения — `32d8887983c347f3bf3d674a5e9414d57eaec75b`, добавивший completed `experiments-v2/RP01-MICRO/WK01`. Raw [A](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/experiments-v2/RP01-MICRO/WK01/results/attempt-001/A/report.json)/[B](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/experiments-v2/RP01-MICRO/WK01/results/attempt-001/B/report.json) показывают реальное обучение4TRAIN×18эпох и0→3/6correctSelected в обеих руках. Непустой Train wall **6,0871→4,0095ms**, allocated bytes **227648→325280**, snapshot bytes **9737→10135**. CPU15,625ms одинаков. Это наблюдаемое улучшение полезного учебного показателя при регрессии памяти и прежней accuracy; по правилу пользователя оно получает ограниченный статус кандидата. Один короткий cold process/arm, возможные JIT/helper costs и отсутствие статистики сохраняются; обязательный production speedup не заявлен.

В [историческом отчёте](https://github.com/lubochka/llm_mvp_core/blob/32d8887983c347f3bf3d674a5e9414d57eaec75b/experiments-v2/RP01-MICRO/WK01/results/attempt-001/report.ru.md) стоит NO_GAIN. Его нельзя расширять с accuracy на все метрики; документ32 сохраняет original и объясняет точное расхождение интерпретации. K-Q2 gold margin0,01106869→0,01885850 подтверждается read-only арифметикой по saved weights; обе величины меньше0,02, поэтому правильного Selected нет. У K-Q4 ошибочное решение остаётся. Constant CodeTraceTraining тоже3/6; качество полного роутера не доказано.

| Что уже есть | Что новое в [FIX-WK32](32-wiki-feature-key-isolation.md) |
|---|---|
| FIX-WT: выходные labels15→17, LabelIndex/decoder/migration | Входные features: `load_customer` как unigram и `load customer` как bigram больше не одна координата. Иной алгоритмический фактор |
| FIX-RET: сохранение PromptFlowGroundingModel memory | Native Wiki perceptron, другой learner; union/max retention сюда не переносится |
| Historical WK source-copy / raw / общий результат | Точные native recipients, codec version и legacy restore, доступ к internal evaluation, постоянный native4/6 тест, существующие guards и полный бюджет, интеграция с прежним E2E |

Изменяемый `Featurize` непосредственно участвует в native Train и Score/Route. Старый snapshot не может быть lossless-разделён на unigram/bigram weights: план назначает явный `FeatureKeyCodecVersion`, missing/null=legacy, новыйTrain=v2, unknown=ошибка. Legacy reader сохраняет старые weights. Historical B уже использует новые keys без версии и потому служит **только oracle**; cold tests читают новый tagged nativeB. Удаления результатов/переписывания raw/скрытого Train при load нет.

Source identities разделены: raw execution coreab583032, source-copy origin62bcc8f, retrieval32d888. Native blob `3672bd2c9dd813eb2a31835c2e6b06071de37869` совпадает с source provenance; математический baseline соответствует прочитанному native. `NOT_COMPILED_NOT_RUN` — старая подготовительная metadata, не отрицание completed raw. Хэши fixture/snapshots сверены с original connector bytes; дополнительный LF в рабочем зеркале не выдаётся за исторические bytes. Это чтение и вычисление по сохранённым параметрам, не новый runtime replay.

Итог предыдущего дополнения — **29документов/23ID:18product candidates+4test-only+1E2E**. Четыре новые карточки29–32; queue вставляет WK послеRET передWT, сохраняет остальные относительные приоритеты. Для WK назначены10nativeTrain calls(8empty+2fit),144presentations и62reader calls с учётом existing helpers; контрактные restore tests0Train. Common03 отдельно описывает единственный narrow friend seam42 и порядок contracts после learning. Старые consumed experiments не запускаются, код и tests сейчас не изменяются.

<a id="wiki-once-pos-evidence"></a>
## 15. Дополнение по1fa9bac: ресурсное обучение WC и недостающее native покрытие E25

Текущая прочитанная база — `1fa9bac16ff209646f7abbd29dad2461df3ed538`. Предыдущие29документов уже содержали планы29–32; их product factors и прежние эксперименты не создаются повторно. Новизна ограничена следующими двумя строками.

| Основание | Подтверждённое улучшение | Новое содержание / граница вывода |
|---|---|---|
| WC01 / [raw A](https://github.com/lubochka/llm_mvp_core/blob/1fa9bac16ff209646f7abbd29dad2461df3ed538/experiments-v2/RP01-MICRO/WC01/results/attempt-001/A/report.json), [raw B](https://github.com/lubochka/llm_mvp_core/blob/1fa9bac16ff209646f7abbd29dad2461df3ed538/experiments-v2/RP01-MICRO/WC01/results/attempt-001/B/report.json) | Allocated bytes непустого Train1 321 440→150 656(−88,60%), wall23,2227→10,539ms(−54,62%). Обе руки0/4→4/4, одинаковый learned state | [33/FIX-WC](33-wiki-training-feature-reuse.md): exact native Train hoist, один per-call cache, повторяемый writer/restore/guard test и полный10Train budget. Source-copy ещё не native product перенос; один короткий process/arm, CPU0/0, gate=false, large-corpus live peak не доказан |
| E25 / [raw1мая](https://github.com/lubochka/llm_mvp_core/blob/1fa9bac16ff209646f7abbd29dad2461df3ed538/35.ExperimentsV2/results/E25-PATTERN-CLASSIFIER/results/run-2026-05-01-001.json), [raw7мая](https://github.com/lubochka/llm_mvp_core/blob/1fa9bac16ff209646f7abbd29dad2461df3ed538/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E25-PATTERN-CLASSIFIER/results/run-2026-05-07-001.json) | Historical validation top1 0,3438→0,875 на32строках, hard accuracy0,8571. Readout wall0,4→15,4ms в первой записи и0,4→6,6ms во второй | [34/TEST-POS](34-pos-bigram-learning-coverage.md): два permanent learning assertions на same-instance before/Train/after. Native learner уже есть в project12; новый6+4fixture аналитический, NOT_MEASURED. Original corpus/table не восстановлены; random synthetic split не доказан family-disjoint. Timing исключал Train |

WC — отдельный от WK/WT фактор: переиспользование неизменных per-row features между эпохами, без нового codec или labels. [Native Train](https://github.com/lubochka/llm_mvp_core/blob/1fa9bac16ff209646f7abbd29dad2461df3ed538/42.ProjectWikiDomain/ProjectWikiRequestRoutingTrainableModel.cs#L58-L83) реально потребляет Featurize в каждой presentation; `[row,features]` до цикла сохраняет порядок float-операций, scores, mistake decisions и обновлений по индукции. Это математическое подкрепление сохранения state и устранения повторной подготовки, а не доказательство процента ускорения на другой машине. Корпус всё время удерживает features; для других learners с меняющимися embeddings, stochastic augmentation или stateful Featurize этот вывод неприменим. В queue WC поставлен непосредственно перед Wiki codec/taxonomy; memory gain считается успехом независимо от отсутствия дополнительного прироста accuracy.

E25 — не потерянный перенос алгоритма из матрицы в новую production unit: [native PatternClassifier](https://github.com/lubochka/llm_mvp_core/blob/1fa9bac16ff209646f7abbd29dad2461df3ed538/12.SemanticCompiler/Translation/PatternClassifier.cs) уже обучаем, а alias в [GlobalUsings](https://github.com/lubochka/llm_mvp_core/blob/1fa9bac16ff209646f7abbd29dad2461df3ed538/35.ExperimentsV2/GlobalUsings.cs) ведёт именно к нему. Существующие E25 runner tests с Train сохраняются; недостающее — явное приобретение VSO на одном экземпляре и различение равных POS multisets разного порядка. Направленные биграммы и k3 дают независимый аналитический oracle34; multiset/constant decoder такой пары не различит. Биграммы не восстанавливают всю синтаксическую структуру и не доказывают общее качество перевода или выбор домена. Математически подкреплённый TEST-POS идёт первым среди coverage-строк после product-кандидатов; 2/4→4/4 остаётся ожидаемым критерием, не выполненным измерением.

Итого текущее дополнение: **31Markdown-документ/25ID —19product candidates+5test-only+1E2E**. Новый общий контракт03 расширен ровно этими конечными budgets;02 задаёт fixed fixtures/state reuse;20 сохраняет прежние запросы/темы и добавляет0Train. Расходы и улучшения WC/E25 не складываются с WK/WT и не выдаются за общий domain-routing успех. Новых опытов, builds, model restore или product changes в этой документной сессии нет.


<a id="ks-nh-sk-evidence"></a>
## 16. Дополнение по3acae42: target scope, selected head и структурное обучение

Проверенный code/evidence срез — [`3acae42f7ef474e449737a57f0cf064921c4bbc7`](https://github.com/lubochka/llm_mvp_core/commit/3acae42f7ef474e449737a57f0cf064921c4bbc7). Сопоставлены31существующий документ целевой папки, completed micro raw/Programs/diffs, native recipients и existing tests. Родительские исследования описывают исторические идеи, но прежний пакет не содержал трёх конкретных переносов/учебных контрактов ниже. Новизна не означает, что уже существующий алгоритм надо ещё раз писать в продукте.

| Новое основание и первичные данные | Полезное измеренное изменение | Что именно добавлено, граница и цена |
|---|---|---|
| [MKSC01 A](https://github.com/lubochka/llm_mvp_core/blob/3acae42f7ef474e449737a57f0cf064921c4bbc7/experiments-v2/RP01-MICRO/MKSC01/results/attempt-0001/A/arm.json), [B](https://github.com/lubochka/llm_mvp_core/blob/3acae42f7ef474e449737a57f0cf064921c4bbc7/experiments-v2/RP01-MICRO/MKSC01/results/attempt-0001/B/arm.json), [Program](https://github.com/lubochka/llm_mvp_core/blob/3acae42f7ef474e449737a57f0cf064921c4bbc7/experiments-v2/RP01-MICRO/MKSC01/Program.cs) | A2/4→2/4, B2/4→4/4; в обеих руках0→4пары; ложных target-категорий2→0 | [35/FIX-KS](35-method-knn-target-scope.md): native target projection без невызванных вложенных тел, неизменные Current/query, явный k1 ingress и настоящий Compile. Allocations1112→2136байт; короткое время не causal speedup. Только exact/zero-distance профиль; defaultk5 не квалифицирован |
| [NH01 A](https://github.com/lubochka/llm_mvp_core/blob/3acae42f7ef474e449737a57f0cf064921c4bbc7/experiments-v2/RP01-MICRO/NH01/results/attempt-0001/A/metrics.json), [B](https://github.com/lubochka/llm_mvp_core/blob/3acae42f7ef474e449737a57f0cf064921c4bbc7/experiments-v2/RP01-MICRO/NH01/results/attempt-0001/B/metrics.json), [diff](https://github.com/lubochka/llm_mvp_core/blob/3acae42f7ef474e449737a57f0cf064921c4bbc7/experiments-v2/RP01-MICRO/NH01/one-factor.diff) |3 768 492 880→3 763 341 608allocated bytes за2DPOupdates:−5 151 272/−0,136693%; обе33/33 TRAIN сохранены, raw final digest совпал | [36/FIX-NH](36-dpo-scored-row-head.md): selection только перед lmHead после полного transformer, backward scatter, exact score mapping и совместимый graph constructor. CPU+0,300%, wall+12,828%, NLL слегка вырос. Нет EXAM/free generation; reported arrays и независимо прочитанные raw различаются |
| [E09 tier1/001](https://github.com/lubochka/llm_mvp_core/blob/3acae42f7ef474e449737a57f0cf064921c4bbc7/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-04-30-001.json), [002](https://github.com/lubochka/llm_mvp_core/blob/3acae42f7ef474e449737a57f0cf064921c4bbc7/35.ExperimentsV2/results/E09-STRUCTURAL-KNN/results/run-2026-04-30-002.json), [runner](https://github.com/lubochka/llm_mvp_core/blob/3acae42f7ef474e449737a57f0cf064921c4bbc7/35.ExperimentsV2/Experiments/E09StructuralKnn.cs) | Historical top1 .3368→.3472; raw+3,08%relative, одинаковые193validation transitions по записанному count/прочитанному runner | [37/TEST-SK](37-structural-knn-learning-coverage.md): отсутствующее native acquisition-покрытие уже существующего StructuralKnn. Время .5→54/.4→54.6ms. Historical baseline уже обучен; hashes binary/source и corpus manifest отсутствуют. Новый6+4fixture не назван replay; ожидаемые0→4/4 ещё не измерены |

Любое полезное измеренное улучшение признаётся по метрике и профилю; accuracy не обязана расти. Поэтому малые NH allocations не выброшены, а небольшой E09 accuracy не превращён в универсальное преимущество. Все три плана связывают метрику с реальным Train/StepVector или получением его результата; pure contract checks отдельно защищают этот путь и не выдаются за новый учебный успех.

### Исправленная интерпретация без изменения старых результатов

В MKSC `targetRows.before=2`, оба ответа Mixed случайно верны из-за empty fallback. Историческое словесное0/2 не используется. Успех B — сохранить target2/2 и приобрести controls0/2→2/2; A теряет targets и приобретает controls. Полная raw панель даёт2/4→2/4 против2/4→4/4. Исторические direct inputs при нынешнем StackDetector были бы Generic в facade: новый consumer-тест использует явно производные одинаково помеченные C# inputs с отдельными hashes, а четыре readonly Extract доказывают, что признаки MethodKnn этим не подменены. Новая facade-панель не выдаётся за старые bytes или уже измеренный результат.

NH historical report отклонял кандидат как слишком малый, хотя фиксировал полезные allocations. Переосмыслен допуск в план, report/raw сохранены. Вторая неточность — `budgetActual.backwards=8`: source bridge агрегирует неверно, detailed timing и native StepVector дают4на руку. Третья —124токена это полная сторона, response33. `comparison.json` получен как LFS pointer: восемь bitwise logits сравнений/32gradient records остаются `reported`; raw loss/margins/readouts и digest прочитаны отдельно. Adapter-value digest9cf24… не назван файловым hash S0. Бинарный restore и новый performance run не выполнялись.

E09 ранний comparable sample —968узлов,773TRAINpairs и193VALtransitions в каждой руке; cap500 не действует. Арифметически совместимые65/193→67/193 не являются прочитанными per-item ответами. Исторические per-file ordinal labels и cross-file пары отличаются от native `%200`/within-file. Поздний [E09 tier2](https://github.com/lubochka/llm_mvp_core/blob/3acae42f7ef474e449737a57f0cf064921c4bbc7/35.ExperimentsV2/results/e01-e53-tier4-20260507-190227/E09-STRUCTURAL-KNN/results/run-2026-05-07-001.json) с5806узлами даёт baseline1161 vs KNN499transitions; headline39,14% не принимается как paired gain. Исходные файлы остаются на месте.

### Математическое подкрепление и предметная область

| План | Проверяемое основание | Что из него не следует |
|---|---|---|
| FIX-KS | При неизменных Current/query/k/distance выбор соседа тот же. Проекция только Next удаляет сигналы невызванного nested тела из разметки внешнего метода | Не доказаны interprocedural calls, новый distance, качествоdefaultk5 или применение AST-проекции к обычному предложению. C# syntax-profile, тема программирование |
| FIX-NH | Для X[T,H], W[V,H], S[R,T]: `(SX)Wᵀ=S(XWᵀ)`; `dX=SᵀGW`, `dW=GᵀSX`. Full context сохранён, map связывает mask[i+1],hidden[i],target[i+1] | Не следует обрезать prompt до attention, обнулить его внутренние gradients или обещать bitwise/performance для любого kernel. Измерена документация C# при exact/batch1/dropout0 |
| TEST-SK | Три zero-distance соседа каждой AST-семьи дают большинство при k5; role/position различают равный текущий SyntaxKind. Constant/current-kind baseline не превосходит2/4 | Не доказаны оптимальные веса, новое структурное обобщение или качество перевода. Имена EXAM новые, но features равны TRAIN; тема программирование |

Эти доказательства подкрепляют ограниченные положительные основания и объясняют очередь01. Target scope стоит сразу после защиты памяти, NH после GB, Structural coverage после POS. Планы не добавляют новый domain router: настоящая передача prompt→domain/handler и cold restart остаётся в20, а локальные consumers проверяются35–37 один раз.

### Проверенные исключения и соседние ветки

Ограниченная дополнительная проверка прочитала источники, где headline мог ошибочно означать «успех обучения»:

- [E24AttributedRouter:28–80](https://github.com/lubochka/llm_mvp_core/blob/3acae42f7ef474e449737a57f0cf064921c4bbc7/35.ExperimentsV2/Experiments/E24AttributedRouter.cs#L28-L80): PredicateDatasetRouter обучается, но при scoring не читается; actual path использует DevelopmentPromptCompiler/AttributedSegmentRouter. Эта метрика не связывает выигрыш с данным Train.
- [E27PatternReorderer](https://github.com/lubochka/llm_mvp_core/blob/3acae42f7ef474e449737a57f0cf064921c4bbc7/35.ExperimentsV2/Experiments/E27PatternReorderer.cs): фиксированные правила без обучения; отдельного учебного основания переноса здесь нет.
- [E38SkillRetriever:27–50](https://github.com/lubochka/llm_mvp_core/blob/3acae42f7ef474e449737a57f0cf064921c4bbc7/35.ExperimentsV2/Experiments/E38SkillRetriever.cs#L27-L50): `Retrieve(TaskDescription, ExpectedCategory)` получает тот же category, по которому считается hit; Train отсутствует. Это не независимое доказательство learned domain routing.
- E13 с положительным headline не добавлен повторно: различие cutoff80%methods/80%pairs и происхождение split требуют отдельной проверки; уже описанный KHE не дублируется под видом нового retention fix.

Сопоставлены62entries path/SHA каталога `35.ExperimentsV2/results` на текущем core и [language_translate@f6032aff](https://github.com/lubochka/llm_mvp_core/tree/f6032aff81f1f83de7c09cf9b9efe6b058dd12b9/35.ExperimentsV2/results), [orchestrator-extensions@b293045b](https://github.com/lubochka/llm_mvp_core/tree/b293045bd89b1eb96d3e32fedd341f1a4170adbc/35.ExperimentsV2/results): совпадают. Это подтверждает сохранность проверенного результата в соседних refs, не три независимые репликации и не полный поиск во всех ветках/проектах.

Итог новой дельты: **34документа/28ID=21product candidates+6учебных покрытий+1E2E**. Старые31документ сохранены; семь общих файлов синхронизированы, три карточки добавлены. Код/tests/skills/исторические results не редактируются, новый build/fit/restore не заявляется. Фактический review этой версии — последний раздел90.

<a id="ast-sibling-evidence"></a>
## 17. AST01: новая экономия на sibling-признаках в настоящем обучении

Проверенный source/result snapshot — `d2ec9b309162deab975ef25435f5314a9567d456`. Новая [карточка38/FIX-AST](38-structural-knn-sibling-cache.md) основана на AST-SIB-CACHE-01, которого ещё нет среди28ID предыдущего пакета. Она не повторяет FileAction parse24, Method target scope35 или Structural acquisition37.

Прочитаны четыре metrics/predictions, manifest, source-ab.diff, Native/A/B, runner, process-status и compare-stdout. Пересчитано: narrow allocations −396984байта/−13,2731%, wide −33486312/−93,5675%; correctness0→12/16 и0→7/16 в обеих руках, post-output disagreement0. Git tree подтверждает одинаковые state-after blobs A/B каждого профиля. Source blob `b3aa5d376be3f93423997674dc015cea7e9fbf58` совпадает с native StructuralKnn на текущем срезе. Это source-copy experiment; будущий native transfer ещё не исполнен.

Математическое основание — переход sibling-работы O(Σmₚdₚ), на широком родителе O(dₚ²), к O(Σdₚ+Σmₚ) с дополнительной временно живой O(Σdₚ) памятью кэша. Иммутабельность дерева, порядок children, cap255 и остальные vector fields дают условную эквивалентность. CPU/peak/semantic code quality не измерены; cold timing не обещает устойчивого ускорения. Compact `%200` accuracy не равна владению языком программирования.

В отличие от runner, facade не вызывает TrainFromSource. Поэтому новый план назначает два writer и запрещает per-file replacement памяти. Дополнительная native multi-file проверка `[narrow," ",wide]` имеет точный ожидаемый размер9228/FileId0/2 и **ещё не измеренную** facade accuracy. Snapshot JSON не выдаётся за Save/Load. `comparison.json` — LFS pointer, его payload не объявлен прочитанным; выводы сверены по самостоятельным per-arm данным и compare-stdout.

Адресный novelty-review дополнительно проверил E26/E43: E26 BuildDefault→reader не является новым приобретением, а недостающее native AddEntry покрытие уже есть в18; E43 fit трёх независимых bigram tables не доказывает native ReplayBuffer/optimizer continuation, уже запланированных по более прямым N63R в16. Проверенные raw E44/E44V2/E45V2 не показали полезного выигрыша, который оправдал бы новый success-based plan. Их исторические результаты не удаляются. Это ограниченная проверка источников, не заявление о полном новом аудите всех веток.

| Дополнительно проверенный источник | Измерение и граница вывода |
|---|---|
| [E26 raw](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/35.ExperimentsV2/results/E26-CONTEXTUAL-DICTIONARY/results/run-2026-04-30-001.json), [runner](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/35.ExperimentsV2/Experiments/E26ContextualDictionary.cs) | Accuracy 0,4→1,0 на 15 примерах, время чтения 0,6→1,8 мс. Сравниваются readers над `BuildDefault`; добавление уроков этим запуском не измеряется. План18 уже задаёт native `AddEntry` по отдельному положительному T-TRAIN-CONTEXT-01. |
| [E43 raw](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/35.ExperimentsV2/results/E43-REPLAY-BUFFER/results/run-2026-04-30-001.json), [runner](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/35.ExperimentsV2/Experiments/E43ReplayBuffer.cs) | Retention 0,3226→0,3978, время 0,1→2,4 мс; 465 old / 414 new / 177 replay пар. Есть настоящее обучение счётчиков, но три вызова private `BuildBigram` создают независимые таблицы. Это поддерживает идею повторов старых примеров, не доказывает native buffer/restore. План16 уже задаёт их перенос по N63R. |
| [E44 raw](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/35.ExperimentsV2/results/E44-DATASET-WEIGHTER/results/run-2026-04-30-001.json) | Primary 0,592→0,588, security gain отрицателен, время 1→3,3 мс; полезного улучшения в этом raw не найдено. |
| [E44V2 raw](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/35.ExperimentsV2/results/E44V2-DATASET-WEIGHTER/results/run-2026-04-30-001.json) | Primary 0,1663→0,0039, security gain −0,232, время 38→375,8 мс. Математическая мотивация weighting не превращает этот исход в измеренный успех. |
| [E45V2 raw](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/35.ExperimentsV2/results/E45V2-CURRICULUM-ORGANIZER/results/run-2026-04-30-001.json), [runner](https://github.com/lubochka/llm_mvp_core/blob/d2ec9b309162deab975ef25435f5314a9567d456/35.ExperimentsV2/Experiments/E45CurriculumOrganizerV2.cs) | Hard accuracy 0,3623→0,3478, raw `scaling_signal=0`, время 35,3→40,2 мс. Текст notes `slopeGain=+1%` противоречит числовому полю: runner присваивает `ScalingSignal = slopeGain`, при почти нулевом baseSlope задаёт slopeGain=0, а notes строит форматом `{slopeGain:+.1%}`. Эта строка не считается отдельным положительным измерением. |

Повторная адресная сверка 62 непосредственных entries `35.ExperimentsV2/results` на d2ec9b3 и двух закреплённых соседних snapshots из§16 снова дала одинаковые path/tree/blob SHA. Это те же сохранённые результаты; другие каталоги и все ветки этим сравнением не охвачены.

<a id="canonical-ab-math-delta"></a>
## 18. По858e984: убрать конфликт исполнения пяти положительных переносов и связать новые математические источники

Проверенный источник — [`858e98452abdcbca43971bb00f749417ea659991`](https://github.com/lubochka/llm_mvp_core/commit/858e98452abdcbca43971bb00f749417ea659991), следующий commit после пакета FIX-AST. Он добавил математические пояснения исходным карточкам, но не новые экспериментальные измерения. Сопоставление `required-changes.md` с35документами/29ID не обнаружило нового квалифицированного результата без плана. Поэтому на этом срезе новые FIX/TEST-карточки не созданы, прежние исключения§3/16/17 не объявлены успехом по одному теоретическому объяснению. Позднее поступление результатов70062c разобрано отдельно в§19.

### Конкретный недостающий ремонт плана

[AGENTS:249–289](https://github.com/lubochka/llm_mvp_core/blob/858e98452abdcbca43971bb00f749417ea659991/AGENTS.md#L249-L289) требует один canonical workspace. Однако [прежний03§9–11](https://github.com/lubochka/llm_mvp_core/blob/858e98452abdcbca43971bb00f749417ea659991/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/03-test-and-execution-contract.md#native-ab-transfer) и29–33 предписывали detached worktrees; прежний§13 исправлял только NH и явно не менял остальные PlanId. Оставить исполнителю обе команды означало заставить его самостоятельно выбирать нарушаемое правило. Это конфликт конкретного обучающего A/B-пути, а не посторонняя проверка репозитория.

| Уже положительное основание | Недостающая часть действующего плана | Текущее документное исправление |
|---|---|---|
| GRADB-LOCALITY-01 / [FIX-GB29](29-matmul-right-gradient-locality.md) | A kernel назначался в другой рабочей копии, общая команда требовала заранее существующий B | Baseline existing→contracts→learning на A, затем назначенный gradB diff/review/commit и Candidate B; тот же S0/B0,4DPOupdates на пару |
| RMS-COORD-ALLOC-01 / [FIX-RMS30](30-broadcast-coordinate-allocation.md) | Другие пути A/B противоречили canonical root | Те же два этапа с готовым continuation400,8SFTupdates на пару; прежние6contract cases/arm |
| BPE-WEIGHTED-CHUNKS-01 / [FIX-BW31](31-bpe-weighted-chunks.md) | Interleaved A/code→B/code→A/enru→B/enru требовал возврата к старому коду | В Baseline оба A-профиля, в Candidate оба B-профиля; два раздельных корпуса, прежние26BPETrain суммарно |
| WIKI-FEATURE-KEYS-01 / [FIX-WK32](32-wiki-feature-key-isolation.md) | Worktree-команды и один вызов с обоими SHA | existingA→learningA до product diff; existingB→learningB→contractsB после него; прежние10Train/144presentations/62readouts |
| WIKI-FEATURES-ONCE-01 / [FIX-WC33](33-wiki-training-feature-reuse.md) | То же; fixture manifest требовал записать candidate SHA, которого при A ещё нет | Те же две стадии, static fixture bytes неизменны; actual SHA в intent/binding/receipts; прежние10Train/324presentations/38readouts |

Численные измерения успеха/регрессий остаются в исходных карточках и raw. Ремонт execution-контракта не получает собственной accuracy, speedup или нового ID. В каждой стадии назначен Rebuild существующего Tests проекта и native DLL provenance, чтобы после product diff не прочитать старую A-сборку. A intent остаётся неизменным, B binding связывает его и completed A; повторный fit для отчёта или E2E не разрешается. Common lifecycle —03§9, собственные WK/WC matrices —03§10–11. Уточнение ссылки NH в36 сохраняет его прежнюю матрицу03§13.

### Новая математическая связь без нового экспериментального вывода

- **FIX-GB29§2.1:** проверены [Proposition9.22/Theorem9.23](https://github.com/lubochka/xiigen-planning/blob/0c5c0b2b9222cf1d0b94d0b5ed62b7480570b351/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0330-0334-ch09-sa-maps-and-low-rank-motors.md#proposition-9-22), продолжение и [Remark9.24](https://github.com/lubochka/xiigen-planning/blob/0c5c0b2b9222cf1d0b94d0b5ed62b7480570b351/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0335-0341-ch09-motor-rank-anisotropy-and-bounds.md#remark-9-24). Формула полного градиента привязана к реальному modulo-batch indexing TensorOps и сохранению порядка слагаемых каждой ячейки. Условия сопоставлены уже назначенным аналитическим, accumulation и broadcast tests; дополнительные тесты/updates не назначены. Локальная оценка ранга не стала доказательством LoRA-успеха или основанием менять rank.
- **FIX-AST38§3:** добавлена проверенная [ссылка§2.5](https://github.com/lubochka/xiigen-planning/blob/0c5c0b2b9222cf1d0b94d0b5ed62b7480570b351/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0093-0100-ch02-encoder-cost-and-matrix-structure.md#section-2-5) и источник обновления карточки858e984. Собственный вывод O(Σmₚdₚ)→expectedO(Σdₚ+Σmₚ) явно ограничен затронутыми родителями mₚ>0 и подготовкой sibling-данных. Сохранение vectors/cap/order/всех файлов проверяется прежними assertions38§5, без нового fit. Книге не приписана теорема об этом AST-кэше.

Это дополнение исправляет **неполную исполнимость уже положительных переносов** и недостающие первичные математические ссылки. Приоритеты/Next, datasets, donor hashes, полезные оси успеха и все зафиксированные ухудшения сохранены. [E2E20§9](20-prompt-domain-e2e.md#resource-transfer-e2e) только связывает готовые A/B receipts через новый binding; прежние prompts, recipients, Wiki gate и0добавочных fits сохранены. Product-код, тесты, модели, старые карточки опытов и их raw в этой задаче не редактируются; новые build/Train/readout измерения отсутствуют.

<a id="ng-br-evidence"></a>
## 19. Позднее поступление70062c: два новых измеренных основания

При предпубликационном чтении HEAD обнаружен [70062c090a55d7350e5eadb2606dd3f07953692f](https://github.com/lubochka/llm_mvp_core/commit/70062c090a55d7350e5eadb2606dd3f07953692f), parent858e984. Он впервые публикует NG01 и BR01; прежние35 target-документов не изменены. Поэтому вывод§18 «новых кандидатов на858 нет» остаётся верным для того snapshot, а актуальная дельта получает ровно два недостающих плана.

| Новый план | Проверенное положительное основание | Регрессии и граница |
|---|---|---|
| [FIX-NG39](39-null-guard-training-label.md) | Actual TRAIN labels9/12→12/12, после fit EXAM3/4→4/4; Q1 0→1, controls3/3. Allocations221720→220296 | CPU15,625→31,25ms, грубый счётчик. Четыре совпадающих по AST-вектору семейства, не unseen-family generalization; source-copy, неполная historical binary custody |
| [FIX-BR40](40-bpe-realized-merge-objective.md) | Реальный BPE Train256/0→257/1; B lambda12→9tokens против A12, TRAIN24→23 | Comment A5/B9, totalEXAM A20/B21; allocations26176 прежние. Один merge, code-doc, источник-copy; не KNN320/нейронная accuracy и не доказанное ускорение |

NG первичные [A](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/experiments-v2/RP01-MICRO/NG01/r/attempt-0001/arm-A.json)/[B](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/experiments-v2/RP01-MICRO/NG01/r/attempt-0001/arm-B.json), Program/SourceA/SourceB/source-origin, events/status прочитаны. «Ухудшений нет» в историческом отчёте не повторяется: CPU delta положительна. Native FileActionKnn blob совпал с pinned исходником после отделения namespace source copy. Подтверждены настоящая запись12rows и четыре trained Predict, не только детерминированный scorer. Новизна — hasNullCondition, тогда как FIX-NKU меняет hasLoop, FIX-KHC — повторный parse. Данные и прогнозы не смешиваются.

BR первичные [A](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/experiments-v2/RP01-MICRO/BR01/r/0001/A.json)/[B](https://github.com/lubochka/llm_mvp_core/blob/70062c090a55d7350e5eadb2606dd3f07953692f/experiments-v2/RP01-MICRO/BR01/r/0001/B.json), Program/source-copy-manifest, exact fixture и initial/final canonical bytes прочитаны и проверены. LFS comparison payload не заявлен прочитанным; per-arm данных достаточно для пересчёта. Выбор CountPairs — factor в самом Train, а learned merges используются Encode; no-update control неизменен. FIX-BPE/BW сохраняют overlapping counts, поэтому realized objective — отдельный кандидат, а не «починка» их oracle.

Математическая опора NG — контрпример `n=1,x=null` к мнимой защите receiver числовым pattern и неизменное k3 голосование по исправленным labels; nullable/type controls сохранены. BR — число неперекрывающихся self-pair замен `floor(L/2)` вместо частоты `L−1`; максимум даёт лучшее немедленное сокращение TRAIN за один шаг, но не глобальную оптимальность/EXAM гарантию. Поэтому BR явно выбирается при Train, а default/callers остаются legacy. Native tests, proposed boundaries и будущий E2E не выдаются за уже измеренные улучшения.

Реестр расширен до31ID/37документов: BW→BR→KHC и NKU→NG→RET. Прежние29ID сохраняют относительный порядок; initial learning-unblock priorities MC/KHE/KS остаются первыми. Все новые recipe/fixtures/runtime provenance задаются заранее. Текущая работа прочитала результаты и спланировала перенос, не запускала старые или новые эксперименты и не меняла исходные карточки/raw.

<a id="new-measured-evidence"></a>
## 20. Тринадцать отсутствовавших переносов по ebaf776, a87fef9 и38c05f2: положительные показатели и точные границы

Проверенный срез — [ebaf7760a36d43172bd88317ff469edfd09a615b](https://github.com/lubochka/llm_mvp_core/commit/ebaf7760a36d43172bd88317ff469edfd09a615b); исходные37 документов уже содержали31ID. Сверены target documents, required-changes, первичные per-arm results, экспериментальные changes и текущие native recipients. Добавлены только недостающие41–53; ранее описанные факторы и существующие алгоритмы повторно не представлены как новые исправления. После этого в ветку пришёл [a87fef91ba52e81c69dd63fbc094daa776f57095](https://github.com/lubochka/llm_mvp_core/commit/a87fef91ba52e81c69dd63fbc094daa776f57095) с PQ01. Его raw и factor проверены отдельно; target-документы и native recipients в этом коммите не менялись. Последним включён [38c05f2e9a0fd5ec5da269ea078daf298b637d3b](https://github.com/lubochka/llm_mvp_core/commit/38c05f2e9a0fd5ec5da269ea078daf298b637d3b) с HCL01; это фиксированная граница источников данного пакета. Все числа ниже — **исторические измерения**, не новые результаты этой документной сессии. Полные pinned raw/source ссылки и точные test recipes находятся в каждой карточке.

| План / первичный опыт | Полезное изменение фактора A→B | Обязательное ограничение или ухудшение |
|---|---|---|
| [41 FIX-AGC](41-grad-accum-copy.md), AGC1 | За один DPO update allocated1 847 691 280→1 761 735 608B; retained46/46 и одинаковые адаптеры | GC0/1/2 по+4; retained heap+28 461 768B сразу и+1560B послеGC. Ни свободная генерация, ни обобщение не измерены |
| [42 FIX-FBM](42-frozen-broadcast-map.md), FBM1 | За один DPO update allocated1 847 680 544→1 837 788 968B; те же46/46 и trained state | Wall+201,3ms, CPU+1640,625ms; GC+2/+2/+3. Изменяется только полностью frozen broadcast |
| [43 FIX-VA](43-verifier-apostrophe-learning.md), QVA01 | Выученные terms1/2→2/2; ложные приёмы2/3→0/3; Train allocations77 440→77 056B | Ложные отказы0/5→2/5, overall6/8 прежний; Train3,0435→8,4032ms, state286→288B. Буквальная верность parser не доказывает семантическую полноту verifier |
| [44 FIX-RRD](44-rms-row-reduce.md), RD01 | За четыре SFT updates allocated13 640 016→12 984 816B; losses/weights/AdamW совпали | Peak+245 760B. TRAIN/EXAM1/2: усвоена строка, общее правило знака не выучено |
| [45 FIX-ATS](45-attention-transpose-strides.md), AT01 | За четыре SFT updates allocated13 640 016→12 457 296B; readout после обучения8 203 240→7 558 120B | Wall119,3876→123,6514ms, CPU93,75→125ms. То же ограничение1/2; historical BEFORE factor-off не задаёт native BEFORE allocation oracle |
| [46 FIX-JO](46-training-json-options-reuse.md), JO1 | Train allocated873 496→334 336B; обе руки0/24→19/24 после восьми уроков | Peak+1 703 936B; три ошибочно принятых ответа остаются. Одно холодное наблюдение времени не доказывает устойчивое ускорение |
| [47 FIX-SSR](47-silu-sigmoid-reuse.md), SILU01-R1 | Wall четырёх SFT updates22,1668→19,9589ms при bitwise parity | Allocated+59 232B, CPU прежний; fixed A→B, один короткий запуск. Training-only factor не распространяется автоматически на readout с RequiresGrad=true |
| [48 FIX-IM](48-learned-route-body-identity.md), IDENTITY-M1-01 | После двух уроков ответы2/3→3/3, positive1/2→2/2; память3801B и её canonical hash одинаковы | Train одинаков и не причина выигрыша; выбор правильного plan body ещё не выполнение внешнего Flow. Предпосылка pair.FlowRef==Chosen.FlowRef сохраняется |
| [49 FIX-KR](49-router-learned-key-normalization.md), KR01 | После двух уроков маршруты2/4→4/4; confidence0,20→0,44, отказы2→0; profiles426B одинаковы | Baseline scorer остаётся baseline. Substring collisions/DomainHint требуют новых честных controls; manifest consumer не исполняет specialist |
| [50 FIX-HNQ](50-hnsw-training-query-norm.md), HNQ01 | Warm Train100,7398→83,0755ms и CPU109,375→78,125ms; main source-derived query norms56→14 по measured trace. Новые ответы0/4→4/4, старые4/4 сохранены | Main wall3,1809→3,9071ms и allocated+56B; warm allocated+32B, вложенные warm Add24,9038→26,3246ms. Сокращение работы локально доказуемо, причинное ускорение всего Train не установлено |
| [51 FIX-PQ](51-pos-query-bigram-reuse.md), POS-QUERY-SET-ONCE-01 / PQ01 | Allocated панели чтения3 050 752→1 799 040B (−1 251 712B,−41,03%); в обеих руках после128уроков11/32→28/32, память/ответы равны | 26EXAM строк повторяютTRAIN; на6новых2/6→2/6. Два прежде верных ответа потеряны при обучении. Кандидат уменьшает расход чтения POS-памяти, не улучшает её точность |
| [52 FIX-IDX](52-translation-training-identity-index.md), IDX01 | Train wall22,2294→19,2621ms и CPU31,25→15,625ms; restore wall81,5613→73,8270ms. Обе руки11/24→19/24, state/readout равны | Train process allocation+8200B, thread+6272B; restore+38 952B, wall всей фазы+25,0429ms, peak+528 384B. Один W→A→B; CPU-квант и JIT ограничивают причинный вывод |
| [53 FIX-HCL](53-hierarchical-local-history.md), HIERARCHICAL-LOCAL-HISTORY-01 / HCL01 | После настоящего обучения A10/16→B16/16; межкластерные6/12→12/12. Смена нумерации сохраняет4/16 ответов A и16/16 B; одинаковые learned tables | Синтетическая оркестрация7events/152tokens/16queries, fixed clusters. Before heartbeat уже усвоен4/4. Время и общий router не квалифицированы; поведение без истории цели в эксперименте не сработало0/128 |

### Новизна относительно ранее созданных планов

AGC меняет первое накопление градиента, а не начальный Tensor.Backward из LF-03 или locality gradB29. FBM убирает ненужную карту, тогда как30 уменьшает координатные allocations; RRD меняет последнюю ось reduction, ATS — перестановку attention. SSR использует сохранённый знаменатель SiLU и требует явного training flag в трёх native файлах. Эти пять факторов нельзя объединить в один B и затем приписать каждому общий выигрыш.

JO сохраняет native translation learner и уменьшает создание JSON metadata внутри Train: это отдельный product factor, тогда как19 лишь покрывает уже работающую память. VA исправляет извлечение строки из учебного marker; N60 меняет допуск противоречивого урока. IM ограничивает reader уже выбранным FlowRef и не повторяет сохранение features RET. KR согласует reader с уже существующей нормализацией writer проекта26; это не feature-key codec Wiki32, taxonomy14 или замена E2E20.

HNQ и IDX первоначально выглядели отрицательными по основному headline/allocations. Повторная проверка всех измеренных окон обнаружила положительные time-оси. По правилу пользователя их нельзя отбросить из-за ухудшения другой метрики: добавлены отдельные ограниченные кандидаты. HNQ warm4 и main8 — два самостоятельных Train; main не добавляет четыре урока к warm. PQ сокращает повторное построение множества запроса в reader уже имеющегося PatternClassifier12, а TEST-POS34 покрывает приобретение этого алгоритма без такой правки. IDX индексирует учебную идентичность и не повторяет JSON policy46 либо уже внедрённый ingress phrase_mappings.

### Математика, поддерживающая приоритет

Доказательства в карточках относятся к конкретному коду и своим предпосылкам. Для композиции backward сверены первичные [Proposition9.4, chain rule](https://github.com/lubochka/xiigen-planning/blob/d7b1704fd681fe0855afeb08ca5234433b5f9f6c/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0312-0317-ch09-motor-taxonomy-and-related-work.md#proposition-9-4) и [Proposition9.6, FFN pullback](https://github.com/lubochka/xiigen-planning/blob/d7b1704fd681fe0855afeb08ca5234433b5f9f6c/llm%20mvp/math-theory/giansalvo-transformer-vol1/source/0318-0324-ch09-mlm-graph-feedforward-and-softmax.md#proposition-9-6). Они не являются доказательствами .NET allocation или процентов ускорения. Собственные выводы по кодовым участкам отделены от положений книги:

- AGC: TensorStorage всё равно создаёт собственный массив, поэтому исключение внешнего ToArray сохраняет владение; одна снятая копия N float экономит около4N payload bytes. При N=21 492 528 это85 970 112B против85 955 672B полного наблюдаемого окна. Различие накладных расходов не скрыто.
- FBM: если ни одному входу не нужен gradient, mappedBIndices не имеет reader в backward. Удаляется только его создание; умножения и обучаемые branches прежние.
- RRD: сумма по непрерывной последней оси с тем же порядком float-сложений совпадает с координатной суммой; параллельная/vectorized переассоциация в фактор не включена.
- ATS: перестановка y[b,h,t,d]=x[b,t,h,d] биективна; backward применяет обратное соответствие. Проверяются values и independent gradient oracle, не только state hash.
- SSR: при неизменном input повторно используется d=1+exp(−x); производная через s=1/d сохраняется. Время жизни input и старые mutable-input overloads защищены отдельными contracts; теория не делает один speed sample устойчивой оценкой.
- JO: фиксированная неизменяемая serializer policy позволяет амортизировать metadata setup. Первый setup остаётся внутри измеренного Train благодаря explicit static constructor; parsed уроки и learned state не кэшируются.
- IM: максимум similarity выбирается внутри множества пар с уже выбранным FlowRef. Иначе более короткая запись другого маршрута может победить Jaccard: в IQ1 исторические0,25 против0,217391. Это конкретная причина несоответствия header/body.
- KR: одинаковое отображение N при записи и чтении восстанавливает совпадения csharp/dotnet;0,20+0,12×2=0,44 пересекает прежний0,40 threshold. Две Replace не доказывают границы слов и не наделяют baseline semantic authority.
- VA: удаляются точные prefix/suffix, а не первый апостроф внутри термина. Восстановление possessive term математически не обеспечивает эквивалентность его парафразе или typographic apostrophe — отсюда сохраняемый предел допуска.

Для HNQ сумма квадратов одного query постоянна внутри данного поиска; её можно вычислить один раз, сохранив dot product, норму каждого соседа, FP-порядок и прежний tie-break. В измеренном main trace56 cosine calls и14query-method calls: число вычислений query norm уменьшается56→14, элементов3584→896. Это не75% всей работы HNSW; warm trace имеет своё14→8. Для PQ одно и то же множество соседних POS-пар запроса переиспользуется для128строк памяти. Это удаляет4064сборки query set за32ответа, не меняя memory sets, Jaccard union==0, stable order, k3 и голосование. Наблюдаемая цена308B на снятую сборку относится к этому corpus, не универсальна.

HCL исправляет адресацию локальной памяти: номер из другого кластера не обозначает то же состояние. При известной истории цели используется её собственный local state. Перенумерация одновременно переводит индексы learned table и observed state; для уникального максимума decoded следующий token сохраняется. Это подкрепляет измеренное16/16, но не обещает общую equivariance при любом tie/fallback или без истории цели. Native модуль34 остаётся экспериментальным, а общий prompt→domain selector20 не заменяется Markov моделью.

### Проверка raw, восстановления и пределов вывода

Per-arm AGC/FBM шаги взяты из steps[0], а не из более широкого costs.training envelope. Доступный comparison.json в части опытов — Git LFS pointer; его payload не объявлен прочитанным. Полные AT01 A/B code-sign.json получены через Git blobs9bb0308ff74466be9be37522290bcac3a906711e/cbf2a15f6001c7578319c2ac4d6159c50a927315 и независимо сопоставлены. JO полные checkpoint и48пар raw outputs сверены; при canonical equality исключены только явно перечисленные служебные clock/cost поля, исходные документы сохранены. Нейронные binary hashes из receipts не выдаются за самостоятельно выполненное восстановление весов в этой сессии.

Для HNQ дополнительно прочитаны warm A0/B0, main A/B и nested Add counters: благоприятное общее warm-время не приписано локальному Add, который в этом окне медленнее. Для IDX прочитаны полные A/B receipts с раздельными restore/Train/phase окнами. Для PQ raw показывает одинаковые majority counts классов5 и7 по19: baseline5 выбран правилом меньшего ID; фраза исходного отчёта «tie не понадобился» не повторяется. Native потребитель LlmIngredient использует classifier проекта29, поэтому source-copy PQ проекта12 не объявлен улучшением этого fusion.

Для IM сохранены настоящие полные checkpoints в A/states.json; для KR сохранены profiles обученных двух уроков. Поэтому native reader-test вправе восстановить их без нового Train: проверяемый код получает результат именно прежнего обучения. VA/JO и пять tensor факторов обязаны выполнить назначенный writer/update в будущем native сравнении; статический контракт или один restore не заменяет проверку их участия в обучении.

На прежнем срезе SC01 исключался по terminal0/2→0/2; это слишком широкий вывод: актуальный FIX-SC учитывает измеренный partial3/49→4/49 (§21), не приписывая освоение terminal; IDX01 и HNQ01 после повторной сверки включены как ограниченные кандидаты: у первого меньше Train wall/CPU, у второго меньше warm Train wall/CPU при математическом сокращении повторных норм. Их плохие основные/ресурсные оси сохранены. Ранняя попытка SFT-EXP-REUSE-01 имела NOT_STARTED/NOT_EVALUATED/OPEN; поздний завершённый SEXP01 уже служит основанием существующего FIX-SEXP (§21). Старые материалы не удалены. Полного нового аудита всех соседних веток или всех страниц математической библиотеки эта дельта не заявляет: историческая проверка сохранена, текущее дополнение адресное.

## 21. Согласование23октября: положительные оси, математика и настоящая новизна

Проверка только50документов основного каталога оказалась недостаточной для вывода о новизне: соседний `fixes-based-on-experimentsperiments` уже содержит9механизмов IL/SC/FSR/SVD/CS/MB/RPE/AC/SEXP. Они не переиздаются как девять новых планов. [56](56-measured-delta-and-existing-coverage.md) сохраняет владельцев; [57](57-native-transfer-contract-gaps.md) добавляет только проверенные пробелы контрактов и consumers. Existing54/55 и новая редакция17 из параллельной работы сохранены, а не перекрыты draft-дубликатами.

| Свидетельство | Действующий вывод | Что действительно изменено в планах |
|---|---|---|
| N63IL/results-after-fix | A0/B200updates,3/42→4/42; Pos NLL хуже, full0/2 | IL batch limits согласованы с MC; строгий trainer/context/provenance guard сохранён |
| SC01, затем SC02 | Partial3/49→4/49 допускает узкий кандидат;terminal0/2 не успех полного навыка. SC02 не доказал невозможность модели | Execution convention и cache/profile identity, legacy capacity и последняя цель описаны57; минимальный824Fbudget сохранён |
| FSR01 |7/11→10/11,static3/3;binding сильно дороже,collision остаётся | Native facade вместо только direct reader;4file-action fit и176companion Train посчитаны отдельно, holdout ограничен правильной памятью |
| SVD01 |After2/5→5/5, directed0/3→3/3;payload+120000B | Уточнены native next-token consumer, FQN/9000tokens и математическая ссылка. Симметричная похожесть не заменена глобально |
| FBQ04 |10completed runs, одинаковые опубликованные start/after/loss/panels; wall/CPU ranges перекрываются |54 больше не использует ранние непересекающиеся ranges как текущую гарантию; процент не установлен, guards не выключаются |
| FTR01 |Измеренные2/2→2/2 и−1507392B, wall/peak хуже |55 сохраняет scoped batch; новые owner/guard overhead ещё не измерены. Один producer250 только при отсутствии назначенного полногоS0 |
| PA01 |Выбран обученный parent:0/40→3/40, optimizer0→1000;full0/2 |21 получает discovery/pending/accepted assertions на уже назначенных tiny checkpoints,0дополнительногообучения |
| WM02 |A/B0→5/8; margin-factorwrong-selected0→2,raw-label7→5,стоимость хуже |Нет нового margin FIX.14/20 дополняют gate/negative-class semantics, сохранённый A используется без повторных18эпох |
| PNL0407 |0correct во всех inspected states не означает отсутствие прежних updates |Не повышать ранние04–07cost observations до acquired-task proof. FBQ/FTR имеют собственные поздние основания |

Полная численная evidence RPE/CS/MB/AC/SEXP остаётся в [внешнем41](../fixes-based-on-experimentsperiments/41-neural-learning-resource-deltas.md), FSR/SVD —внешних51/53; копирование этих таблиц не добавляло бы доказательств. Любой полезный измеренный показатель учитывается по своей оси, с соседними ухудшениями и ограничением домена.

Математический приоритет: сначала устранить утрату допустимых targets/уроков (IL/SC), затем writer-label и направленный SVD-reader, после этого эквивалентные kernels по их измеренной оси. Для SVD симметрияLLᵀ — структурное ограничение, а не гипотеза ускорения; для FBC equality bits↔bytes — точная эквивалентность в одном процессе; для LFT/RPE cache — эквивалентность только при назначенной неизменности входов/времени жизни. Эти доводы не дают новых domain-quality чисел. Порядок определён01, не скрытой оценкой ассистента.

Native code/test/runtime executions в текущем дополнении:0. Бинарные checkpoint сейчас не загружались в модель; отдельные прежние SHA-проверки сохранены как evidence своих авторов. Проверены документы, source methods, reports/structured raw и связь этих материалов с будущим тестом. Полного повторного аудита всех соседних веток и всей математической библиотеки эта дельта не заявляет.

### Поздний OPT01: отдельная учебная квалификация и границы измерения

На `40242e7` появился завершённый [отчёт OPT01](../../../../experiments-v2/RP01-MICRO/OPT01/results-01/report.md) и [raw verdict](../../../../experiments-v2/RP01-MICRO/OPT01/results-01/verdict.json). Он поддерживает один новый [FIX-OSR / 58](58-optimizer-step-metadata-read.md): три A и три B, сохранённые 66/66 TRAIN-токенов суммарно, одинаковые опубликованные start/final hashes и десять losses. Медианные выделения за обновление уменьшились на 387 049,6 байта. Это отдельное позднее основание; числа не приписываются раннему E-OPT задним числом.

Диапазоны выделений не пересекаются. Выигрыш около 0,0205966% полезен по правилу пользователя. Медиана wall уменьшилась на 3,0753%, а медиана CPU выросла на 1,6471%; оба временных диапазона пересекаются, устойчивый speedup не установлен. В Opt01Bridge счётчик охватывает весь цикл из десяти шагов. Имя callback в поле measured_path не превращает этот замер в изолированное измерение callback. Разность размера serialized optimizer-файла (426 788 байт) и среднего allocation gain не доказывает 39 738 байт metadata: такое разложение не измерялось. Уточнённое объяснение по форме moments и первому пустому optimizer state приведено в 58.

С новым кандидатом в очереди **56 ID: 49 изменений кода, 6 планов покрытия и 1 E2E**; в каталоге 55 документов. Девять прежних механизмов сохранены у существующих владельцев. Последний включённый срез evidence — `40242e7`; основной код этим коммитом не менялся.


### FBQ04: полезная allocation-ось при неопределённой величине ускорения

Адресная арифметическая сверка десяти сохранённых строк [FBQ04/verdict](../../../../experiments-v2/RP01-MICRO/FBQ04/results-01/verdict.json) даёт медианы allocated за10updates `18 791 866 096→18 791 493 040`, то есть **−373 056 B** (около−0,001985%). Все пять B меньше любого A. Медиана peak одновременно `1 258 881 024→1 557 372 928 B`; wall/CPU ranges пересекаются. Поэтому FBC остаётся успешным кандидатом по наблюдаемой allocation-оси, но не получает гарантированный процент ускорения. Полная таблица, пределы JSON parity и недопустимость sweep — [54§8](54-frozen-base-check-scope.md#8-завершённый-fbq04-величина-времени-не-установлена-полезная-ось-allocations-сохранена). Это вычисление по прежним raw,0новых запусков.

<a id="sep133-evidence"></a>

## 22. SEP01/133: измеренный формат проверки ранее усвоенного ответа

Проверены первичные [raw](../../../../experiments-v2/RP01-MICRO/SEP01/results-01/separator-format.json), [source](../../../../experiments-v2/RP01-MICRO/SEP01/host/SeparatorFormatProbe.cs) и [карточка](../experimets/cards/SEPARATOR-FORMAT-MISMATCH-01.md) на `ce76f597a81221abe0224d4d09d5a77d99e0b47e`. Исходный опыт провёл 120 SFT updates — 60 для Abs и 60 для Sign, в двух независимых моделях; после этого сравнил два оформления чтения **тех же весов**.

| Модель | A: segmented prompt без LF | B: joint SFT с LF | Положительная ось |
|---|---:|---:|---|
| Abs | 22/23 правильных токена; первый target rank 5 | 23/23; rank 1 | Устранена ложная ошибка первой позиции |
| Sign | 20/21; первый target rank 100 | 21/21; rank 1 | Устранена ещё одна ложная ошибка первой позиции |

Общие ответные target IDs совпали, префиксы сохранены. У необученной базы измерен только A-readout; B-before отсутствует. Поэтому это **не доказательство нового обучения 0→44/44**. Это полезное исправление измерения уже приобретённого результата. Связь с учебным циклом конкретна: `RunProducer:394–422` обучает `Encode(prompt + LF + chosen)`, но принимает решение `if (own.AllCorrect) break` по no-LF панели. Native `IncrementalTrainer:402–403/657–662` использует тот же LF SFT input, а `Trainer:102–108` выполняет обычный next-token shift. Исходный SEP использовал экспериментальную копию Trainer; её содержимое совпадает с проверенным native Trainer, однако весь публичный `IncrementalTrainer` pipeline опытом не выполнялся. Поэтому native wiring остаётся предметом будущего теста.

Учтённая цена исходного опыта: 26 diagnostic Forward, 0 DPO/Generate/external-teacher calls, 189.7799817 секунды, peak 637 665 280 байт. Влияние исправленного readout на число обучающих шагов и время не измерялось: B прочитан только в конце. Переносимый результат ограничен двумя TRAIN-строками темы «язык программирования». Полезная ось принимается без требования роста task accuracy; остальные оси и регрессии нельзя скрывать.

Новый [62 / FIX-SEP](62-supervised-framing-and-learning-readout.md) назначает единственного владельца этого measurement contract. Внешний [54§2](../fixes-based-on-experimentsperiments/54-correction-collision-measured-followup.md) раньше перечислял формат как неизмеренную причину; текущая измеренная достаточность и native readout plan там отсутствуют. Владельцы collision 17, tokenizer 28, shift/cache 57 и E2E 20 сохраняются. Вложение `Pasted text (2)(1).txt:642–664` перечисляет локальные незакоммиченные59–61; их содержимого в опубликованном дереве нет. Номера и темы не дублируются; новые документы для них не создаются.

Для воспроизводимого чтения источников закреплены:

- SEP raw Git blob `9b162d69ec2eb1592bb95d6359645b5d63d5263f`; byte SHA256 `ebad204f2f0e9f78a49994980a3ed6beb97ebe18c674ec4ee3cc635a4dab3ba5`.
- SEP probe Git blob `e46eb0561ad4e0da16ae3b379868305de0bcd597`.
- Native и экспериментальный `Trainer.cs`: совпадающий Git blob `b63d550ba43e24a2b51be2c836519aab537d1312`.
- Config byte SHA256 `e1f960b8c7d883f84c9b4e55d24c7284794cee2fecfd71beac5835d199746f40`; два LFS payload OID/размера, corpus и tokenizer — [62§5](62-supervised-framing-and-learning-readout.md#5-сохранённые-данные-и-восстановление-без-нового-обучения).

Математическое подкрепление — зависимость условной вероятности от контекста и точный сдвиг target indices, [62§4](62-supervised-framing-and-learning-readout.md#4-почему-это-математически-правильно). Оно повышает приоритет устранения ложного отрицательного результата, но не определяет величину выигрыша вне этих строк. Фраза исторического отчёта о сокращении format gap в collision margins не принимается как теорема: `pθ(y|x,LF)` и `pθ(y|x)` могут по-разному меняться между состояниями. Сохранённые margins остаются фактами своего профиля; значения в другом профиле требуют отдельного измерения. Старые raw и отчёты не переписываются.

<a id="dpo-authoring-ce76"></a>

## 23. DPO-партия ce76f597: что действительно создано и чего это ещё не доказывает

Проверка относится к поставленному пользователем вопросу о создании DPO. Она **не является новым кандидатом исправления**: у этой партии нет измеренного улучшения обучения, поэтому здесь не назначаются импорт, обучение, приёмка или новая генерация. Нужно различать два среза: указанный пользователем ce76f597 и продвинувшийся перед публикацией HEAD `726b33aea95325781083668bdc73687febbd29e4`.

**Обновление текущего статуса:** в параллельном `3213f9ce4e900a6a3c22fdc17ab3512de46a3117`, уже входящем в новый HEAD, появились review/accepted/export. [Текущий отчёт M001](https://github.com/lubochka/llm_mvp_core/blob/726b33aea95325781083668bdc73687febbd29e4/learning-sets/TR07-OMISSION/ru-en/M001/report.md) сообщает blind review отдельной сессией: 10/10 valid, tie/invalid=0, затем `DATA_REVIEWED_NOT_NATIVE_TRAIN_READY` и export 10 train-троек со статусом `INTERCHANGE_FORMAT_ONLY`; `native_import_verified=false`, training NOT_RUN. Это уже **не только candidate/STRUCTURE_ONLY**. Ниже описание ce76 сохраняется как история исходного вопроса, а не как отрицание новой приёмки.

По первому вложению `Pasted text (2)(1).txt:977` исполнитель явно получил задачу создать десять строк `xiigen.dpo.v1` с `prompt/chosen/rejected`. Строки1234–1243 и 1330–1339 показывают результат структурной проверки: 10 rows, errors=[], `STRUCTURE_ONLY`, `semantic_quality=NOT_ESTABLISHED`, `training=NOT_RUN`. Строка 1638 отдельно подтверждает candidate-статус, отсутствие blind review/accept/export/training и наблюдаемый selector `Kimi K3 (Kiro)`, без доказанной внутренней версии модели. Следовательно, утверждение «вообще не создавал DPO-тройки» источником не подтверждается. Подтверждён более конкретный разрыв: **созданы кандидаты данных, но не доказаны их независимое качество и готовность к native обучению**.

Это согласуется с [learning-sets/README.md:17–21](https://github.com/lubochka/llm_mvp_core/blob/ce76f597a81221abe0224d4d09d5a77d99e0b47e/learning-sets/README.md#L17): даже accepted означает `DATA_REVIEWED_NOT_NATIVE_TRAIN_READY`, а export — `INTERCHANGE_FORMAT_ONLY`. Нельзя переименовать structural PASS в semantic/training PASS или выдать самооценку генератора за требуемую независимую проверку CODEX.

Native потребители имеют конкретные, разные входы:

| Путь | Проверенный контракт | Практическая граница |
|---|---|---|
| Structured translation units | [TranslationStructuredBatchBuilder.cs:12–22,82–94](https://github.com/lubochka/llm_mvp_core/blob/ce76f597a81221abe0224d4d09d5a77d99e0b47e/27.TranslationDomain/TranslationStructuredBatchBuilder.cs#L12): `direction`, `pair_family`, `a_side`, `validation_outcome` и другие поля; требуется `train_ready`, токенизируется `a_side` | Три interchange поля сами не определяют разложение в tokenizer/executor/verifier lessons |
| Repository-owned neural DPO перевода | [TranslationDpoTrainingHandoff.cs:285–310,429–458,565–568](https://github.com/lubochka/llm_mvp_core/blob/ce76f597a81221abe0224d4d09d5a77d99e0b47e/27.TranslationDomain/TranslationDpoTrainingHandoff.cs#L429): instruction + `\nInput: ` + `ASide`, direction/passport/admission; regex `^Translate from (?<source>[A-Za-z]{2,12}) to (?<target>[A-Za-z]{2,12})\.$` | Нужен явный соответствующий этому маршруту handoff; русский текст задания сам по себе не доказывает совместимость |
| Exact DPO materializer | [DpoExactPairMaterial.cs:19–22,64–78](https://github.com/lubochka/llm_mvp_core/blob/ce76f597a81221abe0224d4d09d5a77d99e0b47e/11.DPO/Models/DpoExactPairMaterial.cs#L19): captured tokenizer, separate segments, admission/digest/masks | Нельзя чинить этот контракт автоматическим LF из SEP или сгенерировать provenance постфактум |

Это не утверждение, что конкретные строки M001 точно нарушают regex: полный payload `draft.jsonl` не получен. На ce76f597 GitHub вернул LFS pointer, OID `f4fd0393161f0f65e060b053f8af51d1bf85f84af20bb1cc65cd01be1e2399e2`, 23 489 bytes; request OID `9f02387b80cdb801a0304d721670dc792f3b9521d40326f37f387217dc05821d`, 3 747 bytes. Вложения не предоставляют полный byte-exact JSONL. На новом HEAD `review.json` также получен как LFS pointer, OID `4352a1f0a3bda48578f1488b93868f4700d5ff99ea407237f64e0ae7a92ea7e5`, 18 834 bytes; acceptance OID `8100fb4b82f309f1f90c74fa54f65e4ee60c0b565bb0241152f7d998d8064b73`, 517 bytes; export manifest OID `31e4e36fc9544653152f7c5f43592f0cd440b0a19317f5175db965e2c63bd26e`, 462 bytes. Поэтому факт опубликованного отчёта о приёмке установлен, но эта документная сессия не выполняла собственный semantic review по полным строкам и не подтверждает скрытую identity reviewer. Реальный native import не выполнен и по самому текущему отчёту. Реконструировать строки, отзывы или модельную identity по догадке нельзя.

Роли в последующем назначенном задании должны быть названы буквально: **CODEX с `astra 6 ultracode` выбирает учебную задачу и независимо оценивает качество DPO; внешняя открытая китайская модель создаёт пары; модель исполнителя остаётся текущей по умолчанию.** Эта сессия не запускала ни одну из этих модельных ролей и не объявляет доступный агент эквивалентом указанной модели. Работа создания заканчивается созданием, пока пользователь отдельно не назначит следующий этап; не подменять его незапрошенной приёмкой, обучением или публикацией новых datasets.

Существующие положительные основания уже принадлежат FIX-N60/11 (ложный verifier lesson), FIX-TS/28 (immutable tokenizer snapshot) и TEST-ORDER/25 (конкретная материализация исторических пар). Ни одно из них не доказывает успех импорта M001. Поэтому новых FIX «починить все DPO» или «починить importer» по ce76f597 не добавлено.

## 24. Параллельный HEAD726b33: сохранены приёмка M001 и ограничение CDG58/134

Перед записью получены два новых коммита поверх ce76f597. Все 55 существовавших файлов целевого каталога сохранили свои Git blob SHA; затронуты learning-sets, required-changes и новый узел CDG58, а не native consumers/SFT/SEP source. Новизна FIX-SEP сохраняется. Дельта строится поверх нового дерева, без перезаписи этих параллельных результатов.

Статус M001 уточнён в §23: текущий опубликованный отчёт уже сообщает blind review/accepted/export; это не native training и не новый измеренный learner gain. По CDG58/134 добавлена только [граница существующего плана17](17-dpo-sft-correction-collision.md#cdg134-schedule-boundary): реальные локальные улучшения NLL/margin показаны рядом с ухудшением correct и других осей; прежняя линейная гипотеза по редким gap-точкам не используется как oracle. Квалифицированный результат нельзя объявить бесполезным лишь из-за падения accuracy; одновременно нельзя объявить универсально лучшим расписание с этими trade-offs. Нового FIX и нового runtime budget для134 нет.

<a id="n63c-sep-shared-state-evidence"></a>
## 25. Адресная дельта 25 октября по 7e79eb: согласовать N63C с измеренным SEP

**Новых самостоятельных кандидатов не добавлено.** На source baseline проверены состав и уже описанные владельцы в корпусах из 56 документов целевого каталога и 11 документов соседнего каталога с опечаткой; по выбранной дельте адресно сопоставлены контракты и первичные источники. Положительные механизмы этой дельты уже имеют владельцев. Новизна текущей дельты — связь точных состояний, неизменного gold и двух readout-профилей у существующих 17/62, а также устранение противоречащих назначений. Source-код, tests, datasets и старые результаты не меняются.

| Обнаруженный пробел | Проверенное основание | Внесённое уточнение |
|---|---|---|
| Две действующие таблицы02 назначали CDB01 и прежний N63C-PROBE | Верхняя актуальная таблица02 и17§4 уже выбирали CDB01; нижняя «Точная карта источников» оставалась старой | Обе таблицы назначают CDB01; старый donor сохранён как historical evidence |
| Связь CDB01 с уже измеренным LF-readout отсутствовала | [CDB01 pointer](https://github.com/lubochka/llm_mvp_core/blob/7e79eb156f418dc76d699966ca948d4616b36cdd/experiments-v2/RP01-MICRO/CDB01/results-01/authority-acquired.bin) и [SEP Abs pointer](https://github.com/lubochka/llm_mvp_core/blob/7e79eb156f418dc76d699966ca948d4616b36cdd/experiments-v2/RP01-MICRO/SEP01/results-01/producer-abs.bin) одинаковы | Один payload SHA-256, независимые instances; qualification22/23 no-LF и23/23 LF без acquisition |
| Бюджет36 был сохранён после требования второго профиля | [N63 Panel/SideScore](https://github.com/lubochka/llm_mvp_core/blob/7e79eb156f418dc76d699966ca948d4616b36cdd/35.ExperimentsV2/Experiments/Rp01Micro/N63C/Common/ContinuousLearning/N63CollisionProbe.cs#L413):36 уже заняты прежней панелью; SEP меняет контекст | Ровно6 дополнительных LF Forward, всего42; учебные2DPO+40SFT прежние |
| После смены reader можно оценить ошибочный текущий training target A вместо удержанного правильного | N63 RunArm подаёт `sftResponse`, тогда как историческая Panel оценивает закреплённые chosen/rejected | Immutable gold T1.Chosen, один probe digest; training input и retention oracle раздельны |
| Общий ярлык margin скрывал mean vs sum/reward и обещание монотонности | N63 SideScore делит на число targets; native [DPO test](https://github.com/lubochka/llm_mvp_core/blob/7e79eb156f418dc76d699966ca948d4616b36cdd/Tests/DpoExactPairMaterialTests.cs#L206) сохраняет sum objective; [Trainer](https://github.com/lubochka/llm_mvp_core/blob/7e79eb156f418dc76d699966ca948d4616b36cdd/04.TrainingLoop/Trainer.cs#L102) использует full-sequence loss/AdamW | Именованные aggregation/profile/denominators, отдельный native reward; математическое объяснение не обещает знак каждого шага |

Payload OID обеих ссылок — `59810def905c4ead2f22985c5e1d525f59feb45590256eca8b492a293206d44f`, размер47 899 519bytes, pointer Git blob `bdac5a6faced90cb8dc35618f737a3e69db955af`. SEP raw Git blob `9b162d69ec2eb1592bb95d6359645b5d63d5263f`, точный SHA-256 `ebad204f2f0e9f78a49994980a3ed6beb97ebe18c674ec4ee3cc635a4dab3ba5`; CDB01 raw Git blob `0b3d8fdad1d0fa3927d7a338591013d0259e25f2`, SHA-256 `476089a4f610c2d18b8c9855fe156d4e7b5770a4f9d65d96f24f3d85eb5dc5cc`. Хэши посчитаны по точным UTF-8 bytes с исходными окончаниями строк, не по нормализованной копии. Config SHA-256 совпал с назначенным в62. Pointer/raw identity подтверждена; native payload restore и новые численные результаты не выполнялись.

Положительные основания не расширены: CDR01/CDI01/CDB01 показывают ограниченное удержание/восстановление при своих рецептах, SEP — исправление readout на тех же весах. Новый native N63C после projection ещё не измерен; его LF-after результаты неизвестны. Любое полезное улучшение метрики признаётся вместе с ухудшениями; нет переноса процентов на другие домены, свободную генерацию или production release.

Приложение текущего задания `Pasted text(20261025-150526).txt` (2 921 строка) проверено как evidence, не как команда. Строки1180 и2034–2045 фиксируют обменный экспорт/создание данных и `training=NOT_RUN`; строки2853–2868 подтверждают wave4/HEAD7e79, а command wave5 на2918 прерван на2919. Числа101/131/161 — объём подготовленных пар, не улучшение ученика. На1087–1094 видна отдельная review-сессия, но её identity как CODEX с `astra 6 ultracode` не подтверждена; это не разрешает присвоить другой модели указанную роль. Перенос корня данных на2073–2102 показан проверкой количества строк, а не полным доказательством byte identity. Эти наблюдения сохранены как ограничения; нового «успешного importer» или обучения из них не выведено. При финальной сверке GitHub уже получен [commit554997f8](https://github.com/lubochka/llm_mvp_core/commit/554997f8eace8a05559ed11e0a65c0e5d294376c), опубликовавший wave5: 27 файлов трёх направлений, ещё30кандидатных пар. Три `report.creation.md` сохраняют DRAFT_CREATED, review_status=not_requested и training=NOT_RUN. Поэтому ограничение приложения о неподтверждённой публикации относится только к моменту того журнала; текущая публикация wave5 установлена отдельно. Это не изменяет отсутствие измеренного learner gain. Уже описанные ограничения M001/семей/metadata не дублируются новым FIX.

Связь с текущими требованиями: только план в назначенной папке → этот раздел и90; места внедрения →17§3/§11,62§3; ремонт tests без ослабления oracle →17§11/03§23; восстановление вместо нового обучения →02§20; E2E →20§17; математика/положительные основания →17§11 и существующий62; замечания исполнителя →проверка приложения выше. Выбор и оценка DPO остаются у CODEX с `astra 6 ultracode`, создание — у внешней открытой китайской модели. Никаких модельных вызовов в этих ролях текущая работа не выполняла.

Историческая запись после публикации f0939c5: тогда59/60/61 и62-successful-experiment-plan-coverage были только сопоставлены и сохранены; в57 отсутствовали anchors `separator-133-calibration` и `response-context-parity`, а карта покрытия назначала иной calibration-класс. **В следующей документной дельте25октября этот конфликт устранён**, см. раздел ниже. Действующий N63C по-прежнему использует [reader FIX-SEP](62-supervised-framing-and-learning-readout.md) и бюджет17:36+6диагностических Forward. TEST-COLLISION-REPAIR сохраняет отдельный damaged donor и10SFT/58Forward; дублирующая квалификация0/4/8снята. Карта покрытия остаётся обзором, не самостоятельной командой запуска.

<a id="transfer-boundaries-2026-10-25"></a>
## Продолжение25октября: единый измеритель и доказанные границы получателей

Текущий запрос — подготовка переноса успешных опытов1–133. Фаза **READ_ONLY_RESEARCH + исправление Markdown-планов**. Срез source `c62acfd85fef97c22ccad1e943b33112f4182433`; runtime, builds/tests, обучение, данные и веса не менялись. Внесены только недостающие контракты существующих кандидатов; новый FIX-ID, новый обучающий запуск и новый продуктовый алгоритм не назначены.

<a id="delta-after-132"></a>
### Основания129–132 и граница их переноса

Эта цель старых ссылок59/карты покрытия восстановлена адресно. [129/CDG55](../../../../experiments-v2/RP01-MICRO/CDG55/results-01/report.md), [130/CDG56](../../../../experiments-v2/RP01-MICRO/CDG56/results-01/report.md), [131/CDG57](../../../../experiments-v2/RP01-MICRO/CDG57/results-01/report.md) имеют полезные оси вместе со встречными изменениями; их сохранённые значения и расходы перечислены в строках129–131 [карты покрытия](62-successful-experiment-plan-coverage.md). Здесь они сопоставлены как основания ограничений плана59, не как новый пересчёт raw и не как три новых product policy. [132/DQ01](../../../../experiments-v2/RP01-MICRO/DQ01/results-02/report.md) — приобретение двух своих ответов у двух отдельных моделей, не общий trained donor для четырёх задач. Его identity со states133 описана ниже; для ремонта используется назначенный damaged donor92. Новые материалы134–139 не становятся дополнительной очередью исполнения этого задания.

### 1. Обучение разблокируется одним измерителем, без повторной квалификации

Положительное основание133 уже покрыто [62-supervised](62-supervised-framing-and-learning-readout.md). По сохранённому `separator-format.json`, прочитанному `/root/successful_transfer_delta`, на одних весах Abs22/23→23/23, Sign20/21→21/21; первые gold ranks5→1 и100→1. Исторические120SFT/26scoreF/0Generate — расходы исходного producer, не текущей работы. Исполнитель будущего FIX-SEP загружает готовые payload и выполняет назначенный Quality Fact6F/0updates; stop predicate и cold restore входят в него, а не в дополнительные прогонки.

Конфликт был практическим:59 и карта покрытия ссылались на отсутствующие разделы57, второй класс `ExperimentResponseContextCalibrationTests` и добавку0/4/8F. Исследователь проверил `git show HEAD` четырёх LFS pointers DQ01/results-02 иSEP01/results-01: обе пары Abs/Sign имеют одинаковые OID/size, приведённые в [57](57-native-transfer-contract-gaps.md#separator-133-calibration). Требование отдельно загрузить DQ01 те же weights не даёт нового основания. Поиск данного anchor в доступной локальной истории57 не обнаружил прежнего опубликованного владельца.

Решение применено в живых планах:57 содержит реальные anchors и правило единственного `SupervisedResponseReadout`;59 использует completed compatible receipt62 и оставляет58Forward/10Backward/10updates. Подменять joint LF inputs exact-DPO материалом нельзя; positive gold и negative contrast фиксируются до воздействия. Сравнения разделены по profile/target-map; mismatch между профилями не отменяет корректные сравнения внутри профиля. Четыре уже назначенных contract Facts59 используют готовые IDs/receipts,0model calls/0BPE Train. Сравнитель не запускает модель. Отсутствующий или несовместимый qualification receipt не вызывает скрытое acquisition.

### 2. Адресное ограничение сравнений, без отмены полезного133

[CDG63/139 report](../../../../experiments-v2/RP01-MICRO/CDG63/results-01/report.md), строки56–80: размах одиночного показания0.249369/0.555212, среднего по40урокам0.067594/0.254377 при различаемой разнице0.021612; порядок плеч меняется и после усреднения. Строки146–157 отзывают сравнения134–138 и сохраняют133. Числа прочитаны из отчёта, не пересчитаны заново по trajectory. Ограничения самого139: две крупности, конкретные окно/длина/LR, teacher forcing; нельзя превращать этот размах в универсальный статистический порог для1–133.

Это свидетельство используется только для ограничения уже существующего [59](59-collision-repair-transfer.md#endpoint-comparison-limit), а не для добавления134–139 в очередь реализации. Наблюдение восстановления92 и форматный контроль133 не отменены. Сохранён успех каждой полезной измеренной оси, но endpoint-регрессия, whole exact и пригодность ранжировать расписания разделены. В существующий metric Fact добавлены лишь synthetic assertions: margin gain без token gain; противоположные ранги на двух переданных endpoints. Дополнительных Facts/Forward/updates не назначено. Native E2E не получает product gate из такой margin.

### 3. HCL/SVD: где действительно вызывается изученный код

На текущем source прочитаны [ExperimentsDispatcher.cs](../../../../34.Experiments/ExperimentsDispatcher.cs), [ExperimentRunner.cs](../../../../34.Experiments/Infrastructure/ExperimentRunner.cs), [E05](../../../../34.Experiments/Experiments/E05HierarchicalPrediction.cs), [E10](../../../../34.Experiments/Experiments/E10SvdPmiEmbedding.cs) в объёме вызывающих участков и сопоставлены существующие планы. E05/E10 вызываются экспериментальным dispatcher/runner; прямые проверки есть в `Tests/ExperimentsTests.cs`. Поиск tracked `*.cs` по `E05HierarchicalPrediction|E10SvdPmiEmbedding|new BigramMarkov`, исключая `experiments-v2`, дал10файлов на каждом срезе и ни одного вне `34.Experiments`/`Tests`:

| Локальный ref | Прочитанный commit |
|---|---|
| HEAD | `c62acfd85fef97c22ccad1e943b33112f4182433` |
| DesignToCode | `04e4a683a711` |
| DpoImplementation | `d2a385f75dd8` |
| DPOLegalLearning | `49d06947b7fd` |
| orchestrator-extensions | `b293045bd89b` |
| external_domains | `1efb668cf1c9` |
| language_translate | `f6032aff81f1` |

Это ограниченный символный поиск на имеющихся локальных refs, не полный семантический аудит всех веток/динамических вызовов; remote refresh не выполнялся. Поэтому [53](53-hierarchical-local-history.md) и [57§6](57-native-transfer-contract-gaps.md#6-fix-svd-точные-native-тесты-и-граница-математического-вывода) сохраняют конкретные ремонты E05/E10, но не называют их закрытой продуктовой интеграцией. Для новой связки с предметным receiver ещё требуется назначенный вызывающий путь и измеренное основание его применимости. Успехи HCL10/16→16/16 и SVD2/5→5/5 сохраняются; +120000B у SVD явно остаётся ценой.

Отдельно прочитаны `TruncatedSvdService.DecomposeRank:94–104` и `SvdWeightCompressor:38–46,90–103` в `33.MatrixAlgorithms`: они уже сохраняют и используют V при `U diag(S) Vᵀ`. Это не тот дефект, который измерен в E10. Замена их тестов вместо тестов E10 создала бы ложный перенос; адресный запрет добавлен в57. Численная корректность всего матричного сервиса здесь не аттестована.

### 4. Покрытие текущей работы и оставшиеся границы

Авторские правки внесены родителем в53/57/59/62-supervised/62-coverage и навигацию01/README/20/этот журнал. `/root/successful_transfer_delta` — один Astra ultra исследователь, только чтение: сопоставил плановое покрытие129–132; прочитал первичные SEP133 raw/report и139 report, проверил pointer identity и адресные native участки; вернул два исследовательских пакета и адресные замечания к применённым правкам. Его review обнаружил оставшийся старый бюджет66F в59; этот абзац исправлен на58F без дополнительной квалификации. Это не независимый пересчёт всех raw1–133 и не выполненные тесты. Проектные навыки прочитаны адресно по архитектуре, планированию, review, prompt patch и человеческой коммуникации; полный reload всех многотысячных skill-файлов не заявляется.

Открытым остаётся product receiver для HCL/SVD и выполнение всех выбранных native проверок на будущем этапе. Наличие плана или математического основания не означает внедрение. Текущая дельта закрывает конкретный конфликт измерителя и предотвращает перенос результата на другой код/профиль; не создаёт дополнительных экспериментов ради заполнения паузы и не меняет старые raw/card результаты.

Статическая проверка изменённых9Markdown-документов:853локальные ссылки разрешаются; указанные в них Markdown anchors найдены; replacement characters не обнаружены. `git diff --check` по каталогу завершился с кодом0. Это проверка документов, не model/runtime acceptance. Замечания адресного reviewer о старом66F и неявной исторической добавке0/4/8исправлены и перечитаны родителем; окончательная версия не называется повторным независимым review всех133опытов.

<a id="delta-through-143"></a>
## 26 октября: расширение до 143 без повторения обучения

**Фаза: READ_ONLY_RESEARCH и запрошенные Markdown-планы.** Source HEAD `b917116b1b85f25df9a9e69a114cae29c336ba6b`. Новые источники 134–143 сопоставлены с уже существующим переносом 1–133. До правок поиск CDG64…67 и новых названий опытов в двух каталогах plans не нашёл их реализации в планах;63 добавляет только отсутствующую интерпретацию/контракты к 59/62. Нового learned алгоритма или FIX-ID по 140–143 не создано.

### Полезная новая дельта и математическое ограничение

Первичные сохранённые dose-curves141–143 пересчитаны `/root/successful_transfer_delta`; их шесть exact paths, bytes и SHA256 зафиксированы в [63§6](63-paired-learning-effect-and-factor-contrasts.md#6-сохранённые-источники-и-будущие-проверки). Проверены unique последовательные lesson IDs, одинаковые denominators между arms, границы correct и размеры массивов; порядок pair IDs сверён с source списками. A141/A142 имеют одинаковые bytes — это replay, не независимые реплики.

- **141:** окно 290…410, макро-среднее пяти fraction по каждому шагу. Δ(A−B): mean+0.0972426280152, range 0.269625447017,117 positive/0 zero/4 negative. Margin: mean−0.0225773716761, range 0.382216930389,53 positive/0 zero/68 negative. Согласованность majority117/121 против 68/121 — измеренный выигрыш полезной оси, но направления majority противоположны. Ни один процент не является доверительной вероятностью. Source65:1385–1404 берёт CorrectTargets/TargetCount из уже выполненного SideScore; одинаковые 14814 scoreForward относятся к сравнению 140/141.
- **142:** Δ(B−A) mean macro fraction+0.208049596110, margin+0.355069162826; обе 121/121 positive. Native outcome по saved raw: Sign 7/21→19/21, ноAbs22/23→11/23, Magnitude7/29→5/29; Negate16/17→16/17. Whole-response exact у всех четырёх false. Это успех Sign и положительный контроль обеих метрик с существенными регрессиями, не безопасная политика ремонта.
- **143:** window 230…290, четыре клетки по 3 пары. C−D fraction+0.150789313904 иmargin+0.051543162820; B−D fraction+0.043509925126 иmargin+0.302953021774. Обе полезные оси сохранены, но individual signs и порядок клеток расходятся. Исправленный A−B относится к имени при Sign-rule, A−C — к правилу при имени Sign. Числа, ordered pairs и все контрасты приведены в 63; универсальный вес имени/правила не установлен.

**Уточнение 139/140:** анализ размаха уровней не заменяет анализ matched difference. Для конечного ряда `Var(A−B)=Var(A)+Var(B)−2Cov(A,B)`; большая общая волна может сократиться. Это алгебраический факт, не предположение независимости. У 140 raw даёт снижение размаха среднего пяти margins до 0.136591053009/0.317981767654, но сотни/≈тысяча пар из отчёта — экстраполяция закона√N на иной показатель. Низкая корреляция не доказывает независимость; median correlations≈.116/.079 относятся к increments171…410, а не к позднему окну 290…410. Не назначать увеличение корпуса или новый прогон по этой оценке. Сравнительные интерпретации 134–138 не восстановлены автоматически 141.

### Две ошибки происхождения 143, не новые результаты обучения

Первая: [report143:54](../../../../experiments-v2/RP01-MICRO/CDG67/results-01/report.md), `CollisionRepairNameVersusRuleProbe.cs:983` и `experiment143.declaredThirdMeasure` неправильно называют A−B вкладом правила. Исправление требуется в будущем receipt/comparator63, не в historical raw. Проценты report143:89–94 без восстановленного определения не перенесены; в 63 отдельно указаны 61 macro и 183 pair-checkpoints.

Вторая: комментарии/декларации о heldout и «A никогда не учило Sign» не соответствуют фактическим `secondCorrectedIds`/routing. Source143:1229–1249,1270–1273,1447–1465 и raw labels дают общие 15corrected-second Sign в обоих arms. У 142 изменены 135 предъявлений, всего правильных Sign 150 вB/15 вA; у 14375 изменённых, всего 90/15. Первое различие полных curve rows —112, поэтому общий префикс 1…111, не 1…170. Во всех raw140–143 осталось stale описание gap35vs23; оно не определяет текущий factor. Старые файлы сохранены; новый manifest обязан брать experiment-specific recipe и actual label→text routing, сохраняя найденное расхождение как provenance.

### Полнота очереди и конкретные места переноса

Индекс имел 57 строк и не содержал уже написанные 59/60/61. Восстановлены существующие TEST-COLLISION-REPAIR, FIX-BWS, FIX-IGC — теперь 60ID,52code candidates/7learning coverage/1E2E. Для BWS/IGC исследователь адресно проверил 12 сохранённых `arm-result.json` (6 наопыт), пересчитал медианы allocations: BWS−164901.6B/update, IGC−625956.8B/update. Внутри каждого опыта совпадают записанные start/final hashes, panels и 10step records, optimizerStep10/state_changed=true. Это проверка полей raw, не новая проверка бинарных weights или optimizer moments. Обе TRAIN строки 33/33 сохранены; встречные time/peak costs остались рядом с выигрышем.

Текущие native цели ещё присутствуют: `DpoTrainer.cs:1103` выделяет softmax scratch, `Tensor.cs:99` копирует initial gradient снаружи; внутренняя owned copy `TensorStorage.cs:23` должна сохраниться. Сами планы 60/61 не переписаны и их budgets не урезаны: у BWS отдельно tiny optimizer applications/contract Backward. Queue вставляет OSR→BWS→IGC→LFT, N63C→TEST-COLLISION-REPAIR→N63R; номера самих опытов не меняет.

Новый 63 назначает чистый `Tests/ExperimentFixes/PairedLearningEffectSummary.cs` и assertions в существующих методах 59, fraction — из уже предусмотренного 62readout. Это конкретный перенос проверки результата настоящего обучения. Product Trainer/DPO/LoRA/router не меняются; итог 141 не является основанием модифицировать их. Стандартные источники N63C/SEP и одноразовая квалификация сохранены:59 —58F/10updates;62 —6F/0updates. Контракт future helper различает общую phase и treatment label: отличие corrected/filler в matchedstep141 законно, не повод отбрасывать точку. Никаких новых модельных вызовов, Facts или test processes ради 63.

### Область текущего исследования

Родитель прочитал адресные текущие governance/skill-разделы, существующие планы, первичные отчёты 134–139, ключи/записи новых JSON, source update/readout и recipe143, затем внёс Markdown-правки. Единственный worker `/root/successful_transfer_delta` (Astra ultra) прочитал 140–143 reports/raw и адресные source, вычислил статистики/hashes, проверил регистрацию и BWS/IGC evidence, затем адресно проверил 63. Исправлены замечания о правильной паре 140↔141 для Forward-cost и различии phase/treatment label. Полный повторный raw-аудит 1–143 и полный semantic review всех host-файлов не заявляются;1–133 имеют прежние evidence pins и планы.

Не запускались модели, обучение, генерация, builds/tests; не менялись product code, runners, datasets, weights, старые experiment reports/cards. Untracked следующий CDG68/карточка 144 и параллельные learning-sets оставлены вне scope. Неразрешённая product-связка HCL/SVD остаётся явно открытой. Доказанный полезный сдвиг не приравнивается к отсутствию любых регрессий или готовности к release.

### Проверка внесённых документов 26 октября

Адресная проверка `/root/successful_transfer_delta` вернула пакет «60 уникальных ID; 143 уникальных номера без пропусков». В нём найдены две устаревшие формулировки: README называл историческую очередь из 57 ID текущей, а строка 134 в карте покрытия оставалась за пределами «текущего диапазона 1–133». Родитель исправил обе и проверил сохранённый текст. Это не повторная независимая проверка исправленных абзацев и не аудит всех первичных результатов 1–143.

Статическая проверка восьми изменённых Markdown-файлов дала 852 разрешающиеся локальные ссылки, включая проверенные Markdown anchors; ошибок UTF-8 replacement characters и незакрытых блоков кода не найдено. Реестр содержит 60 уникальных ID; добавлены ровно FIX-BWS, FIX-IGC и TEST-COLLISION-REPAIR. Относительный порядок прежних 57 ID сохранён, цепочка Next до END не имеет разрывов. В карте покрытия ровно 143 уникальные строки с номерами 1–143. Родитель отдельно прочитал шесть сохранённых dose-curves и подтвердил размеры и SHA256 из плана 63. `git diff --check` по каталогу завершился с кодом 0; HEAD остаётся `b917116b1b85f25df9a9e69a114cae29c336ba6b`.

Изменены только планы 01, 04, 20, 59, карта покрытия 62, план измерителя 62, README и новый документ 63. Проверка ссылок, структуры документов и хешей не является запуском тестов модели или проверкой будущей реализации. В этой работе такие запуски не выполнялись.
