# RP01 BATCH500 — настоящий Student, малый ранний сигнал, минимум накладных расходов

**Действующая редакция: B500-LIVE-RUN-REPAIR-20261015.** Это контракт slice, не новая версия уже запущенного runtime experiment. [Начать с dispatcher](RP01-EXECUTE-NOW.md) → [одна из пяти карточек](tasks/batch500/README.md) + [общий блок](RP01-BATCH500-EXECUTOR-REPAIR-2026-10-15.md). [Текущий code/artifact snapshot](RP01-CURRENT-CODE-MAP.md) обязателен для определения ближайшего действия.

## 1. Приоритет: уже выданное не выдавать ещё раз

На прочитанном K58 опубликованы actual offline runner, C#500 и frozen Main500 protocol. Поэтому первый шаг — read-only reconciliation исходного runtime owner. Нельзя повторить calibration, создать второй Main, сменить ID для обхода replay marker, переписать dataset или потребовать новый большой exam. Отсутствующий completed в удалённом snapshot не означает clean absence на PC.

**CURRENT_FROZEN:** `simulation-csharp-main500-20261015-01`, exact500, batch10, максимум50 updates, epoch1, fixed B0/reference, continuous optimizer внутри запуска, wall budget `none`. Его original evaluation:25 preference +3 free subset,1 repeat,112 planned explicit scorer forwards/6 generation calls, frozen margin tolerance1e-6. Нет обязательных дополнительных DEV/40+20/Q_law проверок. Сохраняется настоящий original verdict, даже если отрицательный.

**NEXT_CAMPAIGN:** отдельное ещё не начатое исследование на новом domain/material/hypothesis. Только здесь до outcomes можно выбрать другие family splits, semantic rubric, schedule, reference boundary, probes и numerical budget. Плановая диаграмма не меняет возможности CLI. First50 streaming, generic resume/parent continuation и ENRU требуют actual compatible code, а не новой строки в документе.

## 2. Выбор, авторство, оценка и обучение

CODEX с **`astra 6 ultracode`** выполняет человеческую роль выбора учебного случая. Отдельный CODEX с **`astra 6 ultracode`** оценивает точную итоговую DPO-пару. В будущем отдельное Astra назначение может симулировать chosen/rejected внешнего открытого китайского автора — с реальным Astra authorship, а не ложным Qwen label. Student остаётся настоящей локальной моделью, отдельное CODEX Astra назначение экзаменует его actual outputs вслепую.

В существующем500 actual provenance **`simulation`**, обе стороны материализованы программой из семантических шаблонов. Это не автоматический alias прямого ASTRA_SYNTHETIC_DEV model output. Dataset name/заявленная requested model не являются actor receipt. Не приписывать500 строкам500 вызовов. Не переписывать bytes/J/источник начатого опыта. Для нового direct Astra authoring — честный source class и действительно поддержанный admission, без Chinese-only grant spoofing.

Локальный Qwen не вызывается ни как author, ни как evaluator в current development inner loop. В production-target контуре внешняя разрешённая открытая китайская модель снова создаёт обе DPO стороны; CODEX остаётся selector и отдельным J. Возврат provider и проверка его источников — отдельная задача, не скрытый тормоз текущего Student исследования. Отсутствующие права/модельные вызовы не выдумывать.

## 3. Бюджет обучения и реалистичный результат

500 unique accepted pairs/domain — стартовый исследовательский бюджет, **не гарантия освоения домена**. ТекущийC# набор имеет25 параметризованных templates×20; independent family count неизвестен. Отдельно считать unique rows, semantic families, token exposures, applied pairs, committed updates.500 строк не равны500 независимым задачам или500 шагам.

Старый1pair/1step,0/12→0/12,+2/12 и120logical outputs сохраняются в своей истории. Порог+2 не переносится в новый опыт и не превращается в обоснованный прогноз. Нельзя поменять его задним числом и принять отрицательные веса.

Следующий предметный маршрут: завершить/reconcile C#500, затем EN→RU500 через actual consumer/continuation; подготовка второй ветки параллельна. Не ждать всех1000, RP02–RP04 или Windows перед полезной работой. Нет автоматической третьей500-парной кампании. SQL/Angular/Figma/orchestration/DPOBuilder остаются обязательствами прежних owners; два домена их не закрывают.

## 4. Что считается прогрессом

