using ToDoApp.Views.Converters;

namespace ToDoApp.Tests.Views.Converters
{
    public class TaskSearchMatcherTests
    {
        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData("   ")]
        public void Matches_WithEmptySearchText_IsAlwaysTrue(string? searchText)
        {
            Assert.True(TaskSearchMatcher.Matches("Cualquier título", "Cualquier descripción", searchText));
        }

        [Fact]
        public void Matches_WhenSearchTextIsInTitle_IsTrue()
        {
            Assert.True(TaskSearchMatcher.Matches("Comprar leche", "", "leche"));
        }

        [Fact]
        public void Matches_WhenSearchTextIsInDescription_IsTrue()
        {
            Assert.True(TaskSearchMatcher.Matches("Comprar", "Leche descremada", "descremada"));
        }

        [Fact]
        public void Matches_IsCaseInsensitive()
        {
            Assert.True(TaskSearchMatcher.Matches("Comprar LECHE", "", "leche"));
        }

        [Fact]
        public void Matches_WhenSearchTextIsNotFound_IsFalse()
        {
            Assert.False(TaskSearchMatcher.Matches("Comprar pan", "Integral", "leche"));
        }
    }
}
