// Copyright 2023-2026 Antonello Provenzano
// 
// Licensed under the MIT License. See LICENSE file in the project root for full license information.

using MongoDB.Bson;

namespace Deveel.Filters {
	/// <summary>
	/// Provides extension methods for converting <see cref="FilterExpression"/> instances
	/// to MongoDB <see cref="BsonDocument"/> representations.
	/// </summary>
	public static class FilterExtensions {
		/// <summary>
		/// Converts the specified <see cref="FilterExpression"/> to a <see cref="BsonDocument"/>.
		/// </summary>
		/// <param name="filter">The filter expression to convert.</param>
		/// <returns>A <see cref="BsonDocument"/> representing the filter.</returns>
		/// <exception cref="FilterException">
		/// Thrown when the filter cannot be converted to a valid BSON document.
		/// </exception>
		public static BsonDocument AsBsonDocument(this FilterExpression filter) {
			var visitor = new BsonFilterVisitor();
			return visitor.BuildDocument(filter);
		}
	}
}
