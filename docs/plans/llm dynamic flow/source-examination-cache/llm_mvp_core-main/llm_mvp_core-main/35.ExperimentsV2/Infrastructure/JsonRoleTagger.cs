using System.Text;
using System.Text.Json;

namespace ExperimentsV2;

// Tags JSON tokens with one of 9 roles via System.Text.Json.Utf8JsonReader.
// Used by E08c (non-code syntax experiment).
//
// Roles: 0=UNKNOWN 1=START_OBJECT 2=END_OBJECT 3=START_ARRAY 4=END_ARRAY
//        5=PROPERTY_NAME 6=STRING_VALUE 7=NUMBER_VALUE 8=BOOL_NULL
public static class JsonRoleTagger
{
    public const int RoleCount     = 9;
    public const int UNKNOWN       = 0;
    public const int START_OBJECT  = 1;
    public const int END_OBJECT    = 2;
    public const int START_ARRAY   = 3;
    public const int END_ARRAY     = 4;
    public const int PROPERTY_NAME = 5;
    public const int STRING_VALUE  = 6;
    public const int NUMBER_VALUE  = 7;
    public const int BOOL_NULL     = 8;

    // Tokenise a JSON string and return a list of role ids.
    public static List<int> TagJson(string json)
    {
        var result = new List<int>();
        var bytes  = Encoding.UTF8.GetBytes(json);
        var reader = new Utf8JsonReader(bytes, new JsonReaderOptions { AllowTrailingCommas = true });

        try
        {
            while (reader.Read())
            {
                result.Add(reader.TokenType switch
                {
                    JsonTokenType.StartObject  => START_OBJECT,
                    JsonTokenType.EndObject    => END_OBJECT,
                    JsonTokenType.StartArray   => START_ARRAY,
                    JsonTokenType.EndArray     => END_ARRAY,
                    JsonTokenType.PropertyName => PROPERTY_NAME,
                    JsonTokenType.String       => STRING_VALUE,
                    JsonTokenType.Number       => NUMBER_VALUE,
                    JsonTokenType.True         => BOOL_NULL,
                    JsonTokenType.False        => BOOL_NULL,
                    JsonTokenType.Null         => BOOL_NULL,
                    _                          => UNKNOWN,
                });
            }
        }
        catch (JsonException) { /* return partial results on invalid JSON */ }

        return result;
    }
}
