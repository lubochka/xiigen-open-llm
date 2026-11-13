# RP01 — 9 основных дочерних карточек и 4 incident-only карточки

Единственный порядок: [RP01-EXECUTE-NOW](../../RP01-EXECUTE-NOW.md). [Правила задач](../AGENTS.md), [родители](../execution/README.md), [научная методика](../../LEARNING-EXAMINATION.md). Число файлов — inventory, не обязательные последовательные запуски. Все перечисленные prompts опубликованы; runtime/model calls этой редакцией не выполнены.

## Основные роли и этапы

| Child | Когда нужен | Собственный результат |
|---|---|---|
| [S-SELECT](RP01.02B-S-SELECT.md) | Нет подходящего selected TRAIN case | CODEX Astra 6 UltraCode выполняет человеческий выбор; TRAIN-only immutable handoff |
| [A-AUTHOR](RP01.02B-A-AUTHOR.md) | Нет пригодного saved authored результата; actual grant есть | Разрешённая открытая китайская модель создаёт **обе chosen/rejected** стороны; resolver-first |
| [J-JUDGE](RP01.02B-J-JUDGE.md) | Точная пара ещё не получила matching genuine J | Отдельный CODEX Astra 6 UltraCode оценивает без изменения bytes; admission отдельно |
| [B0-BOOTSTRAP](RP01.03A-B0-BOOTSTRAP.md) | Parent действительно отсутствует; eligible pair и bootstrap grant есть | Existing owner создаёт InitialBaselineNoUpdate, не optimizer step |
| [T0-CONTROLS](RP01.03A-T0-CONTROLS.md) | Pair/parent существуют, intent отсутствует | Read-only final bindings, full-LoRA metadata и frozen science; **без model allocation/дополнительного C0** |
| [T1-UPDATE](RP01.03A-T1-UPDATE.md) | Ready inputs, clean absence intent, genuine effect grant | Один existing train-and-hold: C0, update, retained Trial/raw evidence |
| [T2-EXAM](RP01.03A-T2-EXAM.md) | Complete held-trial существует | Independent CODEX Astra 6 UltraCode оценивает saved blinded ответы; не новое sampling |
| [T3-PUBLISH-HELD](RP01.03A-T3-PUBLISH-HELD.md) | Genuine positive science/native и publication authority | Host публикует **те же held weights**, без optimizer/calibration |
| [T3-COLD-USER](RP01.03A-T3-COLD-USER.md) | Actual publication receipt есть | Новый ordinary процесс использует accepted state; verifier не продвигает модель |

Complete held→T2. Partial intent/unknown effect→existing recovery, не очистка/новый UUID/replay. Existing compatible parent/eligible pair не пересоздаются. Новый core source anchor `025c42c` описан в [code map](../../RP01-CURRENT-CODE-MAP.md): full-LoRA startup metadata не является training/continuation grant и не требует ещё одной model preparation в T0.

## Четыре независимо выбираемых incident-only карточки

| Child | Trigger и допустимый эффект | Чего не делает |
|---|---|---|
| [Q0](RP01.00B-Q0-503-AND-MANIFEST-TRIAGE.md) | Один observed incident, minimal missing read observation | Не требует inference smoke после любого inventory |
| [A0](RP01.02B-A0-QWEN-INFERENCE-SMOKE.md) | Существенно неизвестен inference outcome; genuine diagnostic grant | Не более **одного** generation request; нет retry, DPO авторства или переноса старого deadline |
| [A1](RP01.02B-A1-TEACHER-BINDING.md) | Actual provider/adapter binding mismatch; read-only mapping/proposal | Не требует A0 и не создаёт пару; authoring остаётся только A |
| [X](RP01.02B-X-ASTRA-ULTRACODE-ADVISOR.md) | Один нерешённый вопрос; настоящее separate CODEX Astra 6 UltraCode advisory appointment | Не запускает probes, не подменяет S/J/T2/05A и не создаёт новый общий review cycle |

Эти четыре тела прочитаны и адресно переписаны в продолжении ремонта 15.10.2026. Это **авторский source/prompt review**, не отдельная независимая Astra приёмка и не проверка фактического PC. Их старые версии доступны в Git history. Нет обязательного Q0→A0→A1→A маршрута: существующие пригодные evidence переиспользуются, здоровый authoring идёт сразу A, already-authored pair→J, held→T2.

## Приёмка каждого ребёнка

Карточка содержит inputs, applicable sources/skills, шаги, own effects/output, GOOD/BAD с объяснением, edge/recovery, falsifier и stop/next owner. READ_ONLY/REUSE_NO_CHANGE → 0 новых product tests. Changed deterministic risk проверяет code owner; качество обучения — независимый domain-law exam, не Windows coverage.

Private exam/gold/rubric не передаются S/A/J; authorised T2/custodian читают evaluation-only материалы. Actual model/session/authority записываются отдельно от requested label. Отчёт различает source/implementation, effect/state, science, publication и adoption. NOT_PERFORMED/NOT_MEASURED не заменяются успехом по наличию файла.