| Наблюдение | Что можно заключить | Чего нельзя заключить |
|---|---|---|
| Exact training transaction/state/readback | Механизм реально изменил и сохранил состояние | Модель стала полезнее |
| Preference margin выросла | Узкий сдвиг предпочтения на указанном panel | Свободный ответ уже правильный |
| Реальный ответ исправил предметную ошибку | Наблюдённый semantic progress на этих cases | Статистически доказанное обобщение на весь домен |
| Новый домен и retention измерены | Есть ограниченное свидетельство переноса/забывания | Все семь доменов и продукт готовы |
| Ordinary API/consumer использует accepted state | Наблюдён конкретный adoption path | Вся Windows/IDE поставка сертифицирована |

Current primary сохраняется точно как frozen25+3. PREFERENCE_ONLY_SIGNAL ценен для диагностики, но не подменяет функциональный результат. Для будущего опыта допустим заранее выбранный Q_law/partial semantic score: небольшой положительный paired change с конкретным исправлением уже полезен для исследования, без обязательного+2/12,95% или большой мощности. Равенство/регресс/неполные данные сообщаются честно. New critical errors не скрываются average. Research continuation и production selection — разные решения.

## 5. Как экономить, не ломая алгоритм

Переиспользовать уже созданные данные, C0, calibration, exact source review, совместимые saved scores/checkpoints. Не вызывать500 раз старый one-pair scientific lifecycle. Pair J выполняется заранее пакетно, E — после над настоящими outputs; Student sampling и Qwen evaluation не вставлять в каждый optimizer step. Existing trainer/reference/LoRA owners сохраняются.

Профилировать один подтверждённый hotspot, а не строить новую общую instrumentation программу. Current step totals около102–111s включают несколько стадий; optimizer-only по ним неизвестен. Save/load и nested checkpoint costs не считать дважды. Hardware/sequence lengths/precision/cache/threading фиксировать. CPU/RAM одного процесса не whole-PC; внешние CODEX costs отдельно. `none` не означает бесконечную эпоху: finite pairs/updates и cooperative cancellation сохраняются. Не вводить произвольный timeout вместо выбора пользователя.

Exact material/response masks, frozen reference, allowed tensor keys, finite values, single writer, durable effect/cursor, full reload и source rights защищаются. Непрерывность optimizer в одном invocation не даёт generic resume после сбоя. При unknown эффекте — original owner, не silent reset.

## 6. Гибридная архитектура сохраняется

Domain/intent/version→semantic token/vector roles→law-aware positive/negative/warning retrieval→learned metric/rerank/adaptive k→real typed consumer/final arbiter. k>1/majority vote не заменяет AST/type/scope, грамматические роли, referent agreement, negation или SQLNULL. Неуверенные inferred признаки — мягкие сигналы, не универсальные запреты.

Tokenizer, semantic projection и LoRA — разные контракты; изменённая schema требует совместимого index/cache. Unit many→many/Flow corrections принадлежат RP03; async Orchestrator/higher-order Graph — RP04; доменная адаптация — RP02. RP01 не создаёт второй trainer и не отменяет эту функциональность. B500.05 связывает владельцев через actual schema/state/caller/gap и один ограниченный causal contrast, а не новую полную матрицу всех сочетаний.

## 7. Проверки и исполнение без нового бюрократического цикла

**Документы/read/reuse:0 новых продуктовых тестов.** Code change: nearest existing observation только для изменённого deterministic риска. Никаких новых coverage quotas/Windows matrix/review pass counts до обучения. Предметный compiler/oracle допустим как проверка конкретного закона, но не learning PASS.

25 исходных исследовательских назначений остаются10+5+5+5, их результаты переиспользуются. Настоящая правка не заявляет их новым выполнением и не запускает их снова. Пять B500 карточек — пять областей исполнения с адресными children, не обязательные пять полных serial reviews. Общий output включает actual effects, quality, costs, limitations и один следующий шаг.

Граница публикации: только planning docs. Ни source K, ни Student weights, ни datasets, ни old/new frozen exam bodies не изменены этой редакцией. Наличие этого контракта не подтверждает Main500 complete, learning gain, full multidomain implementation или отдельный выполненный Astra review.

[Предыдущий generic BATCH500 protocol](https://github.com/lubochka/xiigen-planning/blob/76657d964769914a88556237f28fc47e6d46da01/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-OFFLINE-TEACHER-BATCH500.md) сохранён как история/идеи будущего дизайна, не новые обязательства текущего опыта.
