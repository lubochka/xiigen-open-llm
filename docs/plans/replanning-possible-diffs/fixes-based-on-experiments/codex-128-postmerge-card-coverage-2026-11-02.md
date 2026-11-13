# Покрытие ремонта после local merge

**2 ноября2026.** Основные документы: [аудит126](codex-126-postmerge-transfer-and-experiment-audit-2026-11-02.md), [план127](codex-127-postmerge-addressed-repair-plan-2026-11-02.md), [проверки исполнителя42](../experimets/codex-42-postmerge-executor-checks-2026-11-02.md).

## Разные виды работы и предел заявления

| Вид работы | Объём | Что подтверждает |
|---|---:|---|
|Структурный проход основных каталогов|432файла|Проверка наличия конфликтных строк и состава scoped tree; не полная научная аттестация432текстов|
|Первоначальная сверка конфликтов3cd|179файлов/795marker lines|130экспериментальных карточек,36fixes и13root/contract/report файлов; к735 значительная часть уже исправлена другим коммитом, ниже учтено согласование с ним|
|Новое адресное усиление экспериментов|32карточки|9ранее записанных опытов, включая остатокP104, и все23позиции первоначальной очередиQ108–Q130; после публикацииP108 это10записанных и22будущие позиции|
|Усиление переноса и договоровcore|7файлов|Договор теста, verifier, memory, publication, bundle,118и124|
|Адресное усиление planning|8карточек|Search,semantic,restore,inference,provenance,dispatch,key drift,router acceptance|
|Навигацияcore|8файлов|Один текущий срез126/127, прошлые NEXT подписаны историческими|

Эти множества пересекаются.179+32+7 нельзя выдавать за число уникальных изменённых файлов или выполненных экспериментов. Пофайловый состав и конечные hashes находятся в публикационном manifest пакета доказательств.

## Экспериментальные карточки

В каждой строке новый раздел содержит конкретное неправильное поведение и положительный/отрицательный критерий. Согласованный исходный текст после reconciliation с735 восстанавливается точным удалением единственной новой вставки; старые before/after hashes этапа3cd сохранены отдельно. Учебные бюджеты и raw не редактируются.

