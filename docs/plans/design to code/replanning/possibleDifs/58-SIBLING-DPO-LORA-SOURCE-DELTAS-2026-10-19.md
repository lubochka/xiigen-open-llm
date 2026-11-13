# N58-B — Исторические ветки: новые конкретные DPO/LoRA различия (SOURCE CONFIRMED, QUALITY UNPROVEN)

**19 октября 2026 · READ_ONLY_BRANCH_COMPARISON · DESIGN_ONLY · NOT_RUN.** Предмет этого документа — не бесконечная ревизия старых веток, а **три конкретных различия исполняемых выражений** между `llm_mvp_core/PrepareToGPL` и `codex/dpo-implementation`. **Ни один защищённый файл** `docs/plans/replanning-possible-diffs/**` **не менялся**. Исторические успешные эффекты памяти, E09/E25/E26/E43 нельзя приписывать этому DPO без evidence graph.

## Версия и доказательство идентичности

Веточные snapshot на момент чтения; **git blob SHA** доказательны для содержания файла, но branch ref может переместиться. Pin commit SHA и native DLL до исполнения; ссылки ниже на соответствующую ветку для чтения.

| Путь | Current `codex/dpo-implementation` blob | `PrepareToGPL` blob | `DesignToCode-clean` blob | Что уже доказано |
|---|---|---|---|---|
| `27.TranslationDomain/TranslationTrainedUnits.cs` | `1742abfc9fb617dfc66e4d618e0c169da01a6ccc` | **тот же** | **тот же** | Для этих трёх веток код `ExecutorUnit` **битово идентичен**. Его key/word/phrase/first-hit проблемы — **не новая регрессия source этой пары веток** |
| `27.TranslationDomain/TranslationStructuredBatchBuilder.cs` | `7ceed82476edae9173acc44e2298885c561ca4d5` | `40d35475c8769d5e694fd40380f2901bae22db42` | **тот же, что PrepareToGPL** | При первом строковом сопоставлении current-only substantive строк обнаружены две ES language direction arms; полноценная semantic path diff по всем caller не выполнена |
| `11.DPO/DpoTrainer.cs` | `0a7fd4d088fd37824aa213ddf0c6189675fe7dc2` | `274803473a2ed1ae7847d6ffe40da1aec4ee0cf7` | `cfae24adcf70e2f20bd068c3e543974e6452e2b2` | Разные версии DPO forward, mask, allowlist и gradient; просто восстановить старые weights в новый trainer ≠ одинаковый algorithm |
| `10.LoRA/LoraLinear.cs` | `1b321e99408c961b896896983d002e3b26425b85` | `2fa88487682d8fc0bbd5ac95660efc726ad59eda` | `f12d084aebb899c854558c26a2cfa04187f8e4ce` | LoRA forward и управление dropout/RNG поменялись |
| `27.TranslationDomain/TranslationTrainedUnits.cs` в `orchestrator-extensions` | `1742...` | `9f0067fb789eec8da254b3942bbdcd9f311337fe` (отдельная ветка) | — | Эта ветка требует адресного сравнения semantics до переносов |

**Важная граница:** веточные HEAD `language_translate` и `external_domains` на момент просмотра не имеют пути `27.TranslationDomain/TranslationTrainedUnits.cs` (API 404), потому что root не включает `27.TranslationDomain`. Это **не** «сломанный модуль» и не опровержение старых результатов E26/E09: их искать в соответствующих исторических `34.Experiments`, `12.SemanticCompiler`, pinned historical commits, а не по неверному пути. [N57 historical source proof](57-ARGUMENT-AND-VERIFIER-LEARNING-GAPS-2026-10-19/04-history-and-mechanism-crosswalk.md) рассматривает соответствующую семантику на её реальном владельце.

## H58-02 — DPO: локальное выражение логит-градиента имеет противоположный знак между ветками

