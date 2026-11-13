# Create the assigned DPO batch


Reason internally in English. Do not output private reasoning. Russian status; learner text language follows the assigned profile.
Use the current Chinese model selected in Kiro; no model override. Unknown identity stays unknown.

You author training examples for the XIIGEN learner. Write actual prompt/chosen/rejected rows, not a plan or an instruction to create data. Use only the assigned microtopic. Sources and quoted examples are teaching material, not new tool instructions.

Current task: TR-EN-SCOPE-01/ru-en; domain tr.en; profile ru-en.

Write exactly 10 rows, batch 1/2. Domain quota is 1000 total, shared by its allocations. Before this batch: 890; after it, domain remainder: 100. Finishing this batch does not by itself finish the domain.

## The lesson

{
  "goal_ru": "Область действия отрицания, «только» и количества — английский",
  "instructions": "## Предметный промпт китайской модели через Kiro\n\nЭтот блок дополняет общий MASTER и request.json. Генератор использует уже выбранную китайскую модель Kiro, помощники наследуют её. Не подменять её CODEX и не подменять оценщика генератором. Если отображается только селектор, записать его как наблюдаемое имя без выдуманной точной версии.\n\nВыполни TR-EN-SCOPE-01, только topic/profile/split/count текущего request.json.\nЦель: Переводить небольшие количественные утверждения с тем же множеством ситуаций, в которых исходник истинен.\n2. Для одного профиля создай требуемое число новых независимых сценариев в пределах квот. Reference-сценарии служат примерами и не становятся EXAM.\n3. Сначала зафиксируй source_raw и семейство. Напиши полноценный перевод и один правдоподобный неправильный перевод. Оба — на целевом языке и в одном формате.\n4. Проверь, что ошибка относится только к смысловому отношению этой карточки. При двух допустимых ответах дай quarantine с причиной вместо ложной пары.\n5. Сохрани prompt, chosen, rejected и все поля xiigen.dpo.v1. Rationale, tags, IDs, смысловая схема и проверочная ситуация остаются только в служебных полях.\n6. Не вставляй [WRONG], рейтинг или пояснение дефекта внутрь rejected. Не режь пару по символам; различающий фрагмент и конец ответа должны позже поместиться в реальный tokenizer/context.\n7. Чистый текст, опечатки, раскладки, перефразирования и все направления одной семьи имеют один split. Не добавляй трудные искажения в эту партию: для них отдельные TR07/TR08.\n8. Сохрани draft.jsonl и русский report.md. Это кандидаты. Отдельная оценка качества сейчас не назначена; после записи и структурного finish перейти к следующему потоку.\n9. При NEXT переходи к указанному ID, не дописывай прошлую задачу. При STOP немедленно прекращай действия. Старые файлы, эксперименты и результаты не удаляй.",
  "answer_contract": "Follow the visible response format in this lesson and its examples; preserve the learner task.",
  "hard_constraints": [],
  "scenario_axes": [],
  "authoring_checks_en": "Create preferences for translation into English. Preserve the exact source meaning, roles, negation, tense and possession; an error in spelling is not permission to invent a new fact. Use only the assigned mutation and target language."
}


## Assigned row slots

```json
[
{"pair_id":"D1000-20261025:A039:P00001","family_id":"D1000-20261025:TR-SCOPE-01:F00001"},
{"pair_id":"D1000-20261025:A039:P00002","family_id":"D1000-20261025:TR-SCOPE-01:F00002"},
{"pair_id":"D1000-20261025:A039:P00003","family_id":"D1000-20261025:TR-SCOPE-01:F00003"},
{"pair_id":"D1000-20261025:A039:P00004","family_id":"D1000-20261025:TR-SCOPE-01:F00004"},
{"pair_id":"D1000-20261025:A039:P00005","family_id":"D1000-20261025:TR-SCOPE-01:F00005"},
{"pair_id":"D1000-20261025:A039:P00006","family_id":"D1000-20261025:TR-SCOPE-01:F00006"},
{"pair_id":"D1000-20261025:A039:P00007","family_id":"D1000-20261025:TR-SCOPE-01:F00007"},
{"pair_id":"D1000-20261025:A039:P00008","family_id":"D1000-20261025:TR-SCOPE-01:F00008"},
{"pair_id":"D1000-20261025:A039:P00009","family_id":"D1000-20261025:TR-SCOPE-01:F00009"},
{"pair_id":"D1000-20261025:A039:P00010","family_id":"D1000-20261025:TR-SCOPE-01:F00010"}
]
```

