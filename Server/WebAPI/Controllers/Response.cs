using System.Collections.Generic;
using CommonModels.Interfaces;

namespace WebAPI.Controllers;

public class Response
{
  public Dictionary<string, string> Schema { get; set; }
  public IEnumerable<IDto> Data { get; set; }
}