using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Versioning;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Input;
using Wox.Plugin;
using Bogus;

namespace Community.PowerToys.Run.Plugin.RandomGen
{
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
        public string Description => "Generate random data like passwords, emails, names, addresses, PIN codes, and more";

        private PluginInitContext Context { get; set; }
        private string IconPath { get; set; }
        private bool Disposed { get; set; }

        // Thread-safe Faker instance using ThreadLocal to avoid Bogus library's global lock
        private readonly ThreadLocal<Faker> _threadLocalFaker;
        private readonly List<IDisposable> _disposables = new();
        private readonly CancellationTokenSource _cancellationToken = new();

        // Performance optimization: Cache recently generated data
        private readonly ConcurrentDictionary<string, CachedResult> _cache = new();
        private readonly Timer _cacheCleanupTimer;

        public Main()
        {
            _threadLocalFaker = new ThreadLocal<Faker>(() => new Faker());
            _disposables.Add(_threadLocalFaker);
            _disposables.Add(_cancellationToken);

            // Cleanup cache every 5 minutes to prevent memory leaks
            _cacheCleanupTimer = new Timer(CleanupCache, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
            _disposables.Add(_cacheCleanupTimer);
        }

        private Faker GetFaker()
        {
            return _threadLocalFaker.Value;
        }

        private void CleanupCache(object state)
        {
            try
            {
                var cutoff = DateTime.UtcNow.AddMinutes(-5);
                var keysToRemove = _cache
                    .Where(kvp => kvp.Value.CreatedAt < cutoff)
                    .Select(kvp => kvp.Key)
                    .ToList();

                foreach (var key in keysToRemove)
                {
                    _cache.TryRemove(key, out _);
                }
            }
            catch (Exception)
            {
                // Ignore cleanup errors to prevent crashing the host
            }
        }

        // Method to clean up duplicate action keyword prefixes
        private string CleanupQuery(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return query;

            var parts = query.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            if (parts.Length == 0)
                return query;

            // Remove all consecutive duplicate keywords from the beginning
            var cleanedParts = new List<string>();
            string firstPart = parts[0];

            // Add the first part
            cleanedParts.Add(firstPart);

            // Skip consecutive duplicates of the first part
            for (int i = 1; i < parts.Length; i++)
            {
                if (!parts[i].Equals(firstPart, StringComparison.OrdinalIgnoreCase))
                {
                    // Add all remaining parts starting from this non-duplicate
                    cleanedParts.AddRange(parts.Skip(i));
                    break;
                }
            }

            return string.Join(" ", cleanedParts);
        }

        // Get clean action keyword without duplicates
        private string GetCleanActionKeyword()
        {
            var actionKeyword = Context?.CurrentPluginMetadata?.ActionKeyword ?? "rd";
            return actionKeyword.Split(' ', StringSplitOptions.RemoveEmptyEntries)[0];
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
                var parameter = searchTerms.Length > 1 ? searchTerms[1] : null;

                // Check for exact matches first
                var exactMatch = command switch
                {
                    "password" or "pwd" => GeneratePassword(parameter),
                    "email" => GenerateEmail(),
                    "name" => GenerateName(),
                    "address" => GenerateAddress(),
                    "phone" => GeneratePhone(),
                    "company" => GenerateCompany(),
                    "lorem" => GenerateLorem(parameter),
                    "number" or "num" => GenerateNumber(parameter),
                    "date" => GenerateDate(),
                    "guid" or "uuid" => GenerateGuid(),
                    "color" => GenerateColor(),
                    "url" => GenerateUrl(),
                    "credit" or "creditcard" => GenerateCreditCard(),
                    "pin" => GeneratePin(parameter),
                    _ => null
                };

                // If exact match found, return it
                if (exactMatch != null)
                {
                    return [exactMatch];
                }

                // Otherwise, return filtered suggestions for autocomplete
                return GetFilteredSuggestions(command);
            }
            catch (Exception ex) when (!(ex is OutOfMemoryException || ex is StackOverflowException))
            {
                // Defensive exception handling to prevent crashing PowerToys
                return [new Result
                {
                    Title = "Plugin Error",
                    SubTitle = "An error occurred generating random data",
                    IcoPath = IconPath,
                    ToolTipData = new ToolTipData("Error", $"Error details: {ex.Message}")
                }];
            }
        }

