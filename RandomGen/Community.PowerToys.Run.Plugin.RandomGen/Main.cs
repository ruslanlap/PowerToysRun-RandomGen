using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Text;
using System.Windows;
using System.Windows.Input;
using Wox.Plugin;
using Bogus;

namespace Community.PowerToys.Run.Plugin.RandomGen
{
    /// <summary>
    /// Password generation settings interface.
    /// </summary>
    public interface IPasswordSettings
    {
        bool IncludeLowercase { get; set; }
        bool IncludeUppercase { get; set; }
        bool IncludeNumeric { get; set; }
        bool IncludeSpecial { get; set; }
        int Length { get; set; }
    }

    /// <summary>
    /// Default password generation settings.
    /// </summary>
    public class PasswordSettings : IPasswordSettings
    {
        public bool IncludeLowercase { get; set; } = true;
        public bool IncludeUppercase { get; set; } = true;
        public bool IncludeNumeric { get; set; } = true;
        public bool IncludeSpecial { get; set; } = true;
        public int Length { get; set; } = 12;
    }

    /// <summary>
    /// Main class of this plugin that implement all used interfaces.
    /// </summary>
    [SupportedOSPlatform("windows")]
    public class Main : IPlugin, IContextMenu, IDisposable
    {
        /// <summary>
        /// ID of the plugin.
        /// </summary>
        public static string PluginID => "EFADBA167C1B41D8A7426A7DF808D28E";

        /// <summary>
        /// Name of the plugin.
        /// </summary>
        public string Name => "RandomGen";

        /// <summary>
        /// Description of the plugin.
        /// </summary>
        public string Description => "Generate random data like passwords, emails, names, addresses, and more";

        private PluginInitContext Context { get; set; }

        private string IconPath { get; set; }

        private bool Disposed { get; set; }

        private Faker _faker;
       private string _locale = "en";

        private static readonly HashSet<string> SupportedLocales = new(
            new[]{
                "af_ZA","ar","az","cz","de","de_AT","de_CH","el","en","en_AU","en_AU_ocker",
                "en_BORK","en_CA","en_GB","en_IE","en_IND","en_NG","en_US","en_ZA","es",
                "es_MX","fa","fi","fr","fr_CA","fr_CH","ge","hr","id_ID","it","ja","ko","lv",
                "nb_NO","ne","nl","nl_BE","pl","pt_BR","pt_PT","ro","ru","sk","sv","tr","uk","vi",
                "zh_CN","zh_TW","zu_ZA"
            });

        private Faker GetFaker()
        {
            return _faker ??= new Faker(_locale);
        }

        private void SetLocale(string locale)
        {
            if (SupportedLocales.Contains(locale))
            {
                _locale = locale;
                _faker = new Faker(_locale);
            }
        }

        // Method to clean up duplicate action keyword prefixes
        private string CleanupQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return query;

            // For "rd rd email" -> we want to get "email"
            var parts = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            if (parts.Length >= 3 && 
                parts[0].Equals(parts[1], StringComparison.OrdinalIgnoreCase))
            {
                // Remove the first duplicate part: "rd rd email" -> "rd email"
                return string.Join(" ", parts.Skip(1));
            }
            
