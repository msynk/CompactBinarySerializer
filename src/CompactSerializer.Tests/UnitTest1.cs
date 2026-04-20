using CompactSerializer;

namespace CompactSerializer.Tests;

public sealed class CompactBinarySerializerTests
{
    [Fact]
    public void SerializeAndDeserialize_RoundTrips_ComplexObject()
    {
        var sample = new TestEnvelope
        {
            Id = 42,
            Name = "device-alpha",
            Priority = TestPriority.High,
            CreatedAtUtc = new DateTime(2026, 4, 20, 7, 30, 0, DateTimeKind.Utc),
            CorrelationId = Guid.NewGuid(),
            OptionalCount = 17,
            Tags = ["one", "two", "three"],
            Readings = [12, -5, 77],
            Payload = [1, 2, 3, 4, 5],
            Child = new TestChild
            {
                Label = "nested",
                IsActive = true
            }
        };

        var bytes = CompactBinarySerializer.Serialize(sample);
        var restored = CompactBinarySerializer.Deserialize<TestEnvelope>(bytes);

        Assert.NotNull(restored);
        Assert.Equal(sample.Id, restored.Id);
        Assert.Equal(sample.Name, restored.Name);
        Assert.Equal(sample.Priority, restored.Priority);
        Assert.Equal(sample.CreatedAtUtc, restored.CreatedAtUtc);
        Assert.Equal(sample.CorrelationId, restored.CorrelationId);
        Assert.Equal(sample.OptionalCount, restored.OptionalCount);
        Assert.Equal(sample.Tags, restored.Tags);
        Assert.Equal(sample.Readings, restored.Readings);
        Assert.Equal(sample.Payload, restored.Payload);
        Assert.NotNull(restored.Child);
        Assert.Equal(sample.Child!.Label, restored.Child!.Label);
        Assert.Equal(sample.Child.IsActive, restored.Child.IsActive);
    }

    [Fact]
    public void Serialize_NullRoot_ThrowsArgumentNullException()
    {
        var exception = Assert.Throws<ArgumentNullException>(() => CompactBinarySerializer.Serialize<string>(null!));

        Assert.Equal("value", exception.ParamName);
    }

    [Fact]
    public void Deserialize_EmptyPayload_ThrowsArgumentException()
    {
        var exception = Assert.Throws<ArgumentException>(() => CompactBinarySerializer.Deserialize<TestEnvelope>([]));

        Assert.Equal("payload", exception.ParamName);
    }

    [Fact]
    public void Deserialize_NullForNonNullableRoot_ThrowsInvalidOperationException()
    {
        var payloadForNullString = new byte[] { 0 };

        var exception = Assert.Throws<InvalidOperationException>(() => CompactBinarySerializer.Deserialize<string>(payloadForNullString));

        Assert.Equal("Deserialization produced null for a non-nullable root type.", exception.Message);
    }

    [Fact]
    public void SerializeAndDeserialize_NullableProperty_PreservesNull()
    {
        var sample = new TestEnvelope
        {
            Id = 7,
            Name = "nullable-check",
            Priority = TestPriority.Low,
            CreatedAtUtc = DateTime.UtcNow,
            CorrelationId = Guid.NewGuid(),
            OptionalCount = null,
            Tags = [],
            Readings = [],
            Payload = [],
            Child = null
        };

        var bytes = CompactBinarySerializer.Serialize(sample);
        var restored = CompactBinarySerializer.Deserialize<TestEnvelope>(bytes);

        Assert.Null(restored.OptionalCount);
        Assert.Null(restored.Child);
    }

    private enum TestPriority
    {
        Low = 0,
        Normal = 1,
        High = 2
    }

    private sealed class TestEnvelope
    {
        [SyncOrder(0)]
        public int Id { get; set; }

        [SyncOrder(1)]
        public string Name { get; set; } = string.Empty;

        [SyncOrder(2)]
        public TestPriority Priority { get; set; }

        [SyncOrder(3)]
        public DateTime CreatedAtUtc { get; set; }

        [SyncOrder(4)]
        public Guid CorrelationId { get; set; }

        [SyncOrder(5)]
        public int? OptionalCount { get; set; }

        [SyncOrder(6)]
        public List<string> Tags { get; set; } = [];

        [SyncOrder(7)]
        public int[] Readings { get; set; } = [];

        [SyncOrder(8)]
        public byte[] Payload { get; set; } = [];

        [SyncOrder(9)]
        public TestChild? Child { get; set; }
    }

    private sealed class TestChild
    {
        [SyncOrder(0)]
        public string Label { get; set; } = string.Empty;

        [SyncOrder(1)]
        public bool IsActive { get; set; }
    }
}
