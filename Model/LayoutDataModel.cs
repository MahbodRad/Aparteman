using System.Collections.Generic;
using System.Data;

namespace Aparteman.Models
{
    public class LayoutDataModel
    {
        public UserInfo userInfo { get; set; }
        public FormSetup formSetup { get; set; }
        public List<ListForm> listForms { get; set; }
        public DataTable listNewMsg { get; set; }
        public DataTable listYadavar { get; set; }
    }
}
