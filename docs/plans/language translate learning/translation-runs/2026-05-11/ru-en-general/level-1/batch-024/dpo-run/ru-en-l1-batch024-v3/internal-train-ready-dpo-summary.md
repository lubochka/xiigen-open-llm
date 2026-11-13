# Internal Train Ready DPO Run — BPE tokenizer

- Run id:        `ru-en-l1-batch024-v3`
- Source:        `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-024\training-pairs-train-ready.jsonl`
- Source SHA256: `CFF866D4850B4D87D456E2AD7D8F8D0FE2CA81C8132F155CE838C463B401F108`
- Pairs:         50
- Batches:       5
- Batch size:    10
- Context len:   128
- BPE vocab:     2048
- Result root:   `C:\Users\lubao\Documents\xiigen\llm mvp\language translate learning\translation-runs\2026-05-11\ru-en-general\level-1\batch-024\dpo-run\ru-en-l1-batch024-v3`
- Total elapsed: 22234.8 ms
- Avg loss:      0.686142
- Final loss:    0.687199
- Avg accuracy:  0.680
- Final accuracy:0.700
- Avg margin:    +0.014230
- Final margin:  +0.012068

## Per-batch metrics
| Batch | Pairs | Loss | Accuracy | Margin | ms | First | Last |
|---:|---:|---:|---:|---:|---:|---|---|
| 1 | 10 | 0.684922 | 0.700 | +0.016689 | 5045.5 | ru-en-general-l1-b024-001 | ru-en-general-l1-b024-010 |
| 2 | 10 | 0.685930 | 0.600 | +0.014654 | 5280.3 | ru-en-general-l1-b024-011 | ru-en-general-l1-b024-020 |
| 3 | 10 | 0.683810 | 0.700 | +0.019009 | 4028.7 | ru-en-general-l1-b024-021 | ru-en-general-l1-b024-030 |
| 4 | 10 | 0.688848 | 0.700 | +0.008731 | 3931.9 | ru-en-general-l1-b024-031 | ru-en-general-l1-b024-040 |
| 5 | 10 | 0.687199 | 0.700 | +0.012068 | 3533.6 | ru-en-general-l1-b024-041 | ru-en-general-l1-b024-050 |
