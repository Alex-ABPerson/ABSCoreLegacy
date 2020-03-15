using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSoftware.Core.Locale
{
    /// <summary>
    /// Represents a "unit" of locale (a unit is the locale for a certain plugin or part of an ABSoftware program).
    /// </summary>
    public class LocaleUnit
    {
        /// <summary>
        /// The name of this unit.
        /// </summary>
        public string UnitName;

        /// <summary>
        /// All of the smaller categories inside this unit.
        /// </summary>
        List<LocaleCategory> _categories;

        public Task InitializeUnit()
        {
            // TODO: When ABSave is completed place it in here.
            return new Task(() =>
            {
                _categories.Add(new LocaleCategory());
            });
        }
    }
}
