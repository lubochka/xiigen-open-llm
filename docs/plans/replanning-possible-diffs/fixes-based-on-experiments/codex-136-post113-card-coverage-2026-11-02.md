# После P113: где усилены карточки и чем проверять исправление

**Поздняя публикационная база — 3499063:** автор следующего коммита уже сообщает о продуктовом исключении zero-weight пар; три новые нативные проверки остаются UNRUN. Этот код сохранён, но его реализация данным аудитом повторно не аттестована. R08 теперь задаёт приёмку появившегося переноса; дефекты экспериментальной копии P114 остаются отдельными. Научный срез и проверенный M03 pin не расширяются. Подробности — в публикационной сверке.

## Последняя сверка исходника перед публикацией — 1aaa3bc

Поверх b8c76da появился коммит `1aaa3bcde570b393e012c6f63dd3c31940a9e7ae` с реализацией M03. Канонический основной builder, отдельный PairId, ряд readers/imports, отказ коллизий и удаление PAIR-обхода floor уже есть в исходнике. Повторно назначать их как отсутствующие нельзя. Все шесть нативных проверок автор оставил UNRUN; version/default, достижение кандидата N16, RULE и поведенческая приёмка остаются открыты. У N15 исправлен основной builder-case, у N17 — прежняя причина раннего PAIR-отказа; это SOURCE_IMPLEMENTED / NATIVE_UNRUN.

В той же узкой сверке выявлена **N18**: новый collision-preflight вызывает TryGetProperty на `[]`/`null` и выпускает InvalidOperationException вместо прежнего управляемого typed-контракта. Ремонт включён в R09. Всего пакет содержит 18 находок на трёх закреплённых срезах, с раздельным текущим статусом. Научная часть по-прежнему ограничена 21007e25/b8c; Q129 и прогноз 39 AddPair не изменились. Подробности и шесть source hashes — в [публикационной сверке](../../../../experiments-v2/_audits/2026-11-02-post113-audit/publication-reconciliation.md).

**Проверенные срезы:** основной — core `21007e25ec3f6f6634dd8733fc8631ae267e518c`, planning `5fc34f3c19a12d46b7e48707aea89c28b6ec32a0`; дополнительный — core `b8c76da72257d07fd13c11353de78f9cae355415`, planning `ef93337f29f763ace9e97f09ec701575a0b47a23`. Дата: 2 ноября 2026. Основание — [аудит 134](codex-134-post113-transfer-and-experiment-audit-2026-11-02.md); выбранные решения — [план 135](codex-135-post113-addressed-repair-plan-2026-11-02.md).

Изменены 25 экспериментальных карточек, 12 документов переноса (включая новые владельцы 130–133), действующий план 127, README/реестр/индекс и два общих договора. В planning согласованы две дельты, четыре исторические предметные карточки и навигация. Маркер новых текущих блоков: `POST113-CURRENT-2026-11-02`. Это содержательные требования, а не исполненные новые тесты.

## 1. От находки к владельцу ремонта

| Ошибка | План | Критерий, исключающий повторение |
|---|---|---|
| N01, два writers от одной базы | R01 | Реальный transaction gate всех writers; оба updates/номера сохранены, снятие gate делает тест красным |
| N02, оценка старого состояния у нового | R01 | Measured identity записана; stale result не меняет quality нового state |
| N03/N04, build или наличие exit вместо результата | R02 | Stage/attempt schema, failure/malformed не дают 0, callback ровно назначенное число |
| N09, surviving grandchild скрыт timeout | R02 | Создание PID/start identity наблюдалось, потомок действительно завершён; missing/живой не PASS |
| N05, NaN сравнение проходит | R03 | Finite inputs/tolerance/computed errors перед сравнением; lawful zero сохранён |
| N06, Content подменён при старом hash | R04 | Hash пересчитан из consumed bytes, затем проверен frozen source/family; materializer читает тот же immutable payload |
| N07, третий owner при counts 2/2 | R05 | Exact sets/keys/payload owner_id; законный unknown-owner отказ остаётся разрешённым отказом без update |
| N08, неверная finite координата/fraction | R06 | Поэлементное равенство к независимому expected, энергия из проверенных координат, границы missing state |
| N10–N14, P114: NaN, all-zero optimizer, finalizer, бюджет, deadline | R08 | Finite guard; verifier влияет на итог; все 16 дополнительных Forward учтены; timeout исполняется; all-zero проверяется на настоящей границе optimizer |
| N15–N17, частичная миграция M03 | R09 | Версия соответствует payload; реальный кандидат доходит до floor; schema и restored-reader проверяются раздельно; lawful RULE сохраняется |
| N18, исключение collision preflight на non-object JSON | R09 | ValueKind до TryGetProperty; object сохраняет полезный Pattern, [] даёт управляемый отказ, null сохраняет прежний typed skip/bookkeeping |
| Старый Q129 oracle требует исправленный баг | Тело Q129 и R01/R07 | После malformed на нынешнем staged A/B живое S0; после corrected6/6; обработанный урок и опубликованный различаются |