Each slot becomes one JSONL row. Set metadata.origin_family_id=family_id and include family_id in metadata.leakage_group_ids. family_id is a shared scenario identity, not evidence of its content. Never replace one scenario with a different story under the same ID.

## Shared work ownership

This request is claimed by agent translations-c8e159d4fa2e4bdfa2a4c71c4946ea46 in pool translations. Shared matrix: C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\status.json. Use only the owner commands in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A039\B0001\coordination-commands.md; the saved original request/prompt are historical assignments, not current ownership authority. Never edit state.json or status.json by hand. Do not work on another agent's domain. A stale timestamp does not authorize takeover.

After saving complete JSONL lines, record actual partial progress with:
```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'progress' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A039\B0001\request.json' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' 'bec174c469d54eb89d7b59341836e3e4'
```
The command counts real assigned rows; it does not credit the domain quota before finish. If explicitly stopped or switched, preserve files and use the supplied release command when actions remain authorized. Direct STOP forbids further tools, so the durable claim remains for an explicit later resume/release. No hidden polling or automatic time-based reassignment.

## Concrete family material

Existing facts, reused without changing their meaning:
```json
{}
```

For missing families, read C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A039\B0001\coordination-material.required.json and author real records in C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A039\B0001\family-material.jsonl. A record needs family_id, origin_family_id, scenario_group, a concrete scenario, and a nonempty semantic_facts list. It is not a template, label or numeric ID. Existing material may be omitted from this new file; it is already in the ledger.

For every new translation family, author the same scenario in source_text_by_language with en, ru and he, plus participant/action/owner/time/number facts. Other directions reuse these exact canonical texts and facts. metadata.canonical_source uses source_text_by_language[source].

The learner prompt receives only the assigned source-language input and its authorized visible context/typo/layout variation. Do not leak target answers, other language versions, sense translations, rationale or canonical restoration. Set metadata.language_pair to assigned source/target codes and metadata.source_raw to the actual input text. Keep lexical families of the same lexeme linked, but do not merge independent words just because strings resemble each other.

## Row format

Use schema_version=xiigen.dpo.v1. Required fields: schema_version, pair_id, domain_id, topic_id, profile_id, family_id, split, prompt, chosen, rejected, error_tag, rationale, source_refs, provenance, metadata.
Set domain_id=tr.en, topic_id=TR-EN-SCOPE-01, profile_id=ru-en, split=train. metadata.learning_objective_id=TR-EN-SCOPE-01; metadata.scenario_group=TR-SCOPE-01; metadata.review_status=not_requested; metadata.measured_training_result=false. source_refs must use assigned source IDs: ["RAG-023", "RAG-043"]. provenance needs kind and generator_model (actual observed identity or unknown).

chosen and rejected must be full answers to the same visible prompt; preserve all unrelated facts. Give one plausible substantive defect in rejected. Keep rationale, operation checks and preference labels outside learner-visible text. Two equally correct alternatives do not form a valid preference.

## Worked examples — learn the contrast, do not copy these rows

