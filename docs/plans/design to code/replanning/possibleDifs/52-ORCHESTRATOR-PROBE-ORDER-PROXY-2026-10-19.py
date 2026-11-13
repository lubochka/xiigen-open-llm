#!/usr/bin/env python3
"""Source-equivalent selection proxy for DesignConversionTrainingPipeline.cs:2588-2633.
No .NET model, training, checkpoint or learning quality measurement is performed.
"""
import json
from collections import Counter


def current_probe(items, cap=10):
    selected = items[:cap]  # mirrors append in batch-record order until records.Count == 10
    units = sorted({x['unit'] for x in selected})
    unit_id = units[0] if len(units) == 1 else 'mixed-design-conversion-units'
    return {
        'selected': len(selected),
        'unit_id': unit_id,
        'covered_units': units,
        'per_unit_count': dict(sorted(Counter(x['unit'] for x in selected).items())),
        'illustrative_pass_rate': sum(x['toy_ok'] for x in selected)/len(selected),
    }

A = [{'id': f'A-{i:02d}', 'unit':'unit-A', 'toy_ok':True} for i in range(10)]
B = [{'id': f'B-{i:02d}', 'unit':'unit-B', 'toy_ok':False} for i in range(7)]
C = [{'id': f'C-{i:02d}', 'unit':'unit-C', 'toy_ok':True} for i in range(7)]
sorted_items = A + B + C
interleaved = [item for block in zip(A[:7], B, C) for item in block] + A[7:]
assert set(x['id'] for x in sorted_items) == set(x['id'] for x in interleaved)
assert len(sorted_items) == len(interleaved) == 24
report={
  'label':'ANALYTIC_PROXY_NOT_NATIVE_PRODUCT',
  'source':'44.DesignConversionDomain/DesignConversionTrainingPipeline.cs#L2588-L2633',
  'same_records_and_toy_outcomes':True,
  'total_records':24,
  'total_unit_coverage':3,
  'total_illustrative_pass_rate':sum(x['toy_ok'] for x in sorted_items)/24,
  'sorted':current_probe(sorted_items),
  'interleaved':current_probe(interleaved),
}
print(json.dumps(report,ensure_ascii=False,indent=2,sort_keys=True))
