using System.ComponentModel.DataAnnotations;
using NoCaigo.Domain.Enums;

namespace NoCaigo.Application.Dtos;

/// <summary>Mensaje sospechoso que la persona quiere analizar.</summary>
public sealed class SolicitudAnalisis
{
    public const int LargoMaximoTexto = 2000;

    /// <summary>Texto del mensaje tal como llegó (SMS, WhatsApp o correo).</summary>
    /// <example>Tu cuenta BancoEstado fue bloqueada. Ingresa hoy a bit.ly/3xYz12 y valida tu clave.</example>
    [Required(AllowEmptyStrings = false, ErrorMessage = "El texto del mensaje es obligatorio.")]
    [MaxLength(LargoMaximoTexto, ErrorMessage = "El texto no puede superar los 2000 caracteres.")]
    public string Texto { get; init; } = string.Empty;

    /// <summary>Medio por el que llegó el mensaje.</summary>
    [Required(ErrorMessage = "El canal es obligatorio.")]
    [EnumDataType(typeof(Canal), ErrorMessage = "Canal no válido. Usa Sms, WhatsApp o Correo.")]
    public Canal? Canal { get; init; }
}
