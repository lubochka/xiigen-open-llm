# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR-EN-IF-01/ru-en; domain tr.en; profile ru-en.

Write exactly 10 rows, batch 2/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 860; after it, domain remainder: 130. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Условие действия и направление условной связи — английский",
  "instructions": "## Предметный промпт китайской модели через Kiro\n\nЭтот блок дополняет общий MASTER и request.json. Генератор использует уже выбранную китайскую модель Kiro, помощники наследуют её. Не подменять её CODEX и не подменять оценщика генератором. Если отображается только селектор, записать его как наблюдаемое имя без выдуманной точной версии.\n\nВыполни TR-EN-IF-01, только topic/profile/split/count текущего request.json.\nЦель: Переводить условные правила, сохраняя условие, отрицание внутри него, направление связи и различие «если»/«только если».\n2. Для одного профиля создай требуемое число новых независимых сценариев в пределах квот. Reference-сценарии служат примерами и не становятся EXAM.\n3. Сначала зафиксируй source_raw и семейство. Напиши полноценный перевод и один правдоподобный неправильный перевод. Оба — на целевом языке и в одном формате.\n4. Проверь, что ошибка относится только к смысловому отношению этой карточки. При двух допустимых ответах дай quarantine с причиной вместо ложной пары.\n5. Сохрани prompt, chosen, rejected и все поля xiigen.dpo.v1. Rationale, tags, IDs, смысловая схема и проверочная ситуация остаются только в служебных полях.\n6. Не вставляй [WRONG], рейтинг или пояснение дефекта внутрь rejected. Не режь пару по символам; различающий фрагмент и конец ответа должны позже поместиться в реальный tokenizer/context.\n7. Чистый текст, опечатки, раскладки, перефразирования и все направления одной семьи имеют один split. Не добавляй трудные искажения в эту партию: для них отдельные TR07/TR08.\n8. Сохрани draft.jsonl и русский report.md. Это кандидаты. Отдельная оценка качества сейчас не назначена; после записи и структурного finish перейти к следующему потоку.\n9. При NEXT переходи к указанному ID, не дописывай прошлую задачу. При STOP немедленно прекращай действия. Старые файлы, эксперименты и результаты не удаляй.",
  "answer_contract": "Follow the visible response format in this lesson and its examples; preserve the learner task.",
  "hard_constraints": [],
  "scenario_axes": [],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A037:P00011","family_id":"D1000-20261025:TR-IF-01:F00011"},
{"pair_id":"D1000-20261025:A037:P00012","family_id":"D1000-20261025:TR-IF-01:F00012"},
{"pair_id":"D1000-20261025:A037:P00013","family_id":"D1000-20261025:TR-IF-01:F00013"},
{"pair_id":"D1000-20261025:A037:P00014","family_id":"D1000-20261025:TR-IF-01:F00014"},
{"pair_id":"D1000-20261025:A037:P00015","family_id":"D1000-20261025:TR-IF-01:F00015"},
{"pair_id":"D1000-20261025:A037:P00016","family_id":"D1000-20261025:TR-IF-01:F00016"},
{"pair_id":"D1000-20261025:A037:P00017","family_id":"D1000-20261025:TR-IF-01:F00017"},
{"pair_id":"D1000-20261025:A037:P00018","family_id":"D1000-20261025:TR-IF-01:F00018"},
{"pair_id":"D1000-20261025:A037:P00019","family_id":"D1000-20261025:TR-IF-01:F00019"},
{"pair_id":"D1000-20261025:A037:P00020","family_id":"D1000-20261025:TR-IF-01:F00020"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A037\B0002\material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A037\B0002\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR-EN-IF-01, profile_id=ru-en, split=train. metadata.learning_objective_id=TR-EN-IF-01; metadata.scenario_group=TR-IF-01; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-043"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR-EN-IF-01.ru-en.REF01.P01",
    "profile_id": "ru-en",
    "family_id": "sim:translation:logic-v2:box-empty-close-lid",
    "prompt": "Симуляция перевода. Переведи с языка «русский» на язык «английский». Ответ — только перевод; не добавляй объяснение, служебную разметку или новый факт.\nИсходный текст: Если коробка пуста, закрой крышку.",
    "chosen": "If the box is empty, close the lid.",
    "rejected": "If the box is full, close the lid.",
    "error_tag": "condition_predicate_flip",
    "rationale": "Условие пустой коробки заменено условием полной; действие осталось тем же.",
    "simple_check": "В ситуации «коробка пуста» условие исходника выполнено; условие rejected не выполнено.",
    "counterexample": {},
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "source_language": "ru",
      "target_language": "en",
      "source_raw": "Если коробка пуста, закрой крышку.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR-EN-IF-01.ru-en.REF02.P01",
    "profile_id": "ru-en",
    "family_id": "sim:translation:logic-v2:letter-only-if-address-checked",
    "prompt": "Симуляция перевода. Переведи с языка «русский» на язык «английский». Ответ — только перевод; не добавляй объяснение, служебную разметку или новый факт. Это формулировка формального правила. Сохрани различие между необходимым условием разрешения действия и достаточным условием команды выполнить действие.\nИсходный текст: Отправь письмо, только если адрес проверен.",
    "chosen": "Send the letter only if the address has been checked.",
    "rejected": "Send the letter if the address has been checked.",
    "error_tag": "necessary_vs_sufficient",
    "rationale": "Необходимое условие отправки превращено в достаточное основание для команды отправить; «только если» и «если» не задают одно правило.",
    "simple_check": "Проверенный адрес без отправки не нарушает ограничение only if; rejected уже предписывает отправку.",
    "counterexample": {},
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "source_language": "ru",
      "target_language": "en",
      "source_raw": "Отправь письмо, только если адрес проверен.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR-EN-IF-01.ru-en.REF03.P01",
    "profile_id": "ru-en",
    "family_id": "sim:translation:logic-v2:sensor-on-indicator-on",
    "prompt": "Симуляция перевода. Переведи с языка «русский» на язык «английский». Ответ — только перевод; не добавляй объяснение, служебную разметку или новый факт.\nИсходный текст: Если датчик включён, индикатор загорается.",
    "chosen": "If the sensor is on, the indicator lights up.",
    "rejected": "If the indicator lights up, the sensor is on.",
    "error_tag": "implication_reversal",
    "rationale": "Направление условной связи обращено: из A→B не следует B→A.",
    "simple_check": "Индикатор может загораться и по другой причине; исходник этого не исключает, rejected исключает.",
    "counterexample": {},
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "source_language": "ru",
      "target_language": "en",
      "source_raw": "Если датчик включён, индикатор загорается.",
      "response_contract": "translation_only"
    }
  }
]
```

## Assigned source excerpts

These are bounded, partial source excerpts; they are not a claim to include the entire original or all its requirements. Use the exact linked local file if a detail needed by this lesson is absent.

RAG-023 — rag/sources/RAG-023.md
Различает сырое переводимое предложение, выбранный перевод и тип ошибки; описывает морфологию RU/HE и четыре старых направления.
Applicability: Актуально шесть направлений EN/RU/HE и раздельные темы. Не наследовать overlap>0.60, [WRONG]-аннотации и тезис «высокое сходство значит нет сигнала»: минимальная смысловая пара может быть полезной.
Primary excerpt (partial; full excerpt at the assigned path):
## Why This Document Exists

The orchestration DPO guidance covers ARCH, PLAN, SKILL, and CODE sub-phases.
Translation is a separate capability domain with different:

- A-side source (raw sentence, not compiled orchestration prompt)
- Chosen authority (verified parallel corpus, not XIIGen internal contract)
- Rejected error taxonomy (linguistic errors, not routing/governance errors)
- Reward margin mechanics (token-level linguistic divergence, not predicate match)

Read this document instead of DPO-PAIR-AUTHORING-GUIDANCE-v2.md when writing
translation pairs. Do not mix translation pairs into orchestration batches.

---

## Reward Margin Mechanics for Translation

DPO computes for every pair:


RAG-043 — rag/sources/RAG-043.md
Источник предлагает категории ошибки, пошаговый промпт построения rejected и проверку причинного различия. Одновременно он требует встроенную [WRONG]-аннотацию и перечисляет только четыре направления.
Applicability: Брать категории контрастов и критерий одного содержательного дефекта. [WRONG] и объяснение последствия хранить в rationale/error_tag, не в learner answer. Для текущего набора предусмотрены все шесть направлений EN/RU/HE. Пример formal register не считать чистым стилевым контрастом, если одновременно потеряно обязательство или число дней. Не переносить старые тексты автоматически как новые принятые строки.
Primary excerpt (partial; full excerpt at the assigned path):
A good rejected side is not random bad text. It is a plausible wrong answer that
teaches one specific boundary:

- wrong direction
- wrong topic route
- wrong adapter
- mistranslation
- omission
- addition
- morphology error
- register shift
- named-entity corruption
- Hebrew RTL/script error

Every rejected side must include:

```text
[WRONG: specific_error - functional consequence]

## Create, finish, continue

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A037\B0002\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A037\B0002\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A037\B0002\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A037\B0002\family-material.jsonl' '--model' 'unknown'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
