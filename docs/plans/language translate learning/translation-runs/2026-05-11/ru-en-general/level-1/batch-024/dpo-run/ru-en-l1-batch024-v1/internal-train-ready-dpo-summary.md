# Internal Train Ready DPO Run — BPE tokenizer

- Run id:        `ru-en-l1-batch024-v1`
- Source:        `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-024\training-pairs-train-ready.jsonl`
- Source SHA256: `0654DB2B2426BAEF61CB255E2B441045CEC85DD25F97BC01C3F102F3F31069E5`
- Pairs:         50
- Batches:       5
- Batch size:    10
- Context len:   128
- BPE vocab:     2048
- Result root:   `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-024\dpo-run\ru-en-l1-batch024-v1`
- Total elapsed: 13541.2 ms
- Avg loss:      0.697212
- Final loss:    0.698547
- Avg accuracy:  0.320
- Final accuracy:0.200
- Avg margin:    -0.007986
- Final margin:  -0.010687

## Per-batch metrics
| Batch | Pairs | Loss | Accuracy | Margin | ms | First | Last |
|---:|---:|---:|---:|---:|---:|---|---|
| 1 | 10 | 0.695679 | 0.400 | -0.004966 | 3152.3 | ru-en-general-l1-b024-001 | ru-en-general-l1-b024-010 |
| 2 | 10 | 0.692528 | 0.500 | +0.001476 | 2757.3 | ru-en-general-l1-b024-011 | ru-en-general-l1-b024-020 |
| 3 | 10 | 0.701270 | 0.200 | -0.016079 | 2885.7 | ru-en-general-l1-b024-021 | ru-en-general-l1-b024-030 |
| 4 | 10 | 0.698039 | 0.300 | -0.009674 | 2190.0 | ru-en-general-l1-b024-031 | ru-en-general-l1-b024-040 |
| 5 | 10 | 0.698547 | 0.200 | -0.010687 | 2284.7 | ru-en-general-l1-b024-041 | ru-en-general-l1-b024-050 |
