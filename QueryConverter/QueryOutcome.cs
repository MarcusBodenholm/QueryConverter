using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace QueryConverter;
public class QueryOutcome<T> where T : class
{
    public IQueryable<T> Query { get; set; }
    public bool Success { get; set; } = true;
    public string ErrorMessage { get; set; } = string.Empty;
}
