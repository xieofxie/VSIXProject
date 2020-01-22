using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.VisualStudio.Language.Intellisense;
using Microsoft.VisualStudio.Text;
using Microsoft.VisualStudio.Text.Editor;
using Microsoft.VisualStudio.Text.Operations;
using Microsoft.VisualStudio.Utilities;
using System.ComponentModel.Composition;
using System.Threading;
using Newtonsoft.Json.Linq;
using Microsoft.Bot.Configuration;
using VSIXProject1.Utils;
using VSIXProject1.SuggestedActions;
using VSIXProject1.Models;
using System.IO;

namespace VSIXProject1
{
    [Export(typeof(ISuggestedActionsSourceProvider))]
    [Name("Test Suggested Actions")]
    [ContentType("json")]
    internal class TestSuggestedActionsSourceProvider : ISuggestedActionsSourceProvider
    {
        [Import(typeof(ITextStructureNavigatorSelectorService))]
        internal ITextStructureNavigatorSelectorService NavigatorService { get; set; }

        public ISuggestedActionsSource CreateSuggestedActionsSource(ITextView textView, ITextBuffer textBuffer)
        {
            if (textBuffer == null || textView == null)
            {
                return null;
            }
            return new TestSuggestedActionsSource(this, textView, textBuffer);
        }
    }

    internal class TestSuggestedActionsSource : ISuggestedActionsSource
    {
        private readonly TestSuggestedActionsSourceProvider m_factory;
        private readonly ITextBuffer m_textBuffer;
        private readonly ITextView m_textView;

        public TestSuggestedActionsSource(TestSuggestedActionsSourceProvider testSuggestedActionsSourceProvider, ITextView textView, ITextBuffer textBuffer)
        {
            m_factory = testSuggestedActionsSourceProvider;
            m_textBuffer = textBuffer;
            m_textView = textView;
        }

        private object GetConfigAfterCaret(SnapshotSpan range)
        {
            ITextCaret caret = m_textView.Caret;
            SnapshotPoint point;

            if (caret.Position.BufferPosition > 0)
            {
                point = caret.Position.BufferPosition - 1;
            }
            else
            {
                return null;
            }

            if (point.GetChar() != '{')
            {
                return null;
            }

            var ss = point.Snapshot;
            for (int lineNumber = point.GetContainingLine().LineNumber; lineNumber < ss.LineCount;++lineNumber)
            {
                var line = ss.GetLineFromLineNumber(lineNumber);
                var lineText = line.GetText();
                for (int i = 0;i < lineText.Length;++i)
                {
                    if (lineText[i] == '}')
                    {
                        var resultSpan = new SnapshotSpan(point, line.Start + i + 1);
                        var str = resultSpan.GetText();
                        try
                        {
                            var document = (ITextDocument)m_textBuffer.Properties[typeof(ITextDocument)];
                            var folder = Path.GetDirectoryName(document.FilePath);

                            var obj = JObject.Parse(str);
                            if (obj.Contains("kbId") && obj.Contains("endpointKey"))
                            {
                                var result = new ConfigBase<QnAMakerService>(obj);
                                result.FilePath = $@"{folder}\Deployment\Resources\QnA\en\{obj["id"].ToString()}.lu";
                                return result;
                            }
                            else if (obj.Contains("appid"))
                            {
                                if (obj.Contains("type") && obj["type"].ToString() == "dispatch")
                                {
                                    var result = new ConfigBase<DispatchService>(obj);
                                    if (string.IsNullOrEmpty(result.Config.Version))
                                    {
                                        result.Config.Version = "Dispatch";
                                    }
                                    return result;
                                }
                                else if (obj.Contains("id"))
                                {
                                    var result = new ConfigBase<LuisService>(obj);
                                    result.FilePath = $@"{folder}\Deployment\Resources\LU\en\{obj["id"].ToString()}.lu";
                                    return result;
                                }
                                else
                                {
                                    return null;
                                }
                            }
                            else
                            {
                                return null;
                            }
                        }
                        catch(Exception e)
                        {
                            return null;
                        }
                    }
                }
            }

            return null;
        }

        private IEnumerable<SuggestedActionSet> CreateActionSet(ConfigBase<LuisService> service)
        {
            var result = new List<ISuggestedAction>();
            result.Add(new OpenWebPortal($"https://www.luis.ai/applications/{service.Config.AppId}/versions/{service.Config.Version}/dashboard"));
            result.Add(new OpenSourceFile(service.FilePath));
            return new SuggestedActionSet[] { new SuggestedActionSet("actions", result, "Luis") };
        }

        private IEnumerable<SuggestedActionSet> CreateActionSet(ConfigBase<DispatchService> service)
        {
            var result = new List<ISuggestedAction>();
            result.Add(new OpenWebPortal($"https://www.luis.ai/applications/{service.Config.AppId}/versions/{service.Config.Version}/dashboard"));
            result.Add(new OpenSourceFile(service.FilePath));
            return new SuggestedActionSet[] { new SuggestedActionSet("actions", result, "Dispatch") };
        }

        private IEnumerable<SuggestedActionSet> CreateActionSet(ConfigBase<QnAMakerService> service)
        {
            var result = new List<ISuggestedAction>();
            result.Add(new OpenWebPortal($"https://www.qnamaker.ai/Edit/KnowledgeBase?kbId={service.Config.KbId}"));
            result.Add(new OpenSourceFile(service.FilePath));
            return new SuggestedActionSet[] { new SuggestedActionSet("actions", result, "QnA") };
        }

        public Task<bool> HasSuggestedActionsAsync(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
            return Task.Factory.StartNew(() =>
            {
                return GetConfigAfterCaret(range) != null;
            });
        }

        public IEnumerable<SuggestedActionSet> GetSuggestedActions(ISuggestedActionCategorySet requestedActionCategories, SnapshotSpan range, CancellationToken cancellationToken)
        {
            var obj = GetConfigAfterCaret(range);
            if (obj != null)
            {
                if (obj is ConfigBase<DispatchService> dispatchService)
                {
                    return CreateActionSet(dispatchService);
                }
                else if (obj is ConfigBase<LuisService> luisService)
                {
                    return CreateActionSet(luisService);
                }
                else if(obj is ConfigBase<QnAMakerService> qnAMakerService)
                {
                    return CreateActionSet(qnAMakerService);
                }
                else
                {
                    return Enumerable.Empty<SuggestedActionSet>();
                }
            }
            return Enumerable.Empty<SuggestedActionSet>();
        }

        public event EventHandler<EventArgs> SuggestedActionsChanged;

        public void Dispose()
        {
        }

        public bool TryGetTelemetryId(out Guid telemetryId)
        {
            // This is a sample provider and doesn't participate in LightBulb telemetry
            telemetryId = Guid.Empty;
            return false;
        }
    }
}
