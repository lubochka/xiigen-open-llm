# Internal Train Ready DPO Run — BPE tokenizer

- Run id:        `ru-en-l1-batch025-v6`
- Source:        `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-025\training-pairs-train-ready.jsonl`
- Source SHA256: `3B93DF5B27E8621BB27442F09331E3504ECD82ADE536EFEFF417E5B202DCCDA5`
- Pairs:         50
- Batches:       5
- Batch size:    10
- Context len:   128
- BPE vocab:     2048
- Result root:   `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-025\dpo-run\ru-en-l1-batch025-v6`
- Total elapsed: 14980.9 ms
- Avg loss:      0.690327
- Final loss:    0.689667
- Avg accuracy:  0.540
- Final accuracy:0.600
- Avg margin:    +0.005797
- Final margin:  +0.007080

## Per-batch metrics
| Batch | Pairs | Loss | Accuracy | Margin | ms | First | Last |
|---:|---:|---:|---:|---:|---:|---|---|
| 1 | 10 | 0.684947 | 0.800 | +0.016540 | 3876.1 | ru-en-general-l1-b025-001 | ru-en-general-l1-b025-010 |
| 2 | 10 | 0.696672 | 0.300 | -0.006897 | 2516.7 | ru-en-general-l1-b025-011 | ru-en-general-l1-b025-020 |
| 3 | 10 | 0.691009 | 0.500 | +0.004392 | 3001.4 | ru-en-general-l1-b025-021 | ru-en-general-l1-b025-030 |
| 4 | 10 | 0.689340 | 0.500 | +0.007871 | 2705.7 | ru-en-general-l1-b025-031 | ru-en-general-l1-b025-040 |
| 5 | 10 | 0.689667 | 0.600 | +0.007080 | 2582.1 | ru-en-general-l1-b025-041 | ru-en-general-l1-b025-050 |
