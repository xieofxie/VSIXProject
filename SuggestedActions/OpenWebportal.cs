using Microsoft.VisualStudio.Imaging.Interop;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;

namespace VSIXProject1.SuggestedActions
{
    internal class OpenWebPortal : ISuggestedAction
    {
        private string _url;

        public OpenWebPortal(string url, string message = "Open in browser")
        {
            DisplayText = message;
            _url = url;
        }

        public Task<object> GetPreviewAsync(CancellationToken cancellationToken)
        {
            // TODO should create a new one
            TextBlock preview = new TextBlock();
            preview.Padding = new Thickness(5);
            preview.Inlines.Add(new Run() { Text = _url });
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
            System.Diagnostics.Process.Start(_url);
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
