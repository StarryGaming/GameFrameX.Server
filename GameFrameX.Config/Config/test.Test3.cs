
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
    public sealed partial class Test3 : BeanBase
    {
        /*
        public Test3(int X, int Y) 
        {
            this.X = X;
            this.Y = Y;
            PostInit();
        }        
        */

        public Test3(JsonElement _buf) 
        {
            X = _buf.GetProperty("x").GetInt32();
            Y = _buf.GetProperty("y").GetInt32();
        }
    
        public static Test3 DeserializeTest3(JsonElement _buf)
        {
            return new test.Test3(_buf);
        }

        public int X { private set; get; }
        public int Y { private set; get; }

        private const int __ID__ = 638540133;
        public override int GetTypeId() => __ID__;

        public  void ResolveRef(TablesComponent tables)
        {
            
            
        }

        public override string ToString()
        {
            return "{ "
            + "x:" + X + ","
            + "y:" + Y + ","
            + "}";
        }

        partial void PostInit();
    }
}
