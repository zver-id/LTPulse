using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Reflection;
using System.Threading.Tasks;
using Application;
using AutoMapper;
using CommonModels.Interfaces;
using CommonModels.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using WebAPI.DTO;

namespace WebAPI.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TableSchemaController(IMapper mapper, IRepository repository, ILogger<TableSchemaController> logger)
  : GenericController(repository, mapper)
{
  /// <summary>
  /// Логгер.
  /// </summary>
  private ILogger<TableSchemaController> Logger {get; set;} = logger;

  [HttpGet]
  public async Task<ActionResult<IEnumerable<TableSchemaElement>>> GetTableSchema(string tableName)
  {
    switch (tableName)
    {
      case "ticket":
        Type typeOfTable = typeof(TicketDTO);
        return this.Ok(this.GetSchemaByType(typeOfTable));
        break;
    }
    return this.StatusCode(StatusCodes.Status418ImATeapot, "I'm a teapot");
  }

  private List<TableSchemaElement> GetSchemaByType(Type type)
  {
    var schema = new List<TableSchemaElement>();
    PropertyInfo[] properties = type.GetProperties();
    foreach (PropertyInfo property in properties)
    {
      var display = property.GetCustomAttribute<DisplayAttribute>();
      if (display != null && display.Name != null)
      {
        schema.Add(new TableSchemaElement
        {
          Title = display.Name,
          dataIndex = property.Name,
          key = property.Name
        });
      }
    }
    return schema;
  }
}