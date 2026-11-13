# Code Documentation Batch Run

Status: PASS
Metric mode: runner_verified

| Metric | Value |
|---|---:|
| train rows | 50 |
| held-out rows | 50 |
| routing accuracy | 1.000 |
| source gate accuracy | 1.000 |
| version gate accuracy | 1.000 |
| transit gate accuracy | 1.000 |
| bottom-up order accuracy | 0.840 |
| callee-summary transit accuracy | 0.840 |
| local-vs-delegated accuracy | 0.840 |
| CPU ms | 453.125 |
| peak RAM MB | 35.793 |
| elapsed ms | 408.443 |

This run verifies the trainable documentation router path. It does not claim a full LoRA checkpoint unless a separate DPO model runner is attached.