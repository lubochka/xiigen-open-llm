# R11 — причинный анализ пути к первому достоверному обучению

**Дата:** 2026-10-13  
**Статус:** аналитический результат R11. Код, обучение, Windows/GitHub Actions, tests/coverage, datasets и weights этим проходом не запускались и не менялись.

## 1. Человеческий вывод

Сейчас главный тормоз — не отсутствие ещё одного общего плана и не отсутствие нового trainer/framework. По уже собранным R02/R04–R09 evidence существует достаточно кода и архитектурных заготовок, чтобы приблизиться к первому учебному эксперименту значительно быстрее.

Критический путь должен быть коротким:

**разрешённая учебная задача → внешняя открытая китайская модель создаёт обе финальные стороны DPO chosen/rejected → CODEX на Astra 6 UltraCode оценивает exact projected pair → faithful representation без потери различающего смысла → явно выбранный training objective → одно разрешённое bounded update существующего Student → публикация/ordinary consumer → same-panel before/after examination → следующий новый урок.**

CODEX на Astra 6 UltraCode выполняет две человеческие функции: выбирает учебную задачу и отдельно оценивает качество готовой exact DPO-пары. Китайская модель создаёт DPO-контент, но не принимает архитектурное решение и не является финальным судьёй качества. Independent Student examination после update остаётся отдельной ролью.

Первый достоверный опыт не должен ждать full Windows qualification, всей матрицы доменов, полного DynamicGraph или нового общего vector stack. Эти обязательства сохраняются, но переходят в свои причинные milestones.

## 2. Что реально задерживает первый валидный урок

### 2.1 Exact pair может измениться до optimizer

R05 проследил существующий DPO pipeline и нашёл конкретный seam: prompt/chosen/rejected проходят whitespace rewriting и per-field truncation до 480 characters перед tokenization. R05 также выделяет full-sequence mean objective как второй связанный участок того же seam.

Это сильнее, чем гипотеза «нам нужен новый tokenizer». Проблема не в самом факте существования текущего tokenizer, а в том, что **CODEX может оценить одну пару, а optimizer фактически получить смыслово изменённый материал или objective, который не соответствует выбранному обучающему сигналу**.

Причинное следствие: пока exact evaluated pair не доказанно доходит до нужного update, measured learning gain интерпретировать нельзя.

### 2.2 Ordinary consumer после принятого update пока не закрыт как доказанный путь

R04/R05/R07 сохраняют важную границу: наличие trainer, checkpoint или изменённых параметров ещё не доказывает, что ordinary inference потребляет принятый результат там, где ожидается.

Причинное следствие: первый эксперимент обязан включать ordinary-consumer observation. Иначе можно получить «успешное обучение» только внутри isolated training path.

### 2.3 Representation, kNN, LoRA и domain mapping нельзя чинить одним общим rewrite

R04/R06/R08 показывают, что в системе уже есть несколько представлений и kNN-путей, включая реальный typed consumer. Они не являются одним пространством и не должны сворачиваться в один термин «embedding quality».

Это означает:

- новый tokenizer не является prerequisite первой DPO-пары;
- dynamic kNN должен иметь domain-specific signal/state/owner, а не ручное изменение k;
- LoRA должна оцениваться как конкретная matrix adaptation с реальным downstream consumer, а не как «векторизация вообще»;
- domain mapping, representation, kNN metric/k/radius и LoRA — отдельные causal factors.

Первый bounded experiment меняет один factor или одну объявленную interaction, остальные фиксирует.

### 2.4 Windows/native qualification стоит в неправильном месте критического пути

R09 показал на bounded Actions sample, что Windows-family существенно тяжелее focused-family и повторяется часто. При этом ближайший R05 learning seam находится в DPO/tokenization/objective и сам по себе не требует Windows-native boundary.

Причинное следствие: Windows/native verification сохраняется для native delivery capability, но **не является default prerequisite первого learning lesson**. Она возвращается в путь только если выбранный consumer или seam действительно пересекает native boundary.

### 2.5 Green tests/coverage не являются learning evidence

Тесты полезны для deterministic invariants: provenance, permissions, serialization shape, no-repeat, state restore, rollback, storage bounds, native process/security behavior.

Но они не отвечают на вопросы:

- модель лучше сопоставляет домен?
- dynamic kNN выбирает более полезный путь?
- DPO positive/negative пара действительно дала transfer?
- Flow/Orchestrator/Graph улучшили path choice?
- LoRA result дошёл до ordinary consumer?
- улучшение сохранило старые домены?
- новый вариант быстрее/компактнее при сопоставимом качестве?

