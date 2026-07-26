// Copyright (C) 2026 Luba Karpenko, Genie Soft LTD
// Licensed under GNU Affero General Public License v3.0.
// SPDX-License-Identifier: AGPL-3.0-only
// See LICENSE file. Original project: <https://github.com/lubochka/xiigen-open-llm>

using DatasetPipeline;

namespace LlmFromScratch.Tests;

public class NpyWriterTests
{
    [Fact]
    public void Write_ProducesValidNpyHeader()
    {
        var tmp = Path.GetTempFileName() + ".npy";
        ushort[] data = [1, 2, 3, 4, 5];
        NpyWriter.Write(tmp, data);

        var bytes = File.ReadAllBytes(tmp);
        // Magic: \x93NUMPY
        Assert.Equal(0x93, bytes[0]);
        Assert.Equal((byte)'N', bytes[1]);
        Assert.Equal((byte)'U', bytes[2]);
        Assert.Equal((byte)'M', bytes[3]);
        Assert.Equal((byte)'P', bytes[4]);
        Assert.Equal((byte)'Y', bytes[5]);

        File.Delete(tmp);
    }

    [Fact]
    public void Write_DataRoundTrips_AsUint16()
    {
        var tmp = Path.GetTempFileName() + ".npy";
        ushort[] data = [100, 200, 300, 65535];
        NpyWriter.Write(tmp, data);

        var bytes = File.ReadAllBytes(tmp);
        // header is 128 bytes (64-byte aligned v1.0)
        int headerSize = 128;
        for (int i = 0; i < data.Length; i++)
        {
            ushort val = BitConverter.ToUInt16(bytes, headerSize + i * 2);
            Assert.Equal(data[i], val);
        }

        File.Delete(tmp);
    }
}
