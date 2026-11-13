# RP01 — проверка полезного обучения, а не покрытия тестами

Действующая методика15.10.2026 для [RP01.BATCH500.v1](RP01-OFFLINE-TEACHER-BATCH500.md). Исполнение только по [RP01-EXECUTE-NOW](RP01-EXECUTE-NOW.md). Полный промпт custodian/экзаменатору: [B500.04 EXAM](tasks/batch500/RP01.B500.04-EXAM.md). Методика опубликована; новый эксперимент этой planning-сессией не выполнен.

## 1. Что различаем

Код существует; actual update произошёл; правильное learned state изменилось; новые ответы улучшились; принятый результат использует ordinary consumer; гибрид эффективен на PC — шесть разных утверждений. Один успешный CLI, saved hash, loss или green Windows run не доказывает следующие уровни.

Обучаемость и случайность — разные свойства. DPO objective/masks, разрешённые keys, checkpoint/permissions/journal имеют проверяемые инварианты. Для их конкретной правки нужен ближайший integrity check, но не новый test-coverage campaign. Документы/read/reuse→0 новых product tests. Научный итог дают независимые доменные задачи и реальные time/resource observations.

## 2. Данные и роли нового опыта

Первым C#500 accepted unique DPO pairs; затем EN→RU500 через настоящий domain consumer. Пятьсот — отдельный TRAIN budget на домен, не все authored attempts, не токены/оценочные слоты и не гарантия достаточности. Template/source families учитываются отдельно;500 переименований одного примера не означают500 независимых ситуаций.

CODEX `astra 6 ultracode` S выбирает TRAIN; отдельная Astra A симулирует внешнего Chinese Teacher и сохраняет обе стороны с provenance `ASTRA_SIMULATED_TEACHER`; отдельный CODEX Astra J проверяет exact pair. Независимый CODEX Astra E оценивает Student. Actual model/session receipts обязательны. S/A/J не видят SEALED gold/rubric; E не готовит TRAIN. Одна модель в разных сессиях может иметь общие ошибки; спорные claims проверяются источниками/безопасными domain oracles.

Ни локальный Qwen author, ни локальный Qwen evaluator не вызываются в new inner loop. Student остаётся настоящим локальным объектом измерения. Cached teacher outputs не заменяют его собственные outputs.

## 3. Freeze и быстрый старт

Отдельные pools: TRAIN500; DEV10 для likelihood-кривой; sealed40 preference pairs; sealed20 generative tasks из других source/template families. Это стартовый exploratory дизайн, не доказательство мощности выборки. Cases, source split, rubric/clauses/primary metric/abstention/critical rules, generation settings, budget и original B0/reference фиксируются до main campaign. Public GOOD/BAD examples не используются в sealed exam. Парафраз, перевод или AST-preserving rename не создаёт независимую family.

Разрешён короткий TRAIN-only probe1–4 для stage timing до shard50, без closed quality claims. Main campaign starts с первых50; следующие450 и подготовка второго домена параллельны. Full baseline B0 generation не обязана задерживать optimizer: immutable B0 можно later открыть read-only при тех же settings. Нельзя использовать изменённое theta как baseline. Полный final exam не снимается после каждой пары; DEV probes при0/50/100/250/500 не становятся итоговым SEALED proof.

Проверь source B0: pre-trained/ранее trained/random. Если capacity/базовый язык ещё отсутствуют, не обещай, что preference500 автоматически создаст их. Данные/algorithm/load issues различаются.

## 4. Оценка после500 — содержательный небольшой сигнал

Final sampling:20 B0+20 C500 real outputs, по одному на case/arm, фиксированные settings. Скрыть arm identity и desired outcome от E до фиксации score file/hash. Повторы нужны только отдельному заранее объявленному исследованию фактической вариативности, не для получения пяти копий одного решения.

Primary Q_law: доля заранее заданных обязательных semantic clauses, корректно выполненных внутри case, с равным весом независимых cases/families. Case с critical contradiction получает0 и отдельный critical flag. Частично исправленный смысл становится видимым; длина/красивый стиль/одна exact gold phrase не добавляют балл. Знаменатели/clauses не меняются после outputs.

Дополнительно: fully-correct count; paired n01/n10/n00/n11; новые critical errors; answered/abstained fraction; preference likelihood на40 held-out pairs. Loss/margin-only сдвиг — supporting diagnostic, не доказательство более правильного собственного ответа.

`OBSERVED_SMALL_GAIN` — положительный paired Q_law с конкретными исправленными clauses при валидной механике/bindings и отсутствии новых critical errors. Даже небольшой прирост достаточен как ранний сигнал продолжать исследование. +1/20 полностью решённая задача — достаточный пример, **не обязательный новый минимум**. `PREFERENCE_ONLY_SIGNAL` — вероятности улучшились, новая правильность ещё не наблюдается. `NO_OBSERVED_GAIN`/`REGRESSION` сохраняются и ведут к одной новой гипотезе/порции, а не подгонке экзамена. `INVALID_EVIDENCE` означает исправлять измерение/identity/split, не объявлять качество.

