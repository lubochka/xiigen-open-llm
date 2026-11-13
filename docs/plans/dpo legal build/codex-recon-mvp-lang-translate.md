# Codex Recon — MVP «language translate learning»: DPO-сеты, метод обучения, формат пар

Источник-рекон для плана **ILearningDynamicGraph** (авто-построение DPO для
ILearningUnit/Flow/Orchestrator/DynamicGraph). Только evidence, не разрешение на реализацию.

CURRENT TASK: рекон DPO-сетов перевода и формата пар для будущего плана ILearningDynamicGraph.
NOT THE TASK: исполнять/тренировать/билдить, чинить чужие файлы, git.
EVIDENCE-ONLY: все файлы ниже — read-only evidence.

---

## 1. Что это и где (точные ключевые пути)

Область: `C:/Users/lubao/Documents/xiigen/llm mvp/language translate learning/`

Гайды/спеки (две DPO-плоскости — «переводчик» и «роутер»):
- `DPO-PAIR-AUTHORING-GUIDANCE-TRANSLATION-v1.md` — как писать пары переводчика; reward-margin; 4 типа ошибок rejected (A семантика / B морфология / C регистр / D полнота); `[WRONG:]` коды; token-overlap тест (>0.60 → discard).
- `TRANSLATION-DPO-REGISTRY-GUIDE-v1.md` — семейства пар T01..T09; **JSON-форма пары переводчика**; hard-gates; коды направлений RU_EN/EN_RU/HE_EN/EN_HE; 4 механизма (translation_accuracy / register_appropriateness / morphological_accuracy / back_translation_consistency).
- `TRANSLATION-ROUTING-FLOW-DPO-GUIDANCE-v1.md` — **JSON-форма пары роутера**; уровни R1 direction / R2 topic / R3 register-risk / R4 adapter; контракт `TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>`; promotion-gates (direction acc ≥99%).
- `TRANSLATION-TRAINING-PLAN-v1.md` — метод обучения: one-batch-at-a-time ablation; LoRA-конфиг (Rank 4/Alpha 8/Dropout 0.05, q_proj+v_proj); `TranslationPromptBuilder` (системный промпт вставляется при токенизации, не в `a_side`); метрики BLEU-4/chrF/reward margin/accuracy; фазы 0..6.
- `TRANSLATION-FOUR-LEVEL-LLM-TRAINING-PLAN-v1.md` — актуальный stream-режим: `stream = direction × topic_domain × translation_level`, **batch = 50 train + 50 held-out**; 4 уровня (L1 direction/core → L2 syntax/completeness → L3 morphology/register/NE → L4 …).
- `TRANSLATION-DPO-REJECTED-PROMPTS-v1.md`, `EXAMINATION-METHOD-TRANSLATION-v1.md`, `TRANSLATION-LOOK-LIKE-RUN-REPORT-TEMPLATE-v1.md`, `TRANSLATION-TOPIC-ROUTER-ORCHESTRATION-PLAN-v1.md`, `TRANSLATION-SOURCE-REGISTRY-v1/2/3.md`, `TRANSLATION-SMALL-MODEL-RATIONALE-v1.md`, `prepare_translation_learning_sets.py` (не читались детально — gaps).

Реальные данные на диске: `translation-runs/`
- `translator-ru-en-general/batch-001..005/` — `translation-pairs-train-50.jsonl`, `translation-pairs-validation.json`, `translation-batch-summary.json`, `STREAM-STATE.json`, `source-inventory.jsonl`, `NEXT-BATCH-DECISION.md`, `LOOK-LIKE-RUN-REPORT.md`.
- `router-r1-direction/batch-001..003/` и `router-r2-topic/batch-001..003/` — `translation-router-train-50.jsonl` + `translation-router-heldout-50.jsonl` + `*-heldout-evaluation.json` + `translation-error-ledger.jsonl` + `training-pairs-repair-required.jsonl`.

---

## 2. Как устроено/работает (кратко)

**Две раздельные обучаемые плоскости:**
1. **Переводчик** (генерация) — 4 LoRA-адаптера по направлениям; отдельные веса, не делятся с оркестрацией и не делятся между собой.
2. **Роутер** (оркестрация выбора) — `TranslationRoutingFlow`/`Orchestrator`, выбирает direction+topic+register+risk+adapter ДО генерации.

