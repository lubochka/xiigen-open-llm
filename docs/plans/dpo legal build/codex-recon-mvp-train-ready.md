# Codex Recon — Готовые к обучению датасеты v2: формат и провенанс

Корпус: `mvp` · slug: `mvp-train-ready` · роль: рекон-evidence для плана ILearningDynamicGraph.
Только реально найденные пути. Исполнения/тестов/git не было.

## Что это и где (точные ключевые пути)

ВАЖНО про двусмысленность каталога `train-ready-v2`. Существуют ДВА разных места:

1. Указанная мне цель — `C:\Users\lubao\Documents\xiigen\llm mvp\train-ready-v2\`
   содержит РОВНО ОДИН файл: `dpo-pairs-supplement-dpo-quality-v197.jsonl`
   (14 963 байт, 12 непустых строк JSON Lines, mtime 2026-05-10 23:37). Это
   небольшой ДОПОЛНЯЮЩИЙ (supplement) набор, не основной корпус.

2. Настоящий рабочий корпус v2 лежит ГЛУБЖЕ:
   `C:\Users\lubao\Documents\xiigen\llm mvp\internal learnin 2\model-led-run-20260508-v1\train-ready-v2\`
   — десятки версионированных файлов `dpo-pairs-domain-*-v201..v246-*-trainable.jsonl`
   плюс указатель текущей версии.
   - Указатель: `...\train-ready-v2\CURRENT-TRAIN-READY-POINTER.json`
     current=`TRAIN-READY-V246-V244-PLUS-V245-SKILL-SHORT`,
     path=`...\dpo-pairs-domain-v246-v244-plus-v245-skill-short-trainable.jsonl`,
     `trainable_rows=672`, sha256=`B851169C...0ABDD8`, updated 2026-05-11T04:31.
   - Историческая база, на которую ссылается supplement:
     `...\train-ready-v2\dpo-pairs-merged-v196-stratified-trainable-only.jsonl`
     (3 916 строк).

Подтверждённые провенанс-источники, упомянутые внутри supplement (Test-Path = True):
- `...\internal learnin 2\model-led-run-20260508-v1\evidence\run362-v196-live-dpo-quality-audit.json`
- `...\xiigen\llm mvp\intelrnal learn\XIIGEN-DPO-REGISTRY-GUIDE-v1.md` (реестр-гайд типизации)
- `...\xiigen\llm mvp\orchestrator implementation\missing parts 2.txt` (user-correction)
- `...\train-ready-v2\quality\train-ready-v196-stratified-quality.json`

НЕ найден по указанному в данных пути (Test-Path = False):
- `C:\Projects\llm_mvp_core\experiments-v2\INTERNAL-TRAIN-READY-V191\runner\InternalTrainReadyDpoRunner\11.DPO\DpoTrainer.cs`
  — этот точный путь СТАЛЫЙ. Реальный раннер сейчас: `C:\Projects\llm_mvp_core\11.DPO\DpoTrainer.cs`
  (остальные совпадения — копии в `.claude\worktrees\*`). То есть provenance-ссылка
  на код в supplement указывает на несуществующий путь, хотя описанный механизм реален (см. ниже).

## Как устроено / работает (кратко)

Формат — DPO-триплеты в JSON Lines (одна строка = один JSON-объект = одна
предпочтительная пара). Схема supplement-файла (12 полей):
`id, origin, normalized_family, bucket, tier, source_status, source,
source_span, a_side, chosen, rejected, target_behavior`.
- `a_side` — вопрос/промпт; `chosen` — предпочитаемый ответ; `rejected` —
  отвергаемый, с явным маркером ошибки `[WRONG: ...]` внутри текста.
- Богатые метаданные провенанса: `source` (абсолютный путь-первоисточник),
  `source_span` (какой фрагмент источника), `source_status`
  (`accepted_model_led_from_*`), `origin` (`RUN363-...`), `tier`/`bucket`.

Схема основного корпуса (`...v196-stratified...jsonl`) — НАДМНОЖЕСТВО: те же 12
полей плюс `notes, repaired_from, repair_reason` (15 полей). То есть supplement
формат-совместим с основным корпусом (его поля — строгое подмножество).

Тема всех 12 пар supplement — «качество DPO», 7 normalized_family:
`dpo_quality_metric_interpretation`, `source_anchored_pair_authoring`,
`prompt_user_intent_authority`, `batch_packing_quality_control`,
`rejected_answer_quality`, `repaired_row_governance`, `source_currentness_boundary`.
Они учат модель НЕ выбрасывать корпус из-за низкой batch-accuracy, а
диагностировать состав батча и семантическую «заякоренность» пар.

Подтверждение центрального технического тезиса (пара v197-dpo-quality-002) по
живому коду `C:\Projects\llm_mvp_core\11.DPO\DpoTrainer.cs:64,106,120`:
`Accuracy = margin > 0 ? 1f : 0f`, усреднённая по парам; margin =
`Beta*(polChosenLogP-refChosenLogP) - Beta*(polRejectedLogP-refRejectedLogP)`.
То есть «accuracy» раннера — это live reward-margin (доля пар, где chosen ранжируется
выше rejected), а НЕ held-out семантическая точность. Тезис датасета корректен.

## Релевантность к ILearningDynamicGraph

- DPO/обучение: высокая. Это готовый формат предпочтительных пар (`a_side/chosen/
  rejected/target_behavior`) — ровно то, на чём обучаются `ILogicalUnit`/
  `ILearningMachine`-уровни через DPO. Reward-margin-семантика (margin>0)
  применима к узлам/рёбрам графа, где решение «правильный против неправильного»
  можно выразить парой.
- Измерение: высокая. Явно отделяет live reward-margin от held-out семантической
  оценки и от «доверия к корпусу» — это нужная гигиена метрик для графа
  (нельзя путать batch-accuracy ребра с реальным качеством решения узла).
- Граф/оркестрация: средняя/косвенная. Сами пары не описывают узлы/рёбра/fan-out
  графа; они о качестве авторинга и о провенанс-границах. Полезны как материал
  для обучения отдельных решений, не как схема графа.
- Teacher-provenance: средняя, со смешанным сигналом. Метаданные провенанса
  богатые (`source/source_span/source_status/origin`), НО `source_status` всех
  12 строк = `accepted_model_led_*` — т.е. строки порождены модельным анализом
  («model-led»), а не извлечены дословно. Для Legal Teacher/Provenance Gate это
  ТРЕБУЕТ доисследования: какая именно модель-учитель, режим доступа, лицензия.
  В самих строках это не зафиксировано (нет `teacher_access_mode`,
  `hosting_provider`, `model_id`, `model_license`, `provider_terms_checked`).

## Классификация (по правилам скила)

Объект — ДАТАСЕТ (данные), не код, поэтому уровневая классификация механизма
(ILearningMachine / ILogicalOrchestrator / ILogicalFlow / ILogicalUnit /
ILearningMachinePair) к самому .jsonl неприменима. По
`trainable-architecture-decision-SKILL.md` это материал для «Required Dataset
Table», а не для «Required Review Table».

- Датасет-вердикт (предварительный): для большинства строк — ближе к
  `KEEP_FOR_TRAINING` по форме (валидные DPO-триплеты с явным контрастом и
  target_behavior), НО с понижением до требования провенанс-доочистки из-за
  `model-led` источника без teacher-полей → по Legal Teacher Gate риск
  `DATASET_REBUILD_REQUIRED`, если учителем окажется закрытая проприетарная
  модель (Claude/OpenAI/Gemini запрещены как distillation-учители). Точный
  вердикт нельзя дать без идентификации учителя — см. Пробелы.
- Сопутствующий код-механизм, который датасет ОПИСЫВАЕТ (раннер
  `DpoTrainer.cs`, `Accuracy = margin>0` усреднённая): это детерминистический
  расчёт DPO-loss/метрики — внутренний хелпер обучения, а НЕ отдельный
  trainable-уровень. По правилу скила «KNN/scorer/counter … are not architecture
  patterns; they may be implementation details inside an allowed level only» —
  здесь это корректный eval/training-helper внутри тренировочного пайплайна, не
  претендующий на семантический уровень. Отдельная классификация раннера не
  требуется для данного датасет-рекона; помечаю как honestly-labelled
  training-loss helper (OK_INTERNAL_DETAIL по аналогии).

## Пробелы / что доисследовать

1. Двусмысленность каталога: подтвердить с Luba, какой `train-ready-v2`
   канонический для плана — supplement (12 пар, указанная цель) или глубокий
   корпус v201..v246 (`trainable_rows=672` по pointer). План DPO почти наверняка
   должен опираться на глубокий корпус + pointer, а supplement — это надстройка.
2. Legal Teacher/Provenance: для всех 12 строк `source_status=accepted_model_led_*`,
   но нет полей `teacher_access_mode/hosting_provider/model_id/model_license/
   provider_terms_checked`. Нужно установить, какая модель порождала «model-led»
   строки, и пройти Legal Teacher Gate (закрытые проприетарные учители → rebuild).
3. Сталая ссылка на код: `experiments-v2\INTERNAL-TRAIN-READY-V191\runner\...\DpoTrainer.cs`
   не существует; актуальный путь `C:\Projects\llm_mvp_core\11.DPO\DpoTrainer.cs`.
   Провенанс-ссылки в строках надо актуализировать/проверить на сталость.
4. Полная схема глубокого корпуса: я прочитал ключи только первой строки
   (`...v196-stratified...`, 15 полей) и pointer (V246, 672 строки). Не
   валидированы единообразие схемы по всем v201..v246, дубли по `id`, баланс
   `normalized_family`, реальное содержимое V246.
5. Лицензии первоисточников (`source`-пути в `xiigen\llm mvp\...`): это
   локальные приватные/проектные материалы (user-intent, orchestrator
   implementation, реестр-гайд) — по правилам это, скорее, user-adaptive/private
   leg, не common-training, без явного одобрения. Требует решения R5/Legal-gate.
6. Связь supplement→runner на уровне обучения: подтверждено только то, что
   метрика раннера соответствует тезису; не проверено, что эти конкретные 12 пар
   реально попадают в тренировочный вход (loader/манифест) графа.
