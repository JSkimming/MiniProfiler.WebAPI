namespace StackExchange.Profiling
{
    using System;
    using System.Linq;
    using Ploeh.AutoFixture;
    using Ploeh.SemanticComparison.Fluent;
    using Xunit;

    public class ProfilerExtensionsUnitTests
    {
        [Fact]
        public void ProfilerExtensions_SerializeDeserialize_Test()
        {
            // Arrange
            var fixture = new Fixture();
            var throwingRecursionBehavior = fixture.Behaviors.OfType<ThrowingRecursionBehavior>().FirstOrDefault();
            if (throwingRecursionBehavior != null)
                fixture.Behaviors.Remove(throwingRecursionBehavior);

            fixture.Behaviors.Add(new OmitOnRecursionBehavior());

            var expected = fixture.CreateAnonymous<MiniProfiler>();

            // The date serialization loses precision beyond millisecond.
            expected.Started =
                new DateTime(expected.Started.Year, expected.Started.Month, expected.Started.Day,
                             expected.Started.Hour, expected.Started.Minute, expected.Started.Second,
                             expected.Started.Millisecond%1000);

            // Act
            MiniProfiler actual = ProfilerExtensions.Deserialize(expected.Serialize());

            // Assert
            actual.AsSource()
                  .OfLikeness<MiniProfiler>()
                  .Without(m => m.ClientTimings)
                  .Without(m => m.Head)
                  .Without(m => m.HasDuplicateSqlTimings)
                  .ShouldEqual(expected);
        }
    }
}
