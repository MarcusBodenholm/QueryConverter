using Microsoft.AspNetCore.Http;
using QueryConverter.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace QueryConverter;
public class SearchOptions
{
    public string Search { get; set; } = string.Empty;
    public bool ShouldSearch { get; set; } = true;
    public bool Sorting { get; set; } = false;
    public string SortString { get; set; } = string.Empty;
    public int Limit { get; set; } = 0;
    public int Offset { get; set; } = 0;
    public bool Success { get; set; } = true;
    public string ErrorMessage { get; set; } = string.Empty;
    public static SearchOptions Create<T>(IQueryCollection query)
    {
        var output = BuildSearchOptions<T>(query);
        return output;
    }
    private static SearchOptions BuildSearchOptions<T>(IQueryCollection query)
    {
        var output = new SearchOptions();
        var searchString = "x => ";
        var count = 0;
        foreach (var param in query)
        {
            var paramKey = param.Key.Capitalize();
            count++;
            if (paramKey.Contains("Sort"))
            {
                output = DetermineSorting<T>(paramKey, param.Value!, output);
                continue;
            }
            if (paramKey == "Offset")
            {
                bool parse = int.TryParse(param.Value.ToString(), out int offset);
                if (parse)
                {
                    output.Offset = offset;
                    continue;
                }
                //If the value cannot be parsed, then the query is invalid. 
                output.Success = false;
                output.ErrorMessage = "Invalid query: Offset value could not be parsed";
                return output;
            }
            if (paramKey == "Limit")
            {
                bool parse = int.TryParse(param.Value.ToString(), out int limit);
                if (parse)
                {
                    output.Limit = limit;
                    continue;
                }
                //If the value cannot be parsed, then the query is invalid. 
                output.Success = false;
                output.ErrorMessage = "Invalid query: Limit value could not be parsed";
                return output;

            }
            if (paramKey == "Code") continue;
            if (paramKey.StartsWith("Search"))
            {
                searchString += DetermineSearch<T>(paramKey, param.Value, output);
                //If search could not find the correct property then the input is invalid. 
                if (output.Success == false)
                {
                    return output;
                }
            }
            else
            {
                string prop = paramKey.Split('[')[0].Capitalize();
                if (HasProperty<T>(prop) == false) continue;
                if (PropertyIsNumeric<T>(prop))
                {
                    searchString += AddNumericOperation(paramKey, prop, param.Value!);
                }
                else
                {
                    searchString += $"x.{prop}.ToLower() == \"{param.Value.ToString().ToLower()}\"";
                }
            }
            if (query.Count != count)
            {
                searchString += " && ";
            }
        }
        output.Search = CleanUp(searchString);
        if (output.Search == "x => ") output.ShouldSearch = false;
        return output;
    }
    private static string AddNumericOperation(string fullParamKey, string prop, string paramValue)
    {
        if (fullParamKey.Contains("gt"))
        {
            return $"x.{prop} gt {paramValue}";
        }
        if (fullParamKey.Contains("lt"))
        {
            return $"x.{prop} lt {paramValue}";
        }
        if (fullParamKey.Contains("ge"))
        {
            return $"x.{prop} ge {paramValue}";
        }
        if (fullParamKey.Contains("le"))
        {
            return $"x.{prop} le {paramValue}";
        }
        if (fullParamKey.Contains("gte"))
        {
            return $"x.{prop} ne {paramValue}";
        }
        return $"x.{prop} == {paramValue}";
    }
    private static SearchOptions DetermineSorting<T>(string key, string value, SearchOptions options)
    {
        if (value == null) return options;
        if (HasProperty<T>(value) == false) return options;
        if (options.Sorting)
        {
            options.SortString += $", {value}";

            if (key.ToLower().Contains("desc"))
            {
                options.SortString += " desc";
            }
            return options;
        }
        options.Sorting = true;
        options.SortString = $"{value}";

        if (key.ToLower().Contains("desc"))
        {
            options.SortString = " desc";
        }

        return options;
    }
    private static string DetermineSearch<T>(string key, string value, SearchOptions options)
    {
        int startIdx = key.IndexOf("[") + 1;
        int endIdx = key.IndexOf("]") - startIdx;
        string prop = key.Substring(startIdx, endIdx).Capitalize();
        if (options.Success == false) return "";
        if (IsStringType(GetProperty<T>(prop)))
        {
            return $"x.{prop}.ToLower().Contains(\"{value.ToString().ToLower()}\")";
        }
        else
        {
            options.Success = false;
            options.ErrorMessage = $"The provided property {prop} is not a string value or cannot be searched.";
            return "";
        }
    }

    private static bool HasProperty<T>(string input)
    {
        Type t = typeof(T);
        if (t.GetProperties().Any(p => p.Name.ToLower() == input.ToLower())) return true;
        return false;

    }
    private static string CleanUp(string input)
    {
        if (input.EndsWith(" && "))
        {
            return input.Substring(0, input.Length - 4);
        }
        return input;

    }
    private static bool PropertyIsNumeric<T>(string name)
    {
        PropertyInfo info = GetProperty<T>(name);
        bool isNumeric = IsNumericType(info);
        return isNumeric;
    }
    private static PropertyInfo GetProperty<T>(string name)
    {
        Type t = typeof(T);
        PropertyInfo prop = t.GetProperty(name.Capitalize())!;
        return prop;
    }

    private static bool IsNumericType(PropertyInfo o)
    {
        switch (Type.GetTypeCode(o.PropertyType))
        {
            case TypeCode.Byte:
            case TypeCode.SByte:
            case TypeCode.UInt16:
            case TypeCode.UInt32:
            case TypeCode.UInt64:
            case TypeCode.Int16:
            case TypeCode.Int32:
            case TypeCode.Int64:
            case TypeCode.Decimal:
            case TypeCode.Double:
            case TypeCode.Single:
                return true;
            default:
                return false;
        }
    }
    private static bool IsStringType(PropertyInfo o)
    {
        switch (Type.GetTypeCode(o.PropertyType))
        {
            case TypeCode.String:
                return true;
            default:
                return false;
        }
    }
}
