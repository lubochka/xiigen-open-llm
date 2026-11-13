# Internal Train Ready DPO Run — BPE tokenizer

- Run id:        `ru-en-l1-batch025-v8`
- Source:        `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-025\training-pairs-train-ready.jsonl`
- Source SHA256: `7D2FD94577F06E59F3B9343F7DE03750750C6CA70328201FA42918DC093F70E9`
- Pairs:         50
- Batches:       5
- Batch size:    10
- Context len:   128
- BPE vocab:     2048
- Result root:   `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-025\dpo-run\ru-en-l1-batch025-v8`
- Total elapsed: 16836.8 ms
- Avg loss:      0.693491
- Final loss:    0.691432
- Avg accuracy:  0.500
- Final accuracy:0.700
- Avg margin:    -0.000590
- Final margin:  +0.003498

## Per-batch metrics
| Batch | Pairs | Loss | Accuracy | Margin | ms | First | Last |
|---:|---:|---:|---:|---:|---:|---|---|
| 1 | 10 | 0.692849 | 0.600 | +0.000680 | 3344.6 | ru-en-general-l1-b025-001 | ru-en-general-l1-b025-010 |
| 2 | 10 | 0.689985 | 0.700 | +0.006415 | 3004.1 | ru-en-general-l1-b025-011 | ru-en-general-l1-b025-020 |
| 3 | 10 | 0.697302 | 0.200 | -0.008248 | 2177.1 | ru-en-general-l1-b025-021 | ru-en-general-l1-b025-030 |
| 4 | 10 | 0.695889 | 0.300 | -0.005296 | 4616.2 | ru-en-general-l1-b025-031 | ru-en-general-l1-b025-040 |
| 5 | 10 | 0.691432 | 0.700 | +0.003498 | 3433.7 | ru-en-general-l1-b025-041 | ru-en-general-l1-b025-050 |
