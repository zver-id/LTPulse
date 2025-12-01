using System.Collections;
using System.Runtime.InteropServices;
using System.Text;

namespace TechKasConnector;

/// <summary>
/// Справочник ТехКас.
/// </summary>
public class TechKasReference : IEnumerable<TechKasElement>
{
  private IEnumerable<TechKasElement> enumerableImplementation;

  #region Поля и свойства

  /// <summary>
  /// Количество записей в справочнике.
  /// </summary>
  public int Count
  {
    get
    {
      this.Reference.Open();
      int count = this.Reference.RecordCount;
      this.Reference.Close();
      return count;
    }
  }

  /// <summary>
  /// Справочник. Основное свойство доступа к данным.
  /// </summary>
  private dynamic Reference { get; set; }
  
  #endregion

  #region Методы

  /// <summary>
  /// Добавить фильтр к справочнику.
  /// </summary>
  /// <param name="attributeType">Тип атрибута, по которому ставим фильтр.</param>
  /// <param name="attributeValue">Значение атрибута.</param>
  /// <param name="comparosonType">Тип сравнения (сортировать по этому значению, исключить значение)</param>
  /// <returns>ИД фильтра.</returns>
  public int SetFilter(string attributeType, string attributeValue, string comparisonOperator = "=")
  {
    int referenceFilter = this.Reference.AddWhere(
      $"{this.Reference.TableName}.{this.Reference.Requisites(attributeType).FieldName}" +
      $" {comparisonOperator} '{attributeValue}'");
    return referenceFilter;
  }
  
  /// <summary>
  /// Добавить ограничение из списка парамтеров.
  /// </summary>
  /// <param name="attributeType"></param>
  /// <param name="attributes"></param>
  /// <returns></returns>
  public int SetFilter(string attributeType, List<string> attributes)
  {
    var query = new StringBuilder();
    foreach (var attribute in attributes)
    {
      if (query.Length == 0)
      {
        query.Append($"({this.Reference.TableName}" +
                     $".{this.Reference.Requisites(attributeType).FieldName} = '{attribute}'");
      }
      else
      {
        query.Append($" or {this.Reference.TableName}" +
                     $".{this.Reference.Requisites(attributeType).FieldName} = '{attribute}'");
        if (attributes.Count == attributes.IndexOf(attribute) + 1)
          query.Append(')');
      }
    }
    int filterID = this.Reference.AddWhere(query.ToString());
    return filterID;
  }

  /// <summary>
  /// Удалить фильтр справочника.
  /// </summary>
  /// <param name="filterId">Id фильтра.</param>
  public void DeleteFilter(int filterId)
  {
    this.Reference.DelWhere(filterId);
  }

  /// <summary>
  /// Переключает справочник на следующую запись.
  /// </summary>
  public void NextRecord()
  {
    this.Reference.Cancel();
    this.Reference.CloseRecord();
    this.Reference.Next();
  }

  /// <summary>
  /// Открыть справочник.
  /// </summary>
  public void OpenReference()
  {
    this.Reference.Open();
    this.Reference.First();
  }

  /// <summary>
  /// Закрыть справочник.
  /// </summary>
  public void CloseReference()
  {
    this.Reference.Close();
  }
  
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
  
  /// <summary>
  /// Отфильтровать автоматически решенные обращения.
  /// </summary>
  private void DisableAutoSolved()
  {
    var autoSolvedString = $"({this.Reference.TableName}.{this.Reference.Requisites("ДаНет5").FieldName} = 'Н'" +
                           $" or {this.Reference.TableName}.{this.Reference.Requisites("ДаНет5").FieldName} is Null)";
    this.Reference.AddWhere(autoSolvedString);
  }

  [DllImport("ole32.dll")]
  private static extern int CoInitialize(IntPtr pvReserved);

  [DllImport("ole32.dll")]
  private static extern void CoUninitialize();
  
  #endregion

  #region IEnumerable

  public IEnumerator<TechKasElement> GetEnumerator()
  {
    this.OpenReference();
    while (!this.Reference.EOF)
    {
      yield return new TechKasElement(this.Reference);
      this.NextRecord();
    }
  }
  
  IEnumerator IEnumerable.GetEnumerator()
  {
    return this.GetEnumerator();
  }
  
  #endregion
  
  #region Конструкторы
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
  
  #endregion
}
