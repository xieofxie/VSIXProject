using EnvDTE;
using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Shell;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using Task = System.Threading.Tasks.Task;

namespace VSIXProject1.SuggestedActions
{
    internal class OpenSourceFile : ISuggestedAction
    {
        private string _filePath;

        public OpenSourceFile(string filePath, string message = "Open Source File")
        {
            DisplayText = message;
            _filePath = filePath;
        }

        public Task<object> GetPreviewAsync(CancellationToken cancellationToken)
        {
            // TODO should create a new one
            TextBlock preview = new TextBlock();
            preview.Padding = new Thickness(5);
            preview.Inlines.Add(new Run() { Text = _filePath });
            return Task.FromResult<object>(preview);
        }

        public Task<IEnumerable<SuggestedActionSet>> GetActionSetsAsync(CancellationToken cancellationToken)
        {
            return Task.FromResult<IEnumerable<SuggestedActionSet>>(null);
        }

        public bool HasActionSets
        {
            get { return false; }
        }

        public string DisplayText { get; }

        public ImageMoniker IconMoniker
        {
            get { return default(ImageMoniker); }
        }

        public string IconAutomationText
        {
            get
            {
                return null;
            }
        }

        public string InputGestureText
        {
            get
            {
                return null;
            }
        }

        public bool HasPreview
        {
            get { return true; }
        }

        public void Invoke(CancellationToken cancellationToken)
        {
            ThreadHelper.ThrowIfNotOnUIThread();

            DTE dte = (DTE)Package.GetGlobalService(typeof(DTE));
            dte.ItemOperations.OpenFile(_filePath);
        }

        public void Dispose()
        {
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            // This is a sample action and doesn't participate in LightBulb telemetry
            telemetryId = Guid.Empty;
            return false;
        }
    }
}
