﻿using System.Collections.Generic;

namespace NextLevelSeven.Core
{
    public interface IRepetition
    {
        /// <summary>
        ///     Get data from a specific place in the field repetition. Depth is determined by how many indices are specified.
        /// </summary>
        /// <param name="component">Component index.</param>
        /// <param name="subcomponent">Subcomponent index.</param>
        /// <returns>The the specified element.</returns>
        string GetValue(int component = -1, int subcomponent = -1);

        /// <summary>
        ///     Get data from a specific place in the field repetition. Depth is determined by how many indices are specified.
        /// </summary>
        /// <param name="component">Component index.</param>
        /// <param name="subcomponent">Subcomponent index.</param>
        /// <returns>The occurrences of the specified element.</returns>
        IEnumerable<string> GetValues(int component = -1, int subcomponent = -1);
    }
}