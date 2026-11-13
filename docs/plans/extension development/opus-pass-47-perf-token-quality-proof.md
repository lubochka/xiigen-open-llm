# Pass P47 — Perf / token / quality: доказательство УЛУЧШЕНИЯ (measured proof gaps)

## Фокус прохода
Вопрос: какого ИЗМЕРЕННОГО доказательства не хватает, чтобы заявить, что расширение
что-либо УЛУЧШАЕТ — сокращение токенов, задержка (latency), не-регрессия качества?
Проверка по реальному коду `C:\Projects\llm_mvp_core` и артефактам пакета, не по
copy-статусам другой модели.

## Что уже закрыто прежними проходами (дедуп-база, НЕ повторяю)
- **P01 фаза 13** — token savings STUB/NEEDS_FIX (фазовый статус).
- **P15** — Cursor row12: эвристический токенайзер (не токены модели); baseline↔adaptive
  без независимой переменной; 14-assert guard привязан к not-ready ядру; dead-null
  `EvaluateQualityGate`.
- **P27** — кросс-хостовый: общий сервис не эмитит трейс; нет арифметики редукции +
  схема запрещает savings-claim; инвариант `prompt_hash must match` запрещает
  input-reduction; учёт меряет не ту величину (user-prompt+ответ, не токены к LLM);
  token↔quality подсистемы не сведены; pass-путь пропускает РОСТ токенов.
- **P28** — QUALITY-03 fixture (константа 2/5); presence-checker вместо измерения;
  очередь proof-contract 9/63; QSG-24 material-coverage тавтология; нет продюсера
  выходов 4 слоёв; inter-rater = одиночный fixture-ревьюер.
- **P25** — исполненные DPO-числа: единственный градиентный прогон регрессировал;
  нет delta улучшения по 88 report'ам; held-out ≈ шум; промоутнутые ассеты без
  loss/reward-margin.

P47 берёт УГОЛ, которого нет ни в одном из них: **методология доказательства
улучшения как таковая** — latency, ресурсная стоимость per-request, определённый
числовой ЦЕЛЕВОЙ порог, статистическая строгость (повторы/дисперсия/ДИ),
контрфактный no-XIIGen контроль, инфраструктура не-регрессии между версиями,
и денежная стоимость. Прежние проходы разбирали МЕХАНИЗМ токенов/качества; P47 —
СТАНДАРТ ДОКАЗАТЕЛЬСТВА «улучшает vs НЕ улучшает».

## Ключевые находки (evidence-bound)

### 1. Задержки (latency / overhead добавленного звена) не меряет НИКТО
- `43.SmartSkillsDomain/XiigenExtensionApiService.cs` — общий боундари всех 3 хостов —
  grep `Stopwatch|Elapsed|latency|duration|cpu|ram|response_time` → 0 совпадений по
  таймингу; есть только строки `["token_savings_claimed"]=false` (`:632`, `:691`).
- Единственная РЕАЛЬНО исполненная измерительная программа
  `docs/plans/evidence/extension-full-readiness-dual-path/scenario-execution/frdp-p01-cursor-token-savings-baseline/measurement-20260625T004435/Program.cs`
  — grep `stopwatch|elapsed|latency|duration|tickcount|DateTime.Now` → 0.
- План трактует latency как НЕОБЯЗАТЕЛЬНОЕ:
  `copied-plans/EXTENSION-RUNTIME-TOKEN-QUALITY-LEARNABILITY-PLAN-v1.md:1066`
  «latency if available».
- Расширение вставляет звено `host → extension → core → answer`; его собственные
  накладные расходы (added-hop latency) не измерены нигде. Экономия токенов, даже если
  бы была доказана, не отвечает на вопрос, не стало ли пользователю МЕДЛЕННЕЕ.

### 2. CpuMs / PeakRamMb — подменные (mislabeled) прокси, не настоящая ресурсная стоимость
- `43.SmartSkillsDomain/SkillSelectionMachine.cs:333` `CpuMs: sw.ElapsedMilliseconds`
  и `:335` `ElapsedMs: sw.ElapsedMilliseconds` — CpuMs ТОЖДЕСТВЕН ElapsedMs (обе —
  wall-clock одного Stopwatch), это НЕ процессорное время.
- `:334` `PeakRamMb: GC.GetTotalMemory(forceFullCollection:false)/(1024*1024)` —
  мгновенный снимок управляемой кучи, НЕ пик, НЕ working set процесса.
- То же в `OrchestrationReasoningLogicalHierarchy.cs:118-120`.
- Эти поля принадлежат `LearningBatchResult` (обучение), а не per-request инференсу
  расширения; через боундари `XiigenExtensionApiService` ресурсные числа не
  проходят (grep cpu/ram там → 0). CLAUDE.md Completion evidence pattern запрещает
  `cpu_ms/ram_mb = UNKNOWN`; здесь хуже — значения ЕСТЬ, но семантически неверны.

