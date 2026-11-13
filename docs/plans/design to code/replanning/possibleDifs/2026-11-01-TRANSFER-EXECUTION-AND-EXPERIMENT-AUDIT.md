# Аудит исполнения переносов и точности экспериментов — 1 ноября 2026

<!-- POST88-AUDIT-2026-11-01:START -->
## Действующее дополнение после опыта88

[Аудит исполнения, новые ошибки и план ремонта](2026-11-01-POST88-TRANSFER-AND-EXPERIMENT-AUDIT.md) закреплён на core `dbf4c99c519b76485f618f4a2043d7df024db88e`. В нём53 адресные карточки, независимый пересчёт P82–P88 и свежие native/component проверки без нового обучения. Утренний срез ниже сохраняется как история; его статусы нельзя читать без этой актуализации.
<!-- POST88-AUDIT-2026-11-01:END -->


## Действующий результат

На core `790228c256168a336b01abae4387e28391048c0c` очередь102 имеет **6 DONE / 2 PARTIAL / 1 OPEN / 1 CONDITIONAL**. Предыдущий planning snapshot — `1bec1b8907a87dc2f768bb950a488732ff925a00`. Сверены актуальные коммиты30–31 октября, owners/callers, сохранённые результаты P55–P81 и связанные планы. Новых Train0, native .NET/PowerShell execution0; Python пересчёт113 проверок не является повтором native suites.

- [107: доказательства, закрытые переносы и ограничения](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/107-transfer-and-experiment-audit-2026-11-01.md).
- [108: RF01–RF11, точные владельцы и приёмка](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/108-addressed-repair-plan-2026-11-01.md).
- [109:89 усиленных карточек, hashes и покрытие](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/docs/plans/replanning-possible-diffs/fixes-based-on-experiments/109-card-hardening-and-coverage-2026-11-01.md).
- [Воспроизводимый пересчёт и manifest](https://github.com/lubochka/llm_mvp_core/blob/codex/dpo-implementation/experiments-v2/_audits/2026-11-01-transfer-audit/README.md).

Ссылки выше ведут на действующую ветку; каждый audit фиксирует проверенный immutable source SHA. Это не отдельная scientific очередь. Более ранние ограничения «изменять только planning» относились к прежним заданиям; текущий запрос владельца прямо включает core plans и укрепление карточек в обоих репозиториях.

## Какие старые гипотезы теперь имеют более точный ответ

| Источник planning | Новое свидетельство | Что сохраняется открытым |
|---|---|---|
| N37 E28 adaptation/leakage | Composite project key внедрён, P72 EXAM3/4→4/4 | Current E28.Run всё ещё обучает generic на exam, adapted A не достигается на B, есть cross-file edges. Product consumer не доказан. |
| NEW26 E30 file-vector alias | Current action включён в native experimental address,44 координаты | Подключение к ordinary product flow и его предметная польза отдельно; next/gold не могут быть входом. |
| N23 prefix alias | P69 наблюдает collapse длинных sources и wrong retrieval | Новый encoder/window policy и обобщение ещё требуют отдельного решения. |
| N33 UTF8 cutoff | P76 replacement5→2, strict valid0/4→2/4 на frozen bytes | Full label0/4 после crop; interior corruption нельзя скрыть. Это decoder gain, не новый language skill. |
| N35 convergence | P78 cadence1 даёт3 updates, cadence5/max5 —5updates и1 monitor call | Это не moving window; correctness не выросла, actual calls reconstruction обозначена. |
| N26 verifier polarity | P55 и narrow native can’t guard полезны; P56 whole-token boundary различает already/ready | All tiers/languages не доказаны; eligibility order P64 остаётся. |
| N50 preference identities | P81 contrast1.71827459 при неправильном втором знаке | Prompt conditioning не равна joint sign success и acquisition; scorer/target identity проверяется до обучения. |
| Historical fusion validity | P30 evaluator реализован, но constant tie объявляется informative | Reader RF03 обязан проверять actual assigned alpha/observable, без повторного Train. |

В перечисленные8 исходных документов добавлены адресные актуализации; старый текст сохранён как история. Это прочитанное связанное подмножество, **не утверждение о глубокой проверке всех561 entries** каталога.

## Принятые переносы и приоритет ремонта

Default mutable KNN, route ownership/replacement, pre-step ProbeNN gradients, typed exact topic/source key, narrow polarity guard, malformed-JSON preflight, WikiTrace content fields, DataLoader stride/tail и expression refusal имеют действительный код. Закрывать их заново не требуется. При этом narrow preflight не является полной transaction, а corrected key не является доказательством принятого production integration. Точные исходники, saved receipts и численные границы находятся в107.

Первая очередь: staged typed translation transaction; legacy terminal/no-claim admission и reference-only imports; P30 constant-tie evaluator. Далее: чистый и достижимый E28 comparison, ordinary routing/no-evidence, TranslationKnnStore membership и eligibility before tier stop, два existing red test contracts. Actual typed checkpoint/topology, metric support, convergence semantics и revision authority имеют отдельные критерии108.

В saved broad tests P53/P55 остаётся313 PASS/2 FAIL, P51 —301/1; те же failures были BEFORE. Это не новая доказанная регрессия этих переносов, но общий green gate не получен. Addressed suites3/3,3/3,8/8 полезны в своей границе.

## Как изменена приёмка

89 карточек core получили индивидуальные cases: late typed/eval failure, legacy completed attempt, permanent tie, absent rare class, zero-hit treatment, actual source/file provenance, prompt-only view, revision authority, wrong-answer/refusal/execution-failure и empty denominator. Все44 P82–P125 включены; P125 остаётся alias прежнего результата, не будущим запуском. В P96 исправлена допустимая интерпретация cold native-call count; P88/P94 требуют подходящей mutable snapshot policy.

Дополнения применяются к оценке выводов и следующей назначенной работе. Они не увеличивают budgets, не меняют historical raw и не разрешают rerun. Simple experiment host/seam допустим; ordinary product consumer нужен для product-transfer claim. Новый report reader сначала использует сохранённые ответы и state; отсутствие доступного LFS payload даёт NOT_VERIFIED, не разрешение обучить замену.
