
//------------------------------------------------------------------------------
// <auto-generated>
//     This code was generated by a tool.
//     Changes to this file may cause incorrect behavior and will be lost if
//     the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------

using System.Text.Json;
using GameFrameX.Core.Config;

namespace GameFrameX.Config
{
    public partial struct vec3
    {
        /*
        public vec3(float X, float Y, float Z) 
        {
            this.X = X;
            this.Y = Y;
            this.Z = Z;
            PostInit();
        }        
        */

        public vec3(JsonElement _buf) 
        {
            X = _buf.GetProperty("x").GetSingle();
            Y = _buf.GetProperty("y").GetSingle();
            Z = _buf.GetProperty("z").GetSingle();
        }
    
        public static vec3 Deserializevec3(JsonElement _buf)
        {
            return new vec3(_buf);
        }

        public float X { private set; get; }
        public float Y { private set; get; }
        public float Z { private set; get; }


        public  void ResolveRef(TablesComponent tables)
        {
            
            
            
        }

        public override string ToString()
        {
            return "{ "
            + "x:" + X + ","
            + "y:" + Y + ","
            + "z:" + Z + ","
            + "}";
        }

        partial void PostInit();
    }
}
