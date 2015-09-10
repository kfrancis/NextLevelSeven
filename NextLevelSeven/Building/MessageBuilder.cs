﻿using System.Collections.Generic;
using System.Linq;
using NextLevelSeven.Native;
using NextLevelSeven.Native.Elements;
using NextLevelSeven.Utility;

namespace NextLevelSeven.Building
{
    /// <summary>
    ///     Represents an HL7 message as discrete parts, which can be quickly modified and exported.
    /// </summary>
    internal sealed class MessageBuilder : BuilderBase, IMessageBuilder
    {
        /// <summary>
        ///     Descendant segments.
        /// </summary>
        private readonly Dictionary<int, SegmentBuilder> _segmentBuilders = new Dictionary<int, SegmentBuilder>();

        /// <summary>
        ///     Create a message builder with default MSH segment containing only encoding characters.
        /// </summary>
        public MessageBuilder()
        {
            ComponentDelimiter = '^';
            EscapeDelimiter = '\\';
            RepetitionDelimiter = '~';
            SubcomponentDelimiter = '&';
            FieldDelimiter = '|';
            Fields(1, "MSH", new string(FieldDelimiter, 1),
                new string(new[] {ComponentDelimiter, RepetitionDelimiter, EscapeDelimiter, SubcomponentDelimiter}));
        }

        /// <summary>
        ///     Create a message builder initialized with the specified message content.
        /// </summary>
        /// <param name="baseMessage">Content to initialize with.</param>
        public MessageBuilder(string baseMessage)
        {
            Message(baseMessage);
        }

        /// <summary>
        ///     Create a message builder initialized with the copied content of the specified message.
        /// </summary>
        /// <param name="message">Message to copy content from.</param>
        public MessageBuilder(INativeMessage message)
        {
            Message(message.Value);
        }

        /// <summary>
        ///     Get a descendant segment builder.
        /// </summary>
        /// <param name="index">Index within the message to get the builder from.</param>
        /// <returns>Segment builder for the specified index.</returns>
        public ISegmentBuilder this[int index]
        {
            get
            {
                if (!_segmentBuilders.ContainsKey(index))
                {
                    _segmentBuilders[index] = new SegmentBuilder(this, index);
                }
                return _segmentBuilders[index];
            }
        }

        /// <summary>
        ///     Get the number of segments in the message.
        /// </summary>
        public int Count
        {
            get { return _segmentBuilders.Max(kv => kv.Key); }
        }

        /// <summary>
        ///     Get or set segment content within this message.
        /// </summary>
        public IEnumerable<string> Values
        {
            get
            {
                return new WrapperEnumerable<string>(index => this[index].Value,
                    (index, data) => Segment(index, data),
                    () => Count,
                    1);
            }
            set { Segments(value.ToArray()); }
        }

        /// <summary>
        ///     Get or set the message string.
        /// </summary>
        public string Value
        {
            get
            {
                var result = string.Join("\xD",
                    _segmentBuilders.OrderBy(i => i.Key).Select(i => i.Value.Value));
                return result;
            }
            set { Message(value); }
        }

        /// <summary>
        ///     Set a component's content.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="componentIndex">Component index.</param>
        /// <param name="value">New value.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Component(int segmentIndex, int fieldIndex, int repetition, int componentIndex,
            string value)
        {
            this[segmentIndex].Component(fieldIndex, repetition, componentIndex, value);
            return this;
        }

        /// <summary>
        ///     Replace all component values within a field repetition.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="components">Values to replace with.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Components(int segmentIndex, int fieldIndex, int repetition, params string[] components)
        {
            this[segmentIndex].Components(fieldIndex, repetition, components);
            return this;
        }

