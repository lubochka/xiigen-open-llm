# Конечный срез 256f603e: отдельный N63C-PROBE и уже принятый кандидат

**Конечный source cutoff аудита — `256f603e70032c11b1f4ffabbdd1fefdab642ea5` от 21 октября 2026.** После этого снимка не выполняется бесконечное преследование новых HEAD. Основной корпус на fb9175cc и позднее уточнение d962f958 сохраняются как отдельные слои доказательств. Здесь прочитаны только новая сохранённая A/B проба N63C, её признание кандидатом и изменившийся execution/recovery protocol. Собственных build, Train, inference, load или повторов в аудите —0.

## 1. Не смешивать два разных сохранённых исполнения

| Артефакт | Фактический статус | Что сохраняется |
|---|---|---|
| Исходный `experiments-v2/RP01-MICRO/N63C/results/` | Частичный A:1 DPO+1 SFT+18 side-score; B не начиналось; поздний RNG-save отказ | Raw, claim и checkpoint-файлы остаются. Ошибочные raw counters0 не отменяют выполненное обучение. Старый source Bridge не объявляется исправленным только по новой карточке. |
| Отдельный `experiments-v2/RP01-MICRO/N63C-PROBE/results/` | Новый сохранённый A/B probe; точные выполненные stages, budgets и панели приведены в independent readback | Его показатели нельзя приписать прежнему A-only запуску, считать повторным чтением того же trial или переносить на всю production цепочку. |
| `N63C-PROBE/results-02/` | Отдельные оставшиеся артефакты; наличие completion/result нельзя выводить из существования каталога | Фактическая граница подтверждённых данных указана в [probe review](evidence/core-final-n63c-probe.md). Повторное выполнение здесь не назначено. |

Источники: [исправленный исходный report](https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/experiments-v2/RP01-MICRO/N63C/results/report.md), [отдельный probe report](https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/experiments-v2/RP01-MICRO/N63C-PROBE/report.md), [probe raw](https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/experiments-v2/RP01-MICRO/N63C-PROBE/results/n63c-probe.json). Старое evidence с `UNKNOWN` о probe было frozen readback d962; им нельзя отменять новые receipts на256. И наоборот, новая probe не достраивает отсутствующее B исходного N63C.

## 2. Полезный результат по предпочтению уже есть

На T1 после DPO margin был около **+1.125295**. SFT на старом rejected тексте в A опустил его до **−0.119724**; SFT на исправленном тексте в B сохранил около **+1.115156**. Это наблюдаемый полезный результат по оси сохранения предпочтения в данном C# fixture, даже без роста accuracy. Точные raw значения, NLL, ranks, token counters, все Q1/Q2 изменения и бюджеты — в [независимом разборе](evidence/core-final-n63c-probe.md).

Сохранённый завершённый probe содержит **600 acquisition SFT updates +100 SFT вA +100 SFT вB +2 DPO**, всего800 SFT и2 DPO. Число правильных T1 response tokens после вмешательства ухудшилось: **A22→20/23, B22→21/23**. Поэтому сохранение предпочтения вB — отдельный положительный эффект, рядом с падением token accuracy в обоих плечах.

При этом в before-панели TRAIN были **22/23 правильных response tokens, false whole-response exact; на экзаменационных примерах —0 правильных токенов**. У stale A ухудшается Q2. Точные after-панели сохраняются в raw/readback; partial-token успех не переименован в полный ответ. Улучшение margin не доказывает общий навык, перенос на другие домены или достижимость той же DPO→SFT коллизии на одном артефакте в production. Изменения остальных показателей и ограничения показываются рядом с gain.

В `required-changes.md` и отдельном candidate core результат уже признан автором. Поэтому статус этого пункта — **ALREADY_DOCUMENTED_CANDIDATE_PRESERVE**: данный пакет добавляет проверку доказательств и границ, а не приписывает себе обнаружение чужого нового probe. Положительный результат не отменяется из-за недостаточной whole-answer accuracy. [Разбор принятия кандидата](evidence/core-final-candidates.md), [исходный candidate](https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/experiments-v2/RP01-MICRO/N63C/results/candidate.md).

## 3. Что из предыдущих кандидатов исправлено документно, а что остаётся открытым

