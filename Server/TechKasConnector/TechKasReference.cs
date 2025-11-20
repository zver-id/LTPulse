using System.Runtime.InteropServices;
using System.Text;

namespace TechKasConnector;

/// <summary>
/// Справочник ТехКас.
/// </summary>
public class TechKasReference
{
  /// <summary>
  /// Справочник. Основное свойство доступа к данным.
  /// </summary>
  public dynamic Reference { get; private set; }
  
  /// <summary>
  /// Получить справочник TeхКас по имени.
  /// </summary>
  /// <param name="referenceName">Имя справочника.</param>
  /// <returns>Спрвочник ТехКас</returns>
  /// <exception cref="Exception">Возникает, если ТехКас не установлен.</exception>
  private static dynamic GetReference(string referenceName)
  {
    CoInitialize(IntPtr.Zero);
    Type? loginPointType = Type.GetTypeFromProgID("SBLogon.LoginPoint");
    if (loginPointType == null)
      throw new Exception("Login Point not found");
    dynamic loginPoint = Activator.CreateInstance(loginPointType);
    var app = loginPoint.GetApplication("systemcode=TEHKASNPO");
    return app.ReferencesFactory.ReferenceFactory(referenceName).GetComponent();
  }

  public int SetFilter(string attributeType, List<string> attributeValues, bool comparosonType = true)
  {
    string filterString;
    if (comparosonType)
      filterString = this.GetReferenceQueryEqual(attributeValues, attributeType);
    else
      filterString = this.GetReferenceQueryNotEqual(attributeValues, attributeType);
    int referenceFilter = this.Reference.AddWhere(filterString);
    return referenceFilter;
  }

  [DllImport("ole32.dll")]
  private static extern int CoInitialize(IntPtr pvReserved);

  [DllImport("ole32.dll")]
  private static extern void CoUninitialize();
  
  
  /// <summary>
  /// Получить запрос для ограничения параметров в случае если нужно проверить параметры на эквивалентность.
  /// </summary>
  /// <param name="attributes">Список параметров.</param>
  /// <param name="attributeType">Тип параметра.</param>
  /// <returns>Текст запроса.</returns>
  private string GetReferenceQueryEqual(List<string> attributes, string attributeType)
  {
    var query = new StringBuilder();
    foreach (var attribute in attributes)
    {
      if (query.Length == 0)
      {
        query.Append($"({this.Reference.TableName}" +
                     $".{this.Reference.Requisites(attributeType).FieldName} = '{attribute}')");
      }
      else
      {
        query.Append($" or {this.Reference.TableName}" +
                     $".{this.Reference.Requisites(attributeType).FieldName} = '{attribute}')");
        if (attributes.Count == attributes.IndexOf(attribute) + 1)
          query.Append(')');
      }
    }
    return query.ToString();
  }

  /// <summary>
  /// Получить запрос для ограничения параметров в случае если нужно проверить параметры на неравенство.
  /// </summary>
  /// <param name="attributes">Список параметров. Будет принят только первый.</param>
  /// <param name="attributeType">Тип параметра.</param>
  /// <returns>Текст запроса.</returns>
  private string GetReferenceQueryNotEqual(List<string> attributes, string attributeType)
  {
    return $"{this.Reference.TableName}.{this.Reference.Requisites(attributeType).FieldName} <> {attributes[0]}";
  }

  /// <summary>
  /// Отфильтровать автоматически решенные обращения.
  /// </summary>
  private void DisableAutoSolved()
  {
    var autoSolvedString = $"({this.Reference.TableName}.{this.Reference.Requisites("ДаНет5").FieldName} = 'Н'" +
                        $" or {this.Reference.TableName}.{this.Reference.Requisites("ДаНет5").FieldName} is Null)";
    this.Reference.AddWhere(autoSolvedString);
  }

  /// <summary>
  /// Конструктор.
  /// </summary>
  /// <param name="referenceName">Имя справочника.</param>
  public TechKasReference(string referenceName, bool disableAutoSolved = true)
  {
    this.Reference = GetReference(referenceName);
    if (disableAutoSolved)
      this.DisableAutoSolved();
  }
}