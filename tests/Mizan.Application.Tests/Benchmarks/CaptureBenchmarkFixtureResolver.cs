using System.Security.Cryptography;
using System.Text;
using System.Xml.Linq;

namespace Mizan.Application.Tests.Benchmarks;

public sealed record CaptureBenchmarkFixture(
    string CaseId,
    string Channel,
    string FixtureReference,
    string Path,
    string Sha256,
    long SizeBytes,
    string Format,
    int? Width,
    int? Height,
    int? SampleRate,
    int? Channels,
    double? DurationSeconds);

public static class CaptureBenchmarkFixtureResolver
{
    public static CaptureBenchmarkFixture Resolve(
        string benchmarkRoot,
        string caseId,
        string channel,
        string fixtureReference)
    {
        if (string.IsNullOrWhiteSpace(benchmarkRoot))
            throw new ArgumentException("Benchmark root is required.", nameof(benchmarkRoot));
        if (string.IsNullOrWhiteSpace(caseId))
            throw new ArgumentException("Case id is required.", nameof(caseId));
        if (channel is not ("receipt" or "voice"))
            throw new InvalidOperationException($"Fixture resolution is only supported for receipt/voice channels: {channel}");

        var relative = fixtureReference.Replace('/', Path.DirectorySeparatorChar);
        var fullPath = Path.GetFullPath(Path.Combine(benchmarkRoot, relative));

        var root = Path.GetFullPath(benchmarkRoot)
            .TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar)
            + Path.DirectorySeparatorChar;

        if (!fullPath.StartsWith(root, StringComparison.OrdinalIgnoreCase))
            throw new InvalidOperationException($"Fixture path escapes benchmark root: {fixtureReference}");

        if (!File.Exists(fullPath))
            throw new FileNotFoundException("Benchmark fixture is missing.", fullPath);

        var info = new FileInfo(fullPath);
        if (info.Length == 0)
            throw new InvalidOperationException($"Benchmark fixture is empty: {fixtureReference}");

        var extension = info.Extension.ToLowerInvariant();
        var metadata = channel == "receipt"
            ? ReadImageMetadata(fullPath, extension)
            : ReadAudioMetadata(fullPath, extension);

        var sha256 = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(fullPath))).ToLowerInvariant();

        return new CaptureBenchmarkFixture(
            caseId, channel, fixtureReference, fullPath, sha256, info.Length,
            metadata.Format, metadata.Width, metadata.Height,
            metadata.SampleRate, metadata.Channels, metadata.DurationSeconds);
    }

    private static FixtureMetadata ReadImageMetadata(string path, string extension)
    {
        return extension switch
        {
            ".png" => ReadPng(path),
            ".jpg" or ".jpeg" => ReadJpeg(path),
            ".svg" => ReadSvg(path),
            _ => throw new InvalidOperationException($"Unsupported receipt fixture format: {extension}")
        };
    }

    private static FixtureMetadata ReadAudioMetadata(string path, string extension)
    {
        return extension switch
        {
            ".wav" => ReadWav(path),
            _ => throw new InvalidOperationException($"Unsupported voice fixture format: {extension}")
        };
    }

    private static FixtureMetadata ReadPng(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length < 24 || !bytes.AsSpan(0, 8).SequenceEqual(new byte[] { 137,80,78,71,13,10,26,10 }))
            throw new InvalidOperationException($"Invalid PNG fixture: {path}");
        var width = ReadBigEndianInt32(bytes, 16);
        var height = ReadBigEndianInt32(bytes, 20);
        return new FixtureMetadata("png", width, height, null, null, null);
    }

    private static FixtureMetadata ReadJpeg(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length < 4 || bytes[0] != 0xFF || bytes[1] != 0xD8)
            throw new InvalidOperationException($"Invalid JPEG fixture: {path}");
        return new FixtureMetadata("jpeg", null, null, null, null, null);
    }

    private static FixtureMetadata ReadSvg(string path)
    {
        try
        {
            var document = XDocument.Load(path, LoadOptions.PreserveWhitespace);
            if (document.Root?.Name.LocalName != "svg")
                throw new InvalidOperationException($"SVG root element is not <svg>: {path}");
        }
        catch (Exception ex) when (ex is not InvalidOperationException)
        {
            throw new InvalidOperationException($"Invalid SVG fixture: {path}", ex);
        }

        return new FixtureMetadata("svg", null, null, null, null, null);
    }

    private static FixtureMetadata ReadWav(string path)
    {
        var bytes = File.ReadAllBytes(path);
        if (bytes.Length < 44 ||
            Encoding.ASCII.GetString(bytes, 0, 4) != "RIFF" ||
            Encoding.ASCII.GetString(bytes, 8, 4) != "WAVE")
            throw new InvalidOperationException($"Invalid WAV fixture: {path}");

        var channels = BitConverter.ToUInt16(bytes, 22);
        var sampleRate = BitConverter.ToInt32(bytes, 24);
        var byteRate = BitConverter.ToInt32(bytes, 28);
        var dataSize = BitConverter.ToUInt32(bytes, 40);
        var duration = byteRate > 0 ? dataSize / (double)byteRate : null;

        if (channels <= 0 || sampleRate <= 0 || byteRate <= 0 || duration is null || duration <= 0)
            throw new InvalidOperationException($"Invalid WAV metadata: {path}");

        return new FixtureMetadata("wav", null, null, sampleRate, channels, duration);
    }

    private static int ReadBigEndianInt32(byte[] bytes, int offset) =>
        (bytes[offset] << 24) | (bytes[offset + 1] << 16) | (bytes[offset + 2] << 8) | bytes[offset + 3];

    private sealed record FixtureMetadata(
        string Format,
        int? Width,
        int? Height,
        int? SampleRate,
        int? Channels,
        double? DurationSeconds);
}
