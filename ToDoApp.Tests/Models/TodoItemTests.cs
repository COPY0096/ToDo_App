using System.ComponentModel;
using ToDoApp.Models;

namespace ToDoApp.Tests.Models
{
    public class TodoItemTests
    {
        [Fact]
        public void Estado_DefaultsTo_Pendiente()
        {
            var item = new TodoItem();

            Assert.Equal(TodoEstado.Pendiente, item.Estado);
            Assert.False(item.IsDone);
        }

        [Fact]
        public void SettingIsDone_True_SyncsEstado_ToCompletado()
        {
            var item = new TodoItem();

            item.IsDone = true;

            Assert.Equal(TodoEstado.Completado, item.Estado);
        }

        [Fact]
        public void SettingIsDone_False_SyncsEstado_ToPendiente()
        {
            var item = new TodoItem { IsDone = true };

            item.IsDone = false;

            Assert.Equal(TodoEstado.Pendiente, item.Estado);
        }

        [Fact]
        public void SettingEstado_Completado_SyncsIsDone_ToTrue()
        {
            var item = new TodoItem();

            item.Estado = TodoEstado.Completado;

            Assert.True(item.IsDone);
        }

        [Fact]
        public void SettingEstado_Cancelado_DoesNotMarkIsDone()
        {
            // IsDone only tracks the Completado state, not Cancelado.
            var item = new TodoItem();

            item.Estado = TodoEstado.Cancelado;

            Assert.False(item.IsDone);
        }

        [Fact]
        public void SettingTitle_RaisesPropertyChanged_OnlyWhenValueChanges()
        {
            var item = new TodoItem { Title = "Original" };
            var raisedProperties = new List<string?>();
            item.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

            item.Title = "Original"; // no change
            item.Title = "Updated";  // change

            Assert.Single(raisedProperties);
            Assert.Equal(nameof(TodoItem.Title), raisedProperties[0]);
        }

        [Fact]
        public void SettingIsDone_RaisesPropertyChanged_ForBothIsDoneAndEstado()
        {
            var item = new TodoItem();
            var raisedProperties = new List<string?>();
            item.PropertyChanged += (_, e) => raisedProperties.Add(e.PropertyName);

            item.IsDone = true;

            Assert.Contains(nameof(TodoItem.IsDone), raisedProperties);
            Assert.Contains(nameof(TodoItem.Estado), raisedProperties);
        }
    }
}
