# Независимая проверка продолжения аудита — итог

Дата: 11 ноября 2026. Проверяющий: `/root/skills_scope`.

**Вердикт: PASS для проверенного документного продолжения после ремонта замечаний R1–R5.** Численный checkpoint воспроизведён. Найденные пробелы по concurrent duplicate, authority/restore receipts, повторному PairId, примеру P49 и отключаемым assert устранены в проекте до передачи; изменённые формулировки прочитаны повторно. Этот PASS относится к расчётам и конкретности плана. Product implementation, native execution и полный suite этим не приняты.

Отдельно прочитано итоговое `followup/02-audit-and-repair-addendum.md`; существенного завышения силы численного/source/receipt доказательства в нём не найдено. Полная ссылочная и публикационная проверка всего пакета остаётся действием родителя перед commit/push.

## Закрытие замечаний и граница независимости

| Замечание | Что прочитано после исправления | Итог |
|---|---|---|
| R1: Python optimization выключает assert | В `main`, до parser/output, есть явный отказ при `not __debug__`. Удаление только двух добавленных строк даёт первоначальный SHA256 скрипта: численное тело не изменено. | Закрыто на уровне checker. Отдельный запуск `-O` делал родитель; я повторно его не запускал. |
| R2: concurrent duplicate/неуспешная уже оплаченная доставка | Карточка106 и приоритетное уточнение repair-plan требуют atomic claim до Train, canonical fingerprint, shared task/IN_PROGRESS, terminal `REJECTED_BEFORE_TRAIN`/`FAILED_AFTER_TRAIN`/`UNKNOWN_COMMIT_STATE`/`COMMITTED`. CAS failure после Train не освобождает ключ для новой дозы. | Закрыто на уровне проекта; no second physical Train проверяется счётчиком. |
| R3: receipt authority и restore | Выбран server-owned immutable issued/verification registry у TranslationMachine, opaque IDs и canonical lookup. Тот же checkpoint переносит authoritative записи для committed keys; import проверяет ссылки/payload до atomic publication. Arbitrary caller JSON не создаёт authority; unknown receipt даёт Train=0. | Закрыто на уровне проекта; crash-before-durable-save остаётся вне гарантии. |
| R4: PairId/body/weights/import | Карточки15/48 и repair-plan запрещают changed payload для уже имеющегося PairId до mutation. Exact duplicate идемпотентен. Import проверяет целую revision/pairs/weights связь и сохраняет прежнее состояние при отказе. Фраза об «изменённом ID» исправлена на повтор ID с изменённым payload. | Закрыто на уровне проекта; обычная новая lesson/revision identity не запрещена. |
| R5: несуществующий пример P49 | Приоритетная датированная поправка использует фактические e-t1=(0,1) и new-only 0→3; прежний текст явно снят, исторические байты сохранены. | Закрыто в плане и P49. |

**Бюджет106:** два adapter delivery используют одну из двух уже назначенных corrective доз в исходных 8+2; второй delivery не добавляет flow/verifier/Train calls. Failure-after-Train инфраструктурный сценарий явно имеет counting trainer calls=1 и real learner calls=0. Если отдельно потребуется native Unit failure path, он получает собственную заранее объявленную дозу. Это конкретно исключает скрытый одиннадцатый урок.

Полностью прочитаны все 12 вставок родителя:20/32/48/P49/P50/P54/15/106/repair-plan и planning28/29/60-01. Пять вставок03/04/P48/P52/P58 написаны мной по отдельному поручению, поэтому я не называю их собственную проверку независимой: родитель сообщил о прямом чтении и приёмке; его уточнения про «плечи» и различие TRAIN acquisition/нулевого A/B consumer effect внесены. В P58 пригодный нулевой A/B эффект остаётся VALID с отрицательным научным исходом.