- Исходные RNG filename mismatch, выполненные1+1 updates и ошибочные0counters теперь описаны авторами в отчёте и candidate. Это **документное согласование**; runtime repair первоначального Bridge и проверенный новый полный Save/Load цикл из этого факта не следуют.
- Формулировка d962 «полезного измеренного выигрыша нет» больше не является текущим итогом core: gain признан. Наша критика в10 остаётся историческим объяснением, а актуальный статус находится здесь.
- Стоимость неполного Save, область scoring CPU/alloc и недоступные финальные state checks исходного partial A остаются ограничениями его raw; новый probe не заполняет их задним числом.
- Production candidate требует точного writer→state→consumer доказательства коллизии на одном адаптере и согласованного решения пользователя. Изолированный probe сам по себе не разрешает менять product code, протаскивать экспериментальные исключения в prod или запускать полную тестовую цепочку.
- **Новая подтверждённая проблема воспроизводимости N63C-PROBE-SOURCE-PIN:** completed raw относится к600 acquisition и100 SFT/arm, а сохранённый на том же cutoff `N63CollisionProbe.cs` уже содержит `MaxAcquireUpdates=60`, `ArmSftUpdates=20`. Текущий исходник нельзя выдавать за точный код завершённого raw. Для будущего воспроизведения нужен исходный source receipt/снимок именно этого запуска либо явное признание его отсутствия; менять старые counts или запускать заново ради согласования запрещено в этой фазе. `results-02` рассматривается отдельно, без присвоения ему завершённого результата.

Точные уже изменённые документы, ссылки на code и оставшиеся границы — в [candidate review](evidence/core-final-candidates.md) и [probe review](evidence/core-final-n63c-probe.md).

## 4. Очередь, recovery и полномочия этой сессии

Маршрут core сверяется по [index256](https://github.com/lubochka/llm_mvp_core/blob/256f603e70032c11b1f4ffabbdd1fefdab642ea5/docs/plans/replanning-possible-diffs/experimets/cards/00-execution-index.md); конечный порядок и состояние N63C уточнены в [policy review](evidence/core-final-policy.md). Наш [сводный реестр](04-registry.md) и [dispatcher](data/resolved-dispatch.json) сохраняют три уже интегрированные proposals без дубликатов и две новые отдельные SiLU/HNSW гипотезы.

Все47 ordered tuples (номер,ID,card link) совпадают сd962; NEXT=PA01 сохраняется. Но сам index конфликтует по счётчикам: L7 ещё9saved/38unused, L15 уже10saved-or-partial/37unissued. По строкам10 core ID имеют сохранённую/частичную попытку (включаяN63C),37 ещё не выданы; с нашими двумя предложениями это39 невыданных позиций плюс существующая открытая N63C цель, всего40 документных позиций. Эти числа не объявляются количеством всех runtime attempts: отдельные probes считаются по своим receipts.

Два local useful-axis пропуска у N63I/N63R на256 **закрыты новой вставкой**: any useful gain, необязательный рост accuracy, регрессии рядом; для learning-validity указан отдельный критерий. Исторические169/171 относятся кfb и не переписываются. Наши addenda06 сохраняются как самостоятельные правила и история исправления, а утверждение10 о сохранявшемся omission относится только кd962.

Новый protocol30 различает неудачную попытку и незакрытую учебную цель. Его общие инструкции о ремонте/recovery не расширяют нынешнее поручение: **здесь только документация**, не выдана команда на повтор, восстановление модели, обучение или правки protected. Для будущего отдельно порученного исполнения применяется конкретный согласованный recovery contract с сохранением старых артефактов; текущая документационная граница не превращается в универсальный запрет разумного ремонта экспериментов.

Live STOP/NEXT имеет немедленный приоритет. При NEXT не продолжать предыдущий report/hash/review/Git/retry; для одного назначенного ID NEXT=END. Исходные claims/results не удалять. Указания репозитория изменить protected не отменяют прямой пользовательский запрет этой сессии.

## 5. Границы последней проверки

[Compare253путей](data/final-core-delta-files.json) содержит224 protected-document changes и лабораторные N63C-PROBE артефакты. Повторного полного содержательного чтения всех карточек не заявляется. Три адресных отчёта сохранены отдельно: [policy](evidence/core-final-policy.md), [learning/raw](evidence/core-final-n63c-probe.md), [candidate/status](evidence/core-final-candidates.md). Исторические1324 byte matches и163cards/8prompts относятся к исходному срезу; это не подсчёт всего текущего корпуса.
