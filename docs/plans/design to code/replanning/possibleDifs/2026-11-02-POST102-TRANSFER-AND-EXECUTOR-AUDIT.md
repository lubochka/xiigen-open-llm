# После P102: проверка исполнения и усиление карточек планирования

<!-- POST107-ROOT-AUDIT-2026-11-02:START -->
## Уточнение после P107 — 2 ноября 2026

Это исторический срез POST102. [Дополнение POST107](codex-2026-11-02-POST107-TRANSFER-AND-EXPERIMENT-AUDIT.md) учитывает P104–P107,ужеисполненный terminal-metric ремонт,новые saved-gate/evaluator дефекты и актуальную очередь 107/23. Нижние прежние статусы остаются исходными наблюдениями, не текущим NEXT.
<!-- POST107-ROOT-AUDIT-2026-11-02:END -->


Дата: 2 ноября 2026. Проверенные срезы: core `15745b357e055055ee9bc58b52cc374d90580961`, planning `d03db13dd8a3d4b6323e9d6e8a452f6323b4addd`. **Аудит и документные исправления выполнены; runtime и новые эксперименты не запускались.** Основные материалы: [аудит120](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/120-post102-transfer-and-executor-audit-2026-11-02.md), [план121](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/121-post102-addressed-repair-plan-2026-11-02.md), [покрытие122](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/122-post102-card-coverage-and-context-2026-11-02.md), [контекст исполнителя40](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/40-effective-executor-context-2026-11-02.md).

**Согласование перед публикацией:** сохранён concurrent core `c8f3908f372d2597c87a553f539eaed5d8748ba6` с результатом P103. Пересечения только в Q-index и required-changes объединены без потери результата. Основной scientific source для P102/P85 остаётся15745b; P103 получил ограниченную входную сверку, не полный новый аудит. Текущие103recorded/27designed/NEXTQ104 отражают именно этот более поздний срез.

## Вывод для планирования

Предыдущие аудиты не следует повторять как список всё ещё отсутствующих исправлений: немасштабированная потеря и оба monitor, BPE outcome, отсутствие выдуманного EOS и MethodKnn distance tiebreak уже есть в текущем core. При этом source implementation не равна приёмке обычного потребителя. У P85 сохранённый35/35 Roslyn oracle подтверждает predictor, но штатная сборка проекта и SemanticCompilerFacade остаются отдельной проверкой.

P102 содержит настоящий native DPO primitive внутри experimental copied bridge и полезный отрицательный результат: на EXAM с20 модель до/после отвечает10. Условная точность позиций36/47→37/47 улучшилась, но свободный ответ остался равен memorized TRAIN baseline. Исполнение не выдумано; заявленная приёмка и часть объяснения не выдержали независимой проверки.

## Какие ошибки требуют изменить задания исполнителю

| Находка | Чем опасна для следующей карточки | Новое обязательство |
|---|---|---|
| Calibration использует Trim==gold, actual result StartsWith | Красивый отчёт калибрует другой алгоритм; неверный хвост проходит | Один callable на controls и actual outputs; положительный пересказ и отрицательное противоречие |
|37позиционных попаданий сопоставлены с7prefix tokens | Две метрики названы одним качеством | Формула, знаменатель, режим, actual/reencoded IDs и semantic verdict раздельно |
|Canonical EXAM47 при cap46 | Полный эталонный ответ не помещается в заданное представление | Проверка вместимости до модели; не повышать budget задним числом |
|Nonzero exit разрешает новый suffix claim | Возможен второй child или повтор уже выданного update | Один exact owner, stage-aware recovery, terminal evidence и ожидание исходного handle |
|Manifest341files пропускает explicit source и project closure | Проверены не все потреблённые входы | Evaluated project instances/Compile/imports и immutable consumed bytes |
|Все изменённые веса названы всеми ненулевыми градиентами | Причина обучения подменена косвенным симптомом | Data gradient, AdamW decay/moments, parameter delta и output раздельно |
|Два DPO ответа объявлены отсутствием влияния на остальные tokens | Игнорируется softmax по словарю | Отсутствие третьего target ограничивает гарантию, но не исключает косвенную производную |
|109.94s успешного runtime скрывают6build/4entry | Сравнение цены не включает неудачи подготовки | All-attempt ledger;284.98s build отдельно; неизвестные полные CPU/allocations — UNKNOWN |

Полная последовательность actual generated IDs не сохранена; имеются первый фактически выбранный ID и reencoded arrays. Все четыре output texts одинаковы. Canonical cap mismatch не доказывает абсолютную невозможность более короткой сегментации и не объясняет раннюю ошибку10 вместо20. Оптимизатор после step1 сохраняется, но moment fingerprint передан null: это NOT_MEASURED, не N/A. Эти границы явно сохранены в отчёте.

## Поступивший P103 и граница его проверки

Перед публикацией появился P103 (`c8f3908f`); его исходники и сохранённые показатели прошли ограниченную входную сверку. Основное0→3получателя и0/3→3/3named answers подтверждено. Однако полная попытка включает29observations вместо12основной панели и15непустыхUnit.Train, отдельно rejected empty call и standaloneBPEfit. Дополнительные вызовы сохранены в учёте, не объявлены соблюдением исходного общего бюджета.