Для всех 17 изменённых существующих файлов проверено: удаление ровно датированной вставки восстанавливает baseline bytes и Git blob SHA1. Source code/tests/raw не менялись. Гигантские прежние карточки не перечитывались целиком ради вставки: проверены новые блоки, релевантные прежние договоры и сохранность остального текста.

Счёт документов пересчитан по первой таблице прежнего `card-strengthening.md` и новым manifests:11 прежних существующих карточек/договоров + 16 текущих − 3 пересечения (core 15/106, planning 60-01) = **24 уникальные карточки/договора**. Отдельный repair-plan даёт 25 существующих изменённых документов в объединении. Новые audit supplements не включены в это число.

Счёт входов тоже проверен независимо:9 product + 50 evidence − 1 общий P52 source = **58 уникальных файлов**, hash общего источника совпал. Это объём проверенных inputs, не число научных опытов.

## Проверенные материалы

Полностью прочитаны `followup/recompute_product_measurements.py`, `source-manifest.json`, `product-recomputation.json`, `01-checkpoint-evidence.md` из `publish/llm_mvp_core` (контрольная публикация `0b2cca56`), а также `repair-plan.md` и `acceptance-contract.md` из `current/llm_mvp_core/.../audits/2026-11-11/`. Дополнительно прочитаны ограниченные участки текущих источников, на которые они опираются. Не повторялся полный аудит репозитория, не запускались .NET/PowerShell или модели.

| Файл на начале review | SHA256 |
|---|---|
| `recompute_product_measurements.py` | `c0a7096b13997fe6c4750af3b1860855a9a213cac3d6cb2a86cb67757b72055b` |
| `source-manifest.json` | `53adb7a3bdb672747943f6fdeeff8b2a7c08bb8301f5a1b6d08fc1c72553592e` |
| `product-recomputation.json` | `d165659e498cdfc88f4680f68a91f6ecf70ac14fb52879fc6ca52908f97c2f71` |
| `01-checkpoint-evidence.md` | `264812f29541eb8fdda0e376fe881f7d350a30f022367596f5b6e6b22f81ec44` |
| `repair-plan.md` | `f4c51e4d3bec4827786771ccb4b39e877f4ae0167184ec2e41bc750712cdd897` |
| `acceptance-contract.md` | `f3012f762fb3f29575af9e2ed3c47df6c58241907bba61fbdc3a9d82470239ee` |

## Расчёты: подтверждено

Прочитанный скрипт запущен с `--core repos/llm_mvp_core --output audit_work/review-product-recomputation.json`. Exit 0; новый JSON **побайтно совпал** с опубликованным `product-recomputation.json`, включая девять исходных fingerprint и `inputs_unchanged=true`. Это повтор расчёта, не независимое исполнение C#.

- DP candidate и отдельный exhaustive subsets oracle согласованы на 33 866 синтетических графах для длин 1–5. Сумма проверена отдельно: 2+8+64+1024+32768. Два контрпримера 8/6 слов корректно воспроизводят потерю допускаемого альтернативного плана. Bound три категории обоснован для coverage/span predicate и общего canonical-prefix порядка. Ограничения про tokenizer, target compatibility, фасад, ресурсы и отсутствие exhaustive проверки длин 6–8 явно сохранены.
- Wiki replay соответствует прочитанным Train, Score и Featurize на закреплённом ASCII corpus: однообъектный порядок признаков, 18 эпох, learning rate 0.14, first maximum. Дополнительно независимо проверена форма snapshot: 15 labels, 264 feature vectors длины 15, 15 biases, всего 3 975 конечных double. Результат 7 updates в 4 активных и 14 пустых эпохах подтверждён; перенос числа 4 или 7 в общий early-stop отсутствует.
- P49: разобраны фактические FrozenPair/PanelRow. Warm+new имеет input/pair overlap 4/4; new-only — input3/4, exact pair2/4, один familiar input/new target и один input unseen. Постоянный ответ даёт максимум2/4; функция только от входного token —3/4.
- P52: ASCII gold даёт answer ceiling2/4 и tense ceiling2/4; сохранённый attempt2 JSON имеет поле ceiling0. Поле источника и корректный пересчёт не смешаны.
- Loss: при target logit−20 clipped objective постоянен в окрестности, его центральная разность0; CE derivative≈−1, p−y≈−1. Это скалярный контрпример, не fresh gradient test нейросети.