**Метод обучения = DPO + LoRA, инкрементальный ablation:**
- Единица потока: stream = направление × topic_domain × level. Инкремент = 50 train + 50 held-out пар.
- `reward_margin = logP(chosen|a_side) − logP(rejected|a_side)`; цель >0.05/пара (роутер: >0). chosen↔rejected слишком похожи → margin≈0 → нет сигнала; token-overlap >0.60 → пара выбрасывается.
- Системный промпт направления (`TranslationPromptBuilder`) добавляется к источнику при токенизации; `a_side` хранит только сырой источник; `compiled_prompt_in_training:false`.
- Промоушн только при неухудшении BLEU/chrF и положительном margin; роутер-гейт блокирует промоушн переводчика, если выбор адаптера ненадёжен.
- Teacher/provenance: «Do not use machine translation output as chosen»; chosen = verified human/licensed parallel corpus.

**Формат пары ПЕРЕВОДЧИКА (на диске, `translation-pairs-train-50.jsonl`) — плоский, отличается от спеки T01:**
```json
{"id":"TR-RU-EN-GEN-0101","domain":"general","contract_signature":"TranslationQualityEvaluation",
 "direction":"RU_EN","source":"Мне двадцать лет.","chosen_translation":"I am twenty years old.",
 "rejected_translation":"To me twenty years.","defect_type":"literal_translation",
 "defect_description":"...","source_id":"synthetic-translator-seed",
 "source_license_state":"SYNTHETIC_SEED","batch_index":1}
```
ВАЖНО: реальные файлы используют `source`/`chosen_translation`/`rejected_translation`/`defect_type`+`defect_description`, БЕЗ инлайн-маркера `[WRONG:]`, `source_license_state:"SYNTHETIC_SEED"` (синтетический seed, не licensed corpus). Спека же (registry-guide) требует `a_side`/`chosen`/`rejected` с инлайн `[WRONG:]`, `pair_id=T01-...`, `mechanism`, `source_family`. → расхождение спека↔данные.

**Формат пары РОУТЕРА (на диске, `translation-router-train-50.jsonl`) — вложенный, совпадает со спекой:**
```json
{"id":"TR-ROUTE-R1-0001","domain":"TRANSLATION_ROUTING",
 "contract_signature":"TranslationRoutingFlow : ILogicalFlow<TranslationRoutingRequest>","router_level":"R1",
 "a_side":{"source_text":"Привет, как дела?","target_language":"en", ...},
 "chosen":{"direction":"RU_EN","topic_domain":"general","register":"neutral","risk_class":"normal",
   "translation_level":1,"adapter_id":"adapter-ru-en-general","confidence":0.96,
   "evidence_terms":[...],"rejected_adapters":["adapter-en-ru-general"],"decision_reason":"..."},
 "rejected":{...,"decision_reason":"...\n[WRONG: wrong_direction - ...функциональное последствие...]"},
 "source_id":"synthetic-router-seed","source_license_state":"SYNTHETIC_SEED","heldout_group":"train"}
```
chosen роутера = полное решение (не только adapter_id); rejected = соседний правдоподобный неверный маршрут с `[WRONG:]` + функциональное последствие.

---

## 3. Релевантность к ILearningDynamicGraph

- **DPO/обучение:** прямой шаблон авто-построения пар (chosen/rejected/reason + reward margin + token-overlap + ablation). Переносимо на узлы графа: каждое решение узла = chosen vs соседний rejected с функциональным последствием.
- **Граф/оркестрация:** роутер уже многоуровневый (R1→R2→R3→R4) — естественная декомпозиция в узлы/рёбра DynamicGraph; flow-level пара = решение целого ребра/подграфа, unit-level пара = одно ребро (+обязательное `flow_consequence`). Совпадает с Graph-Style Reconstruction Gate скила (нужны nodes/edges/контракты/fan-out/fan-in/authority на ребро).
- **Измерение:** готовые метрики per-edge/per-level (direction/topic/adapter accuracy, false-low-risk=0, reward margin, BLEU/chrF) — кандидаты в gate'ы графа.
- **Teacher-provenance:** есть запрет MT-как-chosen; но текущие данные `SYNTHETIC_SEED` — провенанс «synthetic», не verified corpus и не назван teacher-движок → для legal-teacher gate ILearningDynamicGraph это пробел (нет teacher_access_mode/hosting/model_license полей в строках).
- **DPO legal build (целевая папка вывода):** формат роутера = хороший образец «полного решения с rejected+reason»; формат переводчика надо привести к инлайн-`[WRONG:]` + реальному provenance перед использованием как legal DPO.