        private Result GeneratePassword(string parameter)
        {
            int length = 12; // default length
            if (int.TryParse(parameter, out int parsedLength) && parsedLength > 0 && parsedLength <= 128)
            {
                length = parsedLength;
            }

            var cacheKey = $"password_{length}";
            if (_cache.TryGetValue(cacheKey, out var cached) && cached.CreatedAt > DateTime.UtcNow.AddSeconds(-2))
            {
                return CreatePasswordResult(cached.Value, length);
            }

            var password = GenerateSecurePassword(length);
            _cache.TryAdd(cacheKey, new CachedResult { Value = password, CreatedAt = DateTime.UtcNow });

            return CreatePasswordResult(password, length);
        }

        private Result CreatePasswordResult(string password, int length)
        {
            return new Result
            {
                QueryTextDisplay = $"password {length}",
                IcoPath = IconPath,
                Title = password,
                SubTitle = $"Secure password ({length} characters) - Click to copy",
                ToolTipData = new ToolTipData("Secure Password", $"Generated {length}-character password with mixed case, numbers, and symbols"),
                Action = _ => SafeClipboardOperation(password),
                ContextData = new GeneratedData { Command = "password", Parameter = length.ToString(), Value = password },
            };
        }

        private Result GeneratePin(string parameter)
        {
            int length = 4; // default length
            if (int.TryParse(parameter, out int parsedLength) && (parsedLength == 4 || parsedLength == 6))
            {
                length = parsedLength;
            }

            var pin = GenerateSecurePin(length);

            return new Result
            {
                QueryTextDisplay = $"pin {length}",
                IcoPath = IconPath,
                Title = pin,
                SubTitle = $"Secure {length}-digit PIN - Click to copy",
                ToolTipData = new ToolTipData("Secure PIN", $"Generated cryptographically secure {length}-digit PIN code"),
                Action = _ => SafeClipboardOperation(pin),
                ContextData = new GeneratedData { Command = "pin", Parameter = length.ToString(), Value = pin },
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
                Action = _ => SafeClipboardOperation(email),
                ContextData = new GeneratedData { Command = "email", Value = email },
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
                Action = _ => SafeClipboardOperation(name),
                ContextData = new GeneratedData { Command = "name", Value = name },
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
                Action = _ => SafeClipboardOperation(address),
                ContextData = new GeneratedData { Command = "address", Value = address },
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
                Action = _ => SafeClipboardOperation(phone),
                ContextData = new GeneratedData { Command = "phone", Value = phone },
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
                Action = _ => SafeClipboardOperation(company),
                ContextData = new GeneratedData { Command = "company", Value = company },
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
                Action = _ => SafeClipboardOperation(lorem),
                ContextData = new GeneratedData { Command = "lorem", Parameter = wordCount.ToString(), Value = lorem },
            };
        }

        private Result GenerateNumber(string parameter)
        {
            int min = 1, max = 100;

            if (!string.IsNullOrEmpty(parameter))
            {
                var parts = parameter.Split('-');
                if (parts.Length >= 1 && int.TryParse(parts[0], out int parsedMin))
                    min = parsedMin;
                if (parts.Length >= 2 && int.TryParse(parts[1], out int parsedMax))
                    max = parsedMax;
            }

            if (min > max) 
            {
                var temp = min;
                min = max;
                max = temp;
            }

            var number = GetFaker().Random.Int(min, max).ToString();

            return new Result
            {
                QueryTextDisplay = $"number {min}-{max}",
                IcoPath = IconPath,
                Title = number,
                SubTitle = $"Random number between {min} and {max} - Click to copy",
                ToolTipData = new ToolTipData("Random Number", $"Generated random integer in range [{min}, {max}]"),
                Action = _ => SafeClipboardOperation(number),
                ContextData = new GeneratedData { Command = "number", Parameter = $"{min}-{max}", Value = number },
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
                Action = _ => SafeClipboardOperation(date),
                ContextData = new GeneratedData { Command = "date", Value = date },
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
                Action = _ => SafeClipboardOperation(guid),
                ContextData = new GeneratedData { Command = "guid", Value = guid },
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
                Action = _ => SafeClipboardOperation(color),
                ContextData = new GeneratedData { Command = "color", Value = color },
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
                Action = _ => SafeClipboardOperation(url),
                ContextData = new GeneratedData { Command = "url", Value = url },
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
                Action = _ => SafeClipboardOperation(creditCard),
                ContextData = new GeneratedData { Command = "creditcard", Value = creditCard },
            };
        }

