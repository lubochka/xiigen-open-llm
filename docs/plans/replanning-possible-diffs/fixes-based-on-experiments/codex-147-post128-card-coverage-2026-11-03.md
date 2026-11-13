# Покрытие карточек и редакционная приёмка после P128

Дата: 3 ноября 2026. Проверенные source pins: core `f8784df037b59cf878889eef4d0956f5d0c985e7`, planning `5df24354aa803f7ed5dd3bac48146466e2feb189`; c5e7113 добавляет только handover. Основание — [аудит 145](codex-145-post128-transfer-and-experiment-audit-2026-11-03.md), исполнение — [план 146](codex-146-post128-addressed-repair-plan-2026-11-03.md).

## Согласование перед публикацией

Поверх исходного аудита прочитаны core `b4af9eef70f52020e3b10558fe86287dfb054023` и planning `95f992c1b2add07e25e9858b57e7c9c0a0d12594`. Сохранены новые записи registry/handover и QA-навигация planningREADME. BOM/hash теперь отмечен исправленным по исходнику b4 с сообщённой 36/36 целевой Release-приёмкой; оставшиеся CAS/lateEvaluate/ordinary-store/fresh-process не закрыты. Семь файлов подготовки P129 не менялись и не приняты как исполненный результат. Основной научный sourcepin и новаядельта названы раздельно.

## Объём именно этого изменения

**18 экспериментальных карточек core +10 связанных planning-документов =28 адресных карточных поправок.** Эти файлы уже имели усиления предыдущего аудита; здесь заменён только их действующий верхний блок. Прежние 36 поправок аудита 142 не посчитаны ещё раз. Нижние исторические паспорта/результаты/бюджеты сохранены; новая научная аттестация всех 131 опытов не заявлена.

Дополнительно обновлены навигация, общие документы 02/03/11 и QA46. К опубликованной native-расписке и SESSION-HANDOVER добавлены датированные поправки без удаления исходного текста. Созданы три документа 145–147, planning-сводка и каталог воспроизводимых доказательств. Продуктовые C#/PowerShell, старые raw/weights/gold и научные бюджеты не изменены; два новых Python-файла только проверяют сохранённые входы и пишут производные JSON своего аудита.

## Core: действующий вход и конкретный остаток

