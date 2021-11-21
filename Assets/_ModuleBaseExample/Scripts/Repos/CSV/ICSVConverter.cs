using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Ero.Daos.Csv
{
    public interface ICSVConverter<T>
    {
        T Convert(string[] headers, string[] values);
    }

    public interface ICSVEntityConverter<T>
    {
        T Convert(CSVEntity entity);
    }
}