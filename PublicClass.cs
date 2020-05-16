using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;

namespace Advanced_Multiple_Rename
{

    public class SortBy_Details
    {
        public int ID
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
    }

    public class filesExtensions
    {
        public int ID
        {
            get;
            set;
        }
        public string ExtensionName
        {
            get;
            set;
        }
        public Boolean Check_Status
        {
            get;
            set;
        }

    }

    public class PrefixClass
    {
        public int ID
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
    }

    public class RenameType
    {
        public int ID
        {
            get;
            set;
        }
        public string Name
        {
            get;
            set;
        }
    }

    class Grid_data_Info 
    {
        public int ID { get; set; }
        public string Name { get; set; }
        public string Location
        {
            set; get;
        }
        public string Type { get; set; }
        public string Folder_Name { get; set; }

        public DateTime Date_Modified { get; set; }
        public DateTime Date_Created { get; set; }
        public string Result { get; set; }
     

        public bool IsSelected { get; set; }


    }

    
}


