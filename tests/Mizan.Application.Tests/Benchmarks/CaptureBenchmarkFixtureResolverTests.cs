using System.Text;
using FluentAssertions;
using Xunit;

namespace Mizan.Application.Tests.Benchmarks;

public sealed class CaptureBenchmarkFixtureResolverTests : IDisposable
{
    private readonly string _root = Path.Combine(Path.GetTempPath(), "mizan-benchmark-fixtures", Guid.NewGuid().ToString("N"));

    public CaptureBenchmarkFixtureResolverTests() => Directory.CreateDirectory(_root);

    [Fact]
    public void Resolver_resolves_valid_svg_and_records_sha256()
    {
        var path = Write("receipts/receipt-001.svg", "<svg xmlns=\"http://www.w3.org/2000/svg\"><text>synthetic</text></svg>");

        var fixture = CaptureBenchmarkFixtureResolver.Resolve(
            _root, "RCPT-001", "receipt", "receipts/receipt-001.svg");

        fixture.CaseId.Should().Be("RCPT-001");
        fixture.Format.Should().Be("svg");
        fixture.SizeBytes.Should().Be(new FileInfo(path).Length);
        fixture.Sha256.Should().HaveLength(64);
    }

    [Fact]
    public void Resolver_rejects_missing_fixture()
    {
        var action = () => CaptureBenchmarkFixtureResolver.Resolve(
            _root, "RCPT-404", "receipt", "receipts/missing.svg");

        action.Should().Throw<FileNotFoundException>();
    }

    [Fact]
    public void Resolver_rejects_fixture_path_escape()
    {
        var action = () => CaptureBenchmarkFixtureResolver.Resolve(
            _root, "RCPT-ESCAPE", "receipt", "../outside.svg");

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*escapes benchmark root*");
    }

    [Fact]
    public void Resolver_reads_wav_metadata()
    {
        var bytes = new byte[44 + 1600];
        Encoding.ASCII.GetBytes("RIFF").CopyTo(bytes, 0);
        BitConverter.GetBytes(44 + 1600 - 8).CopyTo(bytes, 4);
        Encoding.ASCII.GetBytes("WAVE").CopyTo(bytes, 8);
        Encoding.ASCII.GetBytes("fmt ").CopyTo(bytes, 12);
        BitConverter.GetBytes(16).CopyTo(bytes, 16);
        BitConverter.GetBytes((short)1).CopyTo(bytes, 20);
        BitConverter.GetBytes((short)1).CopyTo(bytes, 22);
        BitConverter.GetBytes(8000).CopyTo(bytes, 24);
        BitConverter.GetBytes(16000).CopyTo(bytes, 28);
        BitConverter.GetBytes((short)2).CopyTo(bytes, 32);
        BitConverter.GetBytes((short)16).CopyTo(bytes, 34);
        Encoding.ASCII.GetBytes("data").CopyTo(bytes, 36);
        BitConverter.GetBytes(1600).CopyTo(bytes, 40);
        File.WriteAllBytes(Path.Combine(_root, "voice.wav"), bytes);

        var fixture = CaptureBenchmarkFixtureResolver.Resolve(
            _root, "VOICE-001", "voice", "voice.wav");

        fixture.Format.Should().Be("wav");
        fixture.SampleRate.Should().Be(8000);
        fixture.Channels.Should().Be(1);
        fixture.DurationSeconds.Should().BeApproximately(0.1, 0.0001);
    }

    [Fact]
    public void Resolver_rejects_unsupported_voice_format()
    {
        Write("voice.mp3", "not-a-real-audio-file");

        var action = () => CaptureBenchmarkFixtureResolver.Resolve(
            _root, "VOICE-002", "voice", "voice.mp3");

        action.Should().Throw<InvalidOperationException>()
            .WithMessage("*Unsupported voice fixture format*");
    }

    private string Write(string relativePath, string content)
    {
        var path = Path.Combine(_root, relativePath.Replace('/', Path.DirectorySeparatorChar));
        Directory.CreateDirectory(Path.GetDirectoryName(path)!);
        File.WriteAllText(path, content);
        return path;
    }

    public void Dispose()
    {
        if (Directory.Exists(_root))
            Directory.Delete(_root, recursive: true);
    }
}
