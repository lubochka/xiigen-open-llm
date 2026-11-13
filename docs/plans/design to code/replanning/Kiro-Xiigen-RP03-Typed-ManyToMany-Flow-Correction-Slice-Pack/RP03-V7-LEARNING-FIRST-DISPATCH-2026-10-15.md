# RP03 V7 — единый learning-first dispatcher

Обновлено 16.10.2026. Имя файла и V7 сохранены для устойчивых ссылок. **15 parent tasks / 9 child packets**, не новый цикл планирования. Статус публикации: planning repair, author-audited, независимая модельная приёмка NOT_PERFORMED.

## 1. Исполнитель получает один настоящий prompt

Исполнять выбранную физическую карточку `tasks/v7/RP03.<ID>.md` или её конкретный child. Этот dispatcher определяет зависимости; [алгоритм](RP03-V7-ALGORITHM-AND-EXAMS.md) — общие смысловые границы; [manifest](PACKAGE-MANIFEST.json) — состав. Старые V5/V6 и October15 repair essays — primary/history, не дополнительные приказы. Не складывать несколько старых bodies в один prompt. Не читать все skills и все RP подряд: только названные sections и semantic dependents.

Перед code edit: зафиксировать текущие P=`lubochka/xiigen-planning@llm_plan3`, K=`lubochka/llm_mvp_core@codex/dpo-implementation`, точные source blobs, действительный producer→writer→reader→consumer. Наличие имени класса или README не доказывает работающего механизма. Не выдавать author review за отдельный запуск Astra.

## 2. Сохранённая архитектура

**Unit:** domain/schema/representation-aware many→many с явными request-input/output associations. Существующий scalar `ILogicalUnit<TInput,TOutput>.ExecuteAsync` остаётся ABI; queryable оболочка сама по себе не доказывает many→many или получение payload.

**Flow:** F0 детерминированно ограничивает допустимые действия; F1 учит небольшую коррекцию отношений/маршрута между Units по действительно доступным до результата semantic features. Реальный scorer, writer, checkpoint и reader обязательны. Pairwise Flow loss не переименовывается в Student DPO.

**Orchestrator/Graph:** RP04 владеет asynchronous execution/higher-order relations; их полная реализация не prerequisite первого Flow update. **RP02** владеет domain mapping, lawful semantic vectors/tokens, adaptive kNN; **RP18** — LoRA/semantic-coordinate alignment; **RP01** — Student DPO; **RP05** — дальнейшее continual learning.

Первый Flow эксперимент фиксирует конкретную версию domain/kNN/representation, а не объявляет kNN неважным. Если доступны только ID/порядок кандидатов или нет проверяемых предметных признаков, нельзя объявлять domain-aware Flow learning: 02A указывает минимальную feature seam. RP02 работает параллельно, не ждёт DPO.

## 3. Кратчайший путь — две независимые реализации и ранний trial

| Когда | Карточка | Наблюдаемый результат; что НЕ требуется |
|---|---|---|
| Сразу параллельно | [00B](tasks/v7/RP03.00B.md), [02A](tasks/v7/RP03.02A.md), [00C-A](tasks/v7/children/RP03.00C-A.md) | Typed trace; scorer/state/feature source; настоящее независимое lawful preference. Не ждать остальных доменов. |
| Параллельно | [00A](tasks/v7/RP03.00A.md), [00C-B](tasks/v7/children/RP03.00C-B.md) через [00C](tasks/v7/RP03.00C.md) | Семь source routes и независимые exam families. Это не новая цепочка из 25 запусков. |
| Первое обучение | [02B](tasks/v7/RP03.02B.md) после 02A; UPDATE только после 00C-A | Shared real scorer W0→W1 в isolated trial. BUILD не ждёт сигнала. Обязательный rank flip, accepted serving и Student DPO НЕ требуются. |
| Сразу после W1 | [02D](tasks/v7/RP03.02D.md) | Новый PID читает trial W1; атомарность, replay, rollback. Не присваивать trust ради cold proof. Подготовку serializer можно вести раньше. |
| Независимая typed lane | [01A](tasks/v7/RP03.01A.md)→[01B](tasks/v7/RP03.01B.md)→[01C](tasks/v7/RP03.01C.md) | q1/q2→associated OutputRefs→actual TOutput→next Unit; source identity не теряется. |
| Trial JOIN и экзамен | [02C](tasks/v7/RP03.02C.md), [03B](tasks/v7/RP03.03B.md) | 02C BUILD/JOIN не ждёт quality verdict; 03B сравнивает доступный trial scope C0/T до ordinary adoption. Нельзя выдавать scorer-only опыт за end-to-end. |
| Scoped admission / ordinary use | 02C ADOPT, затем [03C](tasks/v7/RP03.03C.md) | Принятый checkpoint и реальный Host/PC. Если acceptance отсутствует, сохраняется старый serving, но trial learning не отменяется. |
| Отдельные владельцы | [01D](tasks/v7/RP03.01D.md), [03A](tasks/v7/RP03.03A.md), [04A](tasks/v7/RP03.04A.md) | Unit ablation; optional RP01 handoff; отдельный exact-SHA Astra review. Не добавлять их перед первым численным уроком. |

