namespace WebAPI.DTO;

public class EmployeeDTO
{
  /// <summary>
  /// ID.
  /// </summary>
  public virtual int Id { get; set; }
  
  /// <summary>
  /// Имя и фамилия.
  /// </summary>
  public virtual string Name { get; set; }
}