Для этих утверждений нужен learning examination, а не coverage proxy.

### 2.6 Повторное планирование и повторное доказательство уже установленных вещей создают отдельную стоимость

Previous-round history и текущие результаты показывают повторяющийся риск: один worker пытается снова доказать всю программу, читать все RAG и переоткрывать уже установленные роли, вместо потребления bounded handoff.

Причинное следствие: R11 рекомендует **source reuse with attributable limits**, а не «каждый Rxx заново весь repo». Новый worker обязан читать direct sources для своих спорных решений, но не должен повторять unrelated research ради формального ощущения полноты.

## 3. Causal DAG первого урока

Минимальная причинная цепочка:

1. **Lawful lesson choice** — CODEX/Astra выбирает задачу и domain/family.
2. **DPO authoring** — внешняя открытая китайская модель создаёт обе финальные стороны.
3. **Exact pair examination** — CODEX/Astra оценивает exact projected pair; TIE/BOTH_BAD/INSUFFICIENT не forced preference.
4. **Faithful preparation** — смысл пары, structured order, Unicode/code/whitespace и response boundaries не искажены.
5. **Correct objective and recipient** — понятны policy/reference или иной названный learner, masks, target parameters/state owner.
6. **One bounded mutation** — нет скрытого replay; state identity/restore/history известны.
7. **Publish/adopt** — принятый state доступен ordinary consumer.
8. **Same-panel examination** — baseline/challenger/no-update control; domain-law metrics; retention; time/resources.
9. **Bulk continuation** — следующий новый lesson family; bulk-to-bulk curve.

Native Windows qualification, полный UI, полный curriculum, full graph coverage и certification **не находятся между шагами 1–8 по умолчанию**. Они добавляются только при доказанной причинной зависимости.

## 4. Минимальный cut-set реальных препятствий

До первого валидного learning experiment нельзя выкинуть четыре вещи:

1. **Exact pair integrity** — оценённое содержимое должно совпадать с тем, что оптимизируется.
2. **Objective/recipient correctness** — нужно знать, что именно учится и почему это обновление относится к требуемой способности.
3. **State/provenance/no-repeat** — иначе результат нельзя безопасно продолжать и сравнивать.
4. **Ordinary-consumer + same-panel observation** — иначе update не связан с пользовательской способностью.

Все остальные крупные обязательства должны быть проверены на причинную необходимость к выбранному эксперименту, а не автоматически вставляться перед ним.

## 5. Гипотеза test-fitting

Глобальный вывод «мы подгоняем tests под систему» остаётся **INCONCLUSIVE**. R09 не нашёл достаточного evidence для такого обвинения.

Но для R11 это не мешает принять более важное process-решение: даже корректные tests нельзя использовать как proxy learning quality, а expensive native gates нельзя держать в first-lesson path без causal edge.

Иными словами, план можно ускорить без необходимости сначала доказать намеренную подгонку тестов.

## 6. Missing R10 и граница R11

На старте R11 в `results` наблюдались R01–R09, но не `results/R10`. R11 contract прямо разрешает разбирать доказанную process-часть, пока независимая algorithm hypothesis уточняется.

Поэтому:

- process decisions этого документа действительны в bounded scope;
- глобальная окончательная ranking архитектурных alternatives, которая должна опираться на joined R10 brief, здесь не притворяется завершённой;
- отсутствие R10 не блокирует fast-path exact-pair/objective/ordinary-consumer work;
- R12 должен потребить R10, если тот появится, прежде чем фиксировать окончательный baseline/challenger архитектуры.

## 7. Что считать успехом следующего шага

Успех ближайшего implementation/experiment шага — не «все tests green» и не «получен новый checkpoint».

Нужен наблюдаемый пакет:

- exact allowed lesson и domain family;
- exact pair provenance;
- CODEX/Astra pair judgment;
- prepared representation identity;
- objective/recipient/state identity;
- baseline and challenger;
- same-memory no-update control;
- ordinary-consumer outcome;
- before/after semantic quality;
- domain mapping/path diagnostics;
- retention;
- latency/update time;
- peak RAM/VRAM/disk and persistent footprint;
- IMPROVE / REGRESS / INCONCLUSIVE / INVALID_EVIDENCE.

Первый корректный опыт может дать no gain. Это всё равно полезный результат: он falsifies текущую гипотезу и позволяет менять один следующий factor, а не снова строить новый общий framework.
