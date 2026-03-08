namespace TechKasConnector;

/// <summary>
/// Вспомогательный класс для работы с фильтрами.
/// </summary>
public class ReferenceFilterManager(TechKasReference reference) : IDisposable
{
  private readonly List<int> filterIds = new List<int>();

  public int AddFilter(string requisite, string value, string operation = "=")
  {
    int filterId = reference.SetFilter(requisite, value, operation);
    filterIds.Add(filterId);
    return filterId;
  }
  
  public int AddFilter(string attributeType, List<string> attributes)
  {
    int filterId = reference.SetFilter(attributeType, attributes);
    filterIds.Add(filterId);
    return filterId;
  }
  
  public int AddNullableFilter(string requisite, string value, string operation = "=")
  {
    int filterId = reference.SetNullableFilter(requisite, value, operation);
    filterIds.Add(filterId);
    return filterId;
  }

  public void RemoveFilter(int filterId)
  {
    reference.DeleteFilter(filterId);
    this.filterIds.Remove(filterId);
  }

  public void Dispose()
  {
    foreach (int filterId in filterIds)
    {
      reference.DeleteFilter(filterId);
    }
  }
}