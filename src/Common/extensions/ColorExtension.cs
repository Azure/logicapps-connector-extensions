﻿// Copyright (c) .NET Foundation. All rights reserved.
// Licensed under the MIT License. See License.txt in the project root for license information.

namespace Microsoft.Azure.Workflows.ServiceProvider.Extensions.Common
{
    using System;
    using System.Drawing;

    /// <summary>
    /// Color extension.
    /// </summary>
    public static class ColorExtension
    {
        /// <summary>
        /// To hex color.
        /// </summary>
        /// <param name="color">Color</param>
        /// <returns>unit.</returns>
        public static uint ToHexColor(this Color color)
        {
            return (uint)((color.A << 24) | (color.R << 16) |
                   (color.G << 8) | (color.B << 0));
        }
    }
}
