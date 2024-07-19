
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Text.Json;
using GameFrameX.Core.Config;

namespace GameFrameX.Config.test
{
    public sealed partial class Equipment : test.ItemBase
    {
        /*
        public Equipment(int Id, string Name, string Desc, test.DemoEnum Attr, int Value)  : base(Id, Name, Desc) 
        {
            this.Attr = Attr;
            this.Value = Value;
            PostInit();
        }        
        */

        public Equipment(JsonElement _buf)  : base(_buf) 
        {
            Attr = (test.DemoEnum)_buf.GetProperty("attr").GetInt32();
            Value = _buf.GetProperty("value").GetInt32();
        }
    
        public static Equipment DeserializeEquipment(JsonElement _buf)
        {
            return new test.Equipment(_buf);
        }

        public test.DemoEnum Attr { private set; get; }
        public int Value { private set; get; }

        private const int __ID__ = -76837102;
        public override int GetTypeId() => __ID__;

        public override void ResolveRef(TablesComponent tables)
        {
            base.ResolveRef(tables);
            
            
        }

        public override string ToString()
        {
            return "{ "
            + "id:" + Id + ","
            + "name:" + Name + ","
            + "desc:" + Desc + ","
            + "attr:" + Attr + ","
            + "value:" + Value + ","
            + "}";
        }

        partial void PostInit();
    }
}
