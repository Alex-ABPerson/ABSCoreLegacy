using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSoftware.Core.Locale
{
    /// <summary>
    /// Contains a specific piece of text in the locale.
    /// </summary>
    public struct LocaleText
    {

        /// <summary>
        /// The id this locale is given.
        /// </summary>
        public string Id;

        /// <summary>
        /// The text this locale currently has loaded.
        /// </summary>
        public string Text;
    }
}
