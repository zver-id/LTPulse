using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using CommonModels.Interfaces;
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
  public async Task<ActionResult<IEnumerable<TableSchemaElement>>> GetTableSchema()
  {
    
  } 
}