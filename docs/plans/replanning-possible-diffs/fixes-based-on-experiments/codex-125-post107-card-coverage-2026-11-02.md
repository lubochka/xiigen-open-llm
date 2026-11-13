# 125. Покрытие карточек и проверка пакета после P107

Основание: [аудит123](codex-123-post107-transfer-and-experiment-audit-2026-11-02.md), [план124](codex-124-post107-addressed-repair-plan-2026-11-02.md), [инструкция41](../experimets/codex-41-post107-executor-checks-2026-11-02.md). Это опись выполненной документной работы и её границ, не новые исполнения опытов.

## Что изменено

Адресно усилены **30 экспериментальных карточек**, **14 карточек/договоров переноса core** и **9 исходных planning-карточек**: всего53. В30 входят все23 ещё не исполненные позицииQ108–Q130 и7 записанных карточек: P93, P102, P103–P107. Общие контракты, индексы и исторические указатели учитываются отдельно.

В существующих файлах70 вставок:59 в core и11 в planning. Каждая добавлена после заголовка; после удаления вставки весь прежний файл побайтово совпадает с baseline. Planning README согласован с появившимся commit266f15da, его новые требования и ссылки сохранены. Остальной параллельный файл POST101 сохраняется через базовое дерево этого коммита. Полный каталог275 карточек этим проходом заново не сертифицирован.

## Адресная матрица