        // Thread-safe clipboard operation with proper error handling
        private bool SafeClipboardOperation(string text)
        {
            try
            {
                // Use Application.Current.Dispatcher to ensure UI thread execution
                if (Application.Current?.Dispatcher != null)
                {
                    return (bool)Application.Current.Dispatcher.Invoke(() =>
                    {
                        try
                        {
                            Clipboard.SetDataObject(text);
                            return true;
                        }
                        catch
                        {
                            return false;
                        }
                    });
                }
                else
                {
                    Clipboard.SetDataObject(text);
                    return true;
                }
            }
            catch
            {
                return false;
            }
        }

        // Cryptographically secure password generation
        private static string GenerateSecurePassword(int length)
        {
            const string lowercase = "abcdefghijklmnopqrstuvwxyz";
            const string uppercase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string digits = "0123456789";
            const string symbols = "!@#$%^&*()_+-=[]{}|;:,.<>?";

            var allChars = lowercase + uppercase + digits + symbols;
            var password = new StringBuilder();

            // Ensure at least one character from each category using secure random
            password.Append(lowercase[RandomNumberGenerator.GetInt32(lowercase.Length)]);
            password.Append(uppercase[RandomNumberGenerator.GetInt32(uppercase.Length)]);
            password.Append(digits[RandomNumberGenerator.GetInt32(digits.Length)]);
            password.Append(symbols[RandomNumberGenerator.GetInt32(symbols.Length)]);

            // Fill the rest with cryptographically secure random characters
            for (int i = 4; i < length; i++)
            {
                password.Append(allChars[RandomNumberGenerator.GetInt32(allChars.Length)]);
            }

            // Shuffle the password using cryptographically secure random
            var passwordArray = password.ToString().ToCharArray();
            for (int i = passwordArray.Length - 1; i > 0; i--)
            {
                int j = RandomNumberGenerator.GetInt32(i + 1);
                (passwordArray[i], passwordArray[j]) = (passwordArray[j], passwordArray[i]);
            }

            return new string(passwordArray);
        }

        // Cryptographically secure PIN generation with weakness detection
        private static string GenerateSecurePin(int length)
        {
            if (length < 4) throw new ArgumentException("PIN must be at least 4 digits");

            string pin;
            int attempts = 0;
            const int maxAttempts = 100;

            do
            {
                var pinBuilder = new StringBuilder(length);
                for (int i = 0; i < length; i++)
                {
                    pinBuilder.Append(RandomNumberGenerator.GetInt32(0, 10));
                }
                pin = pinBuilder.ToString();
                attempts++;
            }
            while (IsWeakPin(pin) && attempts < maxAttempts);

            return pin;
        }

        // Detect weak PIN patterns
        private static bool IsWeakPin(string pin)
        {
            if (string.IsNullOrEmpty(pin)) return true;

            // Check for all same digits (1111, 0000, etc.)
            if (pin.All(c => c == pin[0])) return true;

            // Check for sequential patterns (1234, 4321, etc.)
            bool isIncreasing = true;
            bool isDecreasing = true;

            for (int i = 1; i < pin.Length; i++)
            {
                int current = pin[i] - '0';
                int previous = pin[i - 1] - '0';

                if (current != (previous + 1) % 10) isIncreasing = false;
                if (current != (previous - 1 + 10) % 10) isDecreasing = false;
            }

            return isIncreasing || isDecreasing;
        }

