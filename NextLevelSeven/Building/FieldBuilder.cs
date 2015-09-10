﻿using System.Collections.Generic;
using System.Linq;
using System.Text;
using NextLevelSeven.Utility;

namespace NextLevelSeven.Building
{
    /// <summary>
    ///     Represents an HL7 field.
    /// </summary>
    internal class FieldBuilder : BuilderBaseDescendant, IFieldBuilder
    {
        /// <summary>
        ///     Descendant builders.
        /// </summary>
        private readonly Dictionary<int, RepetitionBuilder> _repetitionBuilders =
            new Dictionary<int, RepetitionBuilder>();

        /// <summary>
        ///     Create a field builder with the specified encoding configuration.
        /// </summary>
        /// <param name="builder">Ancestor builder.</param>
        /// <param name="index">Index in the ancestor.</param>
        internal FieldBuilder(BuilderBase builder, int index)
            : base(builder, index)
        {
        }

        /// <summary>
        ///     Get a descendant field repetition builder.
        /// </summary>
        /// <param name="index">Index within the field to get the builder from.</param>
        /// <returns>Field repetition builder for the specified index.</returns>
        public virtual IRepetitionBuilder this[int index]
        {
            get
            {
                if (!_repetitionBuilders.ContainsKey(index))
                {
                    _repetitionBuilders[index] = new RepetitionBuilder(this, index);
                }
                return _repetitionBuilders[index];
            }
        }

        /// <summary>
        ///     Get the number of field repetitions in this field, including field repetitions with no content.
        /// </summary>
        public virtual int Count
        {
            get { return (_repetitionBuilders.Count > 0) ? _repetitionBuilders.Max(kv => kv.Key) : 0; }
        }

        /// <summary>
        ///     Get or set field repetition content within this field.
        /// </summary>
        public virtual IEnumerable<string> Values
        {
            get
            {
                return new WrapperEnumerable<string>(index => this[index].Value,
                    (index, data) => FieldRepetition(index, data),
                    () => Count,
                    1);
            }
            set { FieldRepetitions(value.ToArray()); }
        }

        /// <summary>
        ///     Get or set the field string.
        /// </summary>
        public virtual string Value
        {
            get
            {
                var index = 1;
                var result = new StringBuilder();

                foreach (var repetition in _repetitionBuilders.OrderBy(i => i.Key))
                {
                    while (index < repetition.Key)
                    {
                        result.Append(EncodingConfiguration.RepetitionDelimiter);
                        index++;
                    }

                    if (repetition.Key > 0)
                    {
                        result.Append(repetition.Value);
                    }
                }

                return result.ToString();
            }
            set { Field(value); }
        }

        /// <summary>
        ///     Set a component's content.
        /// </summary>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="componentIndex">Component index.</param>
        /// <param name="value">New value.</param>
        /// <returns>This FieldBuilder, for chaining purposes.</returns>
        public IFieldBuilder Component(int repetition, int componentIndex, string value)
        {
            this[repetition].Component(componentIndex, value);
            return this;
        }

        /// <summary>
        ///     Replace all component values within a field repetition.
        /// </summary>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="components">Values to replace with.</param>
        /// <returns>This FieldBuilder, for chaining purposes.</returns>
        public IFieldBuilder Components(int repetition, params string[] components)
        {
            this[repetition].Components(components);
            return this;
        }

        /// <summary>
        ///     Set a sequence of components within a field repetition, beginning at the specified start index.
        /// </summary>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="startIndex">Component index to begin replacing at.</param>
        /// <param name="components">Values to replace with.</param>
        /// <returns>This FieldBuilder, for chaining purposes.</returns>
        public IFieldBuilder Components(int repetition, int startIndex, params string[] components)
        {
            this[repetition].Components(startIndex, components);
            return this;
        }

        /// <summary>
        ///     Set this field's content.
        /// </summary>
        /// <param name="value">New value.</param>
        /// <returns>This FieldBuilder, for chaining purposes.</returns>
        public virtual IFieldBuilder Field(string value)
        {
            _repetitionBuilders.Clear();
            var index = 1;

            value = value ?? string.Empty;
            foreach (var repetition in value.Split(EncodingConfiguration.RepetitionDelimiter))
            {
                FieldRepetition(index++, repetition);
            }

            return this;
        }

