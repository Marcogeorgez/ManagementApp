using System.ComponentModel.DataAnnotations;

namespace ManagementApp.Data.Entities;


public class DataProtectionKey
{
    [Key]
    public string Id { get; set; }
    public string FriendlyName { get; set; }
    public string Xml { get; set; }
}