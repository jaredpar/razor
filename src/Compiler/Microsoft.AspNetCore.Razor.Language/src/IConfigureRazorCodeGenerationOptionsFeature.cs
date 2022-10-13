﻿// Licensed to the .NET Foundation under one or more agreements.
// The .NET Foundation licenses this file to you under the MIT license.

#nullable disable

namespace Microsoft.AspNetCore.Razor.Language;

public interface IConfigureRazorCodeGenerationOptionsFeature : IRazorEngineFeature
{
    int Order { get; }

    void Configure(RazorCodeGenerationOptionsBuilder options);
}