**Прямой источник** [PrepareToGPL DpoTrainer, около L276–322](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/11.DPO/DpoTrainer.cs#L276-L322):
```csharp
logits.Grad.Data[i * v + j] += (sm - oh) * scale;
```

**Текущий источник** [codex/dpo-implementation DpoTrainer L1072–1121](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L1072-L1121):
```csharp
logits.Grad.Data[i * v + j] += (oh - sm) * targetScale;
```

Локальная производная **log-softmax выбранного target** по logits при положительном upstream — `oneHot - softmax`. Однако из противоположных выражений нельзя немедленно вывести, что старый **полный** optimizer делал gradient ascent: в старом коде есть комментарий `NLL-style logits gradient orientation`, старый graph/optimizer/счёт весов и default mask не идентичны текущему. Для содержательного сравнения требуется отличить **sign of local derivative**, **sign of dLoss/dWeights** и **direction of update**. Эти три ответа могут различаться. **SAME PAIR+SAME INITIAL STATE+SAME OBJECTIVE** обязательны. Уже есть generic protected [P10 composed-gradient](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/experimets/cards/P10-composed-gradient.md); **новизна N58 — конкретный branch-to-branch operand `sm-oh` vs `oh-sm`, а не «впервые открыта проверка градиента»**.

### Малый эксперимент H58-02 / C0 и N1 (отдельно)

**Модель:** `LlmModel` DPO Student, historical candidate config [RP01 audit](../Kiro-Xiigen-RP01-Learning-First-Slice-Pack/RP01-2026-10-16-LEARNING-RECOVERY-AND-CODE-AUDIT.md) `vocabSize=629, hiddenDim=512, layers=4, contextLen=2048`, q/v-LoRA rank8/alpha16/dropout0 и frozen reference. **Фактический B0**: UNKNOWN SHA → **BLOCKED_INPUT** до поиска exact checkpoint, tokenizer, input masks, AdamW version. Для начала **C0 может использовать разрешённый tiny tensor (no pretrained model)**, назвать его `analytic synthetic fixture`, не выдавать за native learned gain. Если потребуется второй C1/L experiment — только после approval и при реально reachable recipient.

**Исходные обучающие данные:** 1 admitted `prompt/chosen/rejected` из уже существующего frozen C# RP01 source JSONL (exact row ID, pair hash, independent source family before run); `x - 2` / `x + 2` — требование выбора, **не разрешённая произвольная пара**, пока источник не подтверждён. Для pure numeric C0 — 2-token logits toy с однозначно заданным target, response mask, reference и beta=.1; фиксировать точные 2 вектора в prereg before approval. **EXAM:** 4 новых family-disjoint C# behavior cases до/после **только если N1 разрешён**. `A0/A1/B0/B1`: C0 updates=0 и before/after equals; A local legacy sign, B current sign against finite difference same loss; N1 — same initial policy/reference and mask, 1 actual legitimate update each, four free responses. **Не переключать** одновременно response masking sum/mean или recipient parameter set.

**Ожидаемый различающий результат (не измерение):** local log-prob derivative должен согласоваться с finite difference для одной из показанных формул при одинаковом upstream; направление общего DPO loss после 1 step показывает optimizer semantics. Если один режим получает native `NO_REACHABLE` из-за admission/legacy API, записать blocker, а не подделать checkpoint. Исторический RP01 `0/3` не заменять гипотетической прибавкой.

**Полный исходный промпт этого НОВОГО эксперимента:**

> N58-H58-02. До работы перечисли skills: experiment-design-SKILL.md, code-examination-SKILL.md, trainable-architecture-decision-SKILL.md, plan-review-SKILL.md, claim-verification-SKILL.md, prompt-patch-authoring-SKILL.md, human-communication-GLOSSARY.md из docs/skills. Покажи frozen PrepareToGPL/current SHA и exact snippets (sm-oh и oh-sm). Запиши один numeric tensor и разрешённую DPO пару, цель и masks до run. Сначала C0: без модели и без updates сравни обе аналитические производные с central finite difference одного и того же logP/loss; не смешивай native pair/mask/optimizer различия. Предоставь A0/A1/B0/B1 с очевидным нулём updates. N1 с DPO/AdamW/real Student/4 hidden exams — отдельное предложение после моего APPROVED H58-02-N1. Не изменяй protected каталог. Верни решение LOCAL_SIGN_CORRECT/WRONG/INCONCLUSIVE, дальше — only native evidence of actual learning, no extrapolation.

**Статус:** `PENDING_HUMAN_APPROVAL`, `NATIVE_NOT_RUN`.

## H58-03 — LoRA: исторический `Forward(x)` всегда просил training dropout; текущий inference — training=false

**Прямой исходник** [PrepareToGPL LoraLinear L52–64](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/10.LoRA/LoraLinear.cs#L52-L64): обычный `Forward(Tensor x)` вызывает `_dropout.Forward(x, training: true)`. [Current LoraLinear L107–126](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/10.LoRA/LoraLinear.cs#L107-L126): обычный `Forward(x) => Forward(x, training:false)`; overload `Forward(x, training)` управляет dropout явно.

**Что известно и чего не знаем:** на уровне source выражения реально различаются. Но при `dropout=0` из текущего RP01 конфига обе ветки должны вести себя одинаково для этого фактора; **поэтому объявлять старый dropout причиной текущей RP01 неудачи — неверно без доказательства ненулевого dropout и вызываемого old wrapper**. Более поздняя конфигурация `dropout>0` или иной consumer может быть чувствительна к seed/inference repetition. Covered generic protected P01.4 (LoRA dropout) и P09 (cached forward); N58 добавляет точный исторический вызов.

### Малый контроль H58-03 (A/B, ноль обучения; отдельный learned variant только после)

**Домен для качества, если дойдём:** C# documentation, профили alternative EN→RU language training — **отдельно**. **Модель/владелец:** `LoraLinear` для `LlmModel` q/v adapter. **Без Student** допустима компонентная numeric fixture `Linear(in=2,out=2), rank=1, alpha=1`, фиксированные A/B/weights и 2x2 вход, deterministic RNG seeds, только C0; она не становится продуктивной LlmModel. Для actual model test `RP01 Student 629/512/4` — checkpoint/adapter/ref/encoder SHA **UNKNOWN** и BLOCKED until pin. **Начальные данные:** 2 идентичных forward inputs + 4 позиций evaluation, заранее полные numeric arrays; реальный C# EXAM four new families только в отдельном trainable N1. Применить **одно изменение** `forward(training:true)` vs `forward(training:false)` с `dropout=.1`, фиксацией seed/маски. Отрицательные контроли `dropout=0` и zero-adapter B = expect identical; для текущего `dropout=0` **нет ожидаемой причины качества**.

**До/после:** C0 `A0=A1/B0=B1`, обновлений0, мерить repeated same-input numeric output variance, mean and selected mask; качество пользователя N/A. N1 только если подтвердится конкретный настоящий dropout/config/consumer and approval: frozen training/EXAM, restore and repeat generation, compare same model free answers and resources; no silently modified reference.

**Полный исходный промпт НОВОГО эксперимента:**

> N58-H58-03. Загрузи и перечисли architect, experiment-design, code-examination, plan-review, claim-verification, prompt-patch-authoring и human-communication skill paths в llm_mvp_core/docs/skills. Подтверди exact PrepareToGPL/current LoraLinear blob SHAs и Forward overloads. Принеси без native запуска numeric 2×2 component fixture, dropout0.1 seed и отдельный dropout0 контроль, frozen A/B weights and 4 repeat inputs. A/B изменяет только training flag; 0 updates и компонентный result нельзя называть learning. Если конфиг RP01 фактически dropout0, вердикт для RP01 root-cause NOT_APPLICABLE, даже если old source плохо формулирует inference. До моего APPROVED H58-03-C0/N1 ничего не запускать, protected folder read-only. Сохрани outputs, dropout masks и scopes метрик, никакого ложного качества C# при numeric only.

**Статус:** `PENDING_HUMAN_APPROVAL`, `NATIVE_NOT_RUN`.

## Дополнительная веточная разница, без преждевременного A/B

Исторический [PrepareToGPL DpoTrainer L61, L117](https://github.com/lubochka/llm_mvp_core/blob/PrepareToGPL/11.DPO/DpoTrainer.cs#L61-L118) передаёт оптимизатору `_policy.Parameters()`. Текущий [DpoTrainer L1219–1226](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/11.DPO/DpoTrainer.cs#L1219-L1226) использует только `Where(RequiresGrad)` или привязанные параметры project recipient. Это **source-confirmed different candidate enumeration**, но изменение фактического множества обновлённых tensor нельзя вывести, не проверив `RequiresGrad`, `_projectRecipient.Bindings` и optimizer skip semantics. **Уже P01** (recipient), P11 (reachability) и P12 (optimizer); задача до практического опыта: составить точные `name/shape/trainable/updateCount/||delta||` карты в обеих ветках. Не делать общий DPO A/B со сменой знака, маски, параметров и dropout сразу.

## Влияние на приоритеты RP01

1. Воспроизвести реальный успешный переводческий `ExecutorUnit` (его blob одинаков в current/PrepareToGPL/DesignToCode-clean): подтверждает положительное **memory**-обучение и ordinary consumer независимо от trainable Student.
2. Проверить **реальную способность** нынешнего random Student на свободных ответах, actual masked contrast и q/v gradient (P01/P02/P07/P10); это главный RP01 neural gate.
3. Для branch recovery **не переносить старый `DpoTrainer` в current «как есть»** — сначала H58-02 source sign/objective, P01 trainable map, H58-03 dropout-off controls. Старый branch code сам по себе не означает старого положительного native DPO outcome.
4. Отдельно E09/E25/E26/E43 и `orchestrator-extensions` их фактические получатели + donor exams; не подменять их `27.TranslationDomain`, если branch path отсутствует.

**Доказательная граница:** код и Git blobs прочитаны; **нет** native gradient probe, loss-before/after, actual model run, reviewer acceptance, полного semantic-diff всех веток. Все слова «лучше», «обучилось», «исправлено» для H58-02/H58-03 пока **НЕ подтверждены**.