**Замечание R1, WRITE-TIME для воспроизводимого checker:** проверки candidate/oracle, bit-exact и неизменности входов используют `assert`; Python `-O` отключает их, но поля `candidate_mismatches=0`, `bit_exact=true`, `PASS_ANALYTICAL_MODEL` остаются. Обязательный SHA mismatch использует явный raise и от этого не страдает. Требуется отказ при `not __debug__` либо явные runtime guards. Обычный опубликованный расчёт корректен; R1 не отменяет его чисел.

## План: найденные замечания и основания ремонта

### R2 — два concurrent duplicate могут физически обучить дважды

**Тег: BLOCKING для обещания live-process replay без второй дозы; статус на начало review: требуется дополнение.**

В `repair-plan.md` §3.3 задано `lookup committed outcome → validate captured version → Train → atomic publish outcome+state`. Две доставки одного `ReceiptId+LessonId` могут одновременно увидеть отсутствие committed outcome и одинаковую current version. `TranslationMachine.TrainStructuredBatchAsync` проверяет версию до staging (446–458), но финальный `TryPublishVersion` находится после обучения (669). CAS препятствует второй публикации, а не второму фактическому Train.

Нужна атомарная in-flight резервация ключа и immutable payload fingerprint до Train. Второй identical запрос ждёт/получает IN_PROGRESS либо сохранённый terminal без Train. Изменённый payload с тем же ключом отвергается до присоединения. Переходы reservation→trained→committed/failed/unknown должны запрещать автоматическую новую дозу; аварийная durable гарантия остаётся отдельно ограниченной, как уже написано.

Приёмка: два одновременных identical duplicate, conflicting payload duplicate, duplicate после commit/load, отказ после затраченного Train. Считать реальные вызовы, не только изменение published state.

### R3 — authenticity должна иметь названного владельца

**Тег: BLOCKING для provenance acceptance; статус на начало review: требуется дополнение.**

В §3.3 `TranslationIssuedAnswerReceipt`/`TranslationVerificationReceipt` перечислены поля, но `validate authenticity` не выбирает источник доверия. Типизированные caller-supplied поля или строковый идентификатор не подтверждают, что ответ действительно был выдан и проверен.

Минимальный конкретный вариант: server-owned immutable issued/verification registry. Issuer после успешного `flowResult` сохраняет canonical request/result/captured-version payload; verifier сохраняет свою запись, связанную с issued ID. Adapter разрешает ID через эту запись, сравнивает присланный payload, а actual/state/receiver берёт из canonical record. Изменённый/неизвестный ID и arbitrary self-authored record отвергаются до Train. Это provenance/outcome state, не новая память переводов. Для другого trust mechanism должен быть столь же конкретный authority contract; в план нельзя оставить только глагол «проверить».

Дополнительная обязательная стыковка с restore: поскольку authenticity lookup предшествует committed lookup, совместно загруженный checkpoint должен содержать authoritative issued/verification records для committed keys либо полную эквивалентную canonical запись. Иначе легальный duplicate после load откажет как unknown receipt до идемпотентного возврата. Outstanding uncommitted receipt после load допустимо явно отклонять как UNKNOWN без Train; это не новая гарантия crash recovery.

### R4 — повтор PairId способен рассогласовать active revision и тело

**Тег: BLOCKING для active-revision acceptance; статус на начало review: требуется дополнение.**

`OrchestrationKnnStore.Add`, 16–21, молча игнорирует уже имеющийся PairId. `PromptFlowGroundingModel.Train`, 30–61, после этого всё равно строит и публикует новые weights. При новом target B с PairId прежнего A предполагаемый MemberPairIds может ссылаться на старое тело при новых weights/revision.