Показать выигрыши И потери, effect size/denominators и uncertainty. Paired bootstrap по независимым families либо exact binary companion вычисляются по сохранённым scores без model calls; маленький N/неполная независимость ограничивают вывод. CI через0 не запрещает exploratory continuation, но запрещает заявлять statistical significance. Не выбирать удачный checkpoint по многократно просмотренному sealed panel. Для следующей независимой confirmation нужны untouched families.

Dedup scoring допустим при совпадении case/input/context/rubric/grader bindings И raw bytes. Пять одинаковых ответов одного случая можно оценить один раз, сохранив multiplicities. Одинаковая строка на разных вопросах не переносит score. Actual new/reused calls записываются отдельно от logical slots. Пакетный grader возвращает per-answer evidence без потери строк.

## 5. Доменные функциональные ожидания

C#: AST/type/nullability/scope и source-specific program meaning. Пример PUBLIC TRAIN: `static int Count(string? s) => s?.Length ?? 7;` — для null fallback7, не0 и не обязательный NullReferenceException. Должен улучшаться смысл объяснения/выбранный допустимый путь, не exact wording. Неполный parse prefix отличается от завершённого invalid code. Compiler/безопасный execution oracle может проверять конкретное утверждение, но не подменяет examination новой coverage suite.

EN→RU: сохранение отрицания, modality, actor/recipient, количества, referents и agreement связанных объектов. `Книга лежала на столе. Он был деревянным` допускает masculine стол; общий запрет masculine после feminine неверен. `Анна подошла к Марии. Она улыбнулась` может быть неоднозначно; без контекста нельзя навязать единственный referent. Допустимы несколько адекватных формулировок. POS/coreference uncertainty не превращается в ложный hard law.

SQL: NULL/UNKNOWN, LEFT JOIN ON-vs-WHERE, duplicates/cardinality и exact dialect. Для требования сохранить всех клиентов predicate paid-заказов в WHERE может потерять unmatched clients. `col = NULL` — не универсально запрещённый token sequence: это допустимый объект объяснения/negative evidence, но неверный способ выбрать NULL. SQL имеет отдельный actual consumer/exam и не закрывается C#500.

Другие исходные поверхности — orchestration/planning, Figma→HTML/CSS, Angular/versioned docs, DTC/IDE — сохраняют собственные source/consumer/exam owners. C# и EN→RU pilot не объявляет их “покрытыми”. [Стратегия](../R01-R25-TO-CODE-SLICES-STRATEGY.md) и WorkLeft/RAG lineage не удаляются.

## 6. Причинная проверка механизмов и PC

Student-only curve фиксирует tokenizer/semantic representation/domain mapper/laws/kNN/arbiter/decoder. Параллельный RP02 строит domain-conditioned actual consumers; не ждать всех его задач для first C# step. Потом [B500.05](tasks/batch500/RP01.B500.05-DOMAINS.md) делает один B0/C500 × old/adaptive-policy contrast при одном resource envelope. Hard laws сохраняются в обоих arms. Positive/negative/warning memory — evidence, не автоматическое разрешение плохого output. Adaptive k не сводится к большинству neighbours; exact/ANN efficiency сравнивается при одинаковой lawful memory.

LoRA, tokenizer и semantic projection имеют отдельные смысл/state/version contracts. Unit many→many/Flow correction — RP03; async Orchestrator/higher-order graph — RP04. Static graph/дикт/counter не доказывают learned graph. Для каждого learned блока: actual state owner, обучающий сигнал, saved state, consumer effect, discriminating falsifier.

Измерять author/J, tokenize/ref-cache, Student forward/backward/optimizer, save/reopen, inference, Astra E и recovery раздельно. PC measurement включает simultaneous process-tree RAM/VRAM, base/reference/adapter/optimizer/index/cache/disk, hardware/settings, warm/cold и N. Не суммировать разновременные peaks. Positive speed without equal quality/budget не доказывает лучший hybrid. Gain0→cost-per-gain UNDEFINED. External Astra/Teacher cost остаётся видимой; система не называется полностью offline.

## 7. История и release не смешиваются

Прежний first-root experience12 families×5 repeats, gain≥2, critical0, native positive для publish остаётся неизменным **только для своего старого run**. Новый BATCH500 не переписывает его schema/threshold/verdict и не запускает `resume-baseline-and-hold` для batch. Старый negative не блокирует новое isolated обучение по новому prospective protocol.

Research continuation C50→C100→C250→C500 сохраняет выбранный optimizer/RNG/cursor и does not require production promotion каждый раз. Actual release guards/canonical publisher/cold user adoption остаются отдельными. NO_OBSERVED_GAIN не “провал всей архитектуры”; observed small gain не “готовый продукт”. Честный negative — полезный результат для новой гипотезы, не основание повторять до PASS.

[Историческая полная методика до BATCH500](https://github.com/lubochka/xiigen-planning/blob/822ff1b5e811f1da261919834bfeedbef6e0d41e/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/LEARNING-EXAMINATION.md). [Legacy reference compatibility](RP01-BATCH500-REFERENCE-RECONCILIATION.md). Method references: [DPO](https://arxiv.org/abs/2305.18290), [LoRA](https://arxiv.org/abs/2106.09685); они не являются measurements текущего XIIGEN.