**Нет цикла 02D→accepted serving→03B→02D.** 02D доказывает trial durability. 03B может оценивать его изолированно. 02C BUILD связывает реальные consumers без live promotion. 02C ADOPT требует действительную scoped acceptance, а не только записанный файл. Механизм атомарной публикации/rollback принадлежит 02D; решение о допуске не принимается им по факту cold-load. Все семь доменов не становятся обязательным admission первого ограниченного domain deployment; разрешённый scope фиксируется явно.

## 4. Что считать первым уроком

Нужны два F0-lawful сравнимых варианта одного x, независимое реальное наблюдение, pre-outcome phi, версия domain/law/coordinate, настоящий numeric update и reader. `PARAMETER_APPLIED`, `SCORE_EFFECT`, `RANK_CHANGED`, `TRIAL_COLD_CONSUMED`, `DOMAIN_GAIN`, `USER_ADOPTED` — отдельные поля. Корректный шаг может не менять argmax. Одного изменившегося hash также недостаточно: показать формулу, численный delta и reader.

Нет наблюдения — BUILD_READY_NO_SIGNAL, не W1. Нет semantic feature source — точный feature gap, не суррогат CandidateId. Нет native cold execution — BLOCKED_COLD с настоящим error; не имитация процесса и не отключение Smart App Control. Нельзя сбрасывать уже applied feedback и повторять урок ради красивого отчёта.

## 5. Экзамен и минимальные проверки

SOURCE/PLAN/READ_ONLY/EXAM: **0 новых product tests**. Изменённая механика: только минимальное доказательство затронутого риска — F0/type/tenant, finite update, feedback once-only, coherent checkpoint, cancellation/promotion boundary. Переиспользовать имеющиеся checks; не назначать квоту и blanket Windows Actions. Реальную обязательную security/native проверку сохранять в её владельце, а не объявлять её доказательством learning quality.

C0 и T получают **одни и те же held-out cases**, независимые от TRAIN/OP_DEV; код, mapper, lawful candidate set, tokenizer, kNN, graph, judge и бюджет одинаковы. Отличается только заявленный learned factor. P legacy — дополнительный wiring reference, а не замена C0. C#/EN→RU/SQL — pilot; ORCH/DESIGN/ANGULAR/DPO — четыре самостоятельных nonblocking children. Успех C# не удостоверяет семь обученных доменов.

Измерять domain mapping, lawful coverage/accuracy, errors/abstention, качество реального ответа, paired gain/loss, retention, time-to-first-update/benefit, time/CPU/RAM/VRAM/disk и полную стоимость teacher/judge/failed attempts. Guard calls 1+5+3 — source-level возможное количество, не доказанный 9× slowdown. Все probes читают один W/snapshot и не создают уроков.

## 6. Роли и следующий точный запуск

DPO: CODEX **Astra 6 UltraCode** выбирает case в делегированной роли человека; external open Chinese Teacher создаёт обе exact final sides; отдельный CODEX **Astra 6 UltraCode** оценивает их качество. Не переписывать китайское authorship задним числом. Для Flow допустим независимо вызванный назначенный CODEX observer либо настоящий разрешённый downstream outcome; это не self-vote генератора и не обязательное ожидание живого человека. Все model IDs/call IDs должны существовать, иначе NOT_PERFORMED/NO_SIGNAL.

Стартовый prompt:
> Выполни только RP03.02A из tasks/v7/RP03.02A.md. Найди реальный scorer/feature/state seam и минимальный isolated trial caller; не меняй продукт в read-only задаче. Открой указанные source methods и primary, заполни recipient receipt. Не ищи решения всех RP и не запускай Windows coverage. Затем передай точный allowed patch scope в 02B. Отдельная сессия 00C-A собирает настоящий независимый lawful signal.

[Функциональная карта](RP03-V7-OCT16-FUNCTIONAL-CROSSWALK.md) сохраняет остальные требования; [атлас October16](../RP01-RP05-REPAIR-ATLAS-2026-10-15/17-RP03-FIRST-LESSON-READINESS-REPAIR-2026-10-16.md) объясняет причины. Публикация этих инструкций не является выполненным code/learning/PC/Astra опытом.