## 2. Двадцать пять конкретных экспериментальных карточек

Каждая строка ниже соответствует добавленному адресному блоку в указанном файле. Сохранены прежние gold, данные и порядок; дополнительные модельные вызовы не назначены. Отрицательная порча применяется к копии receipt/массива, если строка прямо не относится к будущему отдельно разрешённому native gate.

| Карточка | Честный положительный случай | Порча, которую должен обнаружить тот же проверяющий | Граница |
|---|---|---|---|
| [P104](../experimets/cards/P104-law-eligibility.md) | Исходные E1–E5 с прежними rank1 | Подмена полного lesson/payload или winner при прежнем числе строк |9 repaired pairs не закрывают 16 старых условий; ordinary consumer отдельно |
| [P107](../experimets/cards/P107-lawful-neighbor-aggregation.md) | Lawful A=B и valid required controls | Тот же pool/count, другой winner/payload/required verdict | S02 выполнен; budget51/50 сохранён |
| [P108](../experimets/cards/P108-metadata-carrier.md) | Законный carrier/null-refusal; dummy build→run1callback | Failed terminal после build; живой или ненаблюдённый grandchild | Missing consumed IDs остаются NOT_OBSERVED;2 Train/64 observations сохранены |
| [P109](../experimets/cards/P109-gold-validity.md) | Полный ответ по прежней rubric | Неверное число/хвост, failure/malformed exit.json | Probabilities отдельно от 0/3; reencoded IDs не generated |
| [P110](../experimets/cards/P110-numeric-matrix-cost.md) | Полный finite output в прежнем допуске | NaN/Infinity/tolerance/finite coordinate | Speed OPEN; adapter не ordinary Predict; нет полного saved weights |
| [P111](../experimets/cards/P111-reward-credit.md) | Исправляется выбранный U2, U1 сохранён | AfterU3 при counts 2/2, чужой payload | Product mapping/restore OPEN,1/2→2/2 подтверждено локально |
| [P112](../experimets/cards/P112-family-disjoint-evaluation.md) | Исходные ledger hashes и frozen split | TEST Content со старой или новой hash в TRAIN | Исторические hashes верны;2/4 и изменённый label balance раскрыты |
| [P113](../experimets/cards/P113-feature-energy.md) | Правильные finite компоненты/энергия | Fraction0,5→0,9 и finite permutation |4/4 у обоих; raw halves/tokenizer не восстановлены из подписей |
| [P114](../experimets/cards/P114-zero-weight-pair.md) | Сохранённые веса и optimizer B совпадают с P-only C по квитанциям и Git blob identity | Q возвращён в denominator; NaN; обязательный false/missing control не может дать PASS | Результат уже записан; 3 updates, 4 processed/5 raw, 24 generation + 24 scoring; ещё 16 Forward вне основного бюджета; R08 не назначает повтор |
| [P115](../experimets/cards/P115-bm25-statistics-cache.md) | Cache статистики того же actual corpus | Новый document со старым hash; один неверный count | Preparation/reader windows раздельны, дополнительного Fit нет |
| [P116](../experimets/cards/P116-e43-file-boundary.md) | Законные file-local transitions | Переход через границу F1/F2 при прежних counts | File identity→token stream→counts |
| [P117](../experimets/cards/P117-translation-direction-memory.md) | Свои direction/topic/state_version | Чужая version/direction или extra owner | R01 concurrency отдельно от научного A/B |
| [P118](../experimets/cards/P118-projectwiki-content.md) | Выученный body достигает назначенного плана | Правильный ID/hash, но чужой body | style_source не заменяет содержимое |
| [P119](../experimets/cards/P119-learning-dynamics.md) | Собственная одна ось из того же S0 | NaN или метрика чужого checkpoint | B/A и B/свой start раздельны; нет лишних seeds/restarts |
| [P120](../experimets/cards/P120-semantic-compression-laws.md) | not/no и save/load через BigramModelV2 | Чужой next-word или межфайловая пара |2 count-fit×4 файла/20 Predict; vector нормировка не добавлена |
| [P121](../experimets/cards/P121-project-adapter-selection.md) | Разрешённый adapter нужного проекта | Extra adapter при прежнем count/project mismatch | Selected/attempted/updated раздельны |
| [P122](../experimets/cards/P122-root-channel-unicode-digits-operators.md) | Реальный Signal исходных ASCII T1/E1 | Finite component/root lineage подмена |2 updates/8 Generate; NO_CONTRAST допустим, Unicode corpus не добавлен |
| [P123](../experimets/cards/P123-design-conversion-critic-evidence.md) | Geometry actual prototype по frozen rubric | Координата или NaN при прежнем summary/hash | Critic и QA aggregate раздельны |
| [P124](../experimets/cards/P124-p3-fixed-discriminator-contract.md) | Новый закон читается, старый сохранён | Изменённый payload/output map при старом hash | Discriminator/gold не подбираются после результата |
| [Q125/P25](../experimets/cards/P25-cold-restore.md) | Ordinary reader нового loaded target | Source-instance вместо target или подменённая map |8 уроков/72 Flow; исторический 0 Train не закрывает |
| [Q126, saved panel](../experimets/cards/SAVED-STATE-PANEL-0407-01.md) | Тот же S0 и frozen panel | State payload/чужая metric при прежнем count |14DPOupdates по полному договору 38 |
| [Q127, order](../experimets/cards/KNN-ORDER-PERMUTATION-01.md) | Input order→predicates→vector→steps | Finite permutation/steps swap при прежней норме |3 fit/30 уроков/36 Execute, новая L2 не второй фактор |
| [Q128, phrase cap](../experimets/cards/PHRASE-COVERAGE-CAP-01.md) | Реальные spans и eligible-before-cap | Чужой candidate/payload при тех же counts |1batch/29 уроков/64 flow+17composer;843/7 прогноз |
| [Q129, held-out](../experimets/cards/HELDOUT-ADMISSION-ATOMICITY-01.md) | После malformed живое S0; corrected и reload2/2 | Partial live publication/подмена retained keep |4batch/4 processed lessons/20 flow+2internal; oldA9/B6 oracle исправлен |
| [Q130, target scope](../experimets/cards/METHOD-KNN-SCOPED-EXPRESSION-TARGET-01.md) | Extracted body именно target→nativePredict | Nested lambda/local expression либо stale content hash |4 Train/32 пар/32 Predict,k1/k5; прогноз не измерение |

