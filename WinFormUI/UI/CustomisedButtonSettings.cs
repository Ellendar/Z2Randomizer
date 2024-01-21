using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Dynamic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Z2Randomizer.WinFormUI
{
    public class CustomisedButtonSettings
    {
        public string Name { get; set; } = string.Empty;
        public string Flagset { get; set; } = string.Empty;
        public string Tooltip { get; set; } = string.Empty;

        public bool IsCustomised { get; set; } = false;
        public bool IsEmpty { get; private set; } = true;

        public CustomisedButtonSettings()
        {
        }

        public CustomisedButtonSettings(string name, string flagset, string tooltip)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new ArgumentException("Name cannot be null or whitespace", nameof(name));
            if (string.IsNullOrWhiteSpace(flagset))
                throw new ArgumentException("Name cannot be null or whitespace", nameof(name));

            Name = name;
            Flagset = flagset;
            Tooltip = tooltip;
            IsCustomised = true;
            IsEmpty = false;
        }

        public CustomisedButtonSettings(string customButtonSettingsJson)
        {
            if ((customButtonSettingsJson ?? "") != "")
            {
                dynamic settings = JObject.Parse(customButtonSettingsJson);

                Name = settings.Name;
                Flagset = settings.Flagset;
                Tooltip = settings.Tooltip;

                IsCustomised = !(Flagset == "");
                IsEmpty = (Flagset == "");
            }
        }

        public string Export()
        {
            if (IsCustomised)
            {
                dynamic settings = new ExpandoObject();
                settings.Name = Name;
                settings.Flagset = Flagset;
                settings.Tooltip = Tooltip;

                return JsonConvert.SerializeObject(settings);
            }
            return "";
        }
    }
}
