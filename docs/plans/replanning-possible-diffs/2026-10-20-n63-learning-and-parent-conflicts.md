# N63: обучение, восстановление и родитель следующего запуска

**Исторический срез до уточнения Любы о незавершённом №08.** Его очередь42/NEXT10 N63C и gap-only P02.PARENT/PC01/H32 больше не действуют. Ошибка решения и замена описаны в [текущем исправлении](2026-10-20-parent-repair-and-corrected-08.md): PARENT-ADOPTION-01 → N63-INGRESS-LENGTH-LEARNED-01 → N63C; учебная цель№08 OPEN. Ниже сохранены прежние evidence/receipt и история вывода, не текущий work order. Квитанция этого среза не проверяла последующие изменения.

20 октября 2026. Фаза: READ_ONLY_RESEARCH + исправление документов только в C:/Projects/llm_mvp_core/docs/plans/replanning-possible-diffs. Опытов, inference, тестов, сборок, публикации checkpoint и Git-записей в этой работе не было.

## Очередь прежнего среза — заменена

[Индекс](experimets/cards/00-execution-index.md) и [машинный реестр](2026-10-20-card-reconciliation/0ea-execution-contract.json) учитывают сохранённый №09. Всего42 уникальных ID:7 прежних завершённых,2 consumed,33 ещё не использованных. №08 и №09 не запускаются повторно; следующий неиспользованный — №10 N63-CORRECTION-COLLISION. Номер не подтверждает готовность отдельной оси измерения.

Паспорта [№08](experimets/cards/N63-ingress-length.md) и [№09](experimets/cards/N63-replay-capacity.md) указывают точные исходные данные, сохранённые результаты, счёт обновлений и ограничения. Старые команды №09 отозваны, его future-output удалён из реестра. Отдельные probes не добавлены в очередь и не замаскированы под бесплатный readout. Исполнитель использует модель своей сессии по умолчанию; архитектура Student закреплена отдельно.

## Разрешённые противоречия

| Утверждение из присланного материала | Первичные данные и исправление |
|---|---|
| Два шага — заданный пользователем предел | Это прежний авторский рецепт. Минимальный набор данных и минимальное число updates — разные требования. Число1000 — первая выбранная успешная точка отдельной пробы, не установленный минимум |
| 0→0 означает сломанный алгоритм или успешное обучение благодаря меньшему NLL | Учебный вердикт при отсутствии правильных response-токенов — LEARNING_VERDICT_INVALID_ZERO_TO_ZERO; NLL/rank и механические результаты остаются отдельными наблюдениями |
| №09 ещё предстоит запустить | N63R/results/n63-ar-cap.json и execution.claim уже существуют. Исходные9updates/32readouts сохранены; повтор запрещён |
| Проба №09 стоит столько же, сколько исходный опыт | N63R-PROBE отдельно выполнила4600updates и40scoring forwards; это иной schedule с третьим плечом |
| B требует на50% больше времени и памяти | По raw B/A:updates+50%, входные токены+86.79%, allocated+106.77%, wall+131.86%; один порядок A→B→C, causal resource effect не установлен |
| Все Step1/parent=null — необученные; native принятие отсутствует | Первый обычный обученный root тоже может иметь Step1/parent=null. Publish, LoadLatestCompatible, PrepareStart и recovery существуют в исходниках; специальная роль InitialBaselineNoUpdate определяется отдельно |
| Любой сохранённый SFT binary можно сделать native parent | Нынешний native manifest требует настоящую DPO applied identity и source continuation bundle. У H16/H32 SFT-проб такого доказательства нет; подставлять фиктивные preference rows нельзя |
| Cold restore без Train не относится к обучению | [P25](experimets/cards/P25-cold-restore.md) исправлен: чтение ранее обученного состояния штатным consumer относится к обучению,0новых updates допустимы |