### Особое исправление Q129

Одного нового предупреждения сверху было недостаточно: основное тело карточки всё ещё требовало `hoa-new` в A после failure и финальные A9/B6. Эти ожидания заменены на текущий staged контракт, включая последовательность действий, identity oracle и запрет возвращать historical A без отдельного назначения. На нынешнем коде malformedA оплачивает обработку урока в закрытой копии, но не публикует его. Равная целостность A/B — законный нулевой результат; ранний отказ B может отдельно экономить staging, если это измерено.

Физическая работа восстановления индекса учитывается отдельно от учебных вызовов. Условный source-derived прогноз для штатной последовательности:12 учебных AddPair +9staging-copy +18 четырёх Load =39. Это не новый урок и не измеренный runtime counter; фактическое число до запуска unknown. Состав уроков и число вызовов программы не увеличены.

## 3. Документы переноса и общие входы

| Файл | Исправленный договор |
|---|---|
| [02 learning state](02-learning-state-and-fixtures.md) | Полный payload/actual owner/version и loaded target; writer и measured identity |
| [03 execution/test](03-test-and-execution-contract.md) | Immutable oracle, настоящая порча, lawful positive, finalizer и truthful terminal |
| [04 evidence](04-evidence-and-novelty.md) | Ось пользы отдельно от accuracy; виды доказательства и missing data |
| [19 translation](19-translation-memory-coverage.md) | Основной M04 уже реализован; R01 и canonical migration открыты |
| [20 E2E](20-prompt-domain-e2e.md) | Реальный prompt→learner→state→consumer, owner mapping без выдуманного API |
| [29 numerical](29-matmul-right-gradient-locality.md) | Finite errors/tolerance, независимый oracle, P110 forward не общий backward |
| [52 identity](52-translation-training-identity-index.md) | PairId отдельно от semantic key, actual bytes и атомарная миграция |
| [126 P108 transfer](126-p108-predicate-carrier-transfer-2026-11-02.md) | Ограниченный сигнал, M06 уже исправлен, old trace неполон, R02 открыт |
| [130 P110](codex-130-p110-numeric-projection-disposition-2026-11-02.md) | 44 чистых сравнения; точный синтетический fixture; отсутствующий исторический покоординатный допуск не выдумывается; общая замена HOLD |
| [131 P111](codex-131-p111-addressed-feedback-transfer-2026-11-02.md) | Точные множества owners; U3 как новая порча; законный unknown-owner отказ; 5 чистых сравнений при прежних 4 Train/10 Execute |
| [132 P112](codex-132-p112-material-lineage-transfer-2026-11-02.md) | Проверенный неизменяемый payload поступает в обучение; guard до DPO; 12 чистых вызовов и legacy NOT_EVALUATED |
| [133 P113](codex-133-p113-channel-balance-disposition-2026-11-02.md) | 39 сравнений сохранённых квитанций отдельно от native этапа; конечные порчи; missing raw/tokenizer; opt-in и HOLD |
| [127 действующий план](codex-127-postmerge-addressed-repair-plan-2026-11-02.md) | Удалено назначение уже выполненного M02a/M03c/M04/S02; остатки названы точно |
| [39 evidence contract](../experimets/39-transfer-evidence-contract-2026-10-30.md) | Actual bytes/sets/components/versions/stages; исправленный checker подключён к finalizer |
| [40 executor context](../experimets/40-effective-executor-context-2026-11-02.md) | Передаваемый prompt содержит собственный counterexample и предел бюджета |
| [01 registry](01-registry.md), [README](README.md), [Q index](../experimets/cards/00-execution-index.md) | Текущий срез: 114 записанных результатов, включая частичный Q002; 16 незапущенных, следующая Q115; владельцы 130–133 сохранены, их создание не означает готовую реализацию |

