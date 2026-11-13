# P27 dataset card - SkillSelection repair dataset candidate

Вердикт: это проверенный кандидат repair dataset для будущего SkillSelection training slice. Это не обучение, не мутация checkpoint, не promotion и не замена common model.

## Что материализовано

P27 взял 8 draft DPO rows из P26 и нормализовал их в dataset candidate. На каждый из 4 провалов P25 есть две строки:

- `IPromptContextTraining`: учит контекстный предварительный выбор правильного skill;
- `ISkillSelectionTraining`: учит финальный выбор skill из того же набора кандидатов.

Конкретные примеры:

- Для `e2e-test-matrix` chosen поведение выбирает skill, который строит scenario matrix с success, idempotency, delayed behavior, recovery, boundary и security checks до implementation. Rejected поведение повторяет ошибку P25: `implementation-integrity`, `plan-review`, `test-integrity` вместо `e2e-test-matrix`.
- Для `no-product-decisions` chosen поведение выбирает skill, когда задача просит выбрать между альтернативами, меняющими user behavior, public interface shape или architecture direction. Rejected поведение уходит в implementation или claim checks до решения владельца продукта.
- Для `output-skepticism` chosen поведение проверяет result claim against refuting evidence, scope validity и proxy-measure risk до отчета. Rejected поведение выбирает plan/reverification соседей и пропускает skeptical claim gate.
- Для `self-verification` chosen поведение выбирает self-check после изменения артефакта: fresh fixtures, blast-radius classification, cascade tracing и prediction matching. Rejected поведение выбирает более общие test/implementation integrity skills и не закрывает самопроверку результата.

## Откуда пришли строки

Источник строк только локальный и проектный: P24 accepted local-private runtime holdout rows, P25 side-by-side runtime comparison failures и P26 failure map plus repair draft. Raw private corpus text не переносился: строки используют redacted summaries, source hashes и skill ids.

## Почему это еще не common training

Все строки имеют `commonTrainingAllowed=false` и `notUsedForTrainingYet=true`. P24 доказал, что holdout rows были disjoint относительно тогдашних checked checkpoints, но P27 не доказывает, что эти local-private строки можно переносить в common training. Для common reuse нужно отдельное разрешение Luba, license/source review, redaction review и новый source-disjoint split, который не опирается на P24/P25/P26 holdout lineage.

## Правила карантина

- Можно рассматривать как local-private train-candidate для будущего отдельного bounded training slice.
- Нельзя использовать как common training rows в P27.
- Нельзя использовать для promotion или semantic-quality claim.
- Нельзя обучать до отдельного source-disjoint split, fresh-load, continue-training, export/import, ablation и candidate-order leakage gates.

## Что должно случиться перед training

Следующий training slice должен быть отдельным work order. Он обязан загрузить существующий checkpoint, построить source-disjoint train/eval split, обучить candidate checkpoint только в разрешенной области, затем пройти fresh-load, continue-training, export/import, ablation/no-shortcut и leakage gates. До этого P27 остается только checked dataset candidate.
