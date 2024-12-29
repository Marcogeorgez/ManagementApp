using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace LuminaryVisuals.Data.Entities;


public class DataProtectionKey
{
    [Key]
    public string Id { get; set; }
    public string FriendlyName { get; set; }
    public string Xml { get; set; }
}