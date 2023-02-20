using System;
using static OpenStack.Debug;

namespace grendgine_collada
{
    public partial class Grendgine_Collada_Bool_Array_String
    {
        public bool[] Value() => Grendgine_Collada_Parse_Utils.String_To_Bool(Value_As_String);
    }

    public partial class Grendgine_Collada_Common_Float2_Or_Param_Type
    {
        public float[] Value() => Grendgine_Collada_Parse_Utils.String_To_Float(Value_As_String);
    }

    public partial class Grendgine_Collada_Float_Array_String
    {
        public float[] Value() => Grendgine_Collada_Parse_Utils.String_To_Float(Value_As_String);
    }

    public partial class Grendgine_Collada_Int_Array_String
    {
        public int[] Value() => Grendgine_Collada_Parse_Utils.String_To_Int(this.Value_As_String);
    }

    public class Grendgine_Collada_Parse_Utils
    {
        public static int[] String_To_Int(string int_array)
        {
            var str = int_array.Split(' ');
            var array = new int[str.LongLength];
            try
            {
                for (var i = 0L; i < str.LongLength; i++)
                    array[i] = Convert.ToInt32(str[i]);
            }
            catch (Exception e)
            {
                Log(e.ToString());
                Log(int_array);
            }
            return array;
        }

        public static float[] String_To_Float(string float_array)
        {
            var str = float_array.Split(' ');
            var array = new float[str.LongLength];
            try
            {
                for (var i = 0L; i < str.LongLength; i++)
                    array[i] = Convert.ToSingle(str[i]);
            }
            catch (Exception e)
            {
                Log(e.ToString());
                Log(float_array);
            }
            return array;
        }

        public static bool[] String_To_Bool(string bool_array)
        {
            var str = bool_array.Split(' ');
            var array = new bool[str.LongLength];
            try
            {
                for (var i = 0L; i < str.LongLength; i++)
                    array[i] = Convert.ToBoolean(str[i]);
            }
            catch (Exception e)
            {
                Log(e.ToString());
                Log(bool_array);
            }
            return array;
        }
    }

    public partial class Grendgine_Collada_SID_Float_Array_String
    {
        public float[] Value() => Grendgine_Collada_Parse_Utils.String_To_Float(Value_As_String);
    }

    public partial class Grendgine_Collada_SID_Int_Array_String
    {
        public int[] Value() => Grendgine_Collada_Parse_Utils.String_To_Int(Value_As_String);
    }

    public partial class Grendgine_Collada_String_Array_String
    {
        public string[] Value() => Value_Pre_Parse.Split(' ');
    }
}