| Карточка | Владелец | Адресное исполнение |
|---|---|---|
| [P25-cold-restore.md](../experimets/cards/P25-cold-restore.md#current-executor-task) | `P25-LEARNED-RESTORE-02 / Q125` | [146/r128-04](codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-04) |
| [P105-frozen-reference.md](../experimets/cards/P105-frozen-reference.md#current-executor-task) | `P105` | [146/r128-02](codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-02) |
| [P111-reward-credit.md](../experimets/cards/P111-reward-credit.md#current-executor-task) | `P111` | [146/r128-02](codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-02) |
| [P112-family-disjoint-evaluation.md](../experimets/cards/P112-family-disjoint-evaluation.md#current-executor-task) | `P112` | [146/r128-03](codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-03) |
| [P115-bm25-statistics-cache.md](../experimets/cards/P115-bm25-statistics-cache.md#current-executor-task) | `P115` | [146/r128-05](codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-05) |
| [P117-translation-direction-memory.md](../experimets/cards/P117-translation-direction-memory.md#current-executor-task) | `P117` | [146/r128-04](codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-04) |
| [P118-projectwiki-content.md](../experimets/cards/P118-projectwiki-content.md#current-executor-task) | `P118` | [146/r128-04](codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-04) |
| [P119-learning-dynamics.md](../experimets/cards/P119-learning-dynamics.md#current-executor-task) | `P119` | [146/r128-05](codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-05) |
| [P120-semantic-compression-laws.md](../experimets/cards/P120-semantic-compression-laws.md#current-executor-task) | `P120` | [146/r128-07](codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-07) |
| [P121-project-adapter-selection.md](../experimets/cards/P121-project-adapter-selection.md#current-executor-task) | `P121 / T121-OWNER/BINDING/SHARING` | [146/r128-02](codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-02) |
| [P122-root-channel-unicode-digits-operators.md](../experimets/cards/P122-root-channel-unicode-digits-operators.md#current-executor-task) | `P122` | [146/r128-07](codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-07) |
| [P123-design-conversion-critic-evidence.md](../experimets/cards/P123-design-conversion-critic-evidence.md#current-executor-task) | `P123` | [146/r128-05](codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-05) |
| [P124-p3-fixed-discriminator-contract.md](../experimets/cards/P124-p3-fixed-discriminator-contract.md#current-executor-task) | `P124` | [146/r128-04](codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-04) |
| [SAVED-STATE-PANEL-0407-01.md](../experimets/cards/SAVED-STATE-PANEL-0407-01.md#current-executor-task) | `SAVED-STATE-PANEL-LEARNED-02 / Q126` | [146/r128-05](codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-05) |
| [KNN-ORDER-PERMUTATION-01.md](../experimets/cards/KNN-ORDER-PERMUTATION-01.md#current-executor-task) | `KNN-ORDER-PERMUTATION-01 / Q127` | [146/r128-07](codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-07) |
| [PHRASE-COVERAGE-CAP-01.md](../experimets/cards/PHRASE-COVERAGE-CAP-01.md#current-executor-task) | `PHRASE-COVERAGE-CAP-01 / Q128` | [146/r128-04](codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-04) |
| [HELDOUT-ADMISSION-ATOMICITY-01.md](../experimets/cards/HELDOUT-ADMISSION-ATOMICITY-01.md#current-executor-task) | `HELDOUT-ADMISSION-ATOMICITY-01 / Q129` | [146/r128-01](codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-01) |
| [METHOD-KNN-SCOPED-EXPRESSION-TARGET-01.md](../experimets/cards/METHOD-KNN-SCOPED-EXPRESSION-TARGET-01.md#current-executor-task) | `METHOD-KNN-SCOPED-EXPRESSION-TARGET-01 / Q130` | [146/r128-03](codex-146-post128-addressed-repair-plan-2026-11-03.md#r128-03) |

В каждом из 18 файлов ровно один `current-executor-task`, один новый верхний блок и полный передаваемый prompt с конкретным consumer, honest примером, адресной порчей, влиянием на итог и границей расхода. P127/P128 имеют RESULT_RECORDED; Q129/Q130 сохраняют исходные дозы. Сообщение handover о работе над Q129 не становится ни записанным результатом, ни новым назначением. Старый pin уже активной попытки остаётся её паспортом.

## Planning: связанные документы

| Файл | Связанный core-владелец |
|---|---|
| [03-DPO-PAIR-AND-GRADIENT.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/03-DPO-PAIR-AND-GRADIENT.md) | [P121-project-adapter-selection.md](../experimets/cards/P121-project-adapter-selection.md#current-executor-task) |
| [07-CONTINUAL-REPLAY.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/07-CONTINUAL-REPLAY.md) | [P112-family-disjoint-evaluation.md](../experimets/cards/P112-family-disjoint-evaluation.md#current-executor-task) |
| [10-TRANSLATION-UNIT-AND-RESTORE-EDGE-GAPS.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/10-TRANSLATION-UNIT-AND-RESTORE-EDGE-GAPS.md) | [P25-cold-restore.md](../experimets/cards/P25-cold-restore.md#current-executor-task) |
| [22-INDEPENDENT-KNN-STATE-IDENTITY-AND-CONFIDENCE-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/22-INDEPENDENT-KNN-STATE-IDENTITY-AND-CONFIDENCE-2026-10-16.md) | [P117-translation-direction-memory.md](../experimets/cards/P117-translation-direction-memory.md#current-executor-task) |
| [27-MICRO-PROTOCOL-SEQUENCE-AND-NEGATION-LEARNING-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/27-MICRO-PROTOCOL-SEQUENCE-AND-NEGATION-LEARNING-2026-10-16.md) | [KNN-ORDER-PERMUTATION-01.md](../experimets/cards/KNN-ORDER-PERMUTATION-01.md#current-executor-task) |
| [27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/27-NEW-SEQUENCE-SPLIT-CONFOUNDING-AND-NEGATION-KNN-GAPS-2026-10-16.md) | [KNN-ORDER-PERMUTATION-01.md](../experimets/cards/KNN-ORDER-PERMUTATION-01.md#current-executor-task) |
| [39-NEW-DPO-EXACT-MATERIAL-SELF-HASH-VS-MODEL-TOKENIZER-IDENTITY-GAP-2026-10-16.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/39-NEW-DPO-EXACT-MATERIAL-SELF-HASH-VS-MODEL-TOKENIZER-IDENTITY-GAP-2026-10-16.md) | [P121-project-adapter-selection.md](../experimets/cards/P121-project-adapter-selection.md#current-executor-task) |
| [54-DESIGNTOCODE-CLEAN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/54-DESIGNTOCODE-CLEAN-CAUSAL-MICROEXPERIMENTS-2026-10-19.md) | [P123-design-conversion-critic-evidence.md](../experimets/cards/P123-design-conversion-critic-evidence.md#current-executor-task) |
| [59-02-EXPERIMENT-LEARNING-PATH-COVERAGE-AND-INTEGRITY-2026-10-19.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/59-02-EXPERIMENT-LEARNING-PATH-COVERAGE-AND-INTEGRITY-2026-10-19.md) | [PHRASE-COVERAGE-CAP-01.md](../experimets/cards/PHRASE-COVERAGE-CAP-01.md#current-executor-task) |
| [59-CAUSAL-LEARNING-PASSPORTS-AND-EXECUTOR-PROMPTS-2026-10-19.md](https://github.com/lubochka/xiigen-planning/blob/llm_plan3/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/59-CAUSAL-LEARNING-PASSPORTS-AND-EXECUTOR-PROMPTS-2026-10-19.md) | [P119-learning-dynamics.md](../experimets/cards/P119-learning-dynamics.md#current-executor-task) |

## Что проверено и что остаётся открытым

Проверка пакета выполняется по [машинной расписке](../../../../experiments-v2/_audits/post128-2026-11-03/package-verification.json), независимый вывод — [package-review.md](../../../../experiments-v2/_audits/post128-2026-11-03/package-review.md). Она различает редакционный результат и продуктовую приёмку:

- Вне текущих карточных блоков исторический текст сохранён; native-расписка и handover расширены добавлением. Основные 131 строка очереди и их 128RESULT_RECORDED не переписаны, порядок переносов остаётся Q01(+R17)→Q02→Q03→Q06.
- Новые ссылки проверены относительно доступных Git-деревьев двух репозиториев и собственных новых файлов, включая anchors. Проверка ссылок не подтверждает содержание всех целей и не считается научным аудитом всех документов.
- Два производных пересчёта повторяются на тех же сохранённых входах; scripts проверяют source hashes экспериментальных файлов. Каждый результат помечен saved/static, число новых Train/model/native calls равно 0.
- Расписка среды содержит действительно предпринятые build/discovery и PowerShell availability с exit127. Поэтому свежие C#/PowerShell тесты не выполнены, source defect не объявляется закрытым и новый продуктовый PASS не выдан.
- Независимый review проверяет соответствие находок, плана и карточек. Замечания о несовпадающих именах evidence-файлов и числе lineage-контролей устранены: независимые шесть случаев сохранены отдельно от четырёхслучайной проекции recount.py.

При публикации применяется обновление ветки с ожидаемым текущим SHA, без force; собственные изменённые пути накладываются на действующее дерево, посторонние коммиты сохраняются. Успех публикации не расширяет source scope научных выводов. Состояние публикации и проверенный удалённый commit сообщает итоговый ответ; этот документ не содержит самоссылочного будущего commit SHA.
