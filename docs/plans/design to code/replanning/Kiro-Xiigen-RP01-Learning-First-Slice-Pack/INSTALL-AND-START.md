# RP01 — старт без повторного планирования и лишних запусков

Редакция 15 октября 2026. Этот pack не требует установки нового trainer/framework или запуска Windows Actions для чтения плана. Product runtime готовится только его уполномоченным владельцем по действующему K/tools/Rp01RawDpoLesson README. **Один порядок: [RP01-EXECUTE-NOW](RP01-EXECUTE-NOW.md).**

## Промпт для следующего исполнителя

> Ты — исполнитель одного bounded задания существующего XIIGEN RP01. Цель: дойти до самого раннего разрешённого реального DPO-урока Student, сохранить научное наблюдение и продвинуть только действительно оценённые веса. Не запускай заново R01–R25, не строй второй trainer и не выполняй все 16 карточек по порядку.
>
> Прочитай действующие repo instructions и pack `tasks/AGENTS.md`, затем `RP01-EXECUTE-NOW.md` и имеющийся state-handoff. Зафиксируй actual P=`xiigen-planning:llm_plan3`, K=`llm_mvp_core:codex/dpo-implementation` и относящиеся blobs. Если state-handoff отсутствует, выполни `tasks/execution/RP01.00B.md` READ_ONLY. Независимый 02A может работать параллельно, не меняя твои state/files. Не называй отсутствие доступа отсутствием состояния.
>
> Выбери ровно одно следующее действие: нет допущенной пары — 02B S/A/J; нет совместимого parent — отдельно разрешённый B0; inputs готовы и intent отсутствует — T0 и затем отдельно разрешённый T1; complete held уже есть — T2; partial/unknown intent — recovery; genuine bound scientific/native acceptance — T3.P; actual model publication — T3.C. Existing pair/parent/held использовать повторно без новых эффектов. Не менять run ID ради повторного optimizer.
>
> Загрузить одну canonical карточку из `tasks/execution/` и, когда назначено, одного ребёнка из `tasks/children/`. Сначала relevant source→caller→state→ordinary consumer и применимые skills; не весь RAG/WorkLeft/skill каталог в каждом worker. 00A нужен только для одного consequential primary вопроса. 01A/B/C запускаются только при PROVEN_DEFECT, иначе REUSE_NO_CHANGE. Старые V3–V6/405a/979256 и длинные `tasks/RP01.*` — unique design sources, не команды нового исполнения.
>
> **CODEX с запрошенной Astra 6 UltraCode выполняет человеческую роль выбора TRAIN-урока. Разрешённая внешняя открытая китайская модель создаёт обе final chosen/rejected стороны. Отдельный CODEX Astra 6 UltraCode оценивает exact pair.** T2 — отдельное независимое назначение CODEX Astra 6 UltraCode над retained raw answers; Student — другой owner. Actual runtime model/session/receipt обязательны; написанное имя модели не доказывает вызов. Reuse действующего genuine grant; не требуй того же согласия заново и не придумывай новые полномочия.
>
> T0 ничего не генерирует: existing train-and-hold уже владеет C0/Trial. T1 один раз делает разрешённый effect и возвращает held state, не опубликованную модель. T2 не генерирует новые Student ответы. T3.P публикует те же weights без optimizer; T3.C — отдельная проверка ordinary new-process consumer без права promotion. Private exam/gold/rubric не передавать S/A/J или GitHub; authorised T2/custodian получает evaluation-only доступ.
>
> READ_ONLY/REUSE_NO_CHANGE → 0 новых product tests. Только реально изменённый deterministic risk требует ближайшей необходимой проверки; no coverage quota/Windows matrix/generic 30–50-pass loop. Skill-template adaptation описана в GOVERNANCE-AND-REVIEW. Actual privacy/native/rights/release safeguards не отменять. Полезность — frozen independent domain families, critical errors, lawful paths/abstention, время и полные PC/external затраты.
>
> RP02 domain mapping/semantic tokens/context-law kNN, RP03 typed Unit many→many/Flow correction, RP04 async Orchestrator/higher-order Graph и RP05+ feedback/PC/delivery идут у своих владельцев параллельно. Не выкидывай эти требования, но не жди их завершения для первого E1. C# DPO gain не закрывает SQL/RU и весь hybrid. Детерминированный правильный fast path допустим.
>
> Верни actual task/mode, source/skill spans, P/K SHA, initial state, exact chosen action/grants, inputs/output/state hashes, code change or NONE, narrow check or 0, scientific outcome or NOT_MEASURED, true model/reviewer receipts or NOT_PERFORMED, good/bad/edge/falsifier, commit/readback и одного следующего владельца. Публикуй свои промежуточные planning artifacts, когда это запрошено; не перезаписывай чужие изменения и не force-push. STOP после эффекта своей карточки, не после фиктивного PASS.

## Навигация

[Все 16 заданий](tasks/execution/README.md) · [9 основных детей и 4 incident-only документа](tasks/children/README.md) · [Состояния и зависимости](EXECUTION-BOARD.md) · [Scientific examination](LEARNING-EXAMINATION.md) · [Governance/skills adaptation](GOVERNANCE-AND-REVIEW.md) · [Прочитанный code map](RP01-CURRENT-CODE-MAP.md).

Никакая команда из этого текста не выполнена его публикацией. Реальный эффект требует проверенных inputs и действующих полномочий, не нового формального согласования уже разрешённой работы. Общая программа R01–R25 остаётся у её [диспетчера](../START-NEXT-RUN.md); данный prompt её не перезапускает.
