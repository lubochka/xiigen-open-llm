# 44 — Архитектор / исследователь / планировщик / code reviewer / human communication

**19.10.2026.** Статус: промпты и проверяемые задания, **не выполненные независимыми агентами**. Действует до отдельного реального native/code-review run. Этот документ обеспечивает работу по ранее названным skills, не симулирует их успешные gate.

## Граница изменений (приоритет выше любого примера в архиве)

**Разрешённое место публикации:** `lubochka/xiigen-planning`, `llm_plan3`, `llm mvp/design to code session/replanning/possibleDifs/**`. **Запрещено изменять:** `lubochka/llm_mvp_core` на любом ref, особенно `codex/dpo-implementation/docs/plans/replanning-possible-diffs`; оба исходных repository refs сегодня только read-only для проекта аудита. Никакой замены frozen DPO/source rows, weight artifacts, current grade, или чужих review docs. Native code fixes требуют отдельного разрешения/этапа; здесь готовятся документы.

Применяемые навыки из [docs/skills](https://github.com/lubochka/llm_mvp_core/tree/a392b6be8f8dec1e2375e2eb4fe21c2f6e8950c6/docs/skills): `human-readable-architecture-audit-SKILL.md`, `phase-planning-SKILL.md`, `plan-review-SKILL.md`, `code-examination-SKILL.md`, `prompt-patch-authoring-SKILL.md`, `human-communication-GLOSSARY.md`, `learning-path-audit-SKILL.md`, `experiment-design-SKILL.md`, `translation-domain-reality-check-SKILL.md`. В конкретной кодовой сессии сначала проверить актуальность SK-INTENT, SK-PLAN, SK-REVIEW, SK-ARCH. В архитектуре **домен** = предмет обучения (перевод, C#, документация, оркестрация); направление, задача, тип Unit и механизм обучения описываются отдельно.

## A44-ARCH — архитектор/историк успешного обучения

> Изучи три зафиксированных среза: `language_translate@f6032aff...`, `PrepareToGPL@31346d8...`, `codex/dpo-implementation@a392b6b...`. Найди не только результаты с названием experiment, но actual recipient learned state и caller. Сделай для каждого исторического успеха причинную цепочку `input→tokenizer/features→train recipient→checkpoint→fresh restore→ordinary consumer→meaningful heldout output`. Старый E26 dictionary / E27 rule / E43 bigram нельзя называть опытом обучения DPO neural Student. Прочти master WorkLeft RAG и *исходник*, а не только индекс. Первой главой R44-01 legacy 4→6 и R44-03 positive phrase map. У каждого элемента отметь исходник, строки, полный SHA, возможный фальсификатор. Не смешивай двух checkpoint owners и учительские/проверяющие роли.

**Выход:** одна человекочитаемая карта с приоритетным восстановлением, уже известными DIF и границами. Проверка: какой именно срез научился и куда исчез его результат? Проектные названия «ILearningUnit» и «ILearningDynamicGraph» сами по себе не являются реализованными научными достижениями.

## A44-CODE — код-исследователь: две **локальные** новые гипотезы и их опровержение

> Прочти *полностью* `35.ExperimentsV2/Infrastructure/ContextualDictionary.cs` в `language_translate`, затем `E26ContextualDictionary.Run` и actual `BuildTestCases`. В `ContextDistance` нарисуй четыре клетки query side present/absent × stored side required/empty. Посчитай `поток` при query left/right empty, compare candidate `thread` и explicit fallback `flow`. Далее сравни `left=[7,9]` и `left=[9,7]` при одинаковом overlap, разные priority. Если исходная .NET логика не совпадает с [proxy](44-E26-SOURCE-EQUIVALENT-PROXY.py), остановись, исправь документацию, а не gold. Отдельно проверь, достигает ли текущий `TranslationRoutingFlow` именно этого matcher или использует иной `ExecutorUnit` — не переносить диагноз автоматически. Найди в соседних планах существующий E26 POS gap [23](23-NEW-E26-POS-CONTEXT-TO-LEARNED-UNIT-2026-10-16.md) и [25](25-MICRO-E26-POS-CONTEXT-CAUSAL-EXPERIMENT-2026-10-16.md), не присваивай уже известное себе. Выпиши минимум одну альтернативную гипотезу, при которой отсутствие контекста означает wildcard и поведение корректно **по предметному контракту**.

**Выход:** таблица `claim / source:line / known vs candidate / downstream reachability / alternative explanation / test`. Сам факт выполненного Python proxy не доказывает нового качества, фиксировать `SOURCE_EQUIVALENT_PROXY`, `NATIVE_NOT_RUN`. Если гипотеза уже покрыта другим агентом, метка `DUPLICATE`, а не новое открытие.

## A44-PLAN — составить один небольшой эксперимент

> Из [X44-01..06](44-FAST-DOMAIN-EXPERIMENTS-2026-10-19.md) выбери одну причину и не меняй другое. Для переводческого контекста зафиксируй старый scorer, ту же пару примеров, unknown/known POS and order-negative, предметную метрику *ложный смысл при недостающем доказательстве* отдельно от coverage. Для learned Unit требуется одобренная chosen-пара, самостоятельные новых source families, store before/after, normal Flow и fresh-process. Для neural Student не запускай уже выполненный 8-step embedding SFT, у которого 0/24; новый contrast должен отличать отсутствие способности base от неверной DPO preference. Для C# только Roslyn AST, не old RU POS numeric codes. Идентичное количество уроков и счётчик затрат для сравниваемых arms.

**Выход:** `experiment ID, branch SHA, A/B, одно изменение, TRAIN/DEV/SEALED, positive/negative, output actual consumer, owner/state, meaningful score, latency/RAM/VRAM, stop/falsifier, artifact names`. Никаких тотальных Windows CI ради одной функции.

## A44-REVIEW — независимая проверка (отдельный автор)

> Проверь **точный candidate SHA**, не «общий PASS». (1) Лежит ли публикация только в permitted planning folder? (2) Сходятся ли source строки и Python formula? (3) Существует ли документированный old-positive receipt, не переименованный в gain иной модели? (4) Не заявлен ли already-known 23/25/41/43 gap как новый? (5) Отличаются ли домены от языков, моделей и механизмов? (6) Реален ли consumer, а не тестовый mock? (7) Контрольный набор исключает leakage и позволяет критическое опровержение? (8) Если missing context, есть ли вариант UNKNOWN вместо угаданного gold? (9) Не нарушены ли provenance/legal/training-once guards? (10) Report сохраняет прогресс и регрессию в раздельных колонках?

**Вердикт:** `PASS(candidate SHA)` / `NEEDS_FIX(candidate SHA)`, конкретная причина, ссылка на source, минимальный patch и readback. Прочтение автором собственного результата **не** является independent review. Нельзя выдавать плановый reviewer prompt за проведённое ревью.

## A44-PROMPT — один bottleneck, WRONG / CORRECT

`NAMED_CHECK: E26-K44-UNKNOWN-CONTEXT`. **WRONG:** «оба POS неизвестны, выбрать `thread`, потому что distance=0 и priority=10». **CORRECT:** «оба POS неизвестны; не объявлять `thread` доказанным, измерить unknown-context/abstain либо потребовать контекст в input». Сначала измерить + решить применимость; затем отдельный code patch при разрешении, одно изменение/одна проверка, без новых DPO pairs в том же эксперименте.

`NAMED_CHECK: E26-K44-ORDER-SENSITIVE`. **WRONG:** «контексты [NOUN,ADJ] и [ADJ,NOUN] равноценны по Any-overlap для задачи, где порядок меняет смысл». **CORRECT:** «сохранить order/role в ранжировании *только если* он признан предметным законом и подтверждён heldout». Контроль: не ставить hard-veto при UNKNOWN POS и не применять к bag-of-words данным.

## A44-HUMAN — короткий отчёт для пользователя

Отвечать последовательно: **что именно работало, что и где разошлось, что новое против уже известного, какая ошибка могла быть в старом положительном эксперименте, что фактически выполнено и что предстоит измерить**. Пример корректной формулировки: «E26 имеет архивный положительный результат на заранее известных 15 случаях; он измеряет контекстный словарь, а не обучение нейросети. В его distance обнаружены два самостоятельных предположения, которые проверены малым proxy; влияние на current Flow ещё не измерено». Некорректно: «LoRA плоха, потому что E26 когда-то имел 100%».

## Контракт публикации

Ссылка на первичный источник и реальный status обязательны. `SOURCE_REVIEWED` ≠ `NATIVE_TRAINING`; `HISTORICAL_RECEIPT` ≠ `CAUSAL_GAIN`; `PROXY` ≠ `FLOW_QUALITY`; `SELF_REVIEW` ≠ `INDEPENDENT_PASS`. Защищённая core-папка остается строго **READ_ONLY**, включая будущие sub-agents.