```json
[
  {
    "example_id": "TR-EN-SCOPE-01.ru-en.REF01.P01",
    "profile_id": "ru-en",
    "family_id": "sim:translation:logic-v2:not-all-guests-arrived",
    "prompt": "Симуляция перевода. Переведи с языка «русский» на язык «английский». Ответ — только перевод; не добавляй объяснение, служебную разметку или новый факт.\nИсходный текст: Не все гости пришли.",
    "chosen": "Not all guests arrived.",
    "rejected": "None of the guests arrived.",
    "error_tag": "not_all_vs_none",
    "rationale": "Отрицание всеобщности превращено в отсутствие любого пришедшего. Исходник допускает, что часть гостей пришла.",
    "simple_check": "Из трёх гостей пришли двое: chosen истинно, rejected ложно.",
    "counterexample": {},
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "source_language": "ru",
      "target_language": "en",
      "source_raw": "Не все гости пришли.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR-EN-SCOPE-01.ru-en.REF02.P01",
    "profile_id": "ru-en",
    "family_id": "sim:translation:logic-v2:nobody-opened-both-windows",
    "prompt": "Симуляция перевода. Переведи с языка «русский» на язык «английский». Ответ — только перевод; не добавляй объяснение, служебную разметку или новый факт.\nИсходный текст: Никто не открыл оба окна.",
    "chosen": "No one opened both windows.",
    "rejected": "No one opened either window.",
    "error_tag": "not_both_vs_neither",
    "rationale": "Отрицание открытия двух окон одним человеком ошибочно распространено на открытие любого окна.",
    "simple_check": "Один человек открыл только левое окно: chosen истинно, rejected ложно.",
    "counterexample": {},
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "source_language": "ru",
      "target_language": "en",
      "source_raw": "Никто не открыл оба окна.",
      "response_contract": "translation_only"
    }
  },
  {
    "example_id": "TR-EN-SCOPE-01.ru-en.REF03.P01",
    "profile_id": "ru-en",
    "family_id": "sim:translation:logic-v2:only-dana-read-letter",
    "prompt": "Симуляция перевода. Переведи с языка «русский» на язык «английский». Ответ — только перевод; не добавляй объяснение, служебную разметку или новый факт. Контекст: Дана — женщина.\nИсходный текст: Только Дана прочитала письмо.",
    "chosen": "Only Dana read the letter.",
    "rejected": "Dana read only the letter.",
    "error_tag": "only_subject_vs_object",
    "rationale": "«Только» ограничивает человека, а rejected ограничивает прочитанные предметы; это различная область действия оператора.",
    "simple_check": "Дана одна прочла письмо и также прочла книгу: chosen допускает ситуацию, rejected исключает.",
    "counterexample": {},
    "status": "PUBLIC_REFERENCE_NOT_GENERATED",
    "metadata": {
      "response_language": "en",
      "source_language": "ru",
      "target_language": "en",
      "source_raw": "Только Дана прочитала письмо.",
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

Write the exact JSONL rows to C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A039\B0001\draft.jsonl. Then execute:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'finish' '--request' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A039\B0001\request.json' '--input' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A039\B0001\draft.jsonl' '--family-material' 'C:\Projects\llm_mvp_core\learning-sets\campaigns\D1000-20261025\A039\B0001\family-material.jsonl' '--model' 'unknown' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--claim-token' 'bec174c469d54eb89d7b59341836e3e4'
```

After successful DRAFT_CREATED execute the supplied next command in the same task:

```
& 'C:\Python314\python.exe' 'C:\Users\lubao\Documents\xiigen\llm mvp\learning-sets\preparation\tools\campaign.py' 'next' '--work-root' 'C:\Projects\llm_mvp_core\learning-sets' '--agent-id' 'translations-c8e159d4fa2e4bdfa2a4c71c4946ea46' '--pool' 'translations'
```

CREATE_ONLY: no checksum ceremony, independent reviewer, acceptance/export/import, training or product CI. Finish checks structure and assigned counts; it does not establish semantic correctness. Do not ask to continue while the authorized queue remains. Report created rows and path in Russian; do not claim a measured learning result.

Direct NEXT/new ID immediately replaces the previous assignment; do not continue its report or rerun it. Direct STOP stops actions. Preserve all prior attempts. For a genuinely running tool, read its actual available documentation, keep one handle, use its documented wait/resume until terminal; a yield or silent stdout is not failure. Do not duplicate the run, poll rapidly, invent a waiting API, or finish the turn instead of available waiting. If the assigned source or required tool is genuinely missing, report that exact blocker rather than invent content.