| Карточка | Новое адресное требование |
|---|---|
|[P53-batch-atomicity.md](../experimets/cards/P53-batch-atomicity.md)|M04 — отказ поздней строки и видимость опубликованной версии|
|[P66-heldout-contrast.md](../experimets/cards/P66-heldout-contrast.md)|M05 — допуск, посещение поиска и фактическая работа|
|[P76-utf8-boundary.md](../experimets/cards/P76-utf8-boundary.md)|M02 — законный префикс и законченный ответ|
|[P99-unknown-token-id.md](../experimets/cards/P99-unknown-token-id.md)|M02 — диапазон ID отдельно от границы символа|
|[P101-learning-budget.md](../experimets/cards/P101-learning-budget.md)|M02 — однотокенная оснастка не задаёт весь алфавит|
|[P102-teacher-forced-decoding.md](../experimets/cards/P102-teacher-forced-decoding.md)|M02/M04 — одинаковая единица сравнения и реальная приёмка|
|[P106-corrected-semantic-target.md](../experimets/cards/P106-corrected-semantic-target.md)|M03 — после загрузки отвечает именно целевая машина|
|[P107-lawful-neighbor-aggregation.md](../experimets/cards/P107-lawful-neighbor-aggregation.md)|M05 — посещённые кандидаты отдельно от конечного победителя|
|[P108-metadata-carrier.md](../experimets/cards/P108-metadata-carrier.md)|M03/M04 — runtime carrier доходит до того же обученного читателя|
|[P109-gold-validity.md](../experimets/cards/P109-gold-validity.md)|M04 — правильный эталон не зависит от имени порчи|
|[P110-numeric-matrix-cost.md](../experimets/cards/P110-numeric-matrix-cost.md)|M05 — реальные вычисления вместо размера панели|
|[P111-reward-credit.md](../experimets/cards/P111-reward-credit.md)|M03/M04 — correction обновляет назначенного получателя|
|[P112-family-disjoint-evaluation.md](../experimets/cards/P112-family-disjoint-evaluation.md)|M03/M05 — split подтверждается фактическими учебными строками|
|[P113-feature-energy.md](../experimets/cards/P113-feature-energy.md)|M02/M05 — исходная энергия отдельно от двух назначенных L2-политик|
|[P114-zero-weight-pair.md](../experimets/cards/P114-zero-weight-pair.md)|M04/M05 — нулевой вес и фактический вклад пары|
|[P115-bm25-statistics-cache.md](../experimets/cards/P115-bm25-statistics-cache.md)|M03/M05 — cache принадлежит фактически обученной версии corpus|
|[P116-e43-file-boundary.md](../experimets/cards/P116-e43-file-boundary.md)|M03/M05 — потреблённые пары сохраняют границы исходных файлов|
|[P117-translation-direction-memory.md](../experimets/cards/P117-translation-direction-memory.md)|M03/M04 — назначенный reload сохраняет направление и источник|
|[P118-projectwiki-content.md](../experimets/cards/P118-projectwiki-content.md)|M03/M04 — actual plan body связан с назначенной машиной и режимом|
|[P119-learning-dynamics.md](../experimets/cards/P119-learning-dynamics.md)|M03/M05 — одна продолжаемая траектория и полная стоимость|
|[P120-semantic-compression-laws.md](../experimets/cards/P120-semantic-compression-laws.md)|M03/M05 — not/no доходят до счётной памяти и Predict|
|[P121-project-adapter-selection.md](../experimets/cards/P121-project-adapter-selection.md)|M03/M05 — назначенный project и фактический получатель update|
|[P122-root-channel-unicode-digits-operators.md](../experimets/cards/P122-root-channel-unicode-digits-operators.md)|M02/M05 — root и реально потреблённый сигнал различаются|
|[P123-design-conversion-critic-evidence.md](../experimets/cards/P123-design-conversion-critic-evidence.md)|M03/M04 — critic потребляет actual geometry назначенного prototype|
|[P124-p3-fixed-discriminator-contract.md](../experimets/cards/P124-p3-fixed-discriminator-contract.md)|M03/M04 — feedback меняет текущую память и сохраняет старый закон|
|[P25-cold-restore.md](../experimets/cards/P25-cold-restore.md)|M03 — отдельный обученный повтор читает восстановленный target|
|[SAVED-STATE-PANEL-0407-01.md](../experimets/cards/SAVED-STATE-PANEL-0407-01.md)|M03 — замороженная панель и правильный экземпляр|
|[KNN-ORDER-PERMUTATION-01.md](../experimets/cards/KNN-ORDER-PERMUTATION-01.md)|M03/M05 — выученный порядок доходит до назначенного графа|
|[PHRASE-COVERAGE-CAP-01.md](../experimets/cards/PHRASE-COVERAGE-CAP-01.md)|M03/M05 — реальный обход phrase cap и выбранные spans|
|[HELDOUT-ADMISSION-ATOMICITY-01.md](../experimets/cards/HELDOUT-ADMISSION-ATOMICITY-01.md)|M03/M04 — failure-state проверяется до исправления, фактор остаётся preflight|
|[METHOD-KNN-SCOPED-EXPRESSION-TARGET-01.md](../experimets/cards/METHOD-KNN-SCOPED-EXPRESSION-TARGET-01.md)|M03/M05 — extracted target доходит до прямого native Predict|
|[P104-law-eligibility.md](../experimets/cards/P104-law-eligibility.md)|S01 — личность строки и победитель в сохранённом наборе|

## Договоры переноса