Нужно закрепить immutable `PairId→canonical payload`. Changed duplicate отвергается до mutation weights/revision/history; exact duplicate имеет явный idempotent outcome. При import проверяется не только существование ссылок, но FlowRef/mode/normalized positive target каждого active member. Повреждённый/неоднозначный импорт оставляет целиком прежний state.

Приёмка: повтор ID A с target B, exact duplicate, conflicting duplicate checkpoint, member с чужим flow/target, отказ load после предварительного разбора с неизменностью текущего состояния. Plan уже правильно отделяет историю и ambiguous legacy от active revision, но эти конкретные входы ещё не названы.

### R5 — пример P49 не относится к закреплённым строкам

**Тег: WRITE-TIME; статус на начало review: требуется точная формулировка.**

`repair-plan.md` §2.2 содержит «Input1→5 при warm1→0». Такой строки нет в закреплённых FrozenPair/PanelRow. Реальный случай новой target при знакомом input: exam `e-t1=(0,1)`, new-only training содержит `(0,3)`. Заменить пример фактической строкой либо прямо назвать отдельной предлагаемой synthetic fixture. Правильные агрегаты 2+1+1 и4+0+0 не оспариваются.

## Уже достаточные части плана

Ownership требует атомарный CreateNew до prep/Train, отдельные owner-only операции, сохранность чужих/прерванных attempts и отказ traversal. Terminal вынесен во внешний процесс, сохранены partial invalid/no-contrast и реальные required cases. P50 decision→status→actual exit достигается тем же host через обозначенный как новый validate-measurements режим; это не выдано за готовую команду. Legacy ambiguity, clone/load, stale-version admission и crash-before-save границы correction прямо описаны. Batch fallback различает absent и invalid solution; weak baseline не расширяется ради зелёного цвета. Эти части повторно не объявляются отсутствующими.

Из предыдущего evidence review остаётся адресное уточнение к §1.2: назвать `summarize-test-batches.ps1` и его missing-TRX→journal fallback как конкретное место изменения, а не считать любой существующий summarizer готовым finalizer. Общая норма acceptance §4 уже запрещает missing TRX→PASS; это реализация существующего договора, не новый научный gate.

## Итоговые отпечатки и граница этого наряда

После R1 SHA256 `recompute_product_measurements.py` = `32883458296466e5010aa0bdf338eb48b38e839fbf780ceca6c46a0c6be8a132`; вычислительное тело побайтно равно первоначальному после исключения единственного guard. Результат первоначального нормального запуска и моей копии остаётся `d165659e498cdfc88f4680f68a91f6ecf70ac14fb52879fc6ca52908f97c2f71`. Нового численного прогона после добавления guard не делалось.

Подробные финальные hashes 17 файлов и доказательства сохранности исходных байтов находятся в рабочем `final-review-document-hashes.json`; расчёт объединения карточек — `final-review-card-union.json`. Они являются учётом review, а общий публикуемый `document-change-manifest.json` готовит родитель.

Новые Train/Predict/Flow/Load/native tests — 0. Только чтение закреплённого кода, арифметический replay и review документов. Моя проверка не принимает ещё не реализованный продукт, не удостоверяет полноту всех 131 опытов и не меняет прежние raw. `skill_load_status` прежний: выбранные current skills и 45/46 прочитаны полностью; AGENTS частично, CLAUDE мной не прочитан. `subagent_id_or_NONE: /root/skills_scope`.

## Позднее дополнение после первоначального review

Первоначальные17 файлов выше сохранены как область этого review. Перед публикацией появился параллельный B01 DRAFT; отдельный [delta review](delta-b01-review.md) проверил текущий exact-count helper, unconnected caller, saved27/29 и ресурсные поправки. Добавлена18-я существующая карточка109, прежние её байты сохранены. Общая карта/manifest содержит окончательные18 файлов; объединённый охват карточек остаётся24.
