﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the Apache License, Version 2.0. See License.txt in the project root for license information.

using System;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.VisualStudio.LanguageServer.ContainedLanguage;
using Microsoft.VisualStudio.LanguageServer.Protocol;
using Microsoft.VisualStudio.Threading;
using Moq;
using Xunit;
using Range = Microsoft.VisualStudio.LanguageServer.Protocol.Range;

namespace Microsoft.VisualStudio.LanguageServerClient.Razor.HtmlCSharp
{
    public class HoverHandlerTest
    {
        public HoverHandlerTest()
        {
            Uri = new Uri("C:/path/to/file.razor");
        }

        private Uri Uri { get; }

        [Fact]
        public async Task HandleRequestAsync_DocumentNotFound_ReturnsNull()
        {
            // Arrange
            var documentManager = new TestDocumentManager();
            var requestInvoker = Mock.Of<LSPRequestInvoker>();
            var projectionProvider = Mock.Of<LSPProjectionProvider>();
            var documentMappingProvider = Mock.Of<LSPDocumentMappingProvider>();
            var hoverhandler = new HoverHandler(requestInvoker, documentManager, projectionProvider, documentMappingProvider);
            var hoverRequest = new TextDocumentPositionParams()
            {
                TextDocument = new TextDocumentIdentifier() { Uri = Uri },
                Position = new Position(0, 1)
            };

            // Act
            var result = await hoverhandler.HandleRequestAsync(hoverRequest, new ClientCapabilities(), CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task HandleRequestAsync_ProjectionNotFound_ReturnsNull()
        {
            // Arrange
            var documentManager = new TestDocumentManager();
            documentManager.AddDocument(Uri, Mock.Of<LSPDocumentSnapshot>());
            var requestInvoker = Mock.Of<LSPRequestInvoker>();
            var projectionProvider = Mock.Of<LSPProjectionProvider>();
            var documentMappingProvider = Mock.Of<LSPDocumentMappingProvider>();
            var hoverhandler = new HoverHandler(requestInvoker, documentManager, projectionProvider, documentMappingProvider);
            var hoverRequest = new TextDocumentPositionParams()
            {
                TextDocument = new TextDocumentIdentifier() { Uri = Uri },
                Position = new Position(0, 1)
            };

            // Act
            var result = await hoverhandler.HandleRequestAsync(hoverRequest, new ClientCapabilities(), CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.Null(result);
        }

        [Fact]
        public async Task HandleRequestAsync_CSharpProjection_InvokesCSharpLanguageServer()
        {
            // Arrange
            var called = false;

            var expectedContents = new SumType<SumType<string, MarkedString>, SumType<string, MarkedString>[], MarkupContent>(
                new MarkedString()
                {
                    Language = "markdown",
                    Value = "Hover Details"
                }
            );

            var lspResponse = new Hover()
            {
                Range = new Range()
                {
                    Start = new Position(10, 0),
                    End = new Position(10, 1)
                },
                Contents = expectedContents
            };

            var expectedItem = new Hover()
            {
                Range = new Range()
                {
                    Start = new Position(0, 0),
                    End = new Position(0, 1)
                },
                Contents = expectedContents
            };

            var hoverRequest = new TextDocumentPositionParams()
            {
                TextDocument = new TextDocumentIdentifier() { Uri = Uri },
                Position = new Position(0, 1)
            };

            var documentManager = new TestDocumentManager();
            documentManager.AddDocument(Uri, Mock.Of<LSPDocumentSnapshot>());

            var requestInvoker = new Mock<LSPRequestInvoker>();
            requestInvoker
                .Setup(r => r.ReinvokeRequestOnServerAsync<TextDocumentPositionParams, Hover>(It.IsAny<string>(), It.IsAny<LanguageServerKind>(), It.IsAny<TextDocumentPositionParams>(), It.IsAny<CancellationToken>()))
                .Callback<string, LanguageServerKind, TextDocumentPositionParams, CancellationToken>((method, serverKind, hoverParams, ct) =>
                {
                    Assert.Equal(Methods.TextDocumentHoverName, method);
                    Assert.Equal(LanguageServerKind.CSharp, serverKind);
                    called = true;
                })
                .Returns(Task.FromResult(lspResponse));

            var projectionResult = new ProjectionResult()
            {
                LanguageKind = RazorLanguageKind.CSharp,
            };
            var projectionProvider = new Mock<LSPProjectionProvider>();
            projectionProvider.Setup(p => p.GetProjectionAsync(It.IsAny<LSPDocumentSnapshot>(), It.IsAny<Position>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(projectionResult));

            var remappingResult = new RazorMapToDocumentRangesResponse()
            {
                Ranges = new[] {
                    new Range()
                    {
                        Start = new Position(0, 0),
                        End = new Position(0, 1)
                    }
                }
            };
            var documentMappingProvider = new Mock<LSPDocumentMappingProvider>();
            documentMappingProvider.Setup(d => d.MapToDocumentRangesAsync(RazorLanguageKind.CSharp, It.IsAny<Uri>(), It.IsAny<Range[]>(), It.IsAny<CancellationToken>())).
                Returns(Task.FromResult(remappingResult));

            var hoverHandler = new HoverHandler(requestInvoker.Object, documentManager, projectionProvider.Object, documentMappingProvider.Object);

            // Act
            var result = await hoverHandler.HandleRequestAsync(hoverRequest, new ClientCapabilities(), CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.True(called);
            Assert.Equal(expectedItem.Contents, result.Contents);
            Assert.Equal(expectedItem.Range, result.Range);
        }

        [Fact]
        public async Task HandleRequestAsync_RazorProjection_InvokesHtmlLanguageServer()
        {
            // Arrange
            var called = false;

            var expectedContents = new SumType<SumType<string, MarkedString>, SumType<string, MarkedString>[], MarkupContent>(
                new MarkedString()
                {
                    Language = "markdown",
                    Value = "HTML Hover Details"
                }
            );

            var lspResponse = new Hover()
            {
                Range = new Range()
                {
                    Start = new Position(10, 0),
                    End = new Position(10, 1)
                },
                Contents = expectedContents
            };

            var expectedItem = new Hover()
            {
                Range = new Range()
                {
                    Start = new Position(0, 0),
                    End = new Position(0, 1)
                },
                Contents = expectedContents
            };

            var hoverRequest = new TextDocumentPositionParams()
            {
                TextDocument = new TextDocumentIdentifier() { Uri = Uri },
                Position = new Position(0, 1)
            };

            var documentManager = new TestDocumentManager();
            documentManager.AddDocument(Uri, Mock.Of<LSPDocumentSnapshot>());

            var requestInvoker = new Mock<LSPRequestInvoker>();
            requestInvoker
                .Setup(r => r.ReinvokeRequestOnServerAsync<TextDocumentPositionParams, Hover>(It.IsAny<string>(), It.IsAny<LanguageServerKind>(), It.IsAny<TextDocumentPositionParams>(), It.IsAny<CancellationToken>()))
                .Callback<string, LanguageServerKind, TextDocumentPositionParams, CancellationToken>((method, serverKind, hoverParams, ct) =>
                {
                    Assert.Equal(Methods.TextDocumentHoverName, method);
                    Assert.Equal(LanguageServerKind.Html, serverKind);
                    called = true;
                })
                .Returns(Task.FromResult(lspResponse));

            var projectionResult = new ProjectionResult()
            {
                LanguageKind = RazorLanguageKind.Html,
            };
            var projectionProvider = new Mock<LSPProjectionProvider>();
            projectionProvider.Setup(p => p.GetProjectionAsync(It.IsAny<LSPDocumentSnapshot>(), It.IsAny<Position>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(projectionResult));

            var remappingResult = new RazorMapToDocumentRangesResponse()
            {
                Ranges = new[] {
                    new Range()
                    {
                        Start = new Position(0, 0),
                        End = new Position(0, 1)
                    }
                }
            };
            var documentMappingProvider = new Mock<LSPDocumentMappingProvider>();
            documentMappingProvider.Setup(d => d.MapToDocumentRangesAsync(RazorLanguageKind.Html, It.IsAny<Uri>(), It.IsAny<Range[]>(), It.IsAny<CancellationToken>())).
                Returns(Task.FromResult(remappingResult));

            var hoverHandler = new HoverHandler(requestInvoker.Object, documentManager, projectionProvider.Object, documentMappingProvider.Object);

            // Act
            var result = await hoverHandler.HandleRequestAsync(hoverRequest, new ClientCapabilities(), CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.True(called);
            Assert.Equal(expectedItem.Contents, result.Contents);
            Assert.Equal(expectedItem.Range, result.Range);
        }

        [Fact]
        public async Task HandleRequestAsync_CSharpProjection_InvokesCSharpLanguageServerWithNoResult()
        {
            // Arrange
            var called = false;
            var hoverRequest = new TextDocumentPositionParams()
            {
                TextDocument = new TextDocumentIdentifier() { Uri = Uri },
                Position = new Position(0, 1)
            };

            var documentManager = new TestDocumentManager();
            documentManager.AddDocument(Uri, Mock.Of<LSPDocumentSnapshot>());

            var requestInvoker = new Mock<LSPRequestInvoker>();
            requestInvoker
                .Setup(r => r.ReinvokeRequestOnServerAsync<TextDocumentPositionParams, Hover>(It.IsAny<string>(), It.IsAny<LanguageServerKind>(), It.IsAny<TextDocumentPositionParams>(), It.IsAny<CancellationToken>()))
                .Callback<string, LanguageServerKind, TextDocumentPositionParams, CancellationToken>((method, serverKind, hoverParams, ct) =>
                {
                    Assert.Equal(Methods.TextDocumentHoverName, method);
                    Assert.Equal(LanguageServerKind.CSharp, serverKind);
                    called = true;
                })
                .Returns(Task.FromResult<Hover>(null));

            var projectionResult = new ProjectionResult()
            {
                LanguageKind = RazorLanguageKind.CSharp,
            };
            var projectionProvider = new Mock<LSPProjectionProvider>();
            projectionProvider.Setup(p => p.GetProjectionAsync(It.IsAny<LSPDocumentSnapshot>(), It.IsAny<Position>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(projectionResult));

            var documentMappingProvider = new Mock<LSPDocumentMappingProvider>();

            var hoverHandler = new HoverHandler(requestInvoker.Object, documentManager, projectionProvider.Object, documentMappingProvider.Object);

            // Act
            var result = await hoverHandler.HandleRequestAsync(hoverRequest, new ClientCapabilities(), CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.True(called);
            Assert.Null(result);
        }

        [Fact]
        public async Task HandleRequestAsync_CSharpProjection_InvokesCSharpLanguageServer_FailsRemappingResultReturnsHoverWithInitialPosition()
        {
            // Arrange
            var called = false;

            var expectedContents = new SumType<SumType<string, MarkedString>, SumType<string, MarkedString>[], MarkupContent>(
                new MarkedString()
                {
                    Language = "markdown",
                    Value = "Hover Details"
                }
            );

            var lspResponse = new Hover()
            {
                Range = new Range()
                {
                    Start = new Position(10, 0),
                    End = new Position(10, 1)
                },
                Contents = expectedContents
            };

            var expectedItem = new Hover()
            {
                Range = new Range()
                {
                    Start = new Position(0, 1),
                    End = new Position(0, 1)
                },
                Contents = expectedContents
            };

            var hoverRequest = new TextDocumentPositionParams()
            {
                TextDocument = new TextDocumentIdentifier() { Uri = Uri },
                Position = new Position(0, 1)
            };

            var documentManager = new TestDocumentManager();
            documentManager.AddDocument(Uri, Mock.Of<LSPDocumentSnapshot>());

            var requestInvoker = new Mock<LSPRequestInvoker>();
            requestInvoker
                .Setup(r => r.ReinvokeRequestOnServerAsync<TextDocumentPositionParams, Hover>(It.IsAny<string>(), It.IsAny<LanguageServerKind>(), It.IsAny<TextDocumentPositionParams>(), It.IsAny<CancellationToken>()))
                .Callback<string, LanguageServerKind, TextDocumentPositionParams, CancellationToken>((method, serverKind, hoverParams, ct) =>
                {
                    Assert.Equal(Methods.TextDocumentHoverName, method);
                    Assert.Equal(LanguageServerKind.CSharp, serverKind);
                    called = true;
                })
                .Returns(Task.FromResult(lspResponse));

            var projectionResult = new ProjectionResult()
            {
                LanguageKind = RazorLanguageKind.CSharp,
            };
            var projectionProvider = new Mock<LSPProjectionProvider>();
            projectionProvider.Setup(p => p.GetProjectionAsync(It.IsAny<LSPDocumentSnapshot>(), It.IsAny<Position>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(projectionResult));

            var documentMappingProvider = new Mock<LSPDocumentMappingProvider>();
            documentMappingProvider.Setup(d => d.MapToDocumentRangesAsync(RazorLanguageKind.CSharp, It.IsAny<Uri>(), It.IsAny<Range[]>(), It.IsAny<CancellationToken>())).
                Returns(Task.FromResult<RazorMapToDocumentRangesResponse>(null));

            var hoverHandler = new HoverHandler(requestInvoker.Object, documentManager, projectionProvider.Object, documentMappingProvider.Object);

            // Act
            var result = await hoverHandler.HandleRequestAsync(hoverRequest, new ClientCapabilities(), CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.True(called);
            Assert.Equal(expectedItem.Contents, result.Contents);
            Assert.Equal(expectedItem.Range, result.Range);
        }

        [Fact]
        public async Task HandleRequestAsync_CSharpProjection_InvokesCSharpLanguageServer_FailsRemappingResultRangeWithHostVersionChanged()
        {
            // Arrange
            var called = false;

            var expectedContents = new SumType<SumType<string, MarkedString>, SumType<string, MarkedString>[], MarkupContent>(
                new MarkedString()
                {
                    Language = "markdown",
                    Value = "Hover Details"
                }
            );

            var lspResponse = new Hover()
            {
                Range = new Range()
                {
                    Start = new Position(10, 0),
                    End = new Position(10, 1)
                },
                Contents = expectedContents
            };

            var hoverRequest = new TextDocumentPositionParams()
            {
                TextDocument = new TextDocumentIdentifier() { Uri = Uri },
                Position = new Position(0, 1)
            };

            var documentManager = new TestDocumentManager();
            documentManager.AddDocument(Uri, Mock.Of<LSPDocumentSnapshot>());

            var requestInvoker = new Mock<LSPRequestInvoker>();
            requestInvoker
                .Setup(r => r.ReinvokeRequestOnServerAsync<TextDocumentPositionParams, Hover>(It.IsAny<string>(), It.IsAny<LanguageServerKind>(), It.IsAny<TextDocumentPositionParams>(), It.IsAny<CancellationToken>()))
                .Callback<string, LanguageServerKind, TextDocumentPositionParams, CancellationToken>((method, serverKind, hoverParams, ct) =>
                {
                    Assert.Equal(Methods.TextDocumentHoverName, method);
                    Assert.Equal(LanguageServerKind.CSharp, serverKind);
                    called = true;
                })
                .Returns(Task.FromResult(lspResponse));

            var projectionResult = new ProjectionResult()
            {
                LanguageKind = RazorLanguageKind.CSharp,
            };
            var projectionProvider = new Mock<LSPProjectionProvider>();
            projectionProvider.Setup(p => p.GetProjectionAsync(It.IsAny<LSPDocumentSnapshot>(), It.IsAny<Position>(), It.IsAny<CancellationToken>())).Returns(Task.FromResult(projectionResult));

            var remappingResult = new RazorMapToDocumentRangesResponse()
            {
                Ranges = new[] { new Range() },
                HostDocumentVersion = 1
            };
            var documentMappingProvider = new Mock<LSPDocumentMappingProvider>();
            documentMappingProvider.Setup(d => d.MapToDocumentRangesAsync(RazorLanguageKind.CSharp, It.IsAny<Uri>(), It.IsAny<Range[]>(), It.IsAny<CancellationToken>())).
                Returns(Task.FromResult(remappingResult));

            var hoverHandler = new HoverHandler(requestInvoker.Object, documentManager, projectionProvider.Object, documentMappingProvider.Object);

            // Act
            var result = await hoverHandler.HandleRequestAsync(hoverRequest, new ClientCapabilities(), CancellationToken.None).ConfigureAwait(false);

            // Assert
            Assert.True(called);
            Assert.Null(result);
        }
    }
}
