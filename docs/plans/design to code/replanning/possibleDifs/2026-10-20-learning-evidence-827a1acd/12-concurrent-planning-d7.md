# Последнее объединение с параллельным пакетом d7c41ad3

**Актуальный документированный порядок: 42 отдельных ID, 7 завершённых, 35 будущих. NEXT — №08 `N63-INGRESS-LENGTH`.** Core cutoff остаётся `6e3e8864933e35fccdb4f21369c9912fa195aaf6`. Во время публикации planning перешёл на `d7c41ad3b306a2d94163d3a013a79edb4369626f`; его файлы и полный README сохранены. Ни одна ранее опубликованная карточка не удалена и не заменена этой работой.

Это адресное дополнение к [сверке core6e3/planningeaed](11-late-reconciliation-6e3-eaed.md), где были 40 ID. Состав source-аудита и семи измеренных кандидатов не меняется. Новое объединение добавляет две уже опубликованные другим агентом самостоятельные гипотезы. Старые counts 40/33 в закреплённых review и снимках относятся к моменту до этого объединения; они не являются текущим поручением.

## Два новых peer фактора сохранены с авторством

Источник — [параллельный пакет training-delta-669477](https://github.com/lubochka/xiigen-planning/tree/d7c41ad3b306a2d94163d3a013a79edb4369626f/llm%20mvp/design%20to%20code%20session/replanning/possibleDifs/2026-10-20-training-delta-669477).

| ID | Точный учебный фактор | Почему это отдельная проверка |
|---|---|---|
| VT-APOSTROPHE-01 | `QualityVerifierUnit.TryParseRequiredTerm`: полностью сохранить required target с внутренним апострофом, например `brother's`, при настоящем TrainOnExample | Меняет writer/parser учебного требования. JSON-OPTIONS меняет lifetime десериализатора Executor и сохраняет targets; witness Pass=true относится к доказательности checkpoint-test, а не этому parser. |
| RMS-ROW-REDUCE-01 | `TensorOps.SumAlongAxis` для последней оси с keepDims: последовательное суммирование без координатных массивов внутри native SFT/LoRA | Не меняет Transpose/attention, MultiplyBroadcast, AccumulateGrad или начальный Tensor.Backward. Совпадение слова «массив» не делает эти операции одним A/B фактором. |

Оба — **DESIGN_ONLY_NOT_MATERIALIZED / NOT_RUN**. Этот аудит не присваивает им измеренные улучшения. Проверены только их registry/map, две точные карточки и два промпта. Полный новый peer корпус повторно не аудировался. [Адресная сверка](reviews/planning-d7-final-delta.md), [машиночитаемые данные](evidence/planning-d7-final-delta.json).

## Очередь и обязательные переходы

Объединение по буквальным ID: 40 нашего предыдущего пакета + 40 d7 − 38 общих =**42**. Первые 34 совпадают; для четырёх peer факторов сохранён уже опубликованный d7 порядок. Два собственных новых фактора добавлены после них, перед readers:

| № | ID | Эффективный NEXT |
|---:|---|---|
| 35 | GRAD-ACCUM-COPY-01 | FROZEN-BROADCAST-MAP-01 |
| 36 | FROZEN-BROADCAST-MAP-01 | **VT-APOSTROPHE-01** |
| 37 | VT-APOSTROPHE-01 | RMS-ROW-REDUCE-01 |
| 38 | RMS-ROW-REDUCE-01 | **ATTN-TRANSPOSE-STRIDES-01** |
| 39 | ATTN-TRANSPOSE-STRIDES-01 | JSON-OPTIONS-REUSE-01 |
| 40 | JSON-OPTIONS-REUSE-01 | IDENTITY-M1-01 |
| 41 | IDENTITY-M1-01 | KN-ROUTER-LEARNED-KEY-01 |
| 42 | KN-ROUTER-LEARNED-KEY-01 | END |

**Этот override распространяется на старые card, prompt и contract одновременно.** FROZEN→ATTN из нашего промежуточного 40-ID снимка заменён на FROZEN→VT; RMS→IDENTITY из d7 заменён на RMS→ATTN. Иных изменений учебного состава, данных, моделей и бюджета этих карточек нет. При поручении одного ID после его завершения `NEXT=END`.

Модель исполнителя — default текущей сессии; отчёт русский, формат/skills заданы [контрактом](05-execution-contract.md). Существующие claims/outputs сохраняются. При команде «следующий» немедленно прекратить прежний опыт, включая report/hash/review/Git/retry; при «остановить всё» прекратить работу. Нового запроса разрешений для текущей документной публикации не требуется.

## Выходы и сохранность

В [output-path-map.json](output-path-map.json) 35 будущих назначений с уникальными result roots. Для VT используется уже назначенный host `C:/Projects/llm_mvp_core/experiments-v2/RP01-MICRO/QVA01` и output `C:/xexp/QVA01/a01`; для RMS — `C:/xg/RD01/a001/results`. Точная команда, source-проект, report/candidate и порядок подготовки взяты из карточек, а не изобретены исполнителем. Старые корни 29 будущих core опытов сохранены.

Семь результатов остаются завершёнными. KN использует исправленный canonical core6e3 observer; EOPT — сохранённый raw вместе с erratum. LF03 сохраняет partial first-token учебный успех, ресурсные улучшения, регрессии и ограничения происхождения; повтор для заполнения полной response-панели не назначен.

При публикации проверяются все изменённые пути и hashes; весь d7 tree используется как неизменяемая основа. Новые изменения ограничены папкой possibleDifs и добавочной шапкой её README. Это документация результатов и кандидатов, без новых native запусков, production исправлений и записей в protected каталог.
