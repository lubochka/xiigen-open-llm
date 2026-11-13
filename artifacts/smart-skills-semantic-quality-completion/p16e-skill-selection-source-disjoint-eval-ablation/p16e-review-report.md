# P16E: source-disjoint eval и ablation для SkillSelection

## Краткий вывод

P16E проверил кандидат `SkillSelection` из P16D только как artifact-level schema. Это не production runtime evaluation: production code не менялся, тестовый код не менялся, обучение не запускалось, checkpoint P16D не мутировался.

Вердикт: `PASS_CANDIDATE_WITH_GAPS_READY_FOR_PARENT_REVIEW`.

Это значит: пакет evidence готов для родительского review, но сам кандидат не прошел положительную source-disjoint проверку. Он показал полезную границу: shortcut leaks в ablation не обнаружены, однако reserved eval/holdout rows не выбираются корректно.

## Что оценивалось

В eval использованы только строки P16B со split-ролями `eval_candidate` и `holdout_candidate`. Они не использовались как training input и не добавлялись в checkpoint.

Итог по 12 строкам:

- positive pass: 0;
- fail: 8;
- gap: 4.

Основные gap:

- `prompt_understanding` не имеет learned profile, потому что единственная train-candidate строка для этой unit была `evidence_only` и правильно исключена в P16D;
- `candidate_position_guard` не может выбирать skill, потому что `candidate_order_leakage_guard` является только диагностикой;
- часть reserved строк ожидает skills, которых нет среди четырех материализованных profiles.

## Ablation

Проверены 72 ablation rows по шести control classes:

- remove prompt signals;
- remove task/context signals;
- candidate order permutation;
- remove diagnostics-only candidate order guard;
- source/id/title/path leakage attempt;
- exact prompt memorization attempt.

Shortcut leak count: 0.

Это хороший знак только для отсутствия явной shortcut-утечки в schema-level evaluator. Это не доказывает semantic quality, потому что positive eval behavior не прошел.

## Contamination proof

Eval/holdout row ids отсутствуют в:

- `trained_pair_ids`;
- section `training_pair_ids`;
- profile `source_pair_ids`.

P16D checkpoint не изменялся и не перезаписывался.

## Что остается слабым

Следующий безопасный шаг зависит от parent review:

- либо добавить train-only SkillSelection coverage для missing units/skills и затем повторить P16E;
- либо принять этот пакет как honest gap evidence и открыть отдельный bounded repair phase.

P16F fresh-load/continue-training имеет смысл только после положительной component eval/ablation evidence или после явного parent решения принять gap-boundary.

## Что не заявляется

P16E не заявляет:

- semantic quality;
- SkillSelection readiness;
- full API readiness;
- trained-model readiness;
- dashboard readiness;
- host runtime readiness;
- final model readiness.