Присланные выводы проверены по raw, runner source и native API. Старые внешние report/candidate вне разрешённой папки не переписывались. Их ошибочные выводы заменены в действующих карточках и одной записи [required-changes](required-changes.md#n63-ar-cap).

## Что действительно выучено и сохранено

H16 RESTORE-MINIMAL-SET: TRAIN3/40правильных response-токенов после1000updates; после fresh-object reload та же полная raw-панель; после ещё200updates —4/40. Полных точных ответов0/2, EXAM correct0. Parent и исследователь отдельно сверили полное равенство сохранённых панелей; более узкий bool из runner не выдаётся за эту проверку. Import=true/step1000 и state hash не доказывают побитную идентичность всех m/v.

H32 N63R-PROBE: общий WA имеет45/92правильных response-токенов на A01/A07,0/2полных точных ответов. У A07/49 значение22 стало A2/B15/C4; EO02/49:15→1/15/3. При этом A01/43:23→2/1/2; новые N01/N02 хуже у B. Все40readouts имеют whole-response exact=false. Это ограниченное удержание частичного поведения, не общее восстановление алгоритмов.

W0-файлы исходного №09 и пробы совпадают по bytes. Это прочитанные сохранённые файлы, не новый запуск. C выравнивает число updates с B, но меняет curriculum и token volume; общий причинный вывод из этого сравнения не следует. TrainedTokens в Program.cs считает sequence.Count; отдельно записаны shifted targets Count−1.

## Бывшая ближайшая карточка — теперь №12

[N63-CORRECTION-COLLISION](experimets/cards/N63-correction-collision.md) сохраняет точныеT1/Q1/Q2,canonical B0 и4updates/36readouts. Прямо записаны simulation-owner/capability, native publication callbacks, optimizer/RNG sidecars и единый N63C/results; retired N62 не служит исполняемой инструкцией. ПромежуточноеS1 позволяет разделить влияние DPO и следующего SFT. Сравнение margin, отдельных токенов и целого ответа имеет разные вердикты. Отдельный pretrained checkpoint и дополнительное обучение не назначены. Исследователь проверил native API и отсутствие host; будущая CLI не выдаётся за собранную.

## Прежний незавершённый дизайн SFT-parent — заменён карточкой ремонта

[Primary evidence](2026-10-20-parent-adoption-primary-evidence.md) содержит точные native paths/строки и границы проверки. [Контракт29](experimets/29-learned-start-and-parent-continuation.md) и [P02.PARENT](2026-10-20-card-reconciliation/parent-continuation-contract.json) назначают H32 WA, точные weights/optimizer и будущий изолированный PC01/results. Модель, checkpoint или каталог исполнителю выбирать не нужно.

Статус NEEDS_TRUTHFUL_SFT_PARENT_CONTRACT относится к новому native SFT-parent дизайну. До исполнимой команды автору нужно замкнуть честную SFT publication schema, полный continuation source bundle, private owner/entry и ограниченное сравнение. Entry отсутствует; рабочая команда не выдумана. Это не доказательство поломки native DPO и не запрет Unit/kNN/BPE или P25. Native START в исследовании не вызывался: его подготовка может менять custody.

Для15 будущих нейронных сравнений статус DESIGNED_UNQUALIFIED_LEARNING_SIGNAL отделяет точный проект от доказанного результата. Acquisition и ресурсные измерения из назначенного старта не требуют заранее обученного checkpoint. Retention/restore вывод требует положительного before; иначе RETENTION_NOT_ESTABLISHED, при0→0 учебный вердикт invalid. Это ограничение вывода, а не новый запрет запуска. Конкретные source/host/entry сохранены; дополнительный Train, поиск альтернативы и обучение до PASS не назначены.

## Охват и происхождение проверки

Срез Git: core81141d57b847bc050881c2fabba8302ac18c0f8a; planning0ea8f0b0094e234ffe7734adfa95e2a9e53203f5. N63R/N63R-PROBE обнаружены среди внешних runtime-артефактов; их наличие не выводилось только из HEAD. Поздний EXISTS_OR_UNKNOWN в cycle.log не доказывает единственность всех прежних попыток.

[Текущая квитанция](2026-10-20-card-reconciliation/n63-learning-parent-readback.json):64text/source/log файлов, включая последнее вложение;13структурированных файлов полностью разобраны. Дополнительно прочитаны bytes6binary файлов только для SHA256/размера, без десериализации модели. Это файловый охват, не64смысловых review всех транзитивных зависимостей. Прежние пакеты остаются учтены в [реестре сверки](2026-10-20-card-reconciliation/README.md); их исторические квитанции не объявлены заново полученными результатами.

Исследователь /root/rp01_primary_evidence вернул [пакет первичных источников](2026-10-20-parent-adoption-primary-evidence.md):39textsource в его перечне,22совпадения с закреплённым Git; overlap с родительским чтением не суммируется. В отдельном ограниченном review он целиком прочитал протокол24, контракт29 и паспортN63R, адресно P25; сверил Program.cs318строк, NativeLearner239строк, csproj и полный JSON N63R-PROBE. Его три замечания — consumed NEXT/observer, знаменатели и scope learned-start — применены в документах. После применения исследователь вернул [итоговый адресный readback пяти документов](2026-10-20-parent-adoption-primary-evidence.md:180): N63C адресно, протокол24/контракт29/N63R полностью, P25 адресно; существенных остатков в этом срезе не найдено. Дополнительно подтверждена source grammar N63C-A/B. Это не одобрение всего корпуса и не runtime acceptance.

Статическая проверка охватила324Markdown,163карточки,133встроенных P-prompts и8отдельных prompts;663JSON разобраны. Missing local links и нарушения проверенных default-model/STOP/NEXT правил не найдены. Объём и результаты фиксируются в квитанции. Она не подтверждает компиляцию будущих hosts, ABI или результат обучения. Продукт, реальные hosts, datasets, weights, raw, AGENTS и skills не изменялись этой работой.