## 4. Что не изменено и почему

P101–P103, P105–P106 и прежние научные результаты не перезапускались и не получили новую полную аттестацию. Их действующие предметные открытые задачи остаются у 118/124/127; общий evidence contract применяется при следующем прямо назначенном ремонте. Все остальные исторические карточки не объявляются индивидуально проверенными только потому, что изменён общий вход.

Продуктовые `.cs`, существующие runners, учебные данные и сохранённые results не редактируются этим пакетом. Три новых `.py` — аудиторские проверки, а не продуктовые реализации. Автоматический повтор обучения после исправления приёмки не назначается. Проектные навыки и AGENTS не переписываются.

## 5. Независимая проверка и публикация

Независимый исследователь P109–P113 проверил основной аудит, план и карточки, затем отдельно разобрал P114 и частичную миграцию M03 на новом срезе. Исправлены найденные им адрес владельца LessonsFor/tolerance, неусловное membership у законного unknown-owner отказа, требование настоящего writer gate в interleaving controls, отсутствующее историческое tokenizer/raw state, Q129 oracle и source-derived стоимость восстановления. Итоговые команды, hashes и ограничения находятся в [пакете](../../../../experiments-v2/_audits/2026-11-02-post113-audit/README.md). Запись документа review означает проверку этого ограниченного scope, не всех исторических планов и не native acceptance.