            return query;
        }

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        /// <param name="query">The query to filter the list.</param>
        /// <returns>A filtered list, can be empty when nothing was found.</returns>
        public List<Result> Query(Query query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query?.Search))
                {
                    return GetHelpResults();
                }

                // Clean up duplicate action keywords first
                var cleanedSearch = CleanupQuery(query.Search);
                var searchTerms = cleanedSearch.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (searchTerms.Length == 0)
                {
                    return GetHelpResults();
                }

                var command = searchTerms[0].ToLowerInvariant();
                var parameters = searchTerms.Length > 1 ? string.Join(" ", searchTerms.Skip(1)) : null;

                // Check for exact matches first
                return command switch
                {
                    "password" or "pwd" => [GeneratePassword(parameters)],
                    "email" => [GenerateEmail()],
                    "name" => [GenerateName()],
                    "address" => [GenerateAddress()],
                    "phone" => [GeneratePhone()],
                    "company" => [GenerateCompany()],
                    "lorem" => [GenerateLorem(parameters)],
                    "number" or "num" => [GenerateNumber(parameters)],
                    "date" => [GenerateDate()],
                    "guid" or "uuid" => [GenerateGuid()],
                    "color" => [GenerateColor()],
                    "url" => [GenerateUrl()],
                    "credit" or "creditcard" => [GenerateCreditCard()],
                    "locale" => [ChangeLocale(parameters)],
                    _ => GetFilteredSuggestions(command)
                };
            }
            catch (Exception ex)
            {
                return [new Result
                {
                    Title = "Error generating data",
                    SubTitle = $"Error: {ex.Message}",
                    IcoPath = IconPath
                }];
            }
        }

        private Result GeneratePassword(string parameter)
        {
            var settings = ParsePasswordSettings(parameter);
            var password = GenerateRandomPassword(settings);

            var optionsText = GetPasswordOptionsText(settings);
            var queryDisplay = string.IsNullOrEmpty(optionsText) ? $"password {settings.Length}" : $"password {settings.Length} {optionsText}";

            return new Result
            {
                QueryTextDisplay = queryDisplay,
                IcoPath = IconPath,
                Title = password,
                SubTitle = $"Random password ({settings.Length} chars{(string.IsNullOrEmpty(optionsText) ? "" : $", {optionsText}")}) - Click to copy",
                ToolTipData = new ToolTipData("Random Password", $"Generated {settings.Length}-character password with options: {GetPasswordOptionsDescription(settings)}"),
                Action = _ =>
                {
                    try
                    {
                        Clipboard.SetDataObject(password);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                },
                ContextData = password,
            };
        }

        private Result GenerateEmail()
        {
            var email = GetFaker().Internet.Email();

            return new Result
            {
                QueryTextDisplay = "email",
                IcoPath = IconPath,
                Title = email,
                SubTitle = "Random email address - Click to copy",
                ToolTipData = new ToolTipData("Random Email", "Generated fake email address for testing purposes"),
                Action = _ =>
                {
                    try
                    {
                        Clipboard.SetDataObject(email);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                },
                ContextData = email,
            };
        }

        private Result GenerateName()
        {
            var name = GetFaker().Name.FullName();

            return new Result
            {
                QueryTextDisplay = "name",
                IcoPath = IconPath,
                Title = name,
                SubTitle = "Random full name - Click to copy",
                ToolTipData = new ToolTipData("Random Name", "Generated fake person name"),
                Action = _ =>
                {
                    try
                    {
                        Clipboard.SetDataObject(name);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                },
                ContextData = name,
            };
        }

        private Result GenerateAddress()
        {
            var address = GetFaker().Address.FullAddress();

            return new Result
            {
                QueryTextDisplay = "address",
                IcoPath = IconPath,
                Title = address,
                SubTitle = "Random address - Click to copy",
                ToolTipData = new ToolTipData("Random Address", "Generated fake address for testing"),
                Action = _ =>
                {
                    try
                    {
                        Clipboard.SetDataObject(address);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                },
                ContextData = address,
            };
        }

        private Result GeneratePhone()
        {
            var phone = GetFaker().Phone.PhoneNumber();

            return new Result
            {
                QueryTextDisplay = "phone",
                IcoPath = IconPath,
                Title = phone,
                SubTitle = "Random phone number - Click to copy",
                ToolTipData = new ToolTipData("Random Phone", "Generated fake phone number"),
                Action = _ =>
                {
                    try
                    {
                        Clipboard.SetDataObject(phone);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                },
                ContextData = phone,
            };
        }

        private Result GenerateCompany()
        {
            var company = GetFaker().Company.CompanyName();

            return new Result
            {
                QueryTextDisplay = "company",
                IcoPath = IconPath,
                Title = company,
                SubTitle = "Random company name - Click to copy",
                ToolTipData = new ToolTipData("Random Company", "Generated fake company name"),
                Action = _ =>
                {
                    try
                    {
                        Clipboard.SetDataObject(company);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                },
                ContextData = company,
            };
        }

        private Result GenerateLorem(string parameter)
        {
            int wordCount = 10; // default word count
            if (int.TryParse(parameter, out int parsedCount) && parsedCount > 0 && parsedCount <= 100)
            {
                wordCount = parsedCount;
            }

            var lorem = string.Join(" ", GetFaker().Lorem.Words(wordCount));

            return new Result
            {
                QueryTextDisplay = $"lorem {wordCount}",
                IcoPath = IconPath,
                Title = lorem,
                SubTitle = $"Lorem ipsum ({wordCount} words) - Click to copy",
                ToolTipData = new ToolTipData("Lorem Ipsum", $"Generated {wordCount} words of placeholder text"),
                Action = _ =>
                {
                    try
                    {
                        Clipboard.SetDataObject(lorem);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                },
                ContextData = lorem,
            };
        }

        private Result GenerateNumber(string parameter)
        {
            var parts = parameter?.Split('-') ?? ["1", "100"];
            int min = 1, max = 100;

            if (parts.Length >= 1 && int.TryParse(parts[0], out int parsedMin))
                min = parsedMin;
            if (parts.Length >= 2 && int.TryParse(parts[1], out int parsedMax))
                max = parsedMax;

            if (min > max) (min, max) = (max, min); // swap if needed

            var number = GetFaker().Random.Int(min, max).ToString();

            return new Result
            {
                QueryTextDisplay = $"number {min}-{max}",
                IcoPath = IconPath,
                Title = number,
                SubTitle = $"Random number between {min} and {max} - Click to copy",
                ToolTipData = new ToolTipData("Random Number", $"Generated random integer in range [{min}, {max}]"),
                Action = _ =>
                {
                    try
                    {
                        Clipboard.SetDataObject(number);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                },
                ContextData = number,
            };
        }

        private Result GenerateDate()
        {
            var date = GetFaker().Date.Between(DateTime.Now.AddYears(-10), DateTime.Now.AddYears(10)).ToString("yyyy-MM-dd");

            return new Result
            {
                QueryTextDisplay = "date",
                IcoPath = IconPath,
                Title = date,
                SubTitle = "Random date - Click to copy",
                ToolTipData = new ToolTipData("Random Date", "Generated random date in ISO format"),
                Action = _ =>
                {
                    try
                    {
                        Clipboard.SetDataObject(date);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                },
                ContextData = date,
            };
        }

        private Result GenerateGuid()
        {
            var guid = Guid.NewGuid().ToString();

            return new Result
            {
                QueryTextDisplay = "guid",
                IcoPath = IconPath,
                Title = guid,
                SubTitle = "Random GUID/UUID - Click to copy",
                ToolTipData = new ToolTipData("Random GUID", "Generated unique identifier"),
                Action = _ =>
                {
                    try
                    {
                        Clipboard.SetDataObject(guid);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                },
                ContextData = guid,
            };
        }

        private Result GenerateColor()
        {
            var color = GetFaker().Internet.Color();

            return new Result
            {
                QueryTextDisplay = "color",
                IcoPath = IconPath,
                Title = color,
                SubTitle = "Random hex color - Click to copy",
                ToolTipData = new ToolTipData("Random Color", "Generated hexadecimal color code"),
                Action = _ =>
                {
                    try
                    {
                        Clipboard.SetDataObject(color);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                },
                ContextData = color,
            };
        }

        private Result GenerateUrl()
        {
            var url = GetFaker().Internet.Url();

            return new Result
            {
                QueryTextDisplay = "url",
                IcoPath = IconPath,
                Title = url,
                SubTitle = "Random URL - Click to copy",
                ToolTipData = new ToolTipData("Random URL", "Generated fake web address"),
                Action = _ =>
                {
                    try
                    {
                        Clipboard.SetDataObject(url);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                },
                ContextData = url,
            };
        }

        private Result GenerateCreditCard()
        {
            var creditCard = GetFaker().Finance.CreditCardNumber();

            return new Result
            {
                QueryTextDisplay = "creditcard",
                IcoPath = IconPath,
                Title = creditCard,
                SubTitle = "Random credit card number (fake) - Click to copy",
                ToolTipData = new ToolTipData("Random Credit Card", "Generated fake credit card number for testing"),
                Action = _ =>
                {
                    try
                    {
                        Clipboard.SetDataObject(creditCard);
                        return true;
                    }
                    catch
                    {
                        return false;
                    }
                },
                ContextData = creditCard,
            };
        }

        private List<Result> GetHelpResults()
        {
            return
            [
                CreateHelpResult("password [length] [options]", "Generate random password (default: 12 chars)", "password 16 -special"),
                        private Result ChangeLocale(string parameter)
        {
            if (string.IsNullOrWhiteSpace(parameter))
            {
                return new Result
                {
                    QueryTextDisplay = "locale",
                    IcoPath = IconPath,
                    Title = $"Current locale: {_locale}",
                    SubTitle = "Specify a locale code to change",
                };
            }

            var code = parameter.Trim();
            if (!SupportedLocales.Contains(code))
            {
                return new Result
                {
                    QueryTextDisplay = $"locale {code}",
                    IcoPath = IconPath,
                    Title = "Invalid locale",
                    SubTitle = $"Supported: {string.Join(",", SupportedLocales)}",
                };
            }

            SetLocale(code);
            return new Result
            {
                QueryTextDisplay = $"locale {code}",
                IcoPath = IconPath,
                Title = $"Locale set to {code}",
                SubTitle = "Locale changed for future results",
            };
        }

                CreateHelpResult("pwd [length] [options]", "Generate password with options (-lower, -upper, -numeric, -special)", "pwd 20 -symbols"),
                CreateHelpResult("email", "Generate random email address", "email"),
                CreateHelpResult("name", "Generate random full name", "name"),
                CreateHelpResult("address", "Generate random address", "address"),
                CreateHelpResult("phone", "Generate random phone number", "phone"),
                CreateHelpResult("company", "Generate random company name", "company"),
                CreateHelpResult("lorem [words]", "Generate lorem ipsum text", "lorem 20"),
                CreateHelpResult("number [min-max]", "Generate random number", "number 1-1000"),
                CreateHelpResult("date", "Generate random date", "date"),
                CreateHelpResult("guid", "Generate random GUID/UUID", "guid"),
                CreateHelpResult("color", "Generate random hex color", "color"),
                CreateHelpResult("url", "Generate random URL", "url"),
                CreateHelpResult("creditcard", "Generate random credit card number", "creditcard")
            ];
        }

        private List<Result> GetFilteredSuggestions(string query)
        {
            var commands = new[]
            {
                ("password", "Generate random password with options (-lower, -upper, -numeric, -special)", "password 16 -special"),
                ("pwd", "Generate random password with options (alias)", "pwd 20 -symbols"),
                ("email", "Generate random email address", "email"),
                ("name", "Generate random full name", "name"),
                ("address", "Generate random address", "address"),
                ("phone", "Generate random phone number", "phone"),
                ("company", "Generate random company name", "company"),
                ("lorem", "Generate lorem ipsum text", "lorem 20"),
                ("number", "Generate random number", "number 1-1000"),
                ("num", "Generate random number (alias)", "num 1-1000"),
                ("date", "Generate random date", "date"),
                ("guid", "Generate random GUID/UUID", "guid"),
                ("uuid", "Generate random GUID/UUID (alias)", "uuid"),
                ("color", "Generate random hex color", "color"),
                ("url", "Generate random URL", "url"),
                ("credit", "Generate random credit card number", "credit"),
                ("locale", "Change data generation locale", "locale fr"),
                ("creditcard", "Generate random credit card number", "creditcard")
            };

            var matches = commands
                .Where(cmd => cmd.Item1.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(cmd => cmd.Item1.Length)
                .Take(5)
                .ToList();

            if (!matches.Any())
            {
                // Fallback to fuzzy matching
                matches = commands
                    .Where(cmd => cmd.Item1.Contains(query, StringComparison.OrdinalIgnoreCase))
                    .OrderBy(cmd => cmd.Item1.Length)
                    .Take(3)
                    .ToList();
            }

            if (!matches.Any())
                return GetHelpResults();

            return matches.Select(cmd => CreateHelpResult(cmd.Item1, cmd.Item2, cmd.Item3)).ToList();
        }

        private Result CreateHelpResult(string command, string description, string example)
        {
            return new Result
            {
                QueryTextDisplay = command,
                IcoPath = IconPath,
                Title = command,
                SubTitle = $"{description} (e.g., {Context.CurrentPluginMetadata.ActionKeyword.Split(' ')[0]} {example})",
                ToolTipData = new ToolTipData("RandomGen Command", description),
                Action = _ =>
                {
                    try
                    {
                        var cleanKeyword = Context.CurrentPluginMetadata.ActionKeyword.Split(' ')[0];
                        Context.API.ChangeQuery($"{cleanKeyword} {command.Split(' ')[0]} ");
                        return false;
                    }
                    catch
                    {
                        return false;
                    }
                },
                ContextData = command,
            };
        }

        private PasswordSettings ParsePasswordSettings(string parameter)
        {
            var settings = new PasswordSettings();
            
            if (string.IsNullOrWhiteSpace(parameter))
                return settings;

            var parts = parameter.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            
            foreach (var part in parts)
            {
                var lowerPart = part.ToLowerInvariant();
                
                // Parse length if it's a number
                if (int.TryParse(part, out int length) && length > 0 && length <= 128)
                {
                    settings.Length = length;
                }
                // Parse character type options - exclusions
                else if (lowerPart.StartsWith("-"))
                {
                    switch (lowerPart)
                    {
                        case "-l" or "-lower" or "-lowercase":
                            settings.IncludeLowercase = false;
                            break;
                        case "-u" or "-upper" or "-uppercase":
                            settings.IncludeUppercase = false;
                            break;
                        case "-n" or "-num" or "-numeric":
                            settings.IncludeNumeric = false;
                            break;
                        case "-s" or "-special" or "-symbols":
                            settings.IncludeSpecial = false;
                            break;
                    }
                }
                // Parse character type options - inclusions
                else if (lowerPart.StartsWith("+"))
                {
                    switch (lowerPart)
                    {
                        case "+l" or "+lower" or "+lowercase":
                            settings.IncludeLowercase = true;
                            break;
                        case "+u" or "+upper" or "+uppercase":
                            settings.IncludeUppercase = true;
                            break;
                        case "+n" or "+num" or "+numeric":
                            settings.IncludeNumeric = true;
                            break;
                        case "+s" or "+special" or "+symbols":
                            settings.IncludeSpecial = true;
                            break;
                    }
                }
                // Parse options without prefix for convenience
                else
                {
                    switch (lowerPart)
                    {
                        case "nolower" or "no-lower":
                            settings.IncludeLowercase = false;
                            break;
                        case "noupper" or "no-upper":
                            settings.IncludeUppercase = false;
                            break;
                        case "nonumeric" or "no-numeric" or "nonumbers" or "no-numbers":
                            settings.IncludeNumeric = false;
                            break;
                        case "nospecial" or "no-special" or "nosymbols" or "no-symbols":
                            settings.IncludeSpecial = false;
                            break;
                    }
                }
            }

            // Ensure at least one character type is enabled
            if (!settings.IncludeLowercase && !settings.IncludeUppercase && 
                !settings.IncludeNumeric && !settings.IncludeSpecial)
            {
                settings.IncludeLowercase = true;
                settings.IncludeUppercase = true;
            }

            return settings;
        }

        private string GetPasswordOptionsText(PasswordSettings settings)
        {
            var excluded = new List<string>();
            
            if (!settings.IncludeLowercase) excluded.Add("lower");
            if (!settings.IncludeUppercase) excluded.Add("upper");
            if (!settings.IncludeNumeric) excluded.Add("numeric");
            if (!settings.IncludeSpecial) excluded.Add("special");
            
            return excluded.Count > 0 ? $"no {string.Join(",", excluded)}" : "all types";
        }

        private string GetPasswordOptionsDescription(PasswordSettings settings)
        {
            var enabled = new List<string>();
            
            if (settings.IncludeLowercase) enabled.Add("lowercase");
            if (settings.IncludeUppercase) enabled.Add("uppercase");
            if (settings.IncludeNumeric) enabled.Add("numbers");
            if (settings.IncludeSpecial) enabled.Add("symbols");
            
            return string.Join(", ", enabled);
        }

        private static string GenerateRandomPassword(PasswordSettings settings)
        {
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string symbols = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            var availableChars = new StringBuilder();
            var guaranteedChars = new List<char>();
            var random = new Random();

            // Build available character set and guarantee at least one from each enabled type
            if (settings.IncludeLowercase)
            {
                availableChars.Append(lowercase);
                guaranteedChars.Add(lowercase[random.Next(lowercase.Length)]);
            }
            if (settings.IncludeUppercase)
            {
                availableChars.Append(uppercase);
                guaranteedChars.Add(uppercase[random.Next(uppercase.Length)]);
            }
            if (settings.IncludeNumeric)
            {
                availableChars.Append(digits);
                guaranteedChars.Add(digits[random.Next(digits.Length)]);
            }
            if (settings.IncludeSpecial)
            {
                availableChars.Append(symbols);
                guaranteedChars.Add(symbols[random.Next(symbols.Length)]);
            }

            // Ensure we have characters to work with
            if (availableChars.Length == 0)
            {
                availableChars.Append(lowercase + uppercase);
                guaranteedChars.Add(lowercase[random.Next(lowercase.Length)]);
                guaranteedChars.Add(uppercase[random.Next(uppercase.Length)]);
            }

            var allChars = availableChars.ToString();
            var password = new StringBuilder();

            // Ensure password length accommodates guaranteed characters
            var finalLength = Math.Max(settings.Length, guaranteedChars.Count);

            // Add guaranteed characters first
            foreach (var ch in guaranteedChars)
            {
                password.Append(ch);
            }

            // Fill the rest randomly
            for (int i = guaranteedChars.Count; i < finalLength; i++)
            {
                password.Append(allChars[random.Next(allChars.Length)]);
            }

            // Shuffle the password
            var passwordArray = password.ToString().ToCharArray();
            for (int i = passwordArray.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (passwordArray[i], passwordArray[j]) = (passwordArray[j], passwordArray[i]);
            }

            return new string(passwordArray);
        }

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        /// <param name="context">The <see cref="PluginInitContext"/> for this plugin.</param>
        public void Init(PluginInitContext context)
        {
            ArgumentNullException.ThrowIfNull(context);

            Context = context;
            IconPath = "Images/randomgen.light.png";
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/> (shown at the right side of the result).
        /// </summary>
        /// <param name="selectedResult">The <see cref="Result"/> for the list with context menu entries.</param>
        /// <returns>A list context menu entries.</returns>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (selectedResult.ContextData is string data)
            {
                return
                [
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Copy to clipboard (Ctrl+C)",
                        FontFamily = "Segoe MDL2 Assets",
                        Glyph = "\xE8C8", // Copy
                        AcceleratorKey = Key.C,
                        AcceleratorModifiers = ModifierKeys.Control,
                        Action = _ =>
                        {
                            try
                            {
                                Clipboard.SetDataObject(data);
                                return true;
                            }
                            catch
                            {
                                return false;
                            }
                        },
                    },
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Generate new",
                        FontFamily = "Segoe MDL2 Assets",
                        Glyph = "\xE117", // Refresh
                        AcceleratorKey = Key.F5,
                        AcceleratorModifiers = ModifierKeys.None,
                        Action = _ =>
                        {
                            try
                            {
                                // Get the current action keyword and clean it if duplicated
                                var actionKeyword = Context.CurrentPluginMetadata.ActionKeyword;
                                var command = selectedResult.QueryTextDisplay;
                                
                                // Handle case where actionKeyword might be duplicated like "rd rd"
                                var cleanKeyword = actionKeyword.Split(' ')[0];
                                
                                // Get new results and show them by updating the query
                                Context.API.ChangeQuery($"{cleanKeyword} {command}");
                                return false;
                            }
                            catch
                            {
                                return false;
                            }
                        },
                    }
                ];
            }

            return [];
        }

        /// <inheritdoc/>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Wrapper method for <see cref="Dispose()"/> that dispose additional objects and events form the plugin itself.
        /// </summary>
        /// <param name="disposing">Indicate that the plugin is disposed.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (Disposed || !disposing)
            {
                return;
            }

            Disposed = true;
        }
    }
}
}