        private List<Result> GetHelpResults()
        {
            return
            [
                CreateHelpResult("password [length]", "Generate secure password (default: 12 chars)", "password 16"),
                CreateHelpResult("pin [4|6]", "Generate secure PIN code (default: 4 digits)", "pin 6"),
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
                ("password", "Generate secure password (default: 12 chars)", "password 16"),
                ("pwd", "Generate secure password (alias)", "pwd 16"),
                ("pin", "Generate secure PIN code (default: 4 digits)", "pin 6"),
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
                ("creditcard", "Generate random credit card number", "creditcard")
            };

            // First try exact prefix matches
            var prefixMatches = commands
                .Where(cmd => cmd.Item1.StartsWith(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(cmd => cmd.Item1.Length)
                .ThenBy(cmd => cmd.Item1)
                .ToList();

            if (prefixMatches.Any())
            {
                return prefixMatches.Take(8).Select(cmd => CreateSuggestionResult(cmd.Item1, cmd.Item2, cmd.Item3)).ToList();
            }

            // Then try substring matches
            var substringMatches = commands
                .Where(cmd => cmd.Item1.Contains(query, StringComparison.OrdinalIgnoreCase))
                .OrderBy(cmd => cmd.Item1.IndexOf(query, StringComparison.OrdinalIgnoreCase))
                .ThenBy(cmd => cmd.Item1.Length)
                .ThenBy(cmd => cmd.Item1)
                .ToList();

            if (substringMatches.Any())
            {
                return substringMatches.Take(5).Select(cmd => CreateSuggestionResult(cmd.Item1, cmd.Item2, cmd.Item3)).ToList();
            }

            // If no matches found, show all commands
            return GetHelpResults();
        }

        private Result CreateHelpResult(string command, string description, string example)
        {
            var cleanKeyword = GetCleanActionKeyword();
            return new Result
            {
                QueryTextDisplay = command,
                IcoPath = IconPath,
                Title = command,
                SubTitle = $"{description} (e.g., {cleanKeyword} {example})",
                ToolTipData = new ToolTipData("RandomGen Command", description),
                Action = _ => SafeQueryChange($"{cleanKeyword} {command.Split(' ')[0]} "),
                ContextData = new SuggestionData { Command = command },
            };
        }

        private Result CreateSuggestionResult(string command, string description, string example)
        {
            var cleanKeyword = GetCleanActionKeyword();
            return new Result
            {
                QueryTextDisplay = command,
                IcoPath = IconPath,
                Title = $"▶ {command}",
                SubTitle = $"{description} - Press Enter or Tab to select",
                ToolTipData = new ToolTipData("AutoComplete Suggestion", $"{description}\n\nExample: {cleanKeyword} {example}"),
                Score = 1000, // Higher score for better positioning
                Action = _ => SafeQueryChange($"{cleanKeyword} {command.Split(' ')[0]} "),
                ContextData = new SuggestionData { Command = command },
            };
        }

        private bool SafeQueryChange(string newQuery)
        {
            try
            {
                Context.API.ChangeQuery(newQuery);
                return false;
            }
            catch
            {
                return false;
            }
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
            // Handle generated data results
            if (selectedResult.ContextData is GeneratedData data)
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
                        Action = _ => SafeClipboardOperation(data.Value),
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
                            var cleanKeyword = GetCleanActionKeyword();
                            var commandWithParam = string.IsNullOrEmpty(data.Parameter) 
                                ? data.Command 
                                : $"{data.Command} {data.Parameter}";

                            return SafeQueryChange($"{cleanKeyword} {commandWithParam}");
                        },
                    }
                ];
            }

            // Handle suggestion results (autocomplete)
            if (selectedResult.ContextData is SuggestionData suggestion)
            {
                return
                [
                    new ContextMenuResult
                    {
                        PluginName = Name,
                        Title = "Select command (Enter)",
                        FontFamily = "Segoe MDL2 Assets",
                        Glyph = "\xE8A7", // Forward
                        AcceleratorKey = Key.Enter,
                        AcceleratorModifiers = ModifierKeys.None,
                        Action = _ =>
                        {
                            var cleanKeyword = GetCleanActionKeyword();
                            return SafeQueryChange($"{cleanKeyword} {suggestion.Command.Split(' ')[0]} ");
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

            try
            {
                _cancellationToken.Cancel();
                _disposables.ForEach(d => d?.Dispose());
                _disposables.Clear();
                _cache.Clear();
            }
            catch
            {
                // Ignore disposal errors
            }
            finally
            {
                Disposed = true;
            }
        }
    }

    // Helper classes for better context data handling
    public class GeneratedData
    {
        public string Command { get; set; }
        public string Parameter { get; set; }
        public string Value { get; set; }
    }

    public class SuggestionData
    {
        public string Command { get; set; }
    }

    // Cache helper class
    public class CachedResult
    {
        public string Value { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}