using System.Collections.Generic;
using CommonModels.Interfaces;
using WebAPI.DTO;

namespace WebAPI.Controllers;

public class Response<T> where T : IDto
{
  public Dictionary<string, string> Schema { get; set; }
  public IEnumerable<T> Data { get; set; }
}