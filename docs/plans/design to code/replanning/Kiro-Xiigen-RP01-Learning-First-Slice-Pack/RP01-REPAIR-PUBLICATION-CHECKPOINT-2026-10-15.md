# RP01 — публикация №5: продолжение после сохранённого основного пакета

**15 октября 2026 · DOCUMENTATION_REPAIR_PUBLISHED / DRAFT.** Target: `lubochka/xiigen-planning:llm_plan3`, существующий RP01 pack. Основной пакет был опубликован в [checkpoint 4](https://github.com/lubochka/xiigen-planning/blob/dea8436862fa6710e67952e832e0b8796cac0d4b/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-REPAIR-PUBLICATION-CHECKPOINT-2026-10-15.md); после проверки его наличия работа продолжена и новые исправления также опубликованы. Это не новый цикл R01–R25 и не утверждение выполненного обучения.

[Начать исполнение](RP01-EXECUTE-NOW.md) · [Готовый стартовый промпт](INSTALL-AND-START.md) · [16 основных заданий](tasks/execution/README.md) · [Дочерние роли/диагностика](tasks/children/README.md) · [Методика обучения](LEARNING-EXAMINATION.md) · [Current code map](RP01-CURRENT-CODE-MAP.md).

## Продолжение: восемь реальных commits, не только статусный файл

| Артефакт | Что исправлено | Commit |
|---|---|---|
| [Q0](tasks/children/RP01.00B-Q0-503-AND-MANIFEST-TRIAGE.md) | Один недостающий read observation; inventory не требует обязательного smoke | `35dde37eb6958adb4a777c2b485621ff3b9f287c` |
| [A0](tasks/children/RP01.02B-A0-QWEN-INFERENCE-SMOKE.md) | Максимум один diagnostic generation, без встроенного retry и копирования production deadline | `a5678cb807f81ad90204edf7edc2871081cf108e` |
| [A1](tasks/children/RP01.02B-A1-TEACHER-BINDING.md) | Только read-only binding diagnosis/proposal; A0 не обязателен, pair authoring остаётся у A | `441bf2b4ade8112edfbf6b7721b533e9cc34427e` |
| [X](tasks/children/RP01.02B-X-ASTRA-ULTRACODE-ADVISOR.md) | Один нерешённый вопрос; советник не запускает probes и не подменяет S/J/T2/reviewer | `10a8e32fe28eef8f210eaee28f3a1a24b03d2dd8` |
| [Code map](RP01-CURRENT-CODE-MAP.md) | Учтена новая core delta full-LoRA checkpoint startup; old scientific anchors сохранены | `e064f295588925152d2b32c97e933b4910934b66` |
| [T0](tasks/children/RP01.03A-T0-CONTROLS.md) | Проверка existing full-LoRA metadata, без model allocation/import или второго C0 | `d85e65fb1a736c068c5a8fde159bd5edc1d2844a` |
| [Children index](tasks/children/README.md) | Девять core и четыре теперь source-reviewed incident prompts; нет Q0→A0→A1→A waterfall | `62eb8d76e82195687e125516bf1d76a463ac8eb0` |
| [Manifest 3.1](PACKAGE-MANIFEST.json) | Согласованы diagnostic role/effect limits, source anchor и count/dispatch metadata | `a714fc05ba791af11a60bef10a3d8203ed190150` |

GitHub compare `dea8436..a714fc0` подтвердил восемь commits и ровно эти восемь изменённых файлов. Изменения находятся только внутри RP01; product code и чужие RP/Rxx файлы этим продолжением не переписывались.

## Конкретные ошибки, найденные после checkpoint 4

A0 одновременно обещал один запрос и разрешал ещё один retry. A1 требовал A0 receipt даже для read-only source examination и сам создавал pair, дублируя canonical A. Q0 автоматически направлял API inventory к smoke. X превращал один incident в полный набор команд, DPO/PC/domains review. Эти четыре тела теперь заменены конкретными bounded prompts с inputs, allowed effects, источниками/skills, GOOD/BAD/EDGE, recovery, falsifier и output. Никаких дополнительных обязательных stages перед первым уроком.

## Новый core source anchor — не выполненная модель

При продолжении прочитан K branch ref **`025c42c74b1d30ffa4660f9824a52cf8fcaf7c88`**, следующий после `97e4af3`. GitHub compare указал 17 touched files. Полностью открыты `CodeDocumentationInitialTrainingConfiguration.cs`, `RepositoryOwnedFullLoraCheckpointDescriptor.cs`, `FullCheckpointLoraTopology.cs`; relevant raw-tool README перечитан. Другие changed files не объявляются полностью аудированными.

Практическое следствие: current startup может использовать exact saved full-LoRA root descriptor, model/topology/seed/hash metadata. Descriptor не означает model import, optimizer/continuation permission или learning. T0 читает существующее binding evidence; `CreateModelForLoad` и import не выполняются в read-only task. Missing identity/topology input разрешает только адресное исправление владельцем, не новый B0, удаление adapter keys или общий repeat preflight. Scientific lifecycle/policy не менялись в этой core delta; сохранённые старые held outcomes не переписываются новым code SHA.

## Проверка удалённых артефактов

Проверенный planning candidate: **`a714fc05ba791af11a60bef10a3d8203ed190150`**. Этот checkpoint добавляет сводку после него, не меняя перечисленные рабочие файлы.

| Проверка | Наблюдение | Что это доказывает |
|---|---|---|
| `tasks/execution` | Git tree `2f2c4b1accb9efdf7ad431ddcf1ef280923dea8f`, `truncated=false`, 16 RP01 cards + README | Основной список сохранён, не превращён в новые mandatory runs |
| `tasks/children` | Git tree `90cb3ae8f662cf96a5b7ce0e54a13658b7883981`, `truncated=false`, 9 core +4 incident +README | Все перечисленные child paths существуют; новые blob IDs совпадают с write results |
| Manifest | Удалённый текст прочитан полностью в двух диапазонах; blob `807020b265d3b7e77e708f541ed51eaaec347cd3` | Counts, paths, single dispatch, role/effect и source anchors согласованы с trees |
| Publication delta | GitHub compare: восемь commits/восемь RP01 файлов | Изменения действительно записаны, не остались в неопубликованном Git tree |

Это авторская source/prompt/remote-identity проверка. Полный автоматический аудит всех historical/source links, всего K, всех skills, WorkLeft/RAG и независимая model review не заявляются. Git paths/blob matches подтверждают публикацию и идентичность, не научный результат.

## Неизменные роли и кратчайший следующий runtime шаг

**CODEX с запрошенной Astra 6 UltraCode выполняет человеческий выбор урока. Внешняя разрешённая открытая китайская модель создаёт обе final chosen/rejected стороны. Отдельный CODEX Astra 6 UltraCode оценивает точную пару.** Actual runtime model/session/authority подтверждаются отдельно; T2 — ещё одно независимое назначение. Ни A0, ни A1, ни X не получают эти роли автоматически.

Следующий исполнитель использует фактический state-handoff либо 00B read-only. Complete held→T2 над уже сохранёнными ответами. Unknown/partial intent→recovery, не повтор optimizer. Только отсутствующий effect при готовых разрешённых inputs ведёт к одному existing train-and-hold. T3.P публикует те же оценённые weights; T3.C отдельно проверяет ordinary cold adoption.

READ_ONLY/REUSE_NO_CHANGE → **0 новых product tests**. Изменённый deterministic риск → ближайшая необходимая проверка, без coverage/Windows matrix/Tier3/30–50-pass ритуала. Actual source/privacy/native/release safeguards сохранены. Domain mapping, law-conditioned adaptive kNN, semantic token/projection, Unit/Flow/Orchestrator/Graph и continual-PC эффективность остаются явными отдельными functional expectations; C# E1 не доказывает весь hybrid scope.

## Граница результата

Опубликованы исправленные документы и manifest. Мы не запускали Teacher/Student/CODEX модели, не меняли product code, Student weights/DPO data, private exam/policy или настройки PC, не восстанавливали runtime state. **Learning gain, PC feasibility, independent Astra acceptance и ordinary cold adoption: NOT_PERFORMED/NOT_MEASURED.** Реальное выполнение и отдельная 05A экспертиза относятся к своим заранее указанным scope/владельцам; публикация плана не выдаёт им фиктивный PASS.
