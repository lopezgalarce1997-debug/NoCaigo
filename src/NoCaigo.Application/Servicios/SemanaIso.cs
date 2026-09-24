using System.Globalization;

namespace NoCaigo.Application.Servicios;

/// <summary>
/// Semanas ISO 8601: empiezan el lunes y la semana 1 es la que contiene el primer jueves del año.
/// Por eso un día puede pertenecer a una semana del año anterior o siguiente
/// (por ejemplo, el viernes 1 de enero de 2027 pertenece a la semana 2026-W53).
/// </summary>
public static class SemanaIso
{
    /// <summary>Lunes de la semana que contiene la fecha.</summary>
    public static DateOnly Inicio(DateOnly fecha)
    {
        // DayOfWeek: domingo = 0 ... sábado = 6. Días desde el lunes: lunes 0 ... domingo 6.
        var diasDesdeLunes = ((int)fecha.DayOfWeek + 6) % 7;
        return fecha.AddDays(-diasDesdeLunes);
    }

    /// <summary>Identificador ISO, por ejemplo "2026-W39". Usa el año ISO, que no siempre es el del calendario.</summary>
    public static string Etiqueta(DateOnly fecha)
    {
        var dia = fecha.ToDateTime(TimeOnly.MinValue);
        return $"{ISOWeek.GetYear(dia)}-W{ISOWeek.GetWeekOfYear(dia):D2}";
    }
}