| Тип | Карточка | Что именно усилено |
|---|---|---|
| transfer-card | [11-translation-verifier-admission.md](11-translation-verifier-admission.md) | Рабочий проверяющий обязан читать тот же смысловой ключ, который записало обучение. |
| transfer-card | [13-dpo-reference-cache.md](13-dpo-reference-cache.md) | P105 подтверждает ограниченный факт неизменности reference, а не приобретённую точность. |
| transfer-card | [15-prompt-flow-retention.md](15-prompt-flow-retention.md) | Удержание — сохранность содержания ответов того же обученного состояния. |
| transfer-card | [16-replay-domain-retention.md](16-replay-domain-retention.md) | Сравнение replay должно продолжать один lineage памяти. |
| transfer-card | [19-translation-memory-coverage.md](19-translation-memory-coverage.md) | Покрытие памяти включает согласованность ключей и атомарную публикацию. |
| transfer-card | [20-prompt-domain-e2e.md](20-prompt-domain-e2e.md) | План должен дойти до исполнения со своим содержимым. |
| transfer-card | [27-predicate-dataset-router-learning-coverage.md](27-predicate-dataset-router-learning-coverage.md) | Получатель урока определяется до преобразования, которое уничтожает признак. |
| transfer-card | [28-dpo-tokenizer-snapshot.md](28-dpo-tokenizer-snapshot.md) | Snapshot tokenizer должен быть фактически прочитан новым владельцем. |
| transfer-card | [33-wiki-training-feature-reuse.md](33-wiki-training-feature-reuse.md) | Переиспользование признаков не изменяет смысл метрик и приёмки. |
| transfer-card | [48-learned-route-body-identity.md](48-learned-route-body-identity.md) | Личность выбранного маршрута должна совпасть с источником его тела. |
| transfer-card | [49-router-learned-key-normalization.md](49-router-learned-key-normalization.md) | Нормализация ключа должна сохранять признаки, нужные последующему решению. |
| transfer-card | [52-translation-training-identity-index.md](52-translation-training-identity-index.md) | Учебная личность и смысловая индексация имеют разные назначения. |
| transfer-card | [54-frozen-base-check-scope.md](54-frozen-base-check-scope.md) | Принять узкое доказательство P105 и не расширять его до качества обучения. |
| transfer-card | [101-persistent-state-bundle-contract-2026-10-30.md](101-persistent-state-bundle-contract-2026-10-30.md) | Bundle считается восстановленным только после его чтения новой единицей. |
| planning-card | [04-KNN-HYBRID-RETRIEVAL.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/04-KNN-HYBRID-RETRIEVAL.md) | Допуск проверяется до отсечения соседей; выбранный ответ возвращается со своим содержимым. |
| planning-card | [05-SEMANTIC-VECTOR-KNN.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/05-SEMANTIC-VECTOR-KNN.md) | Смысловой ключ, личность урока и ранжирование имеют разные назначения. |
| planning-card | [14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/14-KNN-IDENTITY-PREDICATE-SCOPE-REBUILD-GAPS.md) | Допуск, учёт дубликатов и выдача содержимого требуют отдельных достижимых проверок. |
| planning-card | [30-NEW-E20-E21-ROUTER-AND-THRESHOLD-VALIDITY-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/30-NEW-E20-E21-ROUTER-AND-THRESHOLD-VALIDITY-2026-10-16.md) | Числовой договор проверяется и у загруженного подтверждения качества. |
| planning-card | [06-CHECKPOINT-RESTORE-ADOPTION.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/06-CHECKPOINT-RESTORE-ADOPTION.md) | Сохранить файл и восстановить память — разные действия. |
| planning-card | [26-NEW-SEMANTIC-LANGUAGE-KEY-AND-RULETAG-PARITY-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/26-NEW-SEMANTIC-LANGUAGE-KEY-AND-RULETAG-PARITY-2026-10-16.md) | Нормализация должна сохранять признаки языка и грамматики. |
| planning-card | [28-NEW-N28-TOKENIZER-TO-EXECUTOR-KEY-DRIFT-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/28-NEW-N28-TOKENIZER-TO-EXECUTOR-KEY-DRIFT-2026-10-16.md) | Ключ меняется у всех писателей и читателей согласованно. |
| planning-card | [13-MICRO-KNN-CAUSAL-LEARNING-EXPERIMENT.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/13-MICRO-KNN-CAUSAL-LEARNING-EXPERIMENT.md) | Причинный вывод требует различающего состояния и исправного измерителя. |
| planning-card | [02-DPO-DATA-OBJECTIVE-GRADIENT.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/02-DPO-DATA-OBJECTIVE-GRADIENT.md) | Целостность reference, качество ответа и вероятностный сдвиг проверяются отдельно. |
| experiment-card | [P115-bm25-statistics-cache.md](../experimets/cards/P115-bm25-statistics-cache.md) | Равное число документов не означает свежие corpus statistics |
| experiment-card | [P121-project-adapter-selection.md](../experimets/cards/P121-project-adapter-selection.md) | Правильный получатель определяется фактическими буферами проекта |
| experiment-card | [P116-e43-file-boundary.md](../experimets/cards/P116-e43-file-boundary.md) | Доказать происхождение всех пар, а не только сумму 27 |
| experiment-card | [P114-zero-weight-pair.md](../experimets/cards/P114-zero-weight-pair.md) | Ноль вклада пары проверяется до AdamW и против P-only |
| experiment-card | [P119-learning-dynamics.md](../experimets/cards/P119-learning-dynamics.md) | Динамика LR требует той же стартовой памяти оптимизатора и названной метрики |
| experiment-card | [P118-projectwiki-content.md](../experimets/cards/P118-projectwiki-content.md) | PlanText должен дойти до тела ответа, gate — иметь измеренное основание |
| experiment-card | [P111-reward-credit.md](../experimets/cards/P111-reward-credit.md) | Удержание чужой памяти проверяется до восстановления ссылок |
| experiment-card | [P117-translation-direction-memory.md](../experimets/cards/P117-translation-direction-memory.md) | Доступ RU_EN к памяти без обхода закона источника |
| experiment-card | [P120-semantic-compression-laws.md](../experimets/cards/P120-semantic-compression-laws.md) | Сохранённый not/no должен участвовать в реально выученном переходе |
| experiment-card | [P123-design-conversion-critic-evidence.md](../experimets/cards/P123-design-conversion-critic-evidence.md) | Geometry KNN и переданный QA verdict имеют разные источники доказательства |
| experiment-card | [P93-confidence-abstention.md](../experimets/cards/P93-confidence-abstention.md) | Калибровка не заменяет контраст данных и измеренность confidence |
| experiment-card | [P122-root-channel-unicode-digits-operators.md](../experimets/cards/P122-root-channel-unicode-digits-operators.md) | Сохранённые roots могут не влиять на learner: нулевой эффект законен |
| experiment-card | [P124-p3-fixed-discriminator-contract.md](../experimets/cards/P124-p3-fixed-discriminator-contract.md) | Новая память должна изменить verdict, а старый закон — сохраниться |
| experiment-card | [SAVED-STATE-PANEL-0407-01.md](../experimets/cards/SAVED-STATE-PANEL-0407-01.md) | Учебный повтор и сохранённый readback имеют разные доказательства |
| experiment-card | [P25-cold-restore.md](../experimets/cards/P25-cold-restore.md) | Сохранённые bytes должен прочитать новый процесс без скрытого fit |
| experiment-card | [PHRASE-COVERAGE-CAP-01.md](../experimets/cards/PHRASE-COVERAGE-CAP-01.md) | Чувствительность к learned target и цена полного обхода |
| experiment-card | [HELDOUT-ADMISSION-ATOMICITY-01.md](../experimets/cards/HELDOUT-ADMISSION-ATOMICITY-01.md) | Отказавшая партия проверяется на том же состоянии до исправления |
| experiment-card | [METHOD-KNN-SCOPED-EXPRESSION-TARGET-01.md](../experimets/cards/METHOD-KNN-SCOPED-EXPRESSION-TARGET-01.md) | Scope цели должен изменить native обученную пару, а не красивую упаковку |
| experiment-card | [P108-metadata-carrier.md](../experimets/cards/P108-metadata-carrier.md) | Форма коллекции должна дойти до признаков и ответа |
| experiment-card | [P105-frozen-reference.md](../experimets/cards/P105-frozen-reference.md) | Целостность reference, смысл вероятностей и достижимый контроль |
| experiment-card | [P102-teacher-forced-decoding.md](../experimets/cards/P102-teacher-forced-decoding.md) | Полный собственный ответ и честная чувствительность оценщика |
| experiment-card | [P103-stack-before-normalization.md](../experimets/cards/P103-stack-before-normalization.md) | Исправление маршрутизации не доказывает забывание памяти |
| experiment-card | [P109-gold-validity.md](../experimets/cards/P109-gold-validity.md) | Правильность арифметической метки и ответ Student измеряются отдельно |
| experiment-card | [P104-law-eligibility.md](../experimets/cards/P104-law-eligibility.md) | Четыре отрицательных контроля пока подставляют отказ сами |
| experiment-card | [P107-lawful-neighbor-aggregation.md](../experimets/cards/P107-lawful-neighbor-aggregation.md) | Нулевой A/B, расход 51/50 и приёмка отдельно от гипотезы |
| experiment-card | [KNN-ORDER-PERMUTATION-01.md](../experimets/cards/KNN-ORDER-PERMUTATION-01.md) | Граф должен исполнять полный порядок, возвращённый памятью |
| experiment-card | [P106-corrected-semantic-target.md](../experimets/cards/P106-corrected-semantic-target.md) | Исправленная подпись не исправила роли слов и не даёт привилегию PAIR |
| experiment-card | [P113-feature-energy.md](../experimets/cards/P113-feature-energy.md) | Нормы каналов должны менять обычное чтение без утечки target |
| experiment-card | [P110-numeric-matrix-cost.md](../experimets/cards/P110-numeric-matrix-cost.md) | Проверять фактическую обученную матрицу и полное окно Predict |
| experiment-card | [P112-family-disjoint-evaluation.md](../experimets/cards/P112-family-disjoint-evaluation.md) | Независимость семейств проверяется по содержимому после materialization |

## Действительное применение навыков

Архитектура: прослежены saved snapshot→gate→Unit, два orchestration import→export, PAIR writer→semantic unit→executor→verifier. Планирование: назначенияS01–S16 имеют владельца, зависимость, конкретный контрпример, результат и границу стоимости. Проверка плана нашла пропущенный ImportCheckpoint; он добавлен до публикации. Проверка кода отделяет implemented final metrics от нового saved-gate дефекта. Prompt engineering: каждой карточке добавлен собственный WRONG/CORRECT и наблюдаемый результат. Человеческая коммуникация: объяснение ошибки precedes exact technical chain; замечания о склеенном тексте исправлены.

Прочитаны относящиеся к задаче разделы репозиторных навыков; полное чтение всех больших skills не заявляется. Два локальных договора120, на которые сослался concurrent planning, отсутствуют в проверенном опубликованном каталоге; их доступность явно вынесена вS16. Непрочитанное не помечено прочитанным.

## Как проверено

- [Машинная матрица](../../../../experiments-v2/_audits/2026-11-02-post107-audit/codex-card-coverage.json) хранит адресные вставки, SHA256 до/после, размер и место вставки каждого файла.
- [Опись исходников](../../../../experiments-v2/_audits/2026-11-02-post107-audit/codex-source-inventory.json) содержит293 версии файлов с проверенными Git blob SHA и SHA256; это точность полученных bytes, не число целиком отревьюенных файлов.
- [67 проверок опытов](../../../../experiments-v2/_audits/2026-11-02-post107-audit/codex-experiment-recheck.json) и [58 проверок переносов](../../../../experiments-v2/_audits/2026-11-02-post107-audit/codex-transfer-recheck.json) пересчитывают сохранённые данные и исходные предикаты, без .NET и обучения.
- [Независимый review](../../../../experiments-v2/_audits/2026-11-02-post107-audit/codex-independent-root-review.md) проверяет123/124/41, четыре transfer-вставки и planning30. Автор30 experiment cards не засчитывает свои карточки как независимую проверку; основной исполнитель отдельно проверилP104/P108/P117 и уточнил PAIR-контрольP117.
- [Итоговая проверка документов](../../../../experiments-v2/_audits/2026-11-02-post107-audit/codex-document-validation.json) проверяет сохранность70 базовых файлов, новые ссылки, состав очереди, отсутствие продуктовых/raw правок и корректность JSON.

## Границы результата

Созданы план и усиленные требования. Runtime-ремонтыS01–S06, reader поправокS07, targeted P85/facade и другие незакрытые части прежнего плана ещё не выполнены. P103–P107 raw, hosts, продукт, Tests и научные дозы не менялись. Цифры успешных исторических тестов не выдаются за новые исполнения; пересекающиеся наборы не складываются. Текущая очередь107 RESULT_RECORDED /23 DESIGNED_NOT_RUN, следующийQ108; recorded означает наличие результата.
