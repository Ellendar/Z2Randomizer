using System;
using System.Collections.Generic;
using System.Collections.Specialized;
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

        public CustomisedButtonSettings(StringCollection customButtonSettings)
        {
            if (customButtonSettings != null)
            {
                foreach (var buttonSetting in customButtonSettings)
                {
                    if (buttonSetting == null)
                    {
                        continue;
                    }

                    var split = buttonSetting.Split('|');

                    if (split.Length != 2)
                    {
                        continue;
                    }

                    switch (split[0])
                    {
                        case "Name":
                            {
                                Name = split[1];
                                break;
                            }
                        case "Tooltip":
                            {
                                Tooltip = split[1];
                                break;
                            }
                        case "Flagset":
                            {
                                Flagset = split[1];
                                break;
                            }
                    }

                    IsCustomised = true;
                    IsEmpty = false;
                }
            }
        }

        public StringCollection Export()
        {
            if (IsCustomised)
            {
                StringCollection sc = new StringCollection();
                sc.Add($"{nameof(Name)}|{Name}");
                sc.Add($"{nameof(Flagset)}|{Flagset}");
                sc.Add($"{nameof(Tooltip)}|{Tooltip}");
                return sc;
            }

            return new StringCollection();

        }
    }
}
