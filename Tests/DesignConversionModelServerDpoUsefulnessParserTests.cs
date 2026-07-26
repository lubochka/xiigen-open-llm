// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using ModelServer.DesignConversion;

public sealed class DesignConversionModelServerDpoUsefulnessParserTests
{
    [Fact]
    public void Parse_DpoUsefulnessJson_ExposesTypedUsefulnessMetrics()
    {
        const string bodyJson = """
        {
          "endpoint": "/api/design-conversion/dpo-usefulness",
          "method": "GET",
          "httpSeam": "local_http_host_seam_no_socket_no_public_deployment",
          "apiShape": "design_conversion_modelserver_dpo_usefulness_route_v1",
          "runtimeSurface": "modelserver_design_conversion_dpo_usefulness_route",
          "statusCode": 200,
          "contractStatus": "dpo-usefulness-readback-completed",
          "dpoUsefulnessApiShape": "design_conversion_dpo_usefulness_api_v1",
          "rows": [
            {
              "recordId": "record-json-sample",
              "jobId": "job-json-sample",
              "requestId": "request-json-sample",
              "unitId": "critic",
              "reviewStatus": "train-ready",
              "usefulnessStatus": "high_usefulness",
              "usefulnessScore": 0.94,
              "trainReady": true,
              "heldOut": false,
              "quarantined": false,
              "providerMissing": false,
              "rejectedNegativeQuality": false,
              "productNotReady": true,
              "blockReasons": [],
              "retrainDecisionStatus": "ready-for-retrain-intake",
              "retrainDecisionEvidenceRefs": [
                "design-conversion-dpo-retrain-decision:status:ready-for-retrain-intake"
              ],
              "usefulnessMetrics": {
                "score": 0.94,
                "acceptanceReason": "ready-for-retrain-intake",
                "qualityGain": 0.57,
                "negativePairUsefulness": "spacing",
                "sourceCoverage": "source_kind=Figma;source_ref=present;source_disjoint=not_proven",
                "productReady": false,
                "productReadyBoundary": "product_ready=false"
              },
              "boundaryMetadata": {
                "api_product_ready": "False"
              }
            }
          ],
          "rowCount": 1,
          "trainReadyCount": 1,
          "quarantinedCount": 0,
          "providerMissingCount": 0,
          "rejectedNegativeQualityCount": 0,
          "heldOutCount": 0,
          "blockedCount": 0,
          "averageUsefulnessScore": 0.94,
          "minimumUsefulnessScore": 0.94,
          "maximumUsefulnessScore": 0.94,
          "productReady": false,
          "warnings": [
            "product_ready=false"
          ],
          "boundaryMetadata": {
            "product_ready": "False"
          }
        }
        """;

        var parsed = DesignConversionModelServerDpoUsefulnessResponseParser.Parse(bodyJson);

        Assert.True(parsed.ParseSucceeded, parsed.ErrorMessage);
        var typed = parsed.Response!;
        Assert.False(typed.ProductReady);
        var row = Assert.Single(typed.Rows);
        var metrics = row.UsefulnessMetrics;
        Assert.Equal(0.94, metrics.Score, precision: 6);
        Assert.Equal(0.57, metrics.QualityGain, precision: 6);
        Assert.Equal("spacing", metrics.NegativePairUsefulness);
        Assert.Contains("source_kind=Figma", metrics.SourceCoverage, StringComparison.Ordinal);
        Assert.Contains("source_ref=present", metrics.SourceCoverage, StringComparison.Ordinal);
        Assert.Contains("source_disjoint=not_proven", metrics.SourceCoverage, StringComparison.Ordinal);
        Assert.False(metrics.ProductReady);
        Assert.Equal("product_ready=false", metrics.ProductReadyBoundary);
    }
}
