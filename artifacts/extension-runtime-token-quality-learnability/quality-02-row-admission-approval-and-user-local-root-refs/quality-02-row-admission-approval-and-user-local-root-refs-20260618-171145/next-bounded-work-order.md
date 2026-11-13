# Next bounded work order

Work order: QUALITY-02-COLLECT-EXACT-ROW-SPECIFIC-ADMISSION-APPROVAL-NO-EVAL-EXECUTION

Цель: найти только точный row-specific approval_ref для 21 USER_ADAPTIVE_EVAL_ONLY local-private rows или доказать, что такого approval сейчас нет.

- Root ref уже связан этим packet: C:\Users\lubao\Documents\xiigen\sonnet local - analyzes.
- Eval запускать нельзя.
- approved-eval-input.local-private.jsonl можно создавать только в следующем admission packet, если точный approval_ref найден и все 21 rows complete.
- Git authority: forbidden; parent делает commit/push после review.
