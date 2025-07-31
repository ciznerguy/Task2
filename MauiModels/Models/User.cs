using System;

namespace MauiModels.Models
{
    public class User
    {
        public int Id { get; set; }

        public string FullName { get; set; } = string.Empty;

        public string Username { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;

        public string Password { get; set; } = string.Empty;

        public DateTime BirthDate { get; set; }

        public int Role { get; set; }

        // תואם לשדה age בג'ייסון ובטבלה
        public int Age { get; set; }

        // אם תרצה חישוב דינמי, אל תשתמש בו לתקשורת עם DB/JSON
        public int ComputedAge => (int)((DateTime.Now - BirthDate).TotalDays / 365);
    }
}
