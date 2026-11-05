using System;
using System.Threading.Tasks;
using System.Windows;

namespace Gestion.Views
{
    public partial class LoadingWindow : Window
    {
        public LoadingWindow(string message)
        {
            InitializeComponent();
            TxtLoadingMessage.Text = message;
        }
    }

    public static class LoadingDialogService
    {
        public static async Task<T> RunAsync<T>(Window owner, Func<Task<T>> operation, string message)
        {
            if (owner == null) throw new ArgumentNullException(nameof(owner));
            if (operation == null) throw new ArgumentNullException(nameof(operation));

            LoadingWindow loadingWindow = null;
            bool ownerWasEnabled = owner.IsEnabled;

            try
            {
                Task<T> operationTask = operation();
                if (operationTask == null)
                    throw new InvalidOperationException("La operación no devolvió una tarea válida.");

                Task delayTask = Task.Delay(350);
                Task completedTask = await Task.WhenAny(operationTask, delayTask);

                if (completedTask != operationTask && !operationTask.IsCompleted)
                {
                    loadingWindow = new LoadingWindow(message) { Owner = owner };
                    loadingWindow.Show();
                    owner.IsEnabled = false;
                }

                return await operationTask;
            }
            finally
            {
                if (loadingWindow != null)
                    loadingWindow.Close();

                if (owner.IsEnabled != ownerWasEnabled)
                    owner.IsEnabled = ownerWasEnabled;
            }
        }
    }
}