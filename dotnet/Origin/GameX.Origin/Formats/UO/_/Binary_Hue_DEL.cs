using GameX.Formats;
using GameX.Meta;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace GameX.Origin.Formats.UO
{
    public unsafe class Binary_Hue_DEL : IHaveMetaInfo
    {
        public static Task<object> Factory(BinaryReader r, FileSource f, PakFile s) => Task.FromResult((object)new Binary_Hue_DEL(r));

        #region Records

        const int HueWidth = 32; // Each hue is 32 colors
        const int HueHeight = 2048;
        const int HueCount = 4096;
        static readonly ushort[] Hues = new ushort[HueCount];
        static uint[] Pixels;
        static int[] CutOffValuesForWebSafeColors = { 0x19, 0x4C, 0x7F, 0xB2, 0xE5, 0xFF };
        static int[] WebSafeHues = {
            0000, 3881, 3882, 3883, 3884, 3885,
            3886, 3887, 3888, 3889, 3890, 3891,
            3892, 3893, 3894, 3895, 3896, 3897,
            3898, 3899, 3900, 3901, 3902, 3903,
            3904, 3905, 3906, 3907, 3908, 3909,
            3910, 3911, 3912, 3913, 3914, 3915,
            3916, 3917, 3918, 3919, 3920, 3921,
            3922, 3923, 3924, 3925, 3926, 3927,
            3928, 3929, 3930, 3931, 3932, 3933,
            3934, 3935, 3936, 3937, 3938, 3939,
            3940, 3941, 3942, 3943, 3944, 3945,
            3946, 3947, 3948, 3949, 3950, 3951,
            3952, 3953, 3954, 3955, 3956, 3957,
            3958, 3959, 3960, 3961, 3962, 3963,
            3964, 3965, 3966, 3967, 3968, 3969,
            3970, 3971, 3972, 3973, 3974, 3975,
            3976, 3977, 3978, 3979, 3980, 3981,
            3982, 3983, 3984, 3985, 3986, 3987,
            3988, 3989, 3990, 3991, 3992, 3993,
            3994, 3995, 3996, 3997, 3998, 3999,
            4000, 4001, 4002, 4003, 4004, 4005,
            4006, 4007, 4008, 4009, 4010, 4011,
            4012, 4013, 4014, 4015, 4016, 4017,
            4018, 4019, 4020, 4021, 4022, 4023,
            4024, 4025, 4026, 4027, 4028, 4029,
            4030, 4031, 4032, 4033, 4034, 4035,
            4036, 4037, 4038, 4039, 4040, 4041,
            4042, 4043, 4044, 4045, 4046, 4047,
            4048, 4049, 4050, 4051, 4052, 4053,
            4054, 4055, 4056, 4057, 4058, 4059,
            4060, 4061, 4062, 4063, 4064, 4065,
            4066, 4067, 4068, 4069, 4070, 4071,
            4072, 4073, 4074, 4075, 4076, 4077,
            4078, 4079, 4080, 4081, 4082, 4083,
            4084, 4085, 4086, 4087, 4088, 4089,
            4090, 4091, 4092, 4093, 4094, 4095 };

        //public static Texture2DInfo CreateHueSwatch(int width, int height, int[] hues)
        //{
        //    var pixels = new byte[width * height * 2];
        //    for (var i = 0; i < pixels.Length; i++)
        //    {
        //        var hue = hues[i];
        //        var pixel = new byte[1];
        //        //if (hue < _hueTextureHeight) HueTexture0.GetData(0, new RectInt(31, hue % _hueTextureHeight, 1, 1), pixel, 0, 1);
        //        //else HueTexture1.GetData(0, new RectInt(31, hue % _hueTextureHeight, 1, 1), pixel, 0, 1);
        //        pixels[i] = pixel[0];
        //    }
        //    var t = new Texture2DInfo(width, height, TextureFormat.Alpha8, false, pixels);
        //    return t;
        //}

        public static ushort GetHue(int index, int offset)
        {
            index += offset;
            return index < 0 ? (ushort)0xffffU : Hues[index & 0x1fff];
        }

        public static uint[] GetAllHues()
        {
            var hues = new uint[HueCount];
            var pixels = Pixels;
            for (var i = 0; i < HueCount; i++) hues[i] = pixels[i * 32 + 31];
            return hues;
        }

        public static int GetWebSafeHue(int r, int g, int b)
        {
            var index = 0;
            for (var i = 0; i < 6; i++) if (r <= CutOffValuesForWebSafeColors[i]) { index += i * 1; break; }
            for (var i = 0; i < 6; i++) if (g <= CutOffValuesForWebSafeColors[i]) { index += i * 6; break; }
            for (var i = 0; i < 6; i++) if (b <= CutOffValuesForWebSafeColors[i]) { index += i * 36; break; }
            return WebSafeHues[index];
        }

        #endregion

        // file: hues.mul
        public Binary_Hue_DEL(BinaryReader r)
        {
            const float multiplier = 0xff / 0x1f;

            //var blockCount = (int)r.BaseStream.Length / 708;
            //if (blockCount > 375) blockCount = 375;

            var currentHue = 0;
            var currentIndex = 0;
            Pixels = new uint[HueWidth * HueHeight * 2];
            currentIndex += 32;
            while (r.BaseStream.Position < r.BaseStream.Length)
            {
                r.Skip(4);
                for (var entry = 0; entry < 8; entry++)
                {
                    for (var i = 0; i < 32; i++)
                    {
                        var color = r.ReadUInt16();
                        if (i == 31) Hues[currentHue] = color;
                        Pixels[currentIndex++] = 0xFF000000 + (
                            ((uint)(((color >> 10) & 0x1F) * multiplier)) |
                            ((uint)(((color >> 5) & 0x1F) * multiplier) << 8) |
                            ((uint)((color & 0x1F) * multiplier) << 16)
                            );
                    }
                    r.Skip(24);
                    currentHue++;
                }
            }
            var webSafeHuesBegin = HueHeight * 2 - 216;
            for (var B = 0; B < 6; B++)
                for (var G = 0; G < 6; G++)
                    for (var R = 0; R < 6; R++)
                        Pixels[(webSafeHuesBegin + R + G * 6 + B * 36) * 32 + 31] = (uint)(
                            0xff000000 +
                            B * 0x00330000 +
                            G * 0x00003300 +
                            R * 0x00000033);
        }

        // IHaveMetaInfo
        List<MetaInfo> IHaveMetaInfo.GetInfoNodes(MetaManager resource, FileSource file, object tag)
        {
            var nodes = new List<MetaInfo> {
                new MetaInfo(null, new MetaContent { Type = "Text", Name = Path.GetFileName(file.Path), Value = "Hue File" }),
                new MetaInfo("Hue", items: new List<MetaInfo> {
                    new MetaInfo($"Hues: {Hues.Length}"),
                    new MetaInfo($"Data: {Pixels.Length}"),
                })
            };
            return nodes;
        }
    }
}
