# dcm-measurement-GUIDE.md

## Luba Why / Loophole Repair-First Rule

When Luba asks "why", "почему", challenges an answer, or says a failure came
from a loophole in plans, guides, skills, agent files, prompts, or governance
rules, this is an answer-first repair trigger, not a normal status question and
not permission to continue product work first.

Required order:
1. Answer Luba's why/loophole question directly.
2. Classify the previous behavior or claim as verified, overclaim, false,
   misread, or governance gap.
3. Identify the exact missing, weak, stale, or contradictory rule that allowed
   the failure.
4. Repair the governing plan, guide, skill, agent file, or start prompt first.
5. Verify the repair with direct readback, marker search, diff check, or another
   evidence-bound check.
6. Only after the repair is verified, resume the nearest already-authorized
   product task.

Do not substitute apology, status, a plan-quality note, or "I will continue" for
this repair. If no file repair is needed, state the exact evidence proving that
before continuing.

## Luba Global Plan-Quality / Skill-Stack Rule

This file inherits the global project rule: when Luba asks which plan is better,
judge by Luba's criteria: human clarity, readable architecture, architectural
reliability, evidence honesty, requirement fidelity, execution-control quality,
and low overclaim risk. Newer status tables, file counts, ledgers, count
freshness, or claimed pass counts are delta evidence only; use valid deltas to
patch the stronger plan instead of treating bookkeeping freshness as plan
superiority.

