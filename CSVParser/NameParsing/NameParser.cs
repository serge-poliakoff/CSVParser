using CSVParser.ColumnTransformers;
using System.Collections;

namespace CSVParser.NameParsing;

public class NameParser : IEnumerable<Func<string, string>>
{
    private List<Func<string, string>> namePolicies;

    public NameParser()
    {
        namePolicies = new List<Func<string, string>>()
        {
            NameParsingDefaults.ExactMatch
        };
    }

    public NameParser WithNamingPolicy(Func<string, string> policy)
    {
        namePolicies.Clear();
        namePolicies.Add(policy);

        return this;
    }

    public NameParser AddNamingPolicy(Func<string, string> policy)
    {
        namePolicies.Add(policy);

        return this;
    }

    public NameParser AddNamingPolicy(NameParsingStandarts policy)
    {
        switch(policy)
        {
            case NameParsingStandarts.ExactMatch:
                AddNamingPolicy(NameParsingDefaults.ExactMatch);
                break;
            case NameParsingStandarts.SnakeCase:
                AddNamingPolicy(NameParsingDefaults.SnakeCase);
                break;
        }
        return this;
    }

    public NameParser WithNamingPolicy(NameParsingStandarts policy)
    {
        namePolicies.Clear();
        return AddNamingPolicy(policy);
    }

    public IEnumerator<Func<string, string>> GetEnumerator()
    {
        return namePolicies.GetEnumerator();
    }

    IEnumerator IEnumerable.GetEnumerator()
    {
        return GetEnumerator();
    }
}
