using System;
using System.IO;

namespace OA.Ultima.Data
{
    public enum BodyType : byte
    {
        Empty,
        Monster,
        Sea,
        Animal,
        Human,
        Equipment
    }

    public struct Body
    {
        int _bodyID;

        static BodyType[] _types;

        static Body()
        {
            var path = @"data/bodytable.cfg";
            if (File.Exists(path))
                using (var ip = new StreamReader(path))
                {
                    _types = new BodyType[0x1000];
                    string line;
                    while ((line = ip.ReadLine()) != null)
                    {
                        if (line.Length == 0 || line.StartsWith("#"))
                            continue;
                        var split = line.Split('\t');
                        BodyType type;
                        int bodyID;
                        if (int.TryParse(split[0], out bodyID) && Enum.TryParse(split[1], true, out type) && bodyID >= 0 && bodyID < _types.Length)
                            _types[bodyID] = type;
                        else
                        {
                            Console.WriteLine("Warning: Invalid bodyTable entry:");
                            Console.WriteLine(line);
                        }
                    }
                }
            else
            {
                Console.WriteLine("Warning: Data/bodyTable.cfg does not exist");
                _types = new BodyType[0];
            }
        }

        public Body(int bodyID)
        {
            _bodyID = bodyID;
        }

        public BodyType Type
        {
            get
            {
                if (_bodyID >= 0 && _bodyID < _types.Length) return _types[_bodyID];
                else return BodyType.Empty;
            }
        }

        public bool IsHumanoid
        {
            get
            {
                return _bodyID >= 0
                    && _bodyID < _types.Length
                    && _types[_bodyID] == BodyType.Human
                    && _bodyID != 402
                    && _bodyID != 403
                    && _bodyID != 607
                    && _bodyID != 608
                    && _bodyID != 694
                    && _bodyID != 695
                    && _bodyID != 970;
            }
        }

        public bool IsGargoyle
        {
            get
            {
                return _bodyID == 666
                    || _bodyID == 667
                    || _bodyID == 694
                    || _bodyID == 695;
            }
        }

        public bool IsMale
        {
            get
            {
                return _bodyID == 183
                    || _bodyID == 185
                    || _bodyID == 400
                    || _bodyID == 402
                    || _bodyID == 605
                    || _bodyID == 607
                    || _bodyID == 666
                    || _bodyID == 694
                    || _bodyID == 750;
            }
        }

        public bool IsFemale
        {
            get
            {
                return _bodyID == 184
                    || _bodyID == 186
                    || _bodyID == 401
                    || _bodyID == 403
                    || _bodyID == 606
                    || _bodyID == 608
                    || _bodyID == 667
                    || _bodyID == 695
                    || _bodyID == 751;
            }
        }

        public bool IsGhost
        {
            get
            {
                return _bodyID == 402
                    || _bodyID == 403
                    || _bodyID == 607
                    || _bodyID == 608
                    || _bodyID == 694
                    || _bodyID == 695
                    || _bodyID == 970;
            }
        }

        public bool IsMonster
        {
            get
            {
                return _bodyID >= 0
                    && _bodyID < _types.Length
                    && _types[_bodyID] == BodyType.Monster;
            }
        }

        public bool IsAnimal
        {
            get
            {
                return _bodyID >= 0
                    && _bodyID < _types.Length
                    && _types[_bodyID] == BodyType.Animal;
            }
        }

        public bool IsEmpty
        {
            get
            {
                return _bodyID >= 0
                    && _bodyID < _types.Length
                    && _types[_bodyID] == BodyType.Empty;
            }
        }

        public bool IsSea
        {
            get
            {
                return _bodyID >= 0
                    && _bodyID < _types.Length
                    && _types[_bodyID] == BodyType.Sea;
            }
        }

        public bool IsEquipment
        {
            get
            {
                return _bodyID >= 0
                    && _bodyID < _types.Length
                    && _types[_bodyID] == BodyType.Equipment;
            }
        }

        public int BodyID
        {
            get { return _bodyID; }
        }

        public static implicit operator int(Body a)
        {
            return a._bodyID;
        }

        public static implicit operator Body(int a)
        {
            return new Body(a);
        }

        public override string ToString()
        {
            return string.Format("0x{0:X}", _bodyID);
        }

        public override int GetHashCode()
        {
            return _bodyID;
        }

        public override bool Equals(object o)
        {
            if (o == null || !(o is Body)) return false;
            return ((Body)o)._bodyID == _bodyID;
        }

        public static bool operator ==(Body l, Body r)
        {
            return l._bodyID == r._bodyID;
        }

        public static bool operator !=(Body l, Body r)
        {
            return l._bodyID != r._bodyID;
        }

        public static bool operator >(Body l, Body r)
        {
            return l._bodyID > r._bodyID;
        }

        public static bool operator >=(Body l, Body r)
        {
            return l._bodyID >= r._bodyID;
        }

        public static bool operator <(Body l, Body r)
        {
            return l._bodyID < r._bodyID;
        }

        public static bool operator <=(Body l, Body r)
        {
            return l._bodyID <= r._bodyID;
        }
    }
}