# RP01 — как передавать задание модели

Единственный dispatcher — [RP01-EXECUTE-NOW](RP01-EXECUTE-NOW.md). Все 16 canonical parent prompts — [tasks/execution](tasks/execution/README.md); девять самостоятельных stage prompts — [tasks/children](tasks/children/README.md). **Этот файл не добавляет новую очередь C1/C2 и не требует выполнять дополнительные research раунды.**

## Готовые stage packets

| Назначение | Полный промпт |
|---|---|
| Человеческий выбор CODEX Astra 6 UltraCode | [S-SELECT](tasks/children/RP01.02B-S-SELECT.md) |
| Chinese model создаёт обе DPO стороны | [A-AUTHOR](tasks/children/RP01.02B-A-AUTHOR.md) |
| Отдельный CODEX Astra 6 UltraCode оценивает точную пару | [J-JUDGE](tasks/children/RP01.02B-J-JUDGE.md) |
| Только необходимый initial NoUpdate parent | [B0](tasks/children/RP01.03A-B0-BOOTSTRAP.md) |
| Read-only final bindings, без C0 generation | [T0](tasks/children/RP01.03A-T0-CONTROLS.md) |
| Один existing update-and-hold | [T1](tasks/children/RP01.03A-T1-UPDATE.md) |
| Independent CODEX Astra 6 UltraCode, retained blinded answers | [T2](tasks/children/RP01.03A-T2-EXAM.md) |
| Authorised публикация тех же held weights | [T3.P](tasks/children/RP01.03A-T3-PUBLISH-HELD.md) |
| Ordinary cold consumer verification без promotion | [T3.C](tasks/children/RP01.03A-T3-COLD-USER.md) |

Выдать worker ровно назначенную карточку целиком, её actual predecessor handoff и relevant source/skill sections. Не копировать все 9 детей в один prompt, не передавать роли A private exam из T2. Actual grant/model/session и входные hashes дополняются реальными данными владельца, не вымышленными placeholders.

## Когда действительно нужен ещё один child

Только если обнаруженный code/source вопрос разделяется на независимые owned outputs и один worker не может безопасно выполнить его как одну задачу. Сначала выбрать causal/file seam. Не делить исправление только ради количества запусков или мнимой параллельности двух writers одного файла.

Каждый новый leaf получает самостоятельно заполненные поля:

`child_id/parent; CURRENT/NOT; exact input refs и source permissions; owned files/output; actual code method/caller/consumer; selected primary/RAG spans; skills и применимые sections; пошаговый алгоритм; valid/bad/edge examples с причинами; learned recipient или NONE; smallest relevant check/scientific contrast; failure/recovery; effect grant и budget; actual output schema; stop и next owner`.

Это контракт для составителя, **не готовая generic задача**. Заполненный leaf обязан содержать предметные методы/примеры, а не строки «найди sources» и «напиши tests». Передача ребёнку не делает родителя завершённым и не создаёт отдельный model/training grant.

## Пример допустимого разделения 01A

Найден actual defect: response mask включает один prompt target из-за изменившейся границы. Leaf M читает reached tokenizer/material producer, exact framing/version и возвращает конкретное `token-position → target-mask` соответствие плюс boundary counterexample. Leaf C читает reached DpoBatch/consumer и проверяет, что logit[j−1] получает token[j] только для разрешённых response positions. Один integration owner применяет минимальный patch, если он действительно нужен. M/C не авторят DPO и не запускают Student.

GOOD: в illustration `[BOS,p1,p2,y1,y2,EOS,PAD]` выбранная EOS-inclusive политика даёт response targets 3/4/5, а PAD и prompt исключены. BAD: добавить пробел к judged source, чтобы прежний fixture совпал. EDGE: joint BPE merge пересекает границу — не считать `Encode(x+y)=Encode(x)+Encode(y)` без контракта. Examination — ближайший existing boundary probe, не coverage suite. Эти примеры публичные, не реальная TRAIN/SEALED пара.

Если проблема только в одном consumer, M/C делить не нужно. Если source уже исправен, REUSE_NO_CHANGE, 0 новых tests. Полный actual prompt родителя [01A](tasks/execution/RP01.01A.md) остаётся источником предметных деталей; соответствующие 01B/01C/04A–E описывают собственные допустимые seams.

## Передача результата без лишней работы

Все read-only/reuse tasks добавляют 0 product tests. Реальное изменение deterministic risk требует только nearest necessary check. Learning → independent domain/bulk experiment по [методике](LEARNING-EXAMINATION.md), не число assertions. [Governance](GOVERNANCE-AND-REVIEW.md) адаптирует старые skill templates, не отменяя actual safeguards.

При complete held-trial никаких children для нового T1; при partial intent — recovery owner. При T3.C loader failure — конкретная 01C, не новый урок. При no useful gain — сохранить наблюдение и передать новую гипотезу/feedback owner, не переписать examiner rubric.

## История сохранена

Прежние девять C1 design-only examples остаются [в immutable snapshot](https://github.com/lubochka/xiigen-planning/blob/de19e6f25264c7afc353345253d2e249b449b080/llm%20mvp/design%20to%20code%20session/replanning/Kiro-Xiigen-RP01-Learning-First-Slice-Pack/MODEL-READY-CHILD-PACKETS.md), а длинные original task sources не удалены. Это справочная история, не ещё девять обязательных заданий. Четыре incident-only Q0/A0/A1/X документа имеют отдельный статус в children index и не входят в отремонтированные девять основных stage prompts.

Публикация этих инструкций не выполняла subagent, Chinese, CODEX reviewer, Student или PC experiment.