---

## 4. Классификация (уровни vs детерминистический хелпер) — по trainable-architecture-decision-SKILL.md

| Сущность | Заявленный контракт | Уровень / классификация | Обоснование (правило скила) |
|---|---|---|---|
| `TranslationRoutingFlow` | `ILogicalFlow<TranslationRoutingRequest>` | **ILogicalFlow** (allowed) | Полное routing-решение как обучаемый flow; скил: routing/sequence на allowed orchestration level. |
| `TranslationRoutingOrchestrator` | `ILogicalOrchestrator` | **ILogicalOrchestrator** (allowed) | Координатор-ассистент над flow. |
| `TranslationDirectionRouterUnit` / `TopicRouterUnit` / `RegisterRiskRouterUnit` / `AdapterSelectorUnit` | внутр. узлы | **ILogicalUnit** (allowed, только как внутр. узлы) | Скил: unit-уровень допустим как деталь внутри flow; нельзя сводить обучение к изолированным меткам (нужен `flow_consequence`). |
| LoRA-адаптеры переводчика (RU_EN/EN_RU/HE_EN/EN_HE) | генерация | компонент обучаемой модели внутри уровня (генератор) | Веса модели; сами по себе не «архитектурный уровень» — авторитет должен держать flow/unit. |
| `TranslationPromptBuilder` (`Dictionary<string,string> DirectionPrompts`) | сборка промпта | **внутренний детерминистический хелпер** | Статич. словарь шаблонов; скил: dictionary/lookup = implementation detail, НЕ trainable authority. ОК как helper, label обязателен. |
| `contract_signature:"TranslationQualityEvaluation"` в парах переводчика | — | **не уровень** (ярлык механизма пары) | Generic-имя; не один из 5 allowed уровней. Не путать с архитектурным контрактом. |
| token-overlap тест / hard-gates / mojibake-чек | фильтры данных | **внутренние детерминистические хелперы (eval/data-helper)** | Скил: фильтры — helper/eval-only, не runtime semantic authority. |

Прямого «скрытого детерминизма как trainable authority» в рантайме не обнаружено в прочитанных файлах: роутер заявлен как ILogicalFlow (не hardcoded switch — гайд прямо это запрещает). Риск-зоны: (а) `SYNTHETIC_SEED` provenance вместо verified/teacher-labelled corpus; (б) расхождение формата переводчика спека↔данные; (в) `confidence` как поле пары — при рантайме нельзя пускать как floor-shortcut (см. Minimum Breadth Challenge: confidence floors / fallback-to-first-entry).

---

## 5. Пробелы / что доисследовать

- Не читались (детально): `prepare_translation_learning_sets.py` (как реально генерируются пары — детерминированный seed-генератор? тогда provenance-вопрос усиливается), `TRANSLATION-SOURCE-REGISTRY-v3.md` (ACCEPT/QUARANTINE/BLOCK источники и лицензии), `EXAMINATION-METHOD-TRANSLATION-v1.md`, `STREAM-STATE.json`/`*-heldout-evaluation.json` (реальные измеренные margin/BLEU — есть ли вообще числа или только LOOK-LIKE-шаблон).
- Не подтверждено наличие реального раннера/чекпойнтов LoRA на диске (в области видны только наборы JSONL + summary/state; код тренировки — в `llm_mvp_core`, вне рекона).
- Формат-дрейф: привести переводчик к единому контракту (инлайн `[WRONG:]` + реальный provenance) ДО переноса в `dpo legal build`.
- Teacher/provenance поля (teacher_access_mode/hosting_provider/model_id/model_license) отсутствуют в строках — обязательны для legal-teacher gate ILearningDynamicGraph.
- `EN_HE`/`HE_EN`/`router-r2-topic` данные есть в путях, но содержимое их JSONL не открывалось — проверить идентичность формата.