        /// <summary>
        ///     Set a sequence of components within a field repetition, beginning at the specified start index.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="startIndex">Component index to begin replacing at.</param>
        /// <param name="components">Values to replace with.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Components(int segmentIndex, int fieldIndex, int repetition, int startIndex,
            params string[] components)
        {
            this[segmentIndex].Components(fieldIndex, repetition, startIndex, components);
            return this;
        }

        /// <summary>
        ///     Set a field's content.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="value">New value.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Field(int segmentIndex, int fieldIndex, string value)
        {
            this[segmentIndex].Field(fieldIndex, value);
            return this;
        }

        /// <summary>
        ///     Replace all field values within a segment.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fields">Values to replace with.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Fields(int segmentIndex, params string[] fields)
        {
            this[segmentIndex].Fields(fields);
            return this;
        }

        /// <summary>
        ///     Set a sequence of fields within a segment, beginning at the specified start index.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="startIndex">Field index to begin replacing at.</param>
        /// <param name="fields">Values to replace with.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Fields(int segmentIndex, int startIndex, params string[] fields)
        {
            this[segmentIndex].Fields(startIndex, fields);
            return this;
        }

        /// <summary>
        ///     Set a field repetition's content.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="value">New value.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder FieldRepetition(int segmentIndex, int fieldIndex, int repetition, string value)
        {
            this[segmentIndex].FieldRepetition(fieldIndex, repetition, value);
            return this;
        }

        /// <summary>
        ///     Replace all field repetitions within a field.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="repetitions">Values to replace with.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder FieldRepetitions(int segmentIndex, int fieldIndex, params string[] repetitions)
        {
            this[segmentIndex].FieldRepetitions(fieldIndex, repetitions);
            return this;
        }

        /// <summary>
        ///     Set a sequence of field repetitions within a field, beginning at the specified start index.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="startIndex">Field repetition index to begin replacing at.</param>
        /// <param name="repetitions">Values to replace with.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder FieldRepetitions(int segmentIndex, int fieldIndex, int startIndex,
            params string[] repetitions)
        {
            this[segmentIndex].FieldRepetitions(fieldIndex, startIndex, repetitions);
            return this;
        }

        /// <summary>
        ///     Set this message's content.
        /// </summary>
        /// <param name="value">New value.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Message(string value)
        {
            value = value ?? string.Empty;

            var length = value.Length;
            ComponentDelimiter = (length >= 5) ? value[5] : '^';
            EscapeDelimiter = (length >= 6) ? value[6] : '\\';
            FieldDelimiter = (length >= 3) ? value[3] : '|';
            RepetitionDelimiter = (length >= 7) ? value[7] : '~';
            SubcomponentDelimiter = (length >= 8) ? value[8] : '&';

            _segmentBuilders.Clear();
            value = value.Replace("\r\n", "\xD");
            var index = 1;

            foreach (var segment in value.Split('\xD'))
            {
                Segment(index++, segment);
            }

            return this;
        }

        /// <summary>
        ///     Set a segment's content.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="value">New value.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Segment(int segmentIndex, string value)
        {
            this[segmentIndex].Segment(value);
            return this;
        }

        /// <summary>
        ///     Replace all segments within this message.
        /// </summary>
        /// <param name="segments">Values to replace with.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Segments(params string[] segments)
        {
            Message(string.Join("\xD", segments));
            return this;
        }

        /// <summary>
        ///     Set a sequence of segments within this message, beginning at the specified start index.
        /// </summary>
        /// <param name="startIndex">Segment index to begin replacing at.</param>
        /// <param name="segments">Values to replace with.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Segments(int startIndex, params string[] segments)
        {
            var index = startIndex;
            foreach (var segment in segments)
            {
                Segment(index++, segment);
            }
            return this;
        }

        /// <summary>
        ///     Set a subcomponent's content.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="componentIndex">Component index.</param>
        /// <param name="subcomponentIndex">Subcomponent index.</param>
        /// <param name="value">New value.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Subcomponent(int segmentIndex, int fieldIndex, int repetition, int componentIndex,
            int subcomponentIndex,
            string value)
        {
            this[segmentIndex].Subcomponent(fieldIndex, repetition, componentIndex, subcomponentIndex, value);
            return this;
        }

        /// <summary>
        ///     Replace all subcomponents within a component.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="componentIndex">Component index.</param>
        /// <param name="subcomponents">Subcomponent index.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Subcomponents(int segmentIndex, int fieldIndex, int repetition, int componentIndex,
            params string[] subcomponents)
        {
            this[segmentIndex].Subcomponents(fieldIndex, repetition, componentIndex, subcomponents);
            return this;
        }

        /// <summary>
        ///     Set a sequence of subcomponents within a component, beginning at the specified start index.
        /// </summary>
        /// <param name="segmentIndex">Segment index.</param>
        /// <param name="fieldIndex">Field index.</param>
        /// <param name="repetition">Field repetition index.</param>
        /// <param name="componentIndex">Component index.</param>
        /// <param name="startIndex">Subcomponent index to begin replacing at.</param>
        /// <param name="subcomponents">Values to replace with.</param>
        /// <returns>This MessageBuilder, for chaining purposes.</returns>
        public IMessageBuilder Subcomponents(int segmentIndex, int fieldIndex, int repetition, int componentIndex,
            int startIndex, params string[] subcomponents)
        {
            this[segmentIndex].Subcomponents(fieldIndex, repetition, componentIndex, startIndex, subcomponents);
            return this;
        }

        /// <summary>
        ///     Copy the contents of this builder to an HL7 message.
        /// </summary>
        /// <returns>Converted message.</returns>
        public INativeMessage ToMessage()
        {
            return new NativeMessage(Value);
        }

        /// <summary>
        ///     Get the values at the specific location in the message.
        /// </summary>
        /// <param name="segment">Segment index.</param>
        /// <param name="field">Field index.</param>
        /// <param name="repetition">Repetition index.</param>
        /// <param name="component">Component index.</param>
        /// <param name="subcomponent">Subcomponent index.</param>
        /// <returns>Value at the specified location. Returns null if not found.</returns>
        public IEnumerable<string> GetValues(int segment = -1, int field = -1, int repetition = -1, int component = -1,
            int subcomponent = -1)
        {
            if (segment < 0)
            {
                return Values;
            }
            if (field < 0)
            {
                return this[segment].Values;
            }
            if (repetition < 0)
            {
                return this[segment][field].Values;
            }
            if (component < 0)
            {
                return this[segment][field][repetition].Values;
            }
            return (subcomponent < 0)
                ? this[segment][field][repetition][component].Values
                : this[segment][field][repetition][component][subcomponent].Value.Yield();
        }

        /// <summary>
        ///     Get the value at the specific location in the message.
        /// </summary>
        /// <param name="segment">Segment index.</param>
        /// <param name="field">Field index.</param>
        /// <param name="repetition">Repetition index.</param>
        /// <param name="component">Component index.</param>
        /// <param name="subcomponent">Subcomponent index.</param>
        /// <returns>Value at the specified location. Returns null if not found.</returns>
        public string GetValue(int segment = -1, int field = -1, int repetition = -1, int component = -1,
            int subcomponent = -1)
        {
            if (segment < 0)
            {
                return Value;
            }
            if (field < 0)
            {
                return this[segment].Value;
            }
            if (repetition < 0)
            {
                return this[segment][field].Value;
            }
            if (component < 0)
            {
                return this[segment][field][repetition].Value;
            }
            return (subcomponent < 0)
                ? this[segment][field][repetition][component].Value
                : this[segment][field][repetition][component][subcomponent].Value;
        }

        /// <summary>
        ///     Copy the contents of this builder to a string.
        /// </summary>
        /// <returns>Converted message.</returns>
        public override string ToString()
        {
            return Value;
        }
    }
}