| Документ | Новое требование |
|---|---|
|[03-test-and-execution-contract.md](03-test-and-execution-contract.md)|После merge: тестовый договор и результат исполнения|
|[11-translation-verifier-admission.md](11-translation-verifier-admission.md)|После merge: миграция ключа не теряет обязательные слова|
|[19-translation-memory-coverage.md](19-translation-memory-coverage.md)|После merge: найденная память и восстановленный потребитель|
|[90-review-and-publication.md](90-review-and-publication.md)|После merge: проверяется опубликованный результат слияния|
|[101-persistent-state-bundle-contract-2026-10-30.md](101-persistent-state-bundle-contract-2026-10-30.md)|После merge: чтение bundle не заменяется новым обучением|
|[118-post101-addressed-repair-plan-2026-11-02.md](118-post101-addressed-repair-plan-2026-11-02.md)|Уточнение A101-R2: полный Unicode вместо singleton фильтра|
|[codex-124-post107-addressed-repair-plan-2026-11-02.md](codex-124-post107-addressed-repair-plan-2026-11-02.md)|Текущий адресный остаток после local merge|

## Связь с planning

В [новом исходном аудите planning](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/codex-2026-11-02-POSTMERGE-TRANSFER-AND-EXPERIMENT-AUDIT.md) согласованы04/05/06/10/12/17/28/30. Они содержат адресные требования той же M02–M05 логики. Две недоступные локальные ссылки README заменены на реально опубликованные120. Исторический POST107 не переписан как новый прогон.

## Как проверена сохранность

Независимый просмотр проверил все179 решений merge по исходным и разрешённым хешам, оба массива полезного текста, code fences, таблицы130карточек и специальные101/103/109/40/PHRASE решения. Финальный P103 blob точно равен исправленному f3-отчёту. Ошибка одного транспортногоLF в локальной исходной копии была обнаружена независимой проверкой и исправлена до приёмки; исходныйblob снова совпал с GitHub.

Последующие новые вставки имеют отдельные before/after hashes и точный текст в core-hardening/navigation-changes. После прихода735 все184 изменённых Markdown сверены с точными Git blobs и включены в reconciliation; тела новых вставок проверяются уже против согласованного текущего основания. Предыдущие before/after сохранены как pre_reconciliation, отдельный concurrent-reconciliation.json описывает каждый выбор. Новые документы126/127/128/42 проверяются отдельно. Правка Markdown и успех Python пересчёта не объявляют product native gate зелёным.

Применение навыков — архитектурные решения и владельцы127, self-validation цепочки producer→state→consumer, различающие отрицательные случаи, ревизия фактических тестовых contracts, PM-C01–09 с неправильным/правильным примером, русские пояснения. Количество файлов или написанных чеков не превращается в количество независимых reviewer passes.

## Проверка применимости и параллельных изменений

В независимом review первого драфта обнаружено18 новых вставок, переносивших чужой механизм по названию карточки. Все18 исправлены до публикации: actual recipient/factor/calls/budget берутся из самой PRIMARY. P122 остаётся ASCII NormalizeRoots с2 updates/8 Generate; P120 — not/no в BigramModelV2; P121 — выбранный DPO recipient; Q129 — syntax-preflight A/B с ожидаемым частичным изменениемA. Иные продуктовые проверки остаются у своего M04, а не становятся дополнительными PRIMARY calls. История исправлений и ограничения роли reviewer, который затем редактировал эти18 вставок, раскрыты в hardening-review-revisions и отчётах review. PM-C09 запрещает повторять эту ошибку.

Из735 сохранены126 уточнений полного чтения навыков, новые finalizer/semantic controls и действующие оговоркиQ103–107. Отдельно согласованы владелецPHRASE109, единая цель102, PARTIAL103 и историческиеNEXT. Новая строкаP104 отражает реально выполненную частьS01 и конкретные остатки; её первичные данные не переписаны. Числа179/801 другого контекстного отчёта не противоречат нашим179/795: тот scope включает9маркерных строкprompts/P-SERIES, наш —3строкиотчётаP103; наборы разные. Итоговый publication manifest считает только действительную разницу с735.

**Поздний срез066725d8:** после первого reconciliation сохранены ещё7 адресных Markdown-правок (отделение meta-finalizer и фактических Verify calls), новыйP108 проверен отдельным источниковым пересчётом. Q108 записан с ограничениями, текущий счёт108/22 иNEXT109; старые107/23 остаются историческими. Итоговые hashes учитывают оба этапа, а их отдельные receipts не выдаются за проверку более поздних bytes.