No executor session may speak as the active product executor until SK-INTENT,
SK-PLAN, SK-REVIEW, and SK-ARCH when applicable are loaded for the current task.
If the stack is not loaded, say "not loaded yet" and load it before plan
comparison, plan choice, status, executor work order, or implementation.
**Версия:** 1.0 (2026-06-22)
**Применимо к:** DesignConversionDomain (подпроект llm_mvp_core, .NET 9 / C#, ML с нуля)
**Аудитория:** архитектор, исполнитель-сабагент, ревьювер
**Язык:** русский, человекочитаемо
**Статус:** действующий гайд; тарифные цели помечены «решает Luba»

---

## 0. Как используется этот skill

Используй этот гайд как чек-лист измерений перед любым claim, что DCM CriticUnit или ImproverUnit стали обученными.
Он нужен reviewer/архитектору/исполнителю для проверки runner report: есть ли held-out метрики, ablation, leakage checks,
fresh-load, continue-training, speed numbers и доказательство, что source/license gate закрыт.

Если report не закрывает этот гайд, нельзя писать `PASS_AS_TRAINED_FEATURE`, продвигать веса, обещать product quality
или считать deterministic scaffold обученным поведением. Допустимый результат в таком случае — `OPEN`,
`PROMOTION_BLOCKED` или `PASS_AS_HONEST_UNTRAINED_SCAFFOLD`.

---

## 1. Назначение

Этот гайд описывает, как измерять качество, скорость и честность двух обучаемых юнитов DesignConversionDomain — CriticUnit и ImproverUnit. Гайд отвечает на три вопроса: что считать точностью, когда работа юнита считается обученной (а когда — честным незавершённым скелетом), и какие числовые порог-метрики нужны для продвижения весов от Bootstrap к обученному состоянию согласно решению A-LM-03. Без этих измерений фаза считается открытой независимо от наличия кода или артефактов.

Допустимые trainable уровни, о которых можно делать measurement claim: `ILearningMachinePair`, `ILearningMachine`,
`ILogicalOrchestrator`, `ILogicalFlow`, `ILogicalUnit`. Любой deterministic lookup, regex/BM25 shortcut,
static validator или helper-service может быть только bootstrap/scaffold evidence и не засчитывается как обученная единица.

---

## 2. Точность по юниту

### 2.1 ImproverUnit — визуальная точность рендера

ImproverUnit принимает дерево дизайна, исходный html+css и список дефектов, а возвращает улучшенный html+css. Точность измеряется как близость рендера произведённого html к эталонному рендеру по четырём разрешениям: 360 px, 768 px, 1280 px и 1920 px.

**Формула:**

```
visual_accuracy(w) = 1 - norm_pixel_diff(render_produced(w), render_reference(w))

где norm_pixel_diff — нормализованный покоординатный пиксельный diff
                      (сумма |delta_px| / (ширина × высота × 3))

accuracy_improver = mean( visual_accuracy(360),
                          visual_accuracy(768),
                          visual_accuracy(1280),
                          visual_accuracy(1920) )
```

Метрика живёт на held-out множестве (50 пар батча A-LM-04). Для каждой пары accuracy_chosen и accuracy_rejected берутся из поля DPO-записи.

**Пример чисел (одна held-out пара):**

| Разрешение | render_produced vs эталон (norm diff) | visual_accuracy |
|------------|--------------------------------------|-----------------|
| 360 px     | 0.042                                | 0.958           |
| 768 px     | 0.031                                | 0.969           |
| 1280 px    | 0.027                                | 0.973           |
| 1920 px    | 0.038                                | 0.962           |
| **Итого**  |                                      | **0.966**       |

Для этой пары accuracy_chosen = 0.966. Rejected вариант с грубыми ошибками вёрстки даёт 0.71.

---

### 2.2 CriticUnit — точность критики (precision / recall / F1)

CriticUnit принимает дерево дизайна, html+css и рендеры в 4 разрешениях плюс visual-diff, а возвращает QualityVerdict: accuracy 0..1 и список дефектов с типом и локацией.

**Важно:** accuracy внутри QualityVerdict — это семантическое поле вердикта, которое юнит предсказывает. Для измерения качества самого юнита используется другое — precision/recall/F1 по найденным дефектам против ground-truth.

**Формулы:**

```
TP = |detected_defects ∩ ground_truth_defects|   (совпадение по типу и локации)
FP = |detected_defects| - TP                     (лишние дефекты)
FN = |ground_truth_defects| - TP                 (пропущенные дефекты)

precision  = TP / (TP + FP)
recall     = TP / (TP + FN)
F1         = 2 × precision × recall / (precision + recall)

accuracy_critic = F1   (основная метрика CriticUnit для DPO-записи)
```

Совпадение по типу и локации: тип дефекта должен точно совпадать; локация считается совпавшей, если ограничивающий прямоугольник пересекается с ground-truth-прямоугольником с IoU >= 0.5.

**Пример чисел (один held-out эталон):**

- ground_truth_defects: 8 дефектов (цвет кнопки, отступ, шрифт h2, и т.д.)
- detected_defects: 9 дефектов
- TP = 7, FP = 2, FN = 1
- precision = 7/9 = 0.778, recall = 7/8 = 0.875, F1 = 0.824
- accuracy_critic (chosen) = 0.824

Rejected вариант: TP = 4, FP = 5, FN = 4 → precision = 0.444, recall = 0.500, F1 = 0.471.

---

## 3. Качество обучения

### 3.1 Reward margin

Для каждой DPO-пары: reward_margin = accuracy_chosen - accuracy_rejected

Агрегат на held-out: mean_reward_margin — среднее по 50 held-out парам.
- Цель CriticUnit: mean_reward_margin >= 0.20
- Цель ImproverUnit: mean_reward_margin >= 0.15

Ниже порога — юнит не отличает хорошие решения от плохих.

### 3.2 Preference win-rate

win_rate = count(accuracy_chosen > accuracy_rejected) / 50

Цель для обоих юнитов: win_rate >= 0.80 на held-out. Ниже 0.75 — юнит обучен слабо.

**Пример:** 43 пары из 50 дали accuracy_chosen > accuracy_rejected → win_rate = 0.86. Проходит.

### 3.3 Покрытие новых классов дефектов

После каждого батча считается доля held-out пар, в которых юнит корректно детектировал тип дефекта, не встречавшийся в train. Цель: > 0.30 (не менее 30% новых классов обнаружено). Если 0 — юнит запомнил, а не обобщил.

### 3.4 Стабильность под перестановку кандидатов

Для 10 случайных held-out примеров меняем порядок кандидатов (chosen/rejected переставляем) и перезапускаем inference. accuracy_critic не должна меняться более чем на ±0.05. Нарушение — признак позиционной утечки (leakage).

---

## 4. Скорость

Измеряется на одном inference-прогоне с batch=1 на held-out. Все поля обязательны; UNKNOWN не принимается для стадии PASS_AS_TRAINED_FEATURE.

| Поле               | Описание                                             | Целевой уровень |
|--------------------|------------------------------------------------------|-----------------|
| throughput         | пар/сек при batch=8                                  | >= 2 пары/сек   |
| cpu_ms             | миллисекунды на одну пару (1 вызов inference)        | <= 800 мс       |
| ram_mb             | пиковый RAM на inference                             | <= 512 МБ       |
| total_train_time   | время полного обучения 50-пар батча, секунды         | <= 300 с        |
| inference_p95_ms   | 95-й перцентиль латентности inference, мс            | <= 1200 мс      |

**Пример строки метрик (ImproverUnit, батч A-LM-04, run 2026-06-22):**

```json
{
  "unit": "ImproverUnit",
  "batch": "A-LM-04",
  "throughput_pairs_per_sec": 3.1,
  "cpu_ms_per_pair": 320,
  "ram_mb_peak": 410,
  "total_train_time_sec": 187,
  "inference_p95_ms": 640
}
```

---

## 5. Ablation / анти-детерминизм

Это самый критичный раздел: юнит не должен маскировать детерминированный lookup (regex, BM25, словарь) под обученный ответ.

### 5.1 Прогон без обученного состояния (untrained ablation)

Загрузить юнит без checkpoint (cold state). Подать те же 50 held-out примеров. Ожидаемый результат: юнит честно возвращает not_ready / DOCUMENTATION_GRAPH_READY_TRAINING_RUNNER_MISSING, а НЕ псевдо-умный ответ. Если cold-state accuracy > 0.40 — это подозрение на детерминированный shortcut; требуется расследование и правка кода.

**Пример вывода честного cold-state (CriticUnit):**

```json
{
  "status": "not_ready",
  "reason": "checkpoint_missing",
  "verdict": null
}
```

### 5.2 Leakage-rate (exact memory / source)

Для 5 held-out примеров проверяется: входит ли exact prompt из held-out в train-множество? Если да — это data leakage. leakage_rate = count(held-out_in_train) / 5. Норма: 0.0. При leakage_rate > 0 — held-out переформировать, метрика недействительна.

### 5.3 Перестановочный тест (anti-positional)

Описан в параграфе 3.4. Если delta accuracy при перестановке > 0.05 — метрика открыта, юнит не принят как trained.

### 5.4 Firewall: что НЕ принимается как доказательство «обучен»

- validation_only: true
- metric_type: static / contract / mixed
- cpu_ms: UNKNOWN, ram_mb: UNKNOWN
- отчёт без числовых runtime-метрик
- checkpoint без held-out F1/accuracy
- pass report без ablation-строки
- source/license gate без явного `passed`
- product или semantic claim без QA/eval evidence reference

---

## 6. Две PASS-стадии

### PASS_AS_HONEST_UNTRAINED_SCAFFOLD

Юнит-скелет считается честным, если выполнены ВСЕ условия:
- cold-state inference возвращает явный not_ready без семантического содержимого;
- нет детерминированного ответа (regex / словарь / BM25 / flow-index), выданного за «умный»;
- нет фейковых DPO-пар, фейкового checkpoint, сфабрикованных метрик;
- через реальный хост-entrypoint приходит честный degraded-ответ (например пустой список дефектов с флагом not_ready);
- тест на cold-state проходит автоматически (assert: status == "not_ready").

Это веха, не готовая функция. Репортировать как PASS_AS_HONEST_UNTRAINED_SCAFFOLD.

### PASS_AS_TRAINED_FEATURE

Юнит считается обученным, если выполнены ВСЕ условия:
- существует checkpoint с известным путём в манифесте;
- held-out числовые метрики: win_rate >= 0.80, mean_reward_margin >= 0.15 (Improver) / 0.20 (Critic), F1/accuracy_improver зафиксированы;
- ablation пройден: cold-state → not_ready, leakage_rate = 0.0, delta_accuracy_permutation <= 0.05;
- fresh-load: checkpoint выгружен, перезагружен с нуля, метрики воспроизведены (допуск ±0.02);
- backup: перед перезаписью checkpoint сохранён backup reference или явно зафиксировано, почему backup не требуется;
- continue-training: запущено +10 train-пар поверх checkpoint, новый checkpoint сохранён, метрика не упала;
- export/import lifecycle проверен: `ExportAsync` трактуется как metadata-only, `ImportAsync` не заменяет `LoadCheckpointAsync`;
- source/license gate = passed для всех accepted DPO-пар, включая различение Personal и Global legs;
- QA/eval evidence приложена до любого semantic/product claim;
- e2e через реальный хост-entrypoint: минимум 3 сценария с реальными входами и измеренными ответами;
- все поля скорости (параграф 4) не UNKNOWN.

Репортировать как PASS_AS_TRAINED_FEATURE только при прохождении всех условий.

---

## 7. Тарифные цели (провизорные — решает Luba)

| Тариф | Цель accuracy_improver | Цель accuracy_critic (F1) | Потолок cpu_ms | Примечание              |
|-------|------------------------|---------------------------|----------------|-------------------------|
| free  | >= 0.80                | >= 0.70                   | <= 1200 мс     | placeholder, уточняет Luba |
| paid  | >= 0.90                | >= 0.82                   | <= 800 мс      | placeholder, уточняет Luba |
| pro   | >= 0.95                | >= 0.90                   | <= 500 мс      | placeholder, уточняет Luba |

Эти значения выбраны как ориентиры на основе диапазона DPO-пар батча A-LM-04; окончательные пороги утверждает Luba перед продвижением весов.

---

## 8. Гейт продвижения весов (A-LM-03)

Промо Bootstrap → trained разрешено только после публикации валидированных весов тренинг-раннером. Для продвижения необходимо:

| Условие                         | CriticUnit                          | ImproverUnit                         |
|---------------------------------|-------------------------------------|--------------------------------------|
| win_rate (held-out, 50 пар)     | >= 0.80                             | >= 0.80                              |
| mean_reward_margin              | >= 0.20                             | >= 0.15                              |
| ablation cold-state             | not_ready при отсутствии checkpoint | not_ready при отсутствии checkpoint  |
| leakage_rate                    | 0.0                                 | 0.0                                  |
| delta_accuracy_permutation      | <= 0.05                             | <= 0.05                              |
| fresh-load воспроизведение      | допуск ±0.02                        | допуск ±0.02                         |
| backup checkpoint               | ref есть или waiver обоснован       | ref есть или waiver обоснован        |
| source/license gate             | passed                              | passed                               |
| QA/eval evidence                | приложена до product claim          | приложена до product claim           |
| cpu_ms, ram_mb                  | числа, не UNKNOWN                   | числа, не UNKNOWN                    |
| continue-training checkpoint    | новый checkpoint после +10 пар      | новый checkpoint после +10 пар       |
| export/import scope             | Export metadata-only, Import не load | Export metadata-only, Import не load |
| e2e через хост                  | >= 3 сценария с реальными входами   | >= 3 сценария с реальными входами    |

Если хоть одно условие не выполнено, раннер возвращает PROMOTION_BLOCKED с указанием незакрытого условия. Продвижение без закрытия всех условий — ошибка выполнения фазы.

---

## 9. Форма записи метрик и пример заполненных строк

### Поля записи метрик (runner report)

```
run_id               : уникальный UUID прогона
unit                 : CriticUnit | ImproverUnit
batch                : идентификатор батча (A-LM-04)
run_date             : ISO-8601 дата
train_rows           : количество train-пар использовано
held_out_rows        : количество held-out пар оценено
win_rate             : доля пар accuracy_chosen > accuracy_rejected
mean_reward_margin   : среднее (accuracy_chosen - accuracy_rejected)
ablation_cold_state  : passed | failed | not_run
leakage_rate         : 0.0 .. 1.0
delta_accuracy_perm  : максимальное изменение accuracy при перестановке
fresh_load_delta     : |accuracy_after_reload - accuracy_original|
backup_checkpoint_ref: путь backup checkpoint или WAIVED_WITH_REASON
continue_train_ckpt  : путь нового checkpoint после continue-training
export_ref           : metadata ref после ExportAsync
import_scope_verified: true если ImportAsync проверен как metadata-only и не заменяет LoadCheckpointAsync
source_license_gate  : passed | failed | not_run
qa_eval_evidence_ref : ссылка/путь на QA/eval evidence перед semantic/product claim
throughput_pps       : пар/сек (batch=8)
cpu_ms               : мс на пару
ram_mb               : МБ пиковый
total_train_time_sec : сек полного обучения батча
inference_p95_ms     : 95-й перцентиль латентности, мс
promotion_decision   : PROMOTION_ALLOWED | PROMOTION_BLOCKED
promotion_blocked_reason : (если BLOCKED) — какое условие не закрыто
pass_stage           : PASS_AS_HONEST_UNTRAINED_SCAFFOLD | PASS_AS_TRAINED_FEATURE | OPEN
```

---

### Пример заполненной строки — CriticUnit

```json
{
  "run_id": "c71a3f9e-0044-4b2f-aab3-d2e8c0f17b3c",
  "unit": "CriticUnit",
  "batch": "A-LM-04",
  "run_date": "2026-06-22",
  "train_rows": 50,
  "held_out_rows": 50,
  "win_rate": 0.86,
  "mean_reward_margin": 0.244,
  "ablation_cold_state": "passed",
  "leakage_rate": 0.0,
  "delta_accuracy_perm": 0.031,
  "fresh_load_delta": 0.011,
  "backup_checkpoint_ref": "checkpoints/critic/backup/A-LM-04-before-continue-v1.ckpt",
  "continue_train_ckpt": "checkpoints/critic/A-LM-04-continue-v1.ckpt",
  "export_ref": "manifests/critic/A-LM-04-v1.json",
  "import_scope_verified": true,
  "source_license_gate": "passed",
  "qa_eval_evidence_ref": "reports/critic/A-LM-04-qa-eval.json",
  "throughput_pps": 2.8,
  "cpu_ms": 410,
  "ram_mb": 388,
  "total_train_time_sec": 214,
  "inference_p95_ms": 720,
  "promotion_decision": "PROMOTION_ALLOWED",
  "promotion_blocked_reason": null,
  "pass_stage": "PASS_AS_TRAINED_FEATURE"
}
```

Интерпретация: win_rate 0.86 >= 0.80; mean_reward_margin 0.244 >= 0.20; ablation passed; leakage_rate 0.0; delta_accuracy_perm 0.031 <= 0.05; source/license и QA/eval evidence закрыты; все скоростные поля — числа. Промо разрешено.

---

### Пример заполненной строки — ImproverUnit

```json
{
  "run_id": "a88d5e12-ff33-41a7-9c6b-11f0a3b7c204",
  "unit": "ImproverUnit",
  "batch": "A-LM-04",
  "run_date": "2026-06-22",
  "train_rows": 50,
  "held_out_rows": 50,
  "win_rate": 0.82,
  "mean_reward_margin": 0.178,
  "ablation_cold_state": "passed",
  "leakage_rate": 0.0,
  "delta_accuracy_perm": 0.019,
  "fresh_load_delta": 0.008,
  "backup_checkpoint_ref": "checkpoints/improver/backup/A-LM-04-before-continue-v1.ckpt",
  "continue_train_ckpt": "checkpoints/improver/A-LM-04-continue-v1.ckpt",
  "export_ref": "manifests/improver/A-LM-04-v1.json",
  "import_scope_verified": true,
  "source_license_gate": "passed",
  "qa_eval_evidence_ref": "reports/improver/A-LM-04-qa-eval.json",
  "throughput_pps": 3.1,
  "cpu_ms": 320,
  "ram_mb": 410,
  "total_train_time_sec": 187,
  "inference_p95_ms": 640,
  "promotion_decision": "PROMOTION_ALLOWED",
  "promotion_blocked_reason": null,
  "pass_stage": "PASS_AS_TRAINED_FEATURE"
}
```

Интерпретация: win_rate 0.82 >= 0.80; mean_reward_margin 0.178 >= 0.15; ablation passed; leakage 0.0; delta_perm 0.019 <= 0.05. Промо разрешено.

---

*Конец гайда. Версия 1.0 — 2026-06-22. Тарифные цели (параграф 7) — провизорные, утверждает Luba.*
