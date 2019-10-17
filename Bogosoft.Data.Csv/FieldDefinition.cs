using System;

namespace Bogosoft.Data.Csv
{
    /// <summary>
    /// Represents meta data for a field.
    /// </summary>
    public sealed class FieldDefinition
    {
        /// <summary>
        /// Get the name of the current field.
        /// </summary>
        public readonly string Name;

        /// <summary>
        /// Get the parser that the current field uses to parse its value.
        /// </summary>
        public readonly Func<string, object> Parser;

        /// <summary>
        /// Get the type of the values in the current field.
        /// </summary>
        public readonly Type Type;

        /// <summary>
        /// Create a new instance of the <see cref="FieldDefinition"/> class.
        /// </summary>
        /// <param name="name">A value corresponding to the name of the new field.</param>
        /// <param name="type">The data type of the values in the new field.</param>
        /// <param name="parser">
        /// A strategy with which the new field will parse its values.
        /// </param>
        public FieldDefinition(string name, Type type, Func<string, object> parser)
        {
            Name = name;
            Parser = parser;
            Type = type;
        }
    }
}