// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

namespace Deveel.Filters {
	/// <summary>
	/// Options that control how a <see cref="FilterExpression"/> is converted
	/// to a <see cref="FilterModel"/>.
	/// </summary>
    public sealed class FilterBuilderOptions {
		/// <summary>
		/// Gets or sets a value indicating whether binary filter models should prefer
		/// the compact key-value data representation over explicit left/right operands.
		/// The default is <c>true</c>.
		/// </summary>
        public bool PreferBinaryData { get; set; } = true;
    }
}
