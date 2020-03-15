using System.Collections.Generic;
using System.Threading.Tasks;

namespace ABSoftware.Core.Locale
{
    public class LocaleCategory
    {
        /// <summary>
        /// The name of this category.
        /// </summary>
        public string Name;

        /// <summary>
        /// The text within this category.
        /// </summary>
        List<LocaleText> _text;

        public Task<LocaleText> GetTextById(string id)
        {
            return Task.Run(() =>
            {
                for (int i = 0; i < _text.Count; i++)
                    if (_text[i].Id == id)
                        return _text[i];

                // If we failed to find one, just generate a new one that is completely blank.
                return new LocaleText()
                {
                    Id = id,
                    Text = id
                };
            });
        }
    }
}