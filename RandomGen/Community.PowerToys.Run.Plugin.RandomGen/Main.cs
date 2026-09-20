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

        // ponytail: DRY clipboard lambda — 14 callers reduced to one helper
        private static Func<ActionContext, bool> CopyAction(string value) => _ =>
        {
            try { Clipboard.SetDataObject(value); return true; }
            catch { return false; }
        };

        // Method to clean up duplicate action keyword prefixes
        private string CleanupQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return query;

            var parts = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length >= 3 &&
                parts[0].Equals(parts[1], StringComparison.OrdinalIgnoreCase))
            {
                return string.Join(" ", parts.Skip(1));
            }

            return query;
        }

        /// <summary>
        /// Return a filtered list, based on the given query.
        /// </summary>
        public List<Result> Query(Query query)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(query?.Search))
                    return GetHelpResults();

                var cleanedSearch = CleanupQuery(query.Search);
                var searchTerms = cleanedSearch.Split(' ', StringSplitOptions.RemoveEmptyEntries);

                if (searchTerms.Length == 0)
                    return GetHelpResults();

                var command = searchTerms[0].ToLowerInvariant();
                var parameters = searchTerms.Length > 1 ? string.Join(" ", searchTerms.Skip(1)) : null;

                return command switch
                {
                    "password" or "pwd"       => [GeneratePassword(parameters)],
                    "email"                   => [GenerateEmail()],
                    "name"                    => [GenerateName()],
                    "address"                 => [GenerateAddress()],
                    "phone"                   => [GeneratePhone()],
                    "company"                 => [GenerateCompany()],
                    "lorem"                   => [GenerateLorem(parameters)],
                    "number" or "num"         => [GenerateNumber(parameters)],
                    "date"                    => [GenerateDate()],
                    "guid" or "uuid"          => [GenerateGuid()],
                    "color"                   => [GenerateColor()],
                    "url"                     => [GenerateUrl()],
                    "credit" or "creditcard"  => [GenerateCreditCard()],
                    "locale"                  => [ChangeLocale(parameters)],
                    "pin"                     => [GeneratePin(parameters)],
                    "ip"                      => [GenerateIp()],
                    "username" or "user"      => [GenerateUsername()],
                    _                         => GetFilteredSuggestions(command)
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
                Action = CopyAction(password),
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
                Action = CopyAction(email),
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
                Action = CopyAction(name),
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
                Action = CopyAction(address),
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
                Action = CopyAction(phone),
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
                Action = CopyAction(company),
                ContextData = company,
            };
        }

        private Result GenerateLorem(string parameter)
        {
            int wordCount = 10;
            if (int.TryParse(parameter, out int parsedCount) && parsedCount > 0 && parsedCount <= 100)
                wordCount = parsedCount;

            var lorem = string.Join(" ", GetFaker().Lorem.Words(wordCount));
            return new Result
            {
                QueryTextDisplay = $"lorem {wordCount}",
                IcoPath = IconPath,
                Title = lorem,
                SubTitle = $"Lorem ipsum ({wordCount} words) - Click to copy",
                ToolTipData = new ToolTipData("Lorem Ipsum", $"Generated {wordCount} words of placeholder text"),
                Action = CopyAction(lorem),
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

            if (min > max) (min, max) = (max, min);

            var number = GetFaker().Random.Int(min, max).ToString();
            return new Result
            {
                QueryTextDisplay = $"number {min}-{max}",
                IcoPath = IconPath,
                Title = number,
                SubTitle = $"Random number between {min} and {max} - Click to copy",
                ToolTipData = new ToolTipData("Random Number", $"Generated random integer in range [{min}, {max}]"),
                Action = CopyAction(number),
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
                Action = CopyAction(date),
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
                Action = CopyAction(guid),
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
                Action = CopyAction(color),
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
                Action = CopyAction(url),
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
                Action = CopyAction(creditCard),
                ContextData = creditCard,
            };
        }

        private Result GeneratePin(string parameter)
        {
            int length = 4;
            if (int.TryParse(parameter, out int parsed) && parsed >= 4 && parsed <= 12)
                length = parsed;

            string pin;
            int attempts = 0;
            do
            {
                pin = string.Concat(Enumerable.Range(0, length).Select(_ => Random.Shared.Next(10).ToString()));
                attempts++;
            } while (attempts < 20 && IsWeakPin(pin));

            return new Result
            {
                QueryTextDisplay = $"pin {length}",
                IcoPath = IconPath,
                Title = pin,
                SubTitle = $"Random {length}-digit PIN - Click to copy",
                ToolTipData = new ToolTipData("Random PIN", $"Generated {length}-digit PIN (weak patterns avoided)"),
                Action = CopyAction(pin),
                ContextData = pin,
            };
        }

        private static bool IsWeakPin(string pin)
        {
            // All same digit: 0000, 1111, ...
            if (pin.Distinct().Count() == 1) return true;

            // Sequential ascending: 0123, 1234, ...
            bool ascending = true, descending = true;
            for (int i = 1; i < pin.Length; i++)
            {
                if (pin[i] - pin[i - 1] != 1) ascending = false;
                if (pin[i - 1] - pin[i] != 1) descending = false;
            }
            return ascending || descending;
        }

        private Result GenerateIp()
        {
            var ip = GetFaker().Internet.Ip();
            return new Result
            {
                QueryTextDisplay = "ip",
                IcoPath = IconPath,
                Title = ip,
                SubTitle = "Random IP address - Click to copy",
                ToolTipData = new ToolTipData("Random IP", "Generated fake IPv4 address"),
                Action = CopyAction(ip),
                ContextData = ip,
            };
        }

        private Result GenerateUsername()
        {
            var username = GetFaker().Internet.UserName();
            return new Result
            {
                QueryTextDisplay = "username",
                IcoPath = IconPath,
                Title = username,
                SubTitle = "Random username - Click to copy",
                ToolTipData = new ToolTipData("Random Username", "Generated fake username"),
                Action = CopyAction(username),
                ContextData = username,
            };
        }

        private List<Result> GetHelpResults()
        {
            return new List<Result>
            {
                CreateHelpResult("password [length] [options]", "Generate random password (default: 12 chars)", "password 16 -special"),
                CreateHelpResult("pwd [length] [options]", "Generate password with options (-lower, -upper, -numeric, -special)", "pwd 20 -symbols"),
                CreateHelpResult("pin [length]", "Generate random PIN (default: 4 digits, avoids weak patterns)", "pin 6"),
                CreateHelpResult("email", "Generate random email address", "email"),
                CreateHelpResult("username", "Generate random username", "username"),
                CreateHelpResult("name", "Generate random full name", "name"),
                CreateHelpResult("address", "Generate random address", "address"),
                CreateHelpResult("phone", "Generate random phone number", "phone"),
                CreateHelpResult("company", "Generate random company name", "company"),
                CreateHelpResult("lorem [words]", "Generate lorem ipsum text", "lorem 20"),
                CreateHelpResult("number [min-max]", "Generate random number", "number 1-1000"),
                CreateHelpResult("date", "Generate random date", "date"),
                CreateHelpResult("guid", "Generate random GUID/UUID", "guid"),
                CreateHelpResult("color", "Generate random hex color", "color"),
                CreateHelpResult("ip", "Generate random IP address", "ip"),
                CreateHelpResult("url", "Generate random URL", "url"),
                CreateHelpResult("creditcard", "Generate random credit card number", "creditcard"),
                CreateHelpResult("locale [code]", "Change data generation locale (e.g. uk, fr, de)", "locale uk"),
            };
        }

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

        private List<Result> GetFilteredSuggestions(string query)
        {
            var commands = new[]
            {
                ("password",   "Generate random password with options (-lower, -upper, -numeric, -special)", "password 16 -special"),
                ("pwd",        "Generate random password with options (alias)",                              "pwd 20 -symbols"),
                ("pin",        "Generate random PIN (default 4 digits, weak patterns avoided)",              "pin 6"),
                ("email",      "Generate random email address",                                              "email"),
                ("username",   "Generate random username",                                                   "username"),
                ("user",       "Generate random username (alias)",                                           "user"),
                ("name",       "Generate random full name",                                                  "name"),
                ("address",    "Generate random address",                                                    "address"),
                ("phone",      "Generate random phone number",                                               "phone"),
                ("company",    "Generate random company name",                                               "company"),
                ("lorem",      "Generate lorem ipsum text",                                                  "lorem 20"),
                ("number",     "Generate random number",                                                     "number 1-1000"),
                ("num",        "Generate random number (alias)",                                             "num 1-1000"),
                ("date",       "Generate random date",                                                       "date"),
                ("guid",       "Generate random GUID/UUID",                                                  "guid"),
                ("uuid",       "Generate random GUID/UUID (alias)",                                          "uuid"),
                ("color",      "Generate random hex color",                                                  "color"),
                ("ip",         "Generate random IP address",                                                 "ip"),
                ("url",        "Generate random URL",                                                        "url"),
                ("credit",     "Generate random credit card number",                                         "credit"),
                ("creditcard", "Generate random credit card number",                                         "creditcard"),
                ("locale",     "Change data generation locale",                                              "locale fr"),
            };

            var matches = commands
                .Where(cmd => cmd.Item1.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(cmd => cmd.Item1.Length)
                .Take(5)
                .ToList();

            if (!matches.Any())
            {
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

                if (int.TryParse(part, out int length) && length > 0 && length <= 128)
                {
                    settings.Length = length;
                }
                else if (lowerPart.StartsWith("-"))
                {
                    switch (lowerPart)
                    {
                        case "-l" or "-lower" or "-lowercase":  settings.IncludeLowercase = false; break;
                        case "-u" or "-upper" or "-uppercase":  settings.IncludeUppercase = false; break;
                        case "-n" or "-num" or "-numeric":      settings.IncludeNumeric   = false; break;
                        case "-s" or "-special" or "-symbols":  settings.IncludeSpecial   = false; break;
                    }
                }
                else if (lowerPart.StartsWith("+"))
                {
                    switch (lowerPart)
                    {
                        case "+l" or "+lower" or "+lowercase":  settings.IncludeLowercase = true; break;
                        case "+u" or "+upper" or "+uppercase":  settings.IncludeUppercase = true; break;
                        case "+n" or "+num" or "+numeric":      settings.IncludeNumeric   = true; break;
                        case "+s" or "+special" or "+symbols":  settings.IncludeSpecial   = true; break;
                    }
                }
                else
                {
                    switch (lowerPart)
                    {
                        case "nolower" or "no-lower":                                    settings.IncludeLowercase = false; break;
                        case "noupper" or "no-upper":                                    settings.IncludeUppercase = false; break;
                        case "nonumeric" or "no-numeric" or "nonumbers" or "no-numbers": settings.IncludeNumeric   = false; break;
                        case "nospecial" or "no-special" or "nosymbols" or "no-symbols": settings.IncludeSpecial   = false; break;
                    }
                }
            }

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
            if (!settings.IncludeNumeric)   excluded.Add("numeric");
            if (!settings.IncludeSpecial)   excluded.Add("special");
            return excluded.Count > 0 ? $"no {string.Join(",", excluded)}" : "all types";
        }

        private string GetPasswordOptionsDescription(PasswordSettings settings)
        {
            var enabled = new List<string>();
            if (settings.IncludeLowercase) enabled.Add("lowercase");
            if (settings.IncludeUppercase) enabled.Add("uppercase");
            if (settings.IncludeNumeric)   enabled.Add("numbers");
            if (settings.IncludeSpecial)   enabled.Add("symbols");
            return string.Join(", ", enabled);
        }

        private static string GenerateRandomPassword(PasswordSettings settings)
        {
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits    = "0123456789";
            const string symbols   = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            var availableChars  = new StringBuilder();
            var guaranteedChars = new List<char>();
            var random          = new Random();

            if (settings.IncludeLowercase) { availableChars.Append(lowercase); guaranteedChars.Add(lowercase[random.Next(lowercase.Length)]); }
            if (settings.IncludeUppercase) { availableChars.Append(uppercase); guaranteedChars.Add(uppercase[random.Next(uppercase.Length)]); }
            if (settings.IncludeNumeric)   { availableChars.Append(digits);    guaranteedChars.Add(digits[random.Next(digits.Length)]); }
            if (settings.IncludeSpecial)   { availableChars.Append(symbols);   guaranteedChars.Add(symbols[random.Next(symbols.Length)]); }

            if (availableChars.Length == 0)
            {
                availableChars.Append(lowercase + uppercase);
                guaranteedChars.Add(lowercase[random.Next(lowercase.Length)]);
                guaranteedChars.Add(uppercase[random.Next(uppercase.Length)]);
            }

            var allChars    = availableChars.ToString();
            var password    = new StringBuilder();
            var finalLength = Math.Max(settings.Length, guaranteedChars.Count);

            foreach (var ch in guaranteedChars)
                password.Append(ch);

            for (int i = guaranteedChars.Count; i < finalLength; i++)
                password.Append(allChars[random.Next(allChars.Length)]);

            var arr = password.ToString().ToCharArray();
            for (int i = arr.Length - 1; i > 0; i--)
            {
                int j = random.Next(i + 1);
                (arr[i], arr[j]) = (arr[j], arr[i]);
            }

            return new string(arr);
        }

        /// <summary>
        /// Initialize the plugin with the given <see cref="PluginInitContext"/>.
        /// </summary>
        public void Init(PluginInitContext context)
        {
            ArgumentNullException.ThrowIfNull(context);
            Context  = context;
            IconPath = "Images/randomgen.light.png";
        }

        /// <summary>
        /// Return a list context menu entries for a given <see cref="Result"/>.
        /// </summary>
        public List<ContextMenuResult> LoadContextMenus(Result selectedResult)
        {
            if (selectedResult.ContextData is string data)
            {
                return
                [
                    new ContextMenuResult
                    {
                        PluginName         = Name,
                        Title              = "Copy to clipboard (Ctrl+C)",
                        FontFamily         = "Segoe MDL2 Assets",
                        Glyph              = "\xE8C8",
                        AcceleratorKey     = Key.C,
                        AcceleratorModifiers = ModifierKeys.Control,
                        Action             = CopyAction(data),
                    },
                    new ContextMenuResult
                    {
                        PluginName         = Name,
                        Title              = "Generate new",
                        FontFamily         = "Segoe MDL2 Assets",
                        Glyph              = "\xE117",
                        AcceleratorKey     = Key.F5,
                        AcceleratorModifiers = ModifierKeys.None,
                        Action             = _ =>
                        {
                            try
                            {
                                var cleanKeyword = Context.CurrentPluginMetadata.ActionKeyword.Split(' ')[0];
                                Context.API.ChangeQuery($"{cleanKeyword} {selectedResult.QueryTextDisplay}");
                                return false;
                            }
                            catch { return false; }
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

        protected virtual void Dispose(bool disposing)
        {
            if (Disposed || !disposing) return;
            Disposed = true;
        }
    }
}
