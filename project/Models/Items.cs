using System;
namespace project.Models
{
    public class Items
    {
        public int Id_item { get; set; }
        public int Id_list { get; set; }
        public string Name { get; set; }
        public string Tags { get; set; }
        public string String1 { get; set; }
        public string String2 { get; set; }
        public string String3 { get; set; }
        public string Multiline1 { get; set; }
        public string Multiline2 { get; set; }
        public string Multiline3 { get; set; }
        public int Int1 { get; set; }
        public int Int2 { get; set; }
        public int Int3 { get; set; }
        public Checkbox Checkbox1 { get; set; }
        public Checkbox Checkbox2 { get; set; }
        public Checkbox Checkbox3 { get; set; }
        public string Data1 { get; set; }
        public string Data2 { get; set; }
        public string Data3 { get; set; }
    }
    public enum Checkbox
    {
        Yes,
        No
    }
}