### 3. Нет определённого числового ЦЕЛЕВОГО порога улучшения
- `TokenTraceAccounting.cs` и `ExtensionQualityScoring.cs` — grep
  `threshold|target.*reduction|min.*reduction|acceptance.threshold|success.criteri`
  → 0.
- План называет методологию (RC-10,
  `EXTENSION-FULL-READINESS-DUAL-PATH-PLAN-v1.md:1869` «Token savings requires fixed
  workload, tokenizer/version, repeat counts, quality guard») со статусом
  `DONE_FOR_EXECUTION_WITH_REVIEW_SCOPE` (план, не исполнение), но НИГДЕ не зафиксирован
  конкретный продуктовый критерий успеха (например «≥X% редукции токенов при дельте
  качества ≥ −Y%»). Без цели фраза «расширение улучшает» неопровержима и недоказуема.
- Отличие от P27 gap-6 (там: в scorer нет позитивного гейта редукции — дефект кода):
  здесь отсутствует САМ продуктовый порог/критерий приёмки.

### 4. Статистическая строгость не исполнена (повторы / дисперсия / ДИ / размер выборки)
- План требует: `:1278` «repeat token counts across at least two runs or explain
  variance», `:1316` «confidence intervals or per-case margins».
- Но исполнённое состояние: `EXTENSION-FULL-READINESS-DUAL-PATH-...STATE.md:315`
  `repeat_count: null`, `savings_calculation: NOT_CALCULATED_IN_PHASE_03`.
- `TokenTraceAccounting.cs` — grep `repeat|sample|iteration|variance|stddev` → 0
  (нет полей n / повторов / дисперсии).
- Единственный числовой quality-артефакт — 96-строчный FIXTURE (P28), один прогон.
- Итог: где числа и есть — они single-shot без воспроизводимости и разброса; защитимого
  улучшения из одного замера без n/дисперсии/ДИ вывести нельзя.

### 5. Нет контрфактного контроля «без XIIGen» (does-it-improve-over-not-using-it)
- Вопрос «улучшает ли расширение хоть что-то» требует руки-контроля: тот же запрос в
  хосте БЕЗ XIIGen против ТОГО ЖЕ запроса С XIIGen, с измерением (а) качества
  финального ответа и (б) суммарных токенов, оплаченных хостовой модели.
- Все baseline'ы — XIIGen-ВНУТРЕННИЕ: baseline↔adaptive обе ноги зовут ядро
  (P15/P27); `XiigenExtensionApiService` — единственный путь, no-extension арма нет.
- Отличие от P15 (нет независимой переменной ВНУТРИ xiigen) и P27 (учитывается не та
  величина): те — про внутренний парный механизм; здесь — про ОТСУТСТВИЕ внешнего
  контроля, который единственный отвечает на продуктовый вопрос об улучшении.

### 6. Нет персистентного baseline не-регрессии между версиями
- `43.SmartSkillsDomain` — grep
  `regression.baseline|golden.baseline|baseline.snapshot|non.regression|store.baseline`
  → 0.
- Даже если качество измерят однократно, нет зафиксированного эталона (golden baseline)
  и механизма сравнения, чтобы поймать регресс при выкате нового checkpoint/версии.
  «Не-регрессия» бессмысленна без сохранённого предыдущего эталона.
- Отличие от P28 (качество сейчас — fixture/не измерено): здесь — отсутствие
  ИНФРАСТРУКТУРЫ доказательства не-регрессии во времени.

### 7. Нет измерения денежной экономии ($), привязанной к редукции токенов
- Заявление об экономии токенов подразумевает снижение стоимости платной модели, но
  cost-дельты, атрибутируемой XIIGen, нет.
- Единственные $-значения в пакете — собственный бюджет-гард хостового CLI на ОДНУ
  команду: `ORCHESTRATION-AND-PROMPT-TO-TASK-EXTENSIONS-STATE.json:3362`
  `total_cost_usd 0.04564375`, `:3311` `--max-budget-usd 0.05`. Это стоимость одного
  вызова хоста, а не baseline-vs-adaptive экономия от расширения.

## Вывод P47
Ни на одном хосте нет измеренного доказательства, что расширение что-либо УЛУЧШАЕТ:
задержка не меряется (и объявлена опциональной), ресурсные метрики — подменные прокси
из обучения, продуктового числового порога улучшения нет, статистическая строгость не
исполнена (repeat_count=null, один fixture), контрфактного no-XIIGen контроля нет,
инфраструктуры не-регрессии между версиями нет, денежной экономии нет. Это отдельный
слой от P15/P27/P28 (механизм токенов/качества) — здесь отсутствует САМ СТАНДАРТ
ДОКАЗАТЕЛЬСТВА улучшения.
