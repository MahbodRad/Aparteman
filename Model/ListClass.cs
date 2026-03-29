using System.Collections.Generic;
using System.Data;

namespace Aparteman.Models
{
    public class UserInfo
    {
        public int Id { get; set; }
        public int Complex { get; set; }
        public string Name { get; set; }
        public string Knd { get; set; }
    }
    public class FormData
    {
        public DataTable lastForms { get; set; }
        public DataTable Pages { get; set; }
    }

    public class FormSetup
    {
        public int FormId { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string Title { get; set; }
        public string Header { get; set; }
        public string Footer { get; set; }
        public string MSG { get; set; }
        public string Today { get; set; }
        public string Script { get; set; }
    }

    public class ListForm
    {
        public string Daste { get; set; }
        public string Title { get; set; }
        public string Adres { get; set; }
        public string Css { get; set; }

    }

    public class LimitDate
    {
        public string FromDate { get; set; }
        public string ToDate { get; set; }
    }
}