        /// <summary>
        ///     Set a field repetition's content.
        /// </summary>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="value">New value.</param>
        /// <returns>This FieldBuilder, for chaining purposes.</returns>
        public virtual IFieldBuilder FieldRepetition(int repetition, string value)
        {
            if (repetition < 1)
            {
                _repetitionBuilders.Clear();
            }
            else if (_repetitionBuilders.ContainsKey(repetition))
            {
                _repetitionBuilders.Remove(repetition);
            }
            this[repetition].FieldRepetition(value);
            return this;
        }

        /// <summary>
        ///     Replace all field repetitions within this field.
        /// </summary>
        /// <param name="repetitions">Values to replace with.</param>
        /// <returns>This FieldBuilder, for chaining purposes.</returns>
        public IFieldBuilder FieldRepetitions(params string[] repetitions)
        {
            _repetitionBuilders.Clear();
            var index = 1;
            foreach (var repetition in repetitions)
            {
                FieldRepetition(index++, repetition);
            }
            return this;
        }

        /// <summary>
        ///     Set a sequence of field repetitions within this field, beginning at the specified start index.
        /// </summary>
        /// <param name="startIndex">Field repetition index to begin replacing at.</param>
        /// <param name="repetitions">Values to replace with.</param>
        /// <returns>This FieldBuilder, for chaining purposes.</returns>
        public IFieldBuilder FieldRepetitions(int startIndex, params string[] repetitions)
        {
            var index = startIndex;
            foreach (var repetition in repetitions)
            {
                FieldRepetition(index++, repetition);
            }
            return this;
        }

        /// <summary>
        ///     Set a subcomponent's content.
        /// </summary>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="componentIndex">Component index.</param>
        /// <param name="subcomponentIndex">Subcomponent index.</param>
        /// <param name="value">New value.</param>
        /// <returns>This FieldBuilder, for chaining purposes.</returns>
        public IFieldBuilder Subcomponent(int repetition, int componentIndex, int subcomponentIndex, string value)
        {
            this[repetition].Subcomponent(componentIndex, subcomponentIndex, value);
            return this;
        }

        /// <summary>
        ///     Replace all subcomponents within a component.
        /// </summary>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="componentIndex">Component index.</param>
        /// <param name="subcomponents">Subcomponent index.</param>
        /// <returns>This FieldBuilder, for chaining purposes.</returns>
        public IFieldBuilder Subcomponents(int repetition, int componentIndex, params string[] subcomponents)
        {
            this[repetition].Subcomponents(componentIndex, subcomponents);
            return this;
        }

        /// <summary>
        ///     Set a sequence of subcomponents within a component, beginning at the specified start index.
        /// </summary>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="componentIndex">Component index.</param>
        /// <param name="startIndex">Subcomponent index to begin replacing at.</param>
        /// <param name="subcomponents">Values to replace with.</param>
        /// <returns>This FieldBuilder, for chaining purposes.</returns>
        public IFieldBuilder Subcomponents(int repetition, int componentIndex, int startIndex,
            params string[] subcomponents)
        {
            this[repetition].Subcomponents(componentIndex, startIndex, subcomponents);
            return this;
        }

        /// <summary>
        ///     Copy the contents of this builder to a string.
        /// </summary>
        /// <returns>Converted field.</returns>
        public override string ToString()
        {
            return Value;
        }

        /// <summary>
        ///     Get the value at the specified indices.
        /// </summary>
        /// <param name="repetition">Repetition number.</param>
        /// <param name="component">Component index.</param>
        /// <param name="subcomponent">Subcomponent index.</param>
        /// <returns></returns>
        public string GetValue(int repetition = -1, int component = -1, int subcomponent = -1)
        {
            return repetition < 0
                ? Value
                : this[repetition].GetValue(component, subcomponent);
        }

        /// <summary>
        ///     Get the values at the specified indices.
        /// </summary>
        /// <param name="repetition">Repetition number.</param>
        /// <param name="component">Component index.</param>
        /// <param name="subcomponent">Subcomponent index.</param>
        /// <returns></returns>
        public IEnumerable<string> GetValues(int repetition = -1, int component = -1, int subcomponent = -1)
        {
            return repetition < 0
                ? Values
                : this[repetition].GetValues(component, subcomponent);
        }
    }
}