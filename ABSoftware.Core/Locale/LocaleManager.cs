using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ABSoftware.Core.Locale
{
    /// <summary>
    /// Handles strings used in ABSoftware programs.
    /// </summary>
    public class LocaleManager
    {
        private List<LocaleUnit> _units = new List<LocaleUnit>(); 

        public async Task<LocaleUnit> RegisterUnit(string unitName, string localeSource)
        {
            // Generate a locale unit.
            var unit = new LocaleUnit();

            // Initialize the unit, completely asynchronously.
            await unit.InitializeUnit();

            // Finally, add the unit, and return it.
            _units.Add(new LocaleUnit());
            return unit;
        }
    }
}
