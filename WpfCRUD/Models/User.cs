using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace WpfCRUD.Models
{
    public class User
    {
        public int Id { get; set; }
        public string? FullName { get; set; }
        public required string Email { get; set; }
        public required string Password { get; set; }
        public int UserRoleId { get; set; } = 1;
        public UserRole? UserRole { get; set; }
    }
}
