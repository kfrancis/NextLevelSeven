﻿using System.Collections.Generic;

namespace NextLevelSeven.Parsing.Dividers
{
    /// <summary>A splitter which handles getting and setting delimited substrings within a parent string divider.</summary>
    internal sealed class DescendantStringDivider : StringDivider
    {
        /// <summary>[PERF] Cached divisions list.</summary>
        private IReadOnlyList<StringDivision> _divisions;

        /// <summary>Create a subdivider for the specified string divider.</summary>
        /// <param name="baseDivider">Divider to reference.</param>
        /// <param name="delimiter">Delimiter to search for.</param>
        /// <param name="parentIndex">Index within the parent to reference.</param>
        public DescendantStringDivider(StringDivider baseDivider, char delimiter, int parentIndex)
        {
            BaseDivider = baseDivider;
            Index = parentIndex;
            Delimiter = delimiter;
        }

        /// <summary>Parent divider.</summary>
        private StringDivider BaseDivider { get; set; }

        /// <summary>Get or set the substring at the specified index.</summary>
        /// <param name="index">Index of the string to get or set.</param>
        /// <returns>Substring.</returns>
        public override string this[int index]
        {
            get
            {
                if (IsNull)
                {
                    return null;
                }

                var split = Divisions[index];
                return split.Length == 0 ? null : new string(BaseValue, split.Offset, split.Length);
            }
            set { SetValue(index, value); }
        }

        public override bool IsNull
        {
            get { return BaseDivider.IsNull || ValueChars == null || ValueChars.Length == 0; }
        }

        /// <summary>String that is operated upon, as a character array. This points to the parent divider's BaseValue.</summary>
        public override char[] BaseValue
        {
            get { return BaseDivider.BaseValue; }
        }

        /// <summary>Get the number of divisions.</summary>
        public override int Count
        {
            get { return Divisions.Count; }
        }

        /// <summary>Get the division offsets in the string.</summary>
        public override IReadOnlyList<StringDivision> Divisions
        {
            get
            {
                if (_divisions == null || Version != BaseDivider.Version)
                {
                    Update();
                }
                return _divisions;
            }
        }

        /// <summary>Calculated value of all divisions separated by delimiters.</summary>
        public override string Value
        {
            get
            {
                var d = BaseDivider.GetSubDivision(Index);
                return (!d.Valid || d.Length == 0) ? null : new string(BaseValue, d.Offset, d.Length);
            }
            set { BaseDivider[Index] = value; }
        }

        /// <summary>Calculated value of all divisions separated by delimiters, as characters.</summary>
        public override char[] ValueChars
        {
            get
            {
                var d = BaseDivider.GetSubDivision(Index);
                return (!d.Valid || d.Length == 0)
                    ? null
                    : StringDividerOperations.CharSubstring(BaseValue, d.Offset, d.Length);
            }
            set { BaseDivider[Index] = new string(value); }
        }

        /// <summary>Get or set the subdivided values.</summary>
        public override IEnumerable<string> Values
        {
            get
            {
                var count = Divisions.Count;
                for (var i = 0; i < count; i++)
                {
                    yield return this[i];
                }
            }
            set { Value = (Delimiter == '\0') ? string.Concat(value) : string.Join(new string(Delimiter, 1), value); }
        }

        /// <summary>Set the value at the specified subdivision index.</summary>
        /// <param name="index">Index to set.</param>
        /// <param name="value">New value.</param>
        private void SetValue(int index, string value)
        {
            List<StringDivision> divisions;
            var paddedString = StringDividerOperations.GetPaddedString(ValueChars, index, Delimiter, out divisions);
            var d = divisions[index];
            ValueChars = StringDividerOperations.GetSplicedString(paddedString, d.Offset, d.Length,
                StringDividerOperations.GetChars(value));
            _divisions = null;
        }

        /// <summary>[PERF] Refresh internal division cache.</summary>
        private void Update()
        {
            Version = BaseDivider.Version;
            var baseDivision = BaseDivider.GetSubDivision(Index);
            if (baseDivision.Valid)
            {
                _divisions = StringDividerOperations.GetDivisions(BaseValue, Delimiter,
                    BaseDivider.GetSubDivision(Index));
            }
            else
            {
                _divisions = new List<StringDivision>();
            }
        }
    }
}