«Потеря памяти при продолжении» не доказана: host создаёт два независимых объекта с разнымиID и не проверяет первый после создания второго. В source есть пересборка состояния factory, но она не равна измеренному forgetting старого объекта. R12 плана121 и supplemental intake ограничивают вывод без повторного обучения. Основной научный аудит P102/P85 остаётся привязан к15745b; P103 не объявляется столь же полно перепроверенным.

## Исправленные исходные постановки planning

| Файл | Адресное усиление |
|---|---|
|[02](02-DPO-DATA-OBJECTIVE-GRADIENT.md)|Admission, пригодность пары, recipient и actual shared scorer |
|[03](03-DPO-PAIR-AND-GRADIENT.md)|Softmax/decay, разные метрики, причинный предел |
|[10](10-INFERENCE-STOP-TOKENS-AND-UNIT-TRANSFER.md)|Текущий nullable EOS, вместимость и actual token trace |
|[13](13-CAUSAL-LEARNING-PARITY-PROTOCOL.md)|Одинаковые arms/state, actual loader, полный расход/recovery |
|[17](17-HISTORICAL-E13-TRAIN-VALIDATION-BOUNDARY.md)|P91 не воспроизводит архивный E13 автоматически; split/identity/default k |
|[33](33-NEW-UTF8-DECODER-CUTOFF-2026-10-16.md)|Trim хвоста не равен догенерации символа или смысловому качеству |
|[38](38-MICRO-E13-DOMAIN-CAUSAL-PROTOCOL-2026-10-16.md)|Один фактор, полный set scorer, границы proxy/native/consumer |

Это семь исторических постановок с новым адресным блоком, не семь новых опытов. Их прежнее тело сохранено; дозы и PRIMARY не созданы. В core обновлены44экспериментальные карточки, включая все27будущих Q104–Q130, и12карточек переноса. Отдельно исправлены8ссылок в трёх новых карточках. Цифры покрытия описывают документные изменения, не число повторно исполненных опытов.

## Приоритетный остаток продукта

1. **R01–R04:** единый оценщик, корректный decoder receipt, единственный owner попытки, реальные build inputs и полный отчёт. Saved outputs/логи переоцениваются без Train; historical raw не переписываются.
2. **R06:** Translation batch должен либо опубликовать весь accepted state, либо оставить S0. Heldout evaluation после mutation и поздние failures требуют общей transaction boundary; один JSON preflight её не доказывает.
3. **R07:** законность/source threshold проверяются до cap и остановки tier. Пятый незаконный кандидат не должен скрывать шестого законного; недопустимый full tier не должен прятать законный prefix.
4. **R08:** unknown ID и обломок UTF-8 видны strict consumer даже при дословно верном тексте. Русские multi-token символы остаются допустимыми; fabricated EOS уже убран.
5. **R09:** weighted trajectory average остаётся полезным, но publication gate обязан получить отдельно измеренную final-state метрику или явное отсутствие.
6. **R05/R10:** принять нынешний MethodKnn tie fix штатным путём; separately проверить scoped expression target, который ordinary P83 не закрывает. Другие KNN не исправляются автоматически по сходству названия.

У каждого пункта121 есть owner files, negative witness, retained positives и условия завершения. Это план будущего разрешённого исполнения, а не обещание, что код уже исправлен.

## Действующий контекст и очередь

Один пакет40 связывает архитектора, планировщика, plan-review, code-examination, generated review, experiment design, prompt authoring, test integrity и человеческое объяснение с наблюдаемыми решениями. Недостаточно отчитаться «прочитал skills»: исполнитель должен назвать ошибку, фактический callable/consumer и ожидаемый провал негативного примера. Частичное чтение навыка не маскируется полным.

Очередь core теперь **130Q,103recorded,27designed, nextQ104/P104-PRIMARY-01**. Q127–Q130 опубликованы в `15745b35`; прежний вывод POST89 о неопубликованных файлах ограничен своим SHA. P102 recorded не равен PASS. Аудит не переводит Q104 в RUNNING и не разрешает повтор уже оплаченного Train при сбое отчёта.

## Проверка этой работы

Свежая работа: чтение трёх новых core commits и72changed files, hashes полученных bytes, source review12owners, Python пересчёт P102, независимый review core-пакета, документные изменения и статическая сверка. Авторский review семи planning вставок не выдаётся за независимый review этих же вставок; корневой исполнитель проверяет их отдельно. Свежие native/.NET/PowerShell/model/Train вызовы:0. Сохранённый35/35 — прежний авторский oracle. Remote file census не является полной научной проверкой всех275карточек.

Артефакты метода: [evidence pack](https://github.com/lubochka/llm_mvp_core/tree/codex/dpo-implementation/experiments-v2/_audits/2026-11-02-post102-executor-audit). Публикация связывает оба репозитория без перезаписи чужих конкурентных изменений; после записи требуется проверить remote ref и содержимое конечного коммита.
