using System.ComponentModel.DataAnnotations;

namespace WinChat.Infrastructure.Repository;

public class ApplicationData
{
    [Key]
    public string? SettingKey { get; set; }
    public string? SettingValue { get; set; }
}
