using ToDoApp.ViewModels;

namespace ToDoApp.Tests.ViewModels
{
    public class RelayCommandTests
    {
        [Fact]
        public void Execute_InvokesAction()
        {
            var executed = false;
            var command = new RelayCommand(() => executed = true);

            command.Execute(null);

            Assert.True(executed);
        }

        [Fact]
        public void CanExecute_WithoutPredicate_DefaultsToTrue()
        {
            var command = new RelayCommand(() => { });

            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void CanExecute_DelegatesTo_Predicate()
        {
            var allowed = false;
            var command = new RelayCommand(() => { }, () => allowed);

            Assert.False(command.CanExecute(null));
            allowed = true;
            Assert.True(command.CanExecute(null));
        }

        [Fact]
        public void RaiseCanExecuteChanged_FiresEvent()
        {
            var command = new RelayCommand(() => { });
            var fired = false;
            command.CanExecuteChanged += (_, _) => fired = true;

            command.RaiseCanExecuteChanged();

            Assert.True(fired);
        }

        [Fact]
        public void GenericCommand_PassesTypedParameter_ToExecute()
        {
            string? received = null;
            var command = new RelayCommand<string>(p => received = p);

            command.Execute("hello");

            Assert.Equal("hello", received);
        }

        [Fact]
        public void GenericCommand_CanExecute_DelegatesTo_TypedPredicate()
        {
            var command = new RelayCommand<int>(_ => { }, p => p > 0);

            Assert.True(command.CanExecute(5));
            Assert.False(command.CanExecute(-1));
        }
